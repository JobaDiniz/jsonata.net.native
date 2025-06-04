using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Jsonata.Net.Native.Syntax;

internal sealed class RegexNode : Node
{
    public Regex regex { get; }

    public RegexNode(Regex regex)
    {
        this.regex = regex;
    }

    //shorthand constructor for manual DOM construction
    public RegexNode(string regexStr)
        : this(new Regex(regexStr, RegexOptions.Compiled))
    {
    }

    internal override Node optimize()
    {
        return this;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append('/');
        builder.Append(regex.ToString());
        builder.Append('/');
        if (this.regex.Options.HasFlag(System.Text.RegularExpressions.RegexOptions.Multiline))
        {
            builder.Append('m');
        }
        if (this.regex.Options.HasFlag(System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            builder.Append('i');
        }
        return builder.ToString();
    }

    public override bool Equals(Node? other)
    {
        return other is RegexNode otherRegex &&
               this.regex.Options == otherRegex.regex.Options &&
               this.regex.ToString() == otherRegex.regex.ToString();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(regex.ToString(), regex.Options);
    }
}
