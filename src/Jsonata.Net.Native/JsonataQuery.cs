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
            JToken result = this.Eval(data);
            return result.ToIndentedString();
        }

        public JToken Eval(JToken data, JObject? bindings = null)
        {
            EvaluationEnvironment env;
            if (bindings != null)
            {
                // Check if we're using the default environment or creating a new one for bindings
                EvaluationEnvironment baseEnv = EvaluationEnvironment.DefaultEnvironment;
                env = new EvaluationEnvironment(baseEnv, null); // Create a new environment that inherits from DefaultEnvironment

                foreach (KeyValuePair<string, JToken> property in bindings.Properties)
                {
                    if (property.Value is JValue jValue && jValue.Type == JTokenType.String)
                    {
                        string stringValue = (string)jValue.Value!;
                        if (stringValue.StartsWith("LAZY:"))
                        {
                            string valueToParse = stringValue.Substring("LAZY:".Length);
                            env.BindLazyValue(property.Key, () => JToken.Parse(valueToParse, ParseSettings.DefaultSettings));
                        }
                        else
                        {
                            env.BindValue(property.Key, property.Value);
                        }
                    }
                    else
                    {
                        env.BindValue(property.Key, property.Value);
                    }
                }
            }
            else
            {
                env = EvaluationEnvironment.DefaultEnvironment;
            };
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
