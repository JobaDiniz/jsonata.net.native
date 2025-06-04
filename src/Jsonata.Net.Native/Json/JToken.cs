using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Jsonata.Net.Native.Json;

[DebuggerDisplay("{Type}: {ToFlatString()}")]
public abstract class JToken
{
    public readonly JTokenType Type;

    private JToken? parentToken = null;
    internal JToken? parent
    {
        get => this.parentToken;
        set
        {
            if (this == JValue.Undefined && value != null)
            {
                throw new InvalidOperationException($"Attempt to set parent on {nameof(JValue)}.{nameof(JValue.Undefined)}");
            }
            this.parentToken = value;
        }
    }

    protected JToken(JTokenType type)
    {
        this.Type = type;
    }

    private static JValue AsValue(JToken token)
    {
        if (token is JValue value)
        {
            return value;
        }
        throw new Exception("Token is not a JValue");
    }

    public static explicit operator int(JToken value)
    {
        JValue v = AsValue(value);
        if (v.Type != JTokenType.Integer)
        {
            throw new Exception("Cannot convert to int");
        }
        return Convert.ToInt32(v.Value, CultureInfo.InvariantCulture);
    }

    public static explicit operator long(JToken value)
    {
        JValue v = AsValue(value);
        if (v.Type != JTokenType.Integer)
        {
            throw new Exception("Cannot convert to long");
        }
        return Convert.ToInt64(v.Value, CultureInfo.InvariantCulture);
    }

    public static explicit operator decimal(JToken value)
    {
        JValue v = AsValue(value);
        if (v.Type != JTokenType.Float)
        {
            throw new ArgumentException("Can not convert to Decimal");
        }

        return Convert.ToDecimal(v.Value, CultureInfo.InvariantCulture);
    }

    public static explicit operator double(JToken value)
    {
        JValue v = AsValue(value);
        if (v.Type != JTokenType.Float)
        {
            throw new ArgumentException("Can not convert to Double");
        }

        return Convert.ToDouble(v.Value, CultureInfo.InvariantCulture);
    }

    public static explicit operator float(JToken value)
    {
        JValue v = AsValue(value);
        if (v.Type != JTokenType.Float)
        {
            throw new ArgumentException("Can not convert to Double");
        }

        return Convert.ToSingle(v.Value, CultureInfo.InvariantCulture);
    }

    public static explicit operator string(JToken value)
    {
        JValue v = AsValue(value);
        if (v.Type != JTokenType.String)
        {
            throw new ArgumentException("Can not convert to String");
        }

        return Convert.ToString(v.Value, CultureInfo.InvariantCulture)!;
    }

    public static explicit operator bool(JToken value)
    {
        JValue v = AsValue(value);
        if (v.Type != JTokenType.Boolean)
        {
            throw new ArgumentException("Can not convert to Bool");
        }

        return Convert.ToBoolean(v.Value, CultureInfo.InvariantCulture);
    }

    public static JToken Parse(TextReader reader, JsonDocumentOptions? options = null)
    {
        var jsonText = reader.ReadToEnd();
        return ParseFromString(jsonText, options);
    }

    public static JToken Parse(string source, JsonDocumentOptions? options = null)
    {
        return ParseFromString(source, options);
    }

    internal static JToken FromObject(object? sourceObj)
    {
        switch (sourceObj)
        {
            case null:
                return JValue.CreateNull();
            case bool value:
                return new JValue(value);
            case string value:
                return new JValue(value);
            case char value:
                return new JValue(value);
            case int value:
                return new JValue(value);
            case uint value:
                return new JValue(value);
            case long value:
                return new JValue(value);
            case ulong value:
                return new JValue((decimal)value);  //won't fit in (s)long
            case byte value:
                return new JValue(value);
            case sbyte value:
                return new JValue(value);
            case short value:
                return new JValue(value);
            case ushort value:
                return new JValue(value);
            case float value:
                return new JValue(value);
            case double value:
                return new JValue(value);
            case decimal value:
                return new JValue(value);
            case System.Collections.IDictionary dictionary:
                return FromDictionary(dictionary);
            case System.Collections.ICollection list:
                return FromCollection(list);
            default:
                return FromObj(sourceObj);
        }
    }

