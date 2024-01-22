using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.SettingsReader.ConfigurationProvider;

internal class JsonParser
{
     private readonly Dictionary<string, string?> _data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _paths = new Stack<string>();

        public static IDictionary<string, string?> Parse(JsonDocument jsonDocument)
            => new JsonParser().ParseStream(jsonDocument);

        private Dictionary<string, string?> ParseStream(JsonDocument doc)
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException($"Invalid TopLevel JSON Element {doc.RootElement.ValueKind}");
            }
            VisitObjectElement(doc.RootElement);

            return _data;
        }

        private void VisitObjectElement(JsonElement element)
        {
            var isEmpty = true;

            foreach (JsonProperty property in element.EnumerateObject())
            {
                isEmpty = false;
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }

            SetNullIfElementIsEmpty(isEmpty);
        }

        private void VisitArrayElement(JsonElement element)
        {
            int index = 0;

            foreach (JsonElement arrayElement in element.EnumerateArray())
            {
                EnterContext(index.ToString());
                VisitValue(arrayElement);
                ExitContext();
                index++;
            }

            SetNullIfElementIsEmpty(isEmpty: index == 0);
        }

        private void SetNullIfElementIsEmpty(bool isEmpty)
        {
            if (isEmpty && _paths.Count > 0)
            {
                _data[_paths.Peek()] = null;
            }
        }

        private void VisitValue(JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    VisitObjectElement(value);
                    break;

                case JsonValueKind.Array:
                    VisitArrayElement(value);
                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    string key = _paths.Peek();
                    if (_data.ContainsKey(key))
                    {
                        throw new FormatException($"Duplicated key: {key}");
                    }
                    _data[key] = value.ToString();
                    break;

                default:
                    throw new FormatException($"Invalid JSON ValueKind: {value.ValueKind}");
            }
        }

        private void EnterContext(string context) =>
            _paths.Push(_paths.Count > 0 ?
                _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
                context);

        private void ExitContext() => _paths.Pop();
}