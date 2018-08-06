﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using Lykke.SettingsReader.Exceptions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.SettingsReader
{
    /// <summary>
    /// Class for settings json parsing and validation.
    /// </summary>
    [PublicAPI]
    public static partial class SettingsProcessor
    {
        private static CloudQueue _queue;
        private static string _sender;

        /// <summary>
        /// Parses and validates settings json.
        /// </summary>
        /// <typeparam name="T">Type for parsing</typeparam>
        /// <param name="json">Input json</param>
        /// <returns>Parsed object of generic type T</returns>
        public static T Process<T>(string json)
        {
            return ProcessForConfiguration<T>(json).Item1;
        }

        /// <summary>
        /// Parses and validates settings json.
        /// </summary>
        /// <typeparam name="T">Type for parsing</typeparam>
        /// <param name="json">Input json</param>
        /// <returns>Parsed object of generic type T and parsed JToken object for settings json</returns>
        public static (T, JToken) ProcessForConfiguration<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new JsonStringEmptyException();

            JToken jsonObj;
            try
            {
                jsonObj = (JToken)JsonConvert.DeserializeObject(json);
            }
            catch (Exception e)
            {
                throw new IncorrectJsonFormatException(e);
            }

            var result = FillChildrenFields<T>(jsonObj);

            return (result, jsonObj);
        }

        /// <summary>
        /// Checks dependencies
        /// </summary>
        /// <param name="model">model to check dependenices for</param>
        /// <param name="slackConnString">connection string for slack to send failed dependecy message</param>
        /// <param name="queueName">queue name to send failed message</param>
        /// <param name="sender">name of the sender</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<string> CheckDependenciesAsync<T>(T model, string slackConnString, string queueName, string sender)
        {
            var account = CloudStorageAccount.Parse(slackConnString);
            var client = account.CreateCloudQueueClient();
            _queue = client.GetQueueReference(queueName);
            _sender = sender;
            await _queue.CreateIfNotExistsAsync();

            Console.WriteLine("Start checking services...");
            string errorMessages = await ProcessChecks(model);
            Console.WriteLine(string.IsNullOrEmpty(errorMessages)
                ? "Services checked. OK"
                : $"Services checked:{Environment.NewLine}{errorMessages} ");
            return errorMessages;
        }
        
        private static T FillChildrenFields<T>(JToken jsonObj)
        {
            return (T)Convert(jsonObj, typeof(T), "");
        }

        private static object Convert(JToken jsonObj, Type targetType, string path)
        {
            switch (jsonObj.Type)
            {
                case JTokenType.Object:
                    return Convert_FromObject((JObject)jsonObj, targetType, path);
                case JTokenType.Array:
                    return Convert_FromArray((JArray)jsonObj, targetType, path);
                case JTokenType.Property:
                    return Convert_FromProperty((JProperty)jsonObj, targetType, path);
                default:
                    return Convert_FromValue(((JValue)jsonObj).Value, targetType, path);
            }
        }

        private static object Convert_FromArray(JArray jsonObj, Type targetType, string path)
        {
            if (!IsEnumerable(targetType))
            {
                throw new RequiredFieldEmptyException($"{path}.{jsonObj}".Trim('.'));
            }
            var childType = targetType.IsArray
                ? targetType.GetElementType()
                : targetType.GenericTypeArguments.First();

            var concreteType = typeof(List<>).MakeGenericType(childType);
            var res = (IList)Activator.CreateInstance(concreteType);

            foreach (var elem in jsonObj)
            {
                var propertyPath = ConcatPath(path, res.Count.ToString());
                res.Add(Convert(elem, childType, propertyPath));
            }

            if (!targetType.IsArray) 
                return res;
            
            var arr = Array.CreateInstance(targetType.GetElementType(), res.Count);
            
            for (var ii = 0; ii < res.Count; ii++)
            {
                arr.SetValue(res[ii], ii);
            }
            
            return arr;
        }

        private static bool IsGenericEnumerable(Type type)
        {
            return type.GetTypeInfo().IsGenericType &&
                type.GetTypeInfo().GetInterfaces().Any(
                ti => (ti == typeof(IEnumerable<>) || ti.Name == "IEnumerable"));
        }

        private static bool IsEnumerable(Type type)
        {
            return IsGenericEnumerable(type) || type.IsArray;
        }

        private static object Convert_FromProperty(JProperty jsonObj, Type targetType, string path)
        {
            return Convert(jsonObj.Value, targetType, path);
        }

        private static string ConcatPath(string path, string propertyName)
        {
            return $"{path}.{propertyName}".Trim('.');
        }

        private static async Task<string> ProcessChecks<T>(T model)
        {
            if (model == null)
                return null;

            Type objType = model.GetType();
            PropertyInfo[] properties = objType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.GetIndexParameters().Any())
                .ToArray();

            var tasks = properties.Select(p => CheckProperty(p, model)).ToList();

            var errorMessages = (await Task.WhenAll(tasks))
                .Where(item => !string.IsNullOrEmpty(item))
                .ToList();
            
            return errorMessages.Count == 0 ? null : string.Join(Environment.NewLine, errorMessages);
        }

        private static async Task<string> CheckProperty<T>(PropertyInfo property, T model)
        {
            if (!property.CanRead)
            {
                Console.WriteLine($"Can't check {property.Name}. It has no Get method");
                return null;
            }

            object value = property.GetValue(model);
            var checkAttribute = (BaseCheckAttribute)property.GetCustomAttribute(typeof(BaseCheckAttribute));

            List<string> errorMessages;
            List<Task<string>> tasks;
            
            if (checkAttribute != null)
            {
                var checker = checkAttribute.GetChecker();
                string[] valuesToCheck = Array.Empty<string>();
                switch (value)
                {
                    case IReadOnlyList<string> strings:
                        valuesToCheck = strings.ToArray();
                        break;
                    case string str:
                        valuesToCheck = new[] { str };
                        break;
                }

                tasks = valuesToCheck.Select(val => DoCheck(model, property, checker, val)).ToList();

                errorMessages = (await Task.WhenAll(tasks))
                    .Where(item => !string.IsNullOrEmpty(item))
                    .ToList();

                return errorMessages.Count == 0 ? null : string.Join(Environment.NewLine, errorMessages);
            }

            if (!property.CanWrite) 
                return null;
            
            object[] values = GetValuesToCheck(property, model);

            tasks = values.Select(ProcessChecks).ToList();

            errorMessages = (await Task.WhenAll(tasks))
                .Where(item => !string.IsNullOrEmpty(item))
                .ToList();
            
            return errorMessages.Count == 0 ? null : string.Join(Environment.NewLine, errorMessages);
        }

        private static async Task<string> DoCheck<T>(T model, PropertyInfo property, ISettingsFieldChecker checker, string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                var optionalAttribute = property.GetCustomAttribute(typeof(OptionalAttribute));
                
                if (optionalAttribute == null)
                    throw new CheckFieldException(property.Name, val, "Empty setting value");
                
                return null;
            }

            int retryCount = 0;
            CheckFieldResult checkResult;
            
            do
            {
                if (retryCount > 0)
                    Thread.Sleep(100);
                    
                checkResult = checker.CheckField(model, property.Name, val);
                retryCount++;

            } while (!checkResult.Result && retryCount < 3);

            if (!checkResult.Result)
            {
                await SendSlackNotification(checkResult.Description);
                return checkResult.Description;
            }

            return null;
        }

        private static Task SendSlackNotification(string message)
        {
            return _queue.AddMessageAsync(new CloudQueueMessage(new
            {
                Type = "Monitor",
                Sender = _sender,
                Message = message
            }.ToJson()));
        }

        private static object[] GetValuesToCheck<T>(PropertyInfo property, T model)
        {
            var values = new List<object>();

            var getMethod = property.GetGetMethod();

            if (getMethod != null && getMethod.ReturnType != typeof(string)
                && typeof(IEnumerable).IsAssignableFrom(getMethod.ReturnType))
            {
                var arrayObject = getMethod.Invoke(model, null);

                if (arrayObject != null)
                {
                    foreach (object element in (IEnumerable)arrayObject)
                    {
                        values.Add(element);
                    }
                }
            }
            else if (getMethod != null && getMethod.ReturnType != typeof(string)
                && GetGenericArgumentsOfAssignableType(getMethod.ReturnType, typeof(IReadOnlyDictionary<,>)).Any())
            {
                var dictObject = (IDictionary)getMethod.Invoke(model, null);

                if (dictObject != null)
                {
                    foreach (object key in dictObject.Keys)
                    {
                        values.Add(dictObject[key]);
                    }
                }
            }
            else
            {
                if (property.PropertyType.IsClass && !property.PropertyType.IsValueType
                    && !property.PropertyType.IsPrimitive && property.PropertyType != typeof(string)
                    && property.PropertyType != typeof(object))
                {
                    values.Add(property.GetValue(model));
                }
            }

            return values.ToArray();
        }

        private static IReadOnlyList<Type> GetGenericArgumentsOfAssignableType(Type givenType, Type genericType)
        {
            while (true)
            {
                if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                {
                    return givenType.GenericTypeArguments;
                }

                var interfaceTypes = givenType.GetInterfaces();

                foreach (var it in interfaceTypes)
                {
                    if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    {
                        return it.GetGenericArguments();
                    }
                }

                var baseType = givenType.BaseType;
                if (baseType == null)
                {
                    return Array.Empty<Type>();
                }

                givenType = baseType;
            }
        }
    }
}