    private static JToken FromObj(object sourceObj)
    {
        JObject result = new JObject();
        foreach (PropertyInfo pi in sourceObj.GetType().GetProperties())
        {
            result.Add(pi.Name, JToken.FromObject(pi.GetValue(sourceObj)));
        }
        return result;
    }

    private static JToken FromCollection(ICollection list)
    {
        JArray array = new JArray(list.Count);
        foreach (object? item in list)
        {
            array.Add(JToken.FromObject(item));
        }
        return array;
    }

    private static JToken FromDictionary(IDictionary dictionary)
    {
        JObject result = new JObject();
        foreach (DictionaryEntry entry in dictionary)
        {
            result.Add(entry.Key.ToString()!, JToken.FromObject(entry.Value));
        }
        return result;
    }

    internal void ClearParent()
    {
        this.parent = null;
        this.ClearParentNested();
    }

    protected abstract void ClearParentNested();

    public string ToIndentedString()
    {
        return this.ToIndentedString(SerializationSettings.DefaultSettings);
    }

    public string ToIndentedString(SerializationSettings options)
    {
        StringBuilder builder = new StringBuilder();
        this.ToIndentedStringImpl(builder, 0, options);
        return builder.ToString();
    }

    internal abstract void ToIndentedStringImpl(StringBuilder builder, int indent, SerializationSettings options);

    public string ToFlatString()
    {
        return this.ToFlatString(SerializationSettings.DefaultSettings);
    }

    public string ToFlatString(SerializationSettings options)
    {
        StringBuilder builder = new StringBuilder();
        this.ToStringFlatImpl(builder, options);
        return builder.ToString();
    }

    internal abstract void ToStringFlatImpl(StringBuilder builder, SerializationSettings options);

    public abstract JToken DeepClone();

    public static bool DeepEquals(JToken lhs, JToken rhs)
    {
        return lhs.DeepEquals(rhs);
    }

    public abstract bool DeepEquals(JToken other);


    //see https://stackoverflow.com/questions/19176024/how-to-escape-special-characters-in-building-a-json-string
    // https://www.freeformatter.com/json-escape.html
    public static void EscapeString(string source, StringBuilder target)
    {
        foreach (char c in source)
        {
            switch (c)
            {
                case '\b':
                    target.Append(@"\b");
                    break;
                case '\f':
                    target.Append(@"\f");
                    break;
                case '\n':
                    target.Append(@"\n");
                    break;
                case '\r':
                    target.Append(@"\r");
                    break;
                case '\t':
                    target.Append(@"\t");
                    break;
                case '"':
                    target.Append("\\\"");
                    break;
                case '\\':
                    target.Append(@"\\");
                    break;
                default:
                    target.Append(c);
                    break;
            }
        }
    }

    /// <summary>
    /// Creates a JToken from a JsonDocument.
    /// </summary>
    /// <param name="document">The JsonDocument to convert.</param>
    /// <returns>A JToken representing the JSON document.</returns>
    public static JToken FromJsonDocument(JsonDocument document)
    {
        return FromJsonElement(document.RootElement);
    }

    /// <summary>
    /// Creates a JToken from a JsonElement.
    /// </summary>
    /// <param name="element">The JsonElement to convert.</param>
    /// <returns>A JToken representing the JSON element.</returns>
    public static JToken FromJsonElement(JsonElement element)
    {
        return FromJsonElementCore(element);
    }

    private static JToken ParseFromString(string jsonText, JsonDocumentOptions? options)
    {
        var documentOptions = options ?? new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        };

        using var doc = JsonDocument.Parse(jsonText, documentOptions);
        return FromJsonElement(doc.RootElement);
    }

    private static JToken FromJsonElementCore(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                {
                    JArray result = new JArray(element.GetArrayLength());
                    foreach (JsonElement child in element.EnumerateArray())
                    {
                        result.Add(FromJsonElementCore(child));
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
                        result.Add(prop.Name, FromJsonElementCore(prop.Value));
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
}
