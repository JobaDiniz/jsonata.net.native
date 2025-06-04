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
    private readonly JsonataEvaluator jsonataEvaluator;
    private readonly EvaluationEnvironment environment;

    public JsonataQuery(string queryText)
        : this(queryText, EvaluationEnvironment.CreateStandard())
    {
    }

    public JsonataQuery(string queryText, EvaluationEnvironment environment)
        : this(Parser.Parse(queryText), environment)
    {
    }

    internal JsonataQuery(Node node)
        : this(node, EvaluationEnvironment.CreateStandard())
    {
    }

    internal JsonataQuery(Node node, EvaluationEnvironment environment)
    {
        this.node = node.optimize();
        this.environment = environment;
        this.jsonataEvaluator = new JsonataEvaluator();
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
            env = this.environment.CreateChildWithUserBindings(bindings);
        }
        else
        {
            env = this.environment;
        };
        return this.jsonataEvaluator.ExecuteQuery(this.node, data, env);
    }

    public JToken Eval(JToken data, EvaluationEnvironment environment)
    {
        return this.jsonataEvaluator.ExecuteQuery(this.node, data, environment);
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
