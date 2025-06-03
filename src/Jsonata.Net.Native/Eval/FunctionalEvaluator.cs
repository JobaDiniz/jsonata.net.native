using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Dom;
using Jsonata.Net.Native.Extensions;
using System;
using System.Collections.Generic;

namespace Jsonata.Net.Native.Eval;

/// <summary>
/// Evaluates functional nodes including variables, lambdas, function calls, partial application, and object transformations.
/// Handles function definition, invocation, currying, and higher-order function operations in JSONata expressions.
/// </summary>
internal sealed class FunctionalEvaluator
{
    private readonly NodeEvaluator evaluateNode;

    internal FunctionalEvaluator(NodeEvaluator evaluateNode)
    {
        this.evaluateNode = evaluateNode;
    }

    internal JToken EvalVariable(VariableNode variableNode, JToken input, EvaluationEnvironment env)
    {
        if (variableNode.name == "")
        {
            return input;
        }
        ;
        return env.Lookup(variableNode.name);
    }

    internal JToken EvalLambda(LambdaNode lambdaNode, JToken input, EvaluationEnvironment env)
    {
        return new FunctionTokenLambda(
            signature: lambdaNode.signature,
            paramNames: lambdaNode.paramNames,
            body: lambdaNode.body,
            context: input,
            environment: env,
            evaluateNode: evaluateNode
        );
    }

    internal JToken EvalPartial(PartialApplicationNode partialNode, JToken input, EvaluationEnvironment env)
    {
        JToken func = evaluateNode(partialNode.func, input, env);

        if (func is not FunctionToken function)
        {
            throw new JsonataException("T1008", $"Attempted to partially apply a non-function '{func.ToFlatString()}' got from '{partialNode.func}'");
        }
        ;

        List<JToken?> argsOrNulls = new List<JToken?>(partialNode.args.Count);
        foreach (Node argNode in partialNode.args)
        {
            if (argNode is ArgumentPlaceholderNode)
            {
                argsOrNulls.Add(null);
            }
            else
            {
                JToken arg = evaluateNode(argNode, input, env);
                argsOrNulls.Add(arg);
            }
        }
        return new FunctionTokenPartial(function, argsOrNulls);
    }

    internal JToken EvalFunctionCall(FunctionCallNode functionCallNode, JToken input, EvaluationEnvironment env, JToken? evaluatedFirstArgFromApplication)
    {
        JToken func = evaluateNode(functionCallNode.func, input, env);
        if (func is not FunctionToken function)
        {
            throw new JsonataException("T1006", $"Attempted to invoke a non-function '{func.ToFlatString()}' got from '{functionCallNode.func}'");
        }

        List<JToken> args = new List<JToken>();
        if (evaluatedFirstArgFromApplication != null)
        {
            args.Add(evaluatedFirstArgFromApplication);
        }
        ;
        foreach (Node argNode in functionCallNode.args)
        {
            JToken argValue = evaluateNode(argNode, input, env);
            args.Add(argValue);
        }

        JToken? context = evaluatedFirstArgFromApplication != null ? null : input;

        return function.Invoke(args, context, env);
    }

    internal JToken EvalFunctionApplication(FunctionApplicationNode functionApplicationNode, JToken input, EvaluationEnvironment env)
    {
        JToken lhs = evaluateNode(functionApplicationNode.lhs, input, env);
        if (functionApplicationNode.rhs is FunctionCallNode functionCallNode)
        {
            // this is a function _invocation_; invoke it with lhs expression as the first argument
            return EvalFunctionCall(functionCallNode, input, env, evaluatedFirstArgFromApplication: lhs);
        }
        else
        {
            JToken rhs = evaluateNode(functionApplicationNode.rhs, input, env);
            if (rhs.Type != JTokenType.Function)
            {
                throw new JsonataException("T2006", "The right side of the function application operator ~> must be a function, got " + rhs.Type);
            }
            ;
            if (lhs.Type == JTokenType.Function)
            {
                // this is function chaining (func1 ~> func2)
                // λ($f, $g) { λ($x){ $g($f($x)) } }

                //original jsonata-js used following AST here:
                // var chainAST = parser('function($f, $g) { function($x){ $g($f($x)) } }');
                /* TODO: use pre-compiled AST, or at least parse chainAST just once
                return new FunctionTokenLambda(
                    signature: null,
                    paramNames: new List<string>() { "x" },
                    body: new FunctionCallNode(
                        new 
                    ),
                    context: input,
                    environment: env
                );
                */
                JsonataQuery chainAST = new JsonataQuery("function($f, $g) { function($x){ $g($f($x)) } }");
                JToken chain = chainAST.Eval(JsonataEvaluator.UNDEFINED); //TODO: probably need to provide env as an environment here
                if (chain.Type != JTokenType.Function)
                {
                    throw new Exception("should not happen 1");
                }
                ;
                FunctionToken? chainFunction = chain as FunctionToken;
                if (chainFunction == null)
                {
                    throw new Exception("should not happen 2");
                }
                JToken result = ((JToken)chainFunction).TryInvoke(new List<JToken>() { lhs, rhs }, null, env);
                return result;
            }
            else
            {
                if (rhs.Type != JTokenType.Function)
                {
                    throw new Exception("should not happen 3");
                }
                ;
                FunctionToken? chainFunction = rhs as FunctionToken;
                if (chainFunction == null)
                {
                    throw new Exception("should not happen 4, " + rhs.GetType().Name);
                }
                return ((JToken)chainFunction).TryInvoke(new List<JToken>() { lhs }, null, env);
            }
        }
    }

    internal JToken EvalObjectTransformation(ObjectTransformationNode transformationNode, JToken input, EvaluationEnvironment env)
    {
        return new FunctionTokenTransformation(
            pattern: transformationNode.pattern,
            updates: transformationNode.updates,
            deletes: transformationNode.deletes,
            environment: env,
            evaluateNode: evaluateNode
        );
    }
}