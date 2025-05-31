using Jsonata.Net.Native.Dom;
using Jsonata.Net.Native.Eval;
using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jsonata.Net.Native
{
    public sealed class JsonataQuery
    {
        private readonly Node m_node;

        public JsonataQuery(string queryText)
            : this(Parser.Parse(queryText))
        {
        }

        public JsonataQuery(Node node)
        {
            this.m_node = node.optimize();
        }

        public string Eval(string dataJson)
        {
            JToken data = JToken.Parse(dataJson, ParseSettings.DefaultSettings);
            // Call the new primary Eval overload, passing null for bindings and lazyVariableProvider
            JToken result = this.Eval(data, null, null);
            return result.ToIndentedString();
        }

        // This overload now delegates to the new primary overload with lazyVariableProvider = null
        public JToken Eval(JToken data, JObject? bindings = null)
        {
            return this.Eval(data, bindings, null);
        }

        // New primary Eval method with lazyVariableProvider
        public JToken Eval(JToken data, JObject? bindings = null, Func<string, JToken>? lazyVariableProvider = null)
        {
            EvaluationEnvironment env;
            if (bindings != null)
            {
                // If bindings are provided, use them and the lazyVariableProvider.
                // The EvaluationEnvironment constructor (new EvaluationEnvironment(JObject, Func<string,JToken>?)
                // handles if lazyVariableProvider is null.
                env = new EvaluationEnvironment(bindings, lazyVariableProvider);
            }
            else if (lazyVariableProvider != null)
            {
                // If only a lazyVariableProvider is provided, use it.
                // This constructor (new EvaluationEnvironment(Func<string,JToken>?)
                // ensures it parents from DefaultEnvironment.
                env = new EvaluationEnvironment(lazyVariableProvider);
            }
            else
            {
                // If neither bindings nor a provider is given, use the DefaultEnvironment
                env = EvaluationEnvironment.DefaultEnvironment;
            }
            return EvalProcessor.EvaluateJson(this.m_node, data, env);
        }

        public JToken Eval(JToken data, EvaluationEnvironment environment)
        {
            return EvalProcessor.EvaluateJson(this.m_node, data, environment);
        }

        public override string ToString()
        {
            return this.m_node.ToString()!;
        }

        public Node GetDom() {
            return this.m_node;
        }
    }
}
