using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Dom;
using Jsonata.Net.Native.Extensions;
using System;

namespace Jsonata.Net.Native.Eval;

internal sealed class ControlFlowEvaluator
{
    private readonly EvalProcessor evalProcessor;

    internal ControlFlowEvaluator(EvalProcessor evalProcessor)
    {
        this.evalProcessor = evalProcessor;
    }

    internal JToken EvalConditional(ConditionalNode conditionalNode, JToken input, EvaluationEnvironment env)
    {
        JToken condition = evalProcessor.Eval(conditionalNode.predicate, input, env);
        if (condition.Booleanize())
        {
            return evalProcessor.Eval(conditionalNode.thenExpr, input, env);
        }
        else if (conditionalNode.elseExpr != null)
        {
            return evalProcessor.Eval(conditionalNode.elseExpr, input, env);
        }
        else
        {
            return EvalProcessor.UNDEFINED;
        }
    }

    internal JToken EvalAssignment(AssignmentNode assignmentNode, JToken input, EvaluationEnvironment env)
    {
        JToken value = evalProcessor.Eval(assignmentNode.value, input, env);
        env.BindValue(assignmentNode.name, value);
        return value;
    }

    internal JToken EvalBlock(BlockNode blockNode, JToken input, EvaluationEnvironment env)
    {
        // create a new frame to limit the scope of variable assignments
        // TODO, only do this if the post-parse stage has flagged this as required
        EvaluationEnvironment localEnvironment = EvaluationEnvironment.CreateNestedEnvironment(env);

        // invoke each expression in turn
        // only return the result of the last one
        JToken result = EvalProcessor.UNDEFINED;
        foreach (Node expression in blockNode.expressions)
        {
            result = evalProcessor.Eval(expression, input, localEnvironment);
        }
        return result;
    }
}