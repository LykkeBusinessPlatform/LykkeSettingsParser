using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Lykke.SettingsReader.Attributes;
using Lykke.SettingsReader.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.SettingsReader
{
    public static partial class SettingsProcessor
    {
        private static object Convert_FromObject(JObject jsonObject, Type targetType, string path)
        {
            var targetInfo = targetType.GetTypeInfo();

            if (CanImplementWith(typeof(Dictionary<,>), targetType))
                return Convert_FromObjectToDictionary(jsonObject, targetInfo.GenericTypeArguments[0], targetInfo.GenericTypeArguments[1], path);

            if (targetInfo.IsInterface)
                throw new PropertyTypeNotSupportedException(path, targetType);

            try
            {
                return Convert_FromObjectToObject(jsonObject, targetType, path);
            }
            catch (SettingsReaderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PropertyTypeNotSupportedException(path, targetType, ex);
            }
        }

        public static object Convert_FromObjectToObject(JObject jsonObject, Type targetType, string path)
        {
            var result = Activator.CreateInstance(targetType);
            foreach (var property in (from p in targetType.GetTypeInfo().GetProperties()
                                      where p.CanWrite && p.CanRead
                                      select p).ToList())
            {
                var propertyPath = ConcatPath(path, property.Name);
                var jProp = jsonObject.Properties().FirstOrDefault(jp => jp.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));

                if (jProp == null)
                {
                    var jsonProperty = property.GetCustomAttributes(typeof(JsonPropertyAttribute), false);
                    if (jsonProperty.Any())
                    {
                        var jsonPropertyName = ((JsonPropertyAttribute)jsonProperty.First()).PropertyName;
                        jProp = jsonObject.Properties().FirstOrDefault(jp => jp.Name.Equals(jsonPropertyName, StringComparison.OrdinalIgnoreCase));
                    }
                }
                
                if (jProp == null)
                {
                    if (property.GetCustomAttributes(typeof(OptionalAttribute), false).Any())
                        continue;

                    throw new RequiredFieldEmptyException(propertyPath);
                }

                var jResult = Convert(jProp, property.PropertyType, propertyPath);
                property.SetValue(result, jResult);
            }

            return result;
        }

        private static bool CanImplementWith(Type implementingType, Type targetType)
        {
            var implementingInfo = implementingType.GetTypeInfo();
            if (implementingInfo.IsInterface || implementingInfo.IsAbstract)
                return false;

            var targetInfo = targetType.GetTypeInfo();

            if (targetInfo.IsGenericType && implementingInfo.IsGenericTypeDefinition)
            {
                // TODO: analyze implementation generic parameter mapping to make more accurate decision. 
                try
                {
                    var genericArguments = targetInfo.IsGenericType ? targetInfo.GetGenericArguments() : null;
                    implementingType = implementingType.MakeGenericType(genericArguments);
                    implementingInfo = implementingType.GetTypeInfo();
                }
                catch // TODO: Expensive operation, consider some analysis instead
                {
                    return false;
                }
            }

            if (targetInfo.IsInterface)
                return implementingInfo.GetInterfaces().Contains(targetType);

            while (null != implementingType)
            {
                if (implementingType == targetType)
                    return true;
                implementingType = implementingInfo.BaseType;
                implementingInfo = implementingType?.GetTypeInfo();
            }
            return false;
        }

        private static object Convert_FromObjectToDictionary(JObject jsonObject, Type keyType, Type valueType, string path)
        {
            var resultType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var adder = resultType.GetRuntimeMethod("Add", new[] { keyType, valueType });
            var result = Activator.CreateInstance(resultType);

            foreach (var jsonProperty in jsonObject.Properties())
            {
                var propertyPath = ConcatPath(path, jsonProperty.Name);
                adder.Invoke(result, new[]
                {
                    Convert_FromValue(jsonProperty.Name, keyType, propertyPath),
                    Convert(jsonProperty.Value, valueType, propertyPath)
                });
            }

            return result;
        }
    }
}
