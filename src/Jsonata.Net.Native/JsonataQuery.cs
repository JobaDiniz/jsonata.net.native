using Jsonata.Net.Native.Dom;
using Jsonata.Net.Native.Eval;
using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jsonata.Net.Native;

public sealed class JsonataQuery
{
    private readonly Node node;
    private readonly EvalProcessor evalProcessor;

    public JsonataQuery(string queryText)
        : this(Parser.Parse(queryText))
    {
    }

    internal JsonataQuery(Node node)
    {
        this.node = node.optimize();
        this.evalProcessor = new EvalProcessor();
    }

    public string Eval(string dataJson)
    {
        JToken data = JToken.Parse(dataJson, ParseSettings.DefaultSettings);
        JToken result = this.Eval(data);
        return result.ToIndentedString();
    }

    public JToken Eval(JToken data, JObject? bindings = null)
    {
        EvaluationEnvironment env;
        if (bindings != null)
        {
            env = new EvaluationEnvironment(bindings);
        }
        else
        {
            env = EvaluationEnvironment.DefaultEnvironment;
        };
        return this.evalProcessor.EvaluateJson(this.node, data, env);
    }

    public JToken Eval(JToken data, EvaluationEnvironment environment)
    {
        return this.evalProcessor.EvaluateJson(this.node, data, environment);
    }

    public override string ToString()
    {
        return this.node.ToString()!;
    }

    internal Node GetDom()
    {
        return this.node;
    }
}
