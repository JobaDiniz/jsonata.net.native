using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jsonata.Net.Native.Json;

public class JArray : JToken
{
    private readonly List<JToken> values;

    public IReadOnlyList<JToken> ChildrenTokens => this.values;
    public int Count => this.values.Count;

    public JArray()
        : base(JTokenType.Array)
    {
        values = new List<JToken>();
    }

    public JArray(int capacity)
        : base(JTokenType.Array)
    {
        values = new List<JToken>(capacity);
    }

    public void Add(JToken token)
    {
        this.values.Add(token);
    }

    protected override void ClearParentNested()
    {
        foreach (JToken child in this.values)
        {
            child.ClearParent();
        }
    }

    internal override void ToIndentedStringImpl(StringBuilder builder, int indent, SerializationSettings options)
    {
        if (this.values.Count == 0)
        {
            builder.Append("[]");
            return;
        }

        builder.Append('[').AppendJsonLine();
        for (int i = 0; i < this.values.Count; ++i)
        {
            builder.Indent(indent + 1);
            this.values[i].ToIndentedStringImpl(builder, indent + 1, options);
            if (i < this.values.Count - 1)
            {
                builder.Append(',');
            }
            builder.AppendJsonLine();
        }
        builder.Indent(indent);
        builder.Append(']');
    }

    internal override void ToStringFlatImpl(StringBuilder builder, SerializationSettings options)
    {
        builder.Append('[');
        for (int i = 0; i < this.values.Count; ++i)
        {
            this.values[i].ToStringFlatImpl(builder, options);
            if (i < this.values.Count - 1)
            {
                builder.Append(',');
            }
        }
        builder.Append(']');
    }

    public override JToken DeepClone()
    {
        JArray result = DeepCloneArrayNoChildren();
        foreach (JToken child in this.values)
        {
            result.Add(child.DeepClone());
        }
        return result;
    }

    protected virtual JArray DeepCloneArrayNoChildren()
    {
        return new JArray();
    }

    public override bool DeepEquals(JToken other)
    {
        if (this.Type != other.Type)
        {
            return false;
        }
        JArray otherArray = (JArray)other;
        if (this.values.Count != otherArray.values.Count)
        {
            return false;
        }
        for (int i = 0; i < this.values.Count; ++i)
        {
            if (!this.values[i].DeepEquals(otherArray.values[i]))
            {
                return false;
            }
        }
        return true;
    }
}
