using Jsonata.Net.Native.Eval;
using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Functions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jsonata.Net.Native;

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

        // Register functions from new organized static classes
        RegisterFunctionsFromType(result, typeof(StringFunctions));
        RegisterFunctionsFromType(result, typeof(NumericFunctions));
        RegisterFunctionsFromType(result, typeof(ArrayFunctions));
        RegisterFunctionsFromType(result, typeof(BooleanFunctions));
        RegisterFunctionsFromType(result, typeof(ObjectFunctions));
        RegisterFunctionsFromType(result, typeof(DateTimeFunctions));
        RegisterFunctionsFromType(result, typeof(HigherOrderFunctions));

        return result;
    }

    private static void RegisterFunctionsFromType(EvaluationEnvironment env, Type type)
    {
        foreach (MethodInfo mi in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            // Check if the method has a FunctionNameAttribute
            var functionNameAttr = mi.GetCustomAttribute<FunctionNameAttribute>();
            if (functionNameAttr != null)
            {
                env.BindFunction(functionNameAttr.Name, mi);
            }
            else
            {
                // Fallback to method name if no attribute
                env.BindFunctionInternal(mi.Name, mi);
            }
        }
    }

    /// <summary>
    /// Creates an evaluation environment with query execution state for a new query evaluation.
    /// </summary>
    internal static EvaluationEnvironment CreateWithExecutionState(EvaluationEnvironment parentEnvironment)
    {
        EvaluationEnvironment result = new EvaluationEnvironment(parentEnvironment, new QueryExecutionState());
        return result;
    }

    /// <summary>
    /// Creates a nested evaluation environment that shares the parent's execution state.
    /// Used for lambda functions and scoped expressions.
    /// </summary>
    internal static EvaluationEnvironment CreateNested(EvaluationEnvironment parent)
    {
        EvaluationEnvironment result = new EvaluationEnvironment(parent, parent.queryExecutionState);
        return result;
    }

    private readonly Dictionary<string, JToken> bindings = new Dictionary<string, JToken>();
    private readonly EvaluationEnvironment? parent;
    private readonly QueryExecutionState? queryExecutionState;

    private EvaluationEnvironment(EvaluationEnvironment? parent, QueryExecutionState? queryExecutionState)
    {
        this.parent = parent;
        this.queryExecutionState = queryExecutionState;
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
        this.bindings[name] = value;  //allow overrides
    }


    internal void BindFunction(string name, MethodInfo mi)
    {
        this.BindFunctionInternal(name, mi);
    }

    private void BindFunctionInternal(string name, MethodInfo mi)
    {
        this.bindings.Add(name, new FunctionTokenCsharp(name, mi));
    }

    public void BindFunction(string name, Delegate funcDelegate)
    {
        this.bindings.Add(name, new FunctionTokenCsharp(name, funcDelegate));
    }

    /// <summary>
    /// Binds a static method as a function using the method's name.
    /// </summary>
    /// <param name="methodInfo">The MethodInfo for the static method to bind</param>
    public void BindFunction(MethodInfo methodInfo)
    {
        if (!methodInfo.IsStatic)
        {
            throw new ArgumentException("Only static methods can be bound as functions", nameof(methodInfo));
        }
        this.BindFunctionInternal(methodInfo.Name, methodInfo);
    }

    internal JToken Lookup(string name)
    {
        if (this.bindings.TryGetValue(name, out JToken? result))
        {
            return result;
        }
        else if (this.parent != null)
        {
            return this.parent.Lookup(name);
        }
        else
        {
            return JValue.Undefined;
        }
    }

    internal QueryExecutionState GetQueryExecutionState()
    {
        if (this.queryExecutionState == null)
        {
            throw new Exception($"Calling {nameof(GetQueryExecutionState)}() at non-evaluation env. Should not happen");
        };
        return this.queryExecutionState;
    }
}
