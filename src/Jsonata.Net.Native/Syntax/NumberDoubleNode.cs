using System;

namespace Jsonata.Net.Native.Syntax;

internal sealed class NumberDoubleNode : NumberNode
{
    public double value { get; }

    public NumberDoubleNode(double value)
    {
        this.value = value;
    }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        return this.value.ToString();
    }

    public override int GetIntValue()
    {
        return (int)this.value;
    }

    public override bool Equals(Node? other)
    {
        return other is NumberDoubleNode otherDouble && 
               Math.Abs(this.value - otherDouble.value) <= Double.Epsilon * 2;    //TODO: will fail on NaNs
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}
