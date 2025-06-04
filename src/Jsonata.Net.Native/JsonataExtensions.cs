using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Jsonata.Net.Native.Json;

namespace Jsonata.Net.Native;

public static class JsonataExtensions
{
    /// <summary>
    /// Converts a JsonDocument to a JToken.
    /// </summary>
    /// <param name="document">The JsonDocument to convert.</param>
    /// <returns>A JToken representing the JSON document.</returns>
    public static JToken ToJToken(this JsonDocument document)
    {
        return document.RootElement.ToJToken();
    }

    /// <summary>
    /// Converts a JsonElement to a JToken.
    /// </summary>
    /// <param name="element">The JsonElement to convert.</param>
    /// <returns>A JToken representing the JSON element.</returns>
    public static JToken ToJToken(this JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                {
                    JArray result = new JArray(element.GetArrayLength());
                    foreach (JsonElement child in element.EnumerateArray())
                    {
                        result.Add(child.ToJToken());
                    }
                    return result;
                }
            case JsonValueKind.True:
                return new JValue(true);
            case JsonValueKind.False:
                return new JValue(false);
            case JsonValueKind.Number:
                {
                    if (element.TryGetInt32(out int intValue))
                    {
                        return new JValue(intValue);
                    }
                    else if (element.TryGetInt64(out long longValue))
                    {
                        return new JValue(longValue);
                    }
                    else if (element.TryGetDecimal(out decimal decimalValue))
                    {
                        return new JValue(decimalValue);
                    }
                    else if (element.TryGetDouble(out double doubleValue))
                    {
                        return new JValue(doubleValue);
                    }
                    else
                    {
                        throw new Exception("Failed to parse number from " + element);
                    }
                }
            case JsonValueKind.Null:
                return JValue.CreateNull();
            case JsonValueKind.Object:
                {
                    JObject result = new JObject();
                    foreach (JsonProperty prop in element.EnumerateObject())
                    {
                        result.Add(prop.Name, prop.Value.ToJToken());
                    }
                    return result;
                }
            case JsonValueKind.String:
                return new JValue(element.GetString()!);
            case JsonValueKind.Undefined:
                return JValue.CreateUndefined();
            default:
                throw new ArgumentException("JsonValueKind " + element.ValueKind);
        }
    }

    /// <summary>
    /// Converts a JsonNode to a JToken.
    /// </summary>
    /// <param name="node">The JsonNode to convert.</param>
    /// <returns>A JToken representing the JSON node.</returns>
    public static JToken ToJToken(this JsonNode? node)
    {
        //not using node.GetValueKind() because of totally wretched implementation: https://github.com/dotnet/runtime/blob/eeadd653e1982d7037a93a9ab38129c07336e7db/src/libraries/System.Text.Json/src/System/Text/Json/Nodes/JsonValueOfT.cs#L68

        if (node == null)
        {
            return JValue.CreateNull();
        }
        else if (node is JsonArray array)
        {
            JArray result = new JArray(array.Count);
            for (int i = 0; i < array.Count; ++i)
            {
                JsonNode? child = array[i];
                result.Add(child.ToJToken());
            }
            return result;
        }
        else if (node is JsonObject obj)
        {
            JObject result = new JObject();
            foreach (KeyValuePair<string, JsonNode?> prop in obj)
            {
                result.Add(prop.Key, prop.Value.ToJToken());
            }
            return result;
        }
        else if (node is JsonValue value)
        {
            if (value.TryGetValue(out bool boolValue))
            {
                return new JValue(boolValue);
            }
            else if (value.TryGetValue(out int intValue))
            {
                return new JValue(intValue);
            }
            else if (value.TryGetValue(out long longValue))
            {
                return new JValue(longValue);
            }
            else if (value.TryGetValue(out decimal decimalValue))
            {
                return new JValue(decimalValue);
            }
            else if (value.TryGetValue(out double doubleValue))
            {
                return new JValue(doubleValue);
            }
            else if (value.TryGetValue(out string? strValue))
            {
                return new JValue(strValue);
            }
            else
            {
                throw new ArgumentException($"Value {node} is something strange: {node.GetValueKind()}");
            }
        }
        else
        {
            throw new ArgumentException($"Node {node} is something strange: {node.GetValueKind()}");
        }
    }

    /// <summary>
    /// Converts a JToken to a JsonDocument.
    /// </summary>
    /// <param name="value">The JToken to convert.</param>
    /// <returns>A JsonDocument representing the JToken.</returns>
    public static JsonDocument ToSystemTextJson(this JToken value)
    {
        return JsonDocument.Parse(value.ToFlatString());
    }

    /// <summary>
    /// Converts a JToken to a JsonNode.
    /// </summary>
    /// <param name="value">The JToken to convert.</param>
    /// <returns>A JsonNode representing the JToken, or null for null values.</returns>
    public static JsonNode? ToJsonNode(this JToken value)
    {
        switch (value.Type)
        {
            case JTokenType.Array:
                {
                    JArray source = (JArray)value;
                    JsonArray result = new JsonArray();
                    foreach (JToken child in source.ChildrenTokens)
                    {
                        result.Add(child.ToJsonNode());
                    }
                    return result;
                }
            case JTokenType.Object:
                {
                    JObject source = (JObject)value;
                    JsonObject result = new JsonObject();
                    foreach (KeyValuePair<string, JToken> prop in source.Properties)
                    {
                        result.Add(prop.Key, prop.Value.ToJsonNode());
                    }
                    return result;
                }
            case JTokenType.Function:
                throw new NotSupportedException("Not supported for functions");
            case JTokenType.Null:
                return null;    //seems there's no JsonValue for Null: https://github.com/dotnet/runtime/blob/eeadd653e1982d7037a93a9ab38129c07336e7db/src/libraries/System.Text.Json/src/System/Text/Json/Nodes/JsonValue.cs#L67
            case JTokenType.Undefined:
                return JsonValue.Create(new JsonElement()); //this would create a node with JsonValueKind.Undefined, see https://github.com/mikhail-barg/jsonata.net.native/issues/38#issuecomment-2813936416 
            case JTokenType.Float:
                try
                {
                    decimal decimalValue = (decimal)value;
                    return JsonValue.Create(decimalValue);
                }
                catch (OverflowException)
                {
                    //throw new JsonataException("S0102", $"Number out of range: {value} ({ex.Message})");
                    return JsonValue.Create((double)value);
                }
            case JTokenType.Integer:
                return JsonValue.Create((long)value);
            case JTokenType.String:
                return JsonValue.Create((string)value);
            case JTokenType.Boolean:
                return JsonValue.Create((bool)value);
            default:
                throw new Exception("Unexpected type " + value.Type);
        }
    }
}