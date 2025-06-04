using Jsonata.Net.Native.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jsonata.Net.Native;

public sealed class EvaluationEnvironment
{

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
    /// Creates a standard evaluation environment with all built-in JSONata functions.
    /// This is the entry point for most JSONata usage.
    /// </summary>
    public static EvaluationEnvironment CreateStandard()
    {
        return new EvaluationEnvironment();
    }

    /// <summary>
    /// Creates a child environment for query execution with its own execution state.
    /// Used internally when starting a new JSONata query evaluation.
    /// </summary>
    internal EvaluationEnvironment CreateChildForQueryExecution()
    {
        return new EvaluationEnvironment(this, new QueryExecutionState());
    }

    /// <summary>
    /// Creates a child environment for lambda function execution with parameter bindings.
    /// Used internally when evaluating lambda functions.
    /// </summary>
    internal EvaluationEnvironment CreateChildForLambda(IEnumerable<(string name, JToken value)> parameterBindings)
    {
        EvaluationEnvironment child = new EvaluationEnvironment(this, this.queryExecutionState);
        foreach ((string name, JToken value) in parameterBindings)
        {
            child.BindValue(name, value);
        }
        return child;
    }

    /// <summary>
    /// Creates a child environment for block scope evaluation.
    /// Used internally for block expressions that need variable scoping.
    /// </summary>
    internal EvaluationEnvironment CreateChildForBlock()
    {
        return new EvaluationEnvironment(this, this.queryExecutionState);
    }

    /// <summary>
    /// Creates a child environment with additional user-defined variable bindings.
    /// This allows users to provide custom variables for query evaluation.
    /// </summary>
    public EvaluationEnvironment CreateChildWithUserBindings(JObject bindings)
    {
        EvaluationEnvironment child = new EvaluationEnvironment(this, this.queryExecutionState);
        foreach (KeyValuePair<string, JToken> property in bindings.Properties)
        {
            child.BindValue(property.Key, property.Value);
        }
        return child;
    }

    /// <summary>
    /// Creates an evaluation environment with query execution state for a new query evaluation.
    /// </summary>
    [Obsolete("Use CreateChildForQueryExecution() instead")]
    internal static EvaluationEnvironment CreateWithExecutionState(EvaluationEnvironment parentEnvironment)
    {
        return parentEnvironment.CreateChildForQueryExecution();
    }

    /// <summary>
    /// Creates a nested evaluation environment that shares the parent's execution state.
    /// Used for lambda functions and scoped expressions.
    /// </summary>
    [Obsolete("Use CreateChildForLambda() or CreateChildForBlock() instead")]
    internal static EvaluationEnvironment CreateNested(EvaluationEnvironment parent)
    {
        return parent.CreateChildForBlock();
    }

    private readonly Dictionary<string, JToken> bindings = new Dictionary<string, JToken>();
    private readonly EvaluationEnvironment? parent;
    private readonly QueryExecutionState? queryExecutionState;

    private EvaluationEnvironment(EvaluationEnvironment? parent, QueryExecutionState? queryExecutionState)
    {
        this.parent = parent;
        this.queryExecutionState = queryExecutionState;
    }

    /// <summary>
    /// Creates a standard evaluation environment with all built-in JSONata functions.
    /// Equivalent to calling EvaluationEnvironment.CreateStandard().
    /// </summary>
    public EvaluationEnvironment()
        : this(null, null)
    {
        // Register all built-in functions directly in this instance
        RegisterFunctionsFromType(this, typeof(StringFunctions));
        RegisterFunctionsFromType(this, typeof(NumericFunctions));
        RegisterFunctionsFromType(this, typeof(ArrayFunctions));
        RegisterFunctionsFromType(this, typeof(BooleanFunctions));
        RegisterFunctionsFromType(this, typeof(ObjectFunctions));
        RegisterFunctionsFromType(this, typeof(DateTimeFunctions));
        RegisterFunctionsFromType(this, typeof(HigherOrderFunctions));
    }

    /// <summary>
    /// Creates a standard evaluation environment with additional user bindings.
    /// </summary>
    [Obsolete("Use EvaluationEnvironment.CreateStandard().CreateChildWithUserBindings(bindings) instead")]
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
