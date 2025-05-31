using Jsonata.Net.Native.Eval;
using Jsonata.Net.Native.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jsonata.Net.Native
{
    internal class LazyVariable
    {
        private Func<JToken> m_valueProvider;
        private JToken? m_value;
        private bool m_evaluated;

        public LazyVariable(Func<JToken> valueProvider)
        {
            this.m_valueProvider = valueProvider;
            this.m_evaluated = false;
            this.m_value = null;
        }

        public JToken GetValue()
        {
            if (!this.m_evaluated)
            {
                this.m_value = this.m_valueProvider();
                this.m_evaluated = true;
            }
            return this.m_value!;
        }
    }

    public sealed class EvaluationEnvironment
    {
        public static readonly EvaluationEnvironment DefaultEnvironment;

        static EvaluationEnvironment()
        {
            EvaluationEnvironment.DefaultEnvironment = EvaluationEnvironment.CreateDefault();
        }

        internal static EvaluationEnvironment CreateDefault() //main parent, contains default function bindings
        {
            EvaluationEnvironment result = new EvaluationEnvironment(null, null);
            foreach (MethodInfo mi in typeof(BuiltinFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                result.BindFunction(mi);
            }
            return result;
        }

        //used at actual EvalProcessor.EvaluateJson start to inject EvaluationSupplement
        internal static EvaluationEnvironment CreateEvalEnvironment(EvaluationEnvironment parentEnvironment)
        {
            EvaluationEnvironment result = new EvaluationEnvironment(parentEnvironment, new EvaluationSupplement());
            return result;
        }

        //used during evaluation when nesting
        internal static EvaluationEnvironment CreateNestedEnvironment(EvaluationEnvironment parent)
        {
            EvaluationEnvironment result = new EvaluationEnvironment(parent, parent.m_evaluationSupplement);
            return result;
        }

        private readonly Dictionary<string, object> m_bindings = new Dictionary<string, object>();
        private readonly EvaluationEnvironment? m_parent;
        private readonly EvaluationSupplement? m_evaluationSupplement;

        private EvaluationEnvironment(EvaluationEnvironment? parent, EvaluationSupplement? evaluationSupplement)
        {
            this.m_parent = parent;
            this.m_evaluationSupplement = evaluationSupplement;
        }

        //public version to provide for JsonataQuery.Eval()
        public EvaluationEnvironment()
            : this(EvaluationEnvironment.DefaultEnvironment, null)
        {

        }

        public EvaluationEnvironment(JObject bindings)
            : this()
        {
            foreach (KeyValuePair<string, JToken> property in bindings.Properties)
            {
                this.BindValue(property.Key, property.Value);
            }
        }


        public void BindValue(string name, JToken value)
        {
            this.m_bindings[name] = value;  //allow overrides
        }

        public void BindLazyValue(string name, Func<JToken> valueProvider)
        {
            this.m_bindings[name] = new LazyVariable(valueProvider); //allow overrides
        }

        public void BindFunction(MethodInfo mi)
        {
            this.BindFunction(mi.Name, mi);
        }

        public void BindFunction(string name, MethodInfo mi)
        {
            this.m_bindings.Add(name, new FunctionTokenCsharp(name, mi));
        }

        public void BindFunction(string name, Delegate funcDelegate)
        {
            this.m_bindings.Add(name, new FunctionTokenCsharp(name, funcDelegate));
        }

        internal JToken Lookup(string name)
        {
            if (this.m_bindings.TryGetValue(name, out object? resultObj))
            {
                if (resultObj is JToken token)
                {
                    return token;
                }
                else if (resultObj is LazyVariable lazyVariable)
                {
                    JToken value = lazyVariable.GetValue();
                    // Cache the value for future lookups by replacing the LazyVariable
                    // instance with the actual JToken. This ensures that the Func<JToken>
                    // is only evaluated once.
                    this.m_bindings[name] = value;
                    return value;
                }
            }
            else if (this.m_parent != null)
            {
                return this.m_parent.Lookup(name);
            }

            return EvalProcessor.UNDEFINED;
        }

        internal EvaluationSupplement GetEvaluationSupplement()
        {
            if (this.m_evaluationSupplement == null)
            {
                throw new Exception($"Calling {nameof(GetEvaluationSupplement)}() at non-evaluation env. Should not happen");
            };
            return this.m_evaluationSupplement;
        }
    }
}
