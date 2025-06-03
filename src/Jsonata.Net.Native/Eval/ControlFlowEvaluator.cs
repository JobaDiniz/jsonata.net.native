using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Dom;
using Jsonata.Net.Native.Extensions;
using System;

namespace Jsonata.Net.Native.Eval;

/// <summary>
/// Evaluates control flow nodes including conditionals, assignments, and block expressions.
/// Handles program flow control, variable binding, and scoped execution in JSONata expressions.
/// </summary>
internal sealed class ControlFlowEvaluator
{
    private readonly NodeEvaluator evaluateNode;

    internal ControlFlowEvaluator(NodeEvaluator evaluateNode)
    {
        this.evaluateNode = evaluateNode;
    }

    internal JToken EvalConditional(ConditionalNode conditionalNode, JToken input, EvaluationEnvironment env)
    {
        JToken condition = evaluateNode(conditionalNode.predicate, input, env);
        if (condition.Booleanize())
        {
            return evaluateNode(conditionalNode.thenExpr, input, env);
        }
        else if (conditionalNode.elseExpr != null)
        {
            return evaluateNode(conditionalNode.elseExpr, input, env);
        }
        else
        {
            return JsonataEvaluator.UNDEFINED;
        }
    }

    internal JToken EvalAssignment(AssignmentNode assignmentNode, JToken input, EvaluationEnvironment env)
    {
        JToken value = evaluateNode(assignmentNode.value, input, env);
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
        JToken result = JsonataEvaluator.UNDEFINED;
        foreach (Node expression in blockNode.expressions)
        {
            result = evaluateNode(expression, input, localEnvironment);
        }
        return result;
    }
}