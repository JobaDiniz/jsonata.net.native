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
    // LazyVariable class removed

    public sealed class EvaluationEnvironment
    {
        public static readonly EvaluationEnvironment DefaultEnvironment;

        static EvaluationEnvironment()
        {
            EvaluationEnvironment.DefaultEnvironment = EvaluationEnvironment.CreateDefault();
        }

        internal static EvaluationEnvironment CreateDefault() //main parent, contains default function bindings
        {
            EvaluationEnvironment result = new EvaluationEnvironment(null, null, null); // No parent, no supplement, no lazy provider
            foreach (MethodInfo mi in typeof(BuiltinFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                result.BindFunction(mi);
            }
            return result;
        }

        //used at actual EvalProcessor.EvaluateJson start to inject EvaluationSupplement
        internal static EvaluationEnvironment CreateEvalEnvironment(EvaluationEnvironment parentEnvironment)
        {
            // Inherit lazyVariableProvider from the parent environment
            EvaluationEnvironment result = new EvaluationEnvironment(parentEnvironment, new EvaluationSupplement(), parentEnvironment.m_lazyVariableProvider);
            return result;
        }

        //used during evaluation when nesting
        internal static EvaluationEnvironment CreateNestedEnvironment(EvaluationEnvironment parent)
        {
            // Inherit lazyVariableProvider from the parent environment
            EvaluationEnvironment result = new EvaluationEnvironment(parent, parent.m_evaluationSupplement, parent.m_lazyVariableProvider);
            return result;
        }

        private readonly Dictionary<string, JToken> m_bindings = new Dictionary<string, JToken>(); // Type changed back
        private readonly EvaluationEnvironment? m_parent;
        private readonly EvaluationSupplement? m_evaluationSupplement;
        private readonly Func<string, JToken>? m_lazyVariableProvider;

        private EvaluationEnvironment(EvaluationEnvironment? parent, EvaluationSupplement? evaluationSupplement, Func<string, JToken>? lazyVariableProvider = null)
        {
            this.m_parent = parent;
            this.m_evaluationSupplement = evaluationSupplement;
            this.m_lazyVariableProvider = lazyVariableProvider;
        }

        //public version to provide for JsonataQuery.Eval()
        public EvaluationEnvironment(Func<string, JToken>? lazyVariableProvider = null)
            : this(EvaluationEnvironment.DefaultEnvironment, null, lazyVariableProvider)
        {

        }

        public EvaluationEnvironment(JObject bindings, Func<string, JToken>? lazyVariableProvider = null)
            : this(lazyVariableProvider)

        {
            // It's important that the base constructor (this(lazyVariableProvider)) is called first,
            // which sets up the DefaultEnvironment as parent and the provider.
            // Then, process the explicit bindings.
            foreach (KeyValuePair<string, JToken> property in bindings.Properties)
            {
                this.BindValue(property.Key, property.Value);
            }
        }


        public void BindValue(string name, JToken value)
        {
            this.m_bindings[name] = value;  //allow overrides
        }

        // BindLazyValue method removed

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
            // Try to get the value from the local bindings dictionary
            if (this.m_bindings.TryGetValue(name, out JToken? resultToken))
            {
                return resultToken;
            }

            // If not in local bindings, try the lazy variable provider
            if (this.m_lazyVariableProvider != null)
            {
                JToken? providerResult = this.m_lazyVariableProvider(name);
                // Check if the provider returned a valid, non-undefined token.
                // A provider returning EvalProcessor.UNDEFINED means it didn't handle this variable.
                if (providerResult != null && providerResult != EvalProcessor.UNDEFINED)
                {
                    return providerResult;
                }
            }

            // If not found locally or by provider, try the parent environment
            if (this.m_parent != null)
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
