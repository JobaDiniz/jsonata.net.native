using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jsonata.Net.Native.Syntax;

/// <summary>
/// Represents a function definition expression in a JSONata query.
/// Contains parameter signatures and function body for user-defined functions.
/// </summary>
internal sealed class LambdaNode : Node
{
    public bool isShorthand { get; }
    public IReadOnlyList<string> paramNames { get; }
    public Signature? signature { get; }
    public Node body { get; }

    public LambdaNode(bool isShorthand, IReadOnlyList<string> paramNames, Signature? signature, Node body)
    {
        this.isShorthand = isShorthand;
        this.paramNames = paramNames;
        this.signature = signature;
        this.body = body;
    }

    //shorthand constructor for manual DOM construction
    public LambdaNode(IReadOnlyList<string> paramNames, Node body)
        : this(
            isShorthand: false,
            paramNames: paramNames,
            signature: null,
            body: body
        )
    {
    }

    internal override Node optimize()
    {
        Node body = this.body.optimize();
        if (body != this.body)
        {
            return new LambdaNode(this.isShorthand, this.paramNames, this.signature, body);
        }
        else
        {
            return this;
        }
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(this.isShorthand ? "λ" : "function");
        builder.Append('(');
        for (int i = 0; i < this.paramNames.Count; ++i)
        {
            if (i != 0)
            {
                builder.Append(",");
            };
            builder.Append('$');
            builder.Append(this.paramNames[i]);
        }
        builder.Append(')');
        if (this.signature != null)
        {
            this.signature.ToString(builder);
        };
        builder.Append('{');
        builder.Append(this.body.ToString());
        builder.Append('}');
        return builder.ToString();
    }

    public override bool Equals(Node? other)
    {
        return other is LambdaNode otherLambda &&
               this.isShorthand == otherLambda.isShorthand &&
               this.paramNames.SequenceEqual(otherLambda.paramNames) &&
               // && this.signature.Equals(otherLambda.signature) //TODO: implement Signature equals
               this.body.Equals(otherLambda.body);
    }

    public override int GetHashCode()
    {
        var paramNamesHash = paramNames.Aggregate(0, (hash, name) => HashCode.Combine(hash, name));
        return HashCode.Combine(isShorthand, paramNamesHash, body);
    }

    internal enum ParamOpt
    {
        None,

        // ParamOptional denotes an optional parameter.
        Optional,

        // ParamVariadic denotes a variadic parameter.
        Variadic,

        // ParamContextable denotes a parameter that can be
        // replaced by the evaluation context if no value is
        // provided by the caller.
        Contextable
    };

    [Flags]
    internal enum ParamType
    {
        Bool = 0x01,
        Number = 0x02,
        String = 0x04,
        Null = 0x08,

        Array = 0x10,
        Object = 0x20,

        Func = 0x40,

        Simple = Bool | Number | String | Null,
        Json = Simple | Array | Object,
        Any = Json | Func,
        None = 0x0
    }

    internal static readonly Tuple<ParamType, string>[] s_paramTypePriorityLetters = {
            Tuple.Create(ParamType.Any, "x"),
            Tuple.Create(ParamType.Json, "j"),
            Tuple.Create(ParamType.Simple, "u"),

            Tuple.Create(ParamType.Bool, "b"),
            Tuple.Create(ParamType.Number, "n"),
            Tuple.Create(ParamType.String, "s"),
            Tuple.Create(ParamType.Null, "l"),

            Tuple.Create(ParamType.Array, "a"),
            Tuple.Create(ParamType.Object, "o"),

            Tuple.Create(ParamType.Func, "f"),
        };

    internal sealed class Param
    {
        public ParamType type { get; }
        public ParamOpt option { get; }
        public Signature? subSignature { get; }

        public Param(ParamType type, ParamOpt option, Signature? subSignature)
        {
            this.type = type;
            this.option = option;
            this.subSignature = subSignature;
        }

        public static string ParamOptToString(ParamOpt opt)
        {
            switch (opt)
            {
                case ParamOpt.None:
                    return "";
                case ParamOpt.Optional:
                    return "?";
                case ParamOpt.Variadic:
                    return "+";
                case ParamOpt.Contextable:
                    return "-";
                default:
                    throw new Exception("Unexpected param opt " + opt);
            }
        }

        public static string ParamTypeToString(ParamType type)
        {
            foreach (Tuple<ParamType, string> t in s_paramTypePriorityLetters)
            {
                if (type == t.Item1)
                {
                    return t.Item2;
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            foreach (Tuple<ParamType, string> t in s_paramTypePriorityLetters)
            {
                if ((type & t.Item1) == t.Item1)
                {
                    builder.Append(t.Item2);
                    type = type & ~t.Item1;
                }
            }
            builder.Append(')');
            return builder.ToString();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.ToString(builder);
            return builder.ToString();
        }

        internal void ToString(StringBuilder builder)
        {
            builder.Append(ParamTypeToString(this.type));
            if (this.subSignature != null)
            {
                this.subSignature.ToString(builder);
            };
            builder.Append(ParamOptToString(this.option));
        }
    }

    internal sealed class Signature
    {
        public IReadOnlyList<Param> args { get; }
        public Param? result { get; }

        public Signature(IReadOnlyList<Param> args, Param? result)
        {
            this.args = args;
            this.result = result;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.ToString(builder);
            return builder.ToString();
        }

        internal void ToString(StringBuilder builder)
        {
            builder.Append('<');
            foreach (Param arg in args)
            {
                arg.ToString(builder);
            };
            if (result != null)
            {
                builder.Append(':');
                result.ToString(builder);
            };
            builder.Append('>');
        }
    }
}
