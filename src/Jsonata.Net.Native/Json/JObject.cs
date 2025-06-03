using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jsonata.Net.Native.Extensions;

namespace Jsonata.Net.Native.Json;

public sealed class JObject : JToken
{
    private readonly Dictionary<string, JToken> properties = new Dictionary<string, JToken>();

    public int Count => this.properties.Count;

    public IReadOnlyDictionary<string, JToken> Properties => this.properties;
    public ICollection<string> Keys => this.properties.Keys;

    public JObject()
        : base(JTokenType.Object)
    {
    }

    public void Add(string name, JToken value)
    {
        this.properties.Add(name, value);
    }

    public void Set(string key, JToken value)
    {
        this.properties[key] = value;
    }

    public void Merge(JObject update)
    {
        foreach (KeyValuePair<string, JToken> prop in update.Properties)
        {
            this.properties[prop.Key] = prop.Value;
        }
    }

    public void Remove(string key)
    {
        this.properties.Remove(key);
    }

    protected override void ClearParentNested()
    {
        foreach (JToken child in this.properties.Values)
        {
            child.ClearParent();
        }
    }

    internal override void ToIndentedStringImpl(StringBuilder builder, int indent, SerializationSettings options)
    {
        builder.Append('{');
        bool serializedSomething = false;
        foreach (KeyValuePair<string, JToken> prop in this.properties)
        {
            if (!options.SerializeNullProperties && prop.Value.Type == JTokenType.Null)
            {
                //skip null properties
                continue;
            }

            if (serializedSomething)
            {
                builder.Append(',');
            }
            builder.AppendJsonLine();

            builder.Indent(indent + 1);

            builder.Append('"');
            JToken.EscapeString(prop.Key, builder);
            builder.Append('"').Append(':').Append(' ');
            prop.Value.ToIndentedStringImpl(builder, indent + 1, options);
            serializedSomething = true;
        }

        if (serializedSomething)
        {
            builder.AppendJsonLine();
            builder.Indent(indent);
        }
        builder.Append('}');
    }

    internal override void ToStringFlatImpl(StringBuilder builder, SerializationSettings options)
    {
        builder.Append('{');
        bool serializedSomething = false;
        foreach (KeyValuePair<string, JToken> prop in this.properties)
        {
            if (!options.SerializeNullProperties && prop.Value.Type == JTokenType.Null)
            {
                //skip null properties
                continue;
            }

            if (serializedSomething)
            {
                builder.Append(',');
            }

            builder.Append('"');
            JToken.EscapeString(prop.Key, builder);
            builder.Append('"').Append(':');
            prop.Value.ToStringFlatImpl(builder, options);
            serializedSomething = true;
        }
        builder.Append('}');
    }

    public override JToken DeepClone()
    {
        JObject result = new JObject();
        foreach (KeyValuePair<string, JToken> prop in this.properties)
        {
            result.Add(prop.Key, prop.Value.DeepClone());
        }
        return result;
    }

    public override bool DeepEquals(JToken other)
    {
        if (this.Type != other.Type)
        {
            return false;
        }
        JObject otherObj = (JObject)other;
        if (this.properties.Count != otherObj.properties.Count)
        {
            return false;
        }
        foreach (KeyValuePair<string, JToken> prop in this.properties)
        {
            if (!otherObj.properties.TryGetValue(prop.Key, out JToken? otherValue))
            {
                return false;
            }
            if (otherValue == null)
            {
                return false;
            }
            if (!prop.Value.DeepEquals(otherValue))
            {
                return false;
            }
        }
        return true;
    }
}
