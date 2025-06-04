using Jsonata.Net.Native.Json;
using System;
using Jsonata.Net.Native.Dom;

namespace Jsonata.Net.Native.Eval;

/// <summary>
/// Delegate for evaluating JSONata expression nodes.
/// </summary>
/// <param name="node">The node to evaluate</param>
/// <param name="input">The input context data</param>
/// <param name="env">The evaluation environment</param>
/// <returns>The result of evaluating the node</returns>
internal delegate JToken NodeEvaluator(Node node, JToken input, EvaluationEnvironment env);

/// <summary>
/// Coordinates the evaluation of JSONata expressions by delegating to specialized evaluators.
/// </summary>
internal sealed class JsonataEvaluator
{

    private readonly LiteralEvaluator literalEvaluator;
    private readonly PathEvaluator pathEvaluator;
    private readonly OperatorEvaluator operatorEvaluator;
    private readonly StructuralEvaluator structuralEvaluator;
    private readonly FunctionalEvaluator functionalEvaluator;
    private readonly ControlFlowEvaluator controlFlowEvaluator;

    internal JsonataEvaluator()
    {
        literalEvaluator = new LiteralEvaluator();
        pathEvaluator = new PathEvaluator(EvaluateNode);
        operatorEvaluator = new OperatorEvaluator(EvaluateNode);
        structuralEvaluator = new StructuralEvaluator(EvaluateNode);
        functionalEvaluator = new FunctionalEvaluator(EvaluateNode);
        controlFlowEvaluator = new ControlFlowEvaluator(EvaluateNode);
    }

    internal JToken ExecuteQuery(Node rootNode, JToken data, EvaluationEnvironment parentEnvironment)
    {
        EvaluationEnvironment environment = parentEnvironment.CreateChildForQueryExecution();

        environment.BindValue("$", data);

        if (data.Type == JTokenType.Array)
        {
            // if the input is a JSON array, then wrap it in a singleton sequence so it gets treated as a single input
            JArray dataArr = new Sequence() { outerWrapper = true };
            dataArr.Add(data);
            data = dataArr;
        }
        JToken result = EvaluateNode(rootNode, data, environment);
        if (result is Sequence seq)
        {
            //result = seq.GetValue();
            if (seq.Count == 1 && !seq.keepSingletons)
            {
                result = seq.ChildrenTokens[0];
            }
        }

        //to release unused tokens
        result.ClearParent();

        return result;
    }

    internal JToken EvaluateNode(Node node, JToken input, EvaluationEnvironment env)
    {
        JToken result = EvaluateInternal(node, input, env);
        if (result is Sequence sequence)
        {
            if (sequence.Count == 0)
            {
                return JValue.Undefined;
            }
            else if (sequence.Count == 1 && !sequence.keepSingletons)
            {
                return sequence.ChildrenTokens[0];
            }
        }
        ;
        return result;
    }

    private JToken EvaluateInternal(Node node, JToken input, EvaluationEnvironment env)
    {
        switch (node)
        {
            // Literal evaluations
            case StringNode stringNode:
                return literalEvaluator.EvalString(stringNode, input, env);
            case NumberDoubleNode numberDoubleNode:
                return literalEvaluator.EvalNumber(numberDoubleNode, input, env);
            case NumberIntNode numberIntNode:
                return literalEvaluator.EvalNumber(numberIntNode, input, env);
            case BooleanNode booleanNode:
                return literalEvaluator.EvalBoolean(booleanNode, input, env);
            case NullNode nullNode:
                return literalEvaluator.EvalNull(nullNode, input, env);
            case RegexNode regexNode:
                return literalEvaluator.EvalRegex(regexNode, input, env);
            
            // Functional evaluations
            case VariableNode variableNode:
                return functionalEvaluator.EvalVariable(variableNode, input, env);
            case LambdaNode lambdaNode:
                return functionalEvaluator.EvalLambda(lambdaNode, input, env);
            case ObjectTransformationNode transformationNode:
                return functionalEvaluator.EvalObjectTransformation(transformationNode, input, env);
            case PartialApplicationNode partialNode:
                return functionalEvaluator.EvalPartial(partialNode, input, env);
            case FunctionCallNode functionCallNode:
                return functionalEvaluator.EvalFunctionCall(functionCallNode, input, env, null);
            case FunctionApplicationNode functionApplicationNode:
                return functionalEvaluator.EvalFunctionApplication(functionApplicationNode, input, env);
            
            // Path evaluations
            case FieldNameNode nameNode:
                return pathEvaluator.EvalName(nameNode, input, env);
            case ParentNode parentNode:
                return pathEvaluator.EvalParent(parentNode, input, env);
            case PathNode pathNode:
                return pathEvaluator.EvalPath(pathNode, input, env);
            case WildcardNode wildcardNode:
                return pathEvaluator.EvalWildcard(wildcardNode, input, env);
            case DescendantNode descendentNode:
                return pathEvaluator.EvalDescendent(descendentNode, input, env);
            
            // Operator evaluations
            case NegationNode negationNode:
                return operatorEvaluator.EvalNegation(negationNode, input, env);
            case NumericOperatorNode numericOperatorNode:
                return operatorEvaluator.EvalNumericOperator(numericOperatorNode, input, env);
            case ComparisonOperatorNode comparisonOperatorNode:
                return operatorEvaluator.EvalComparisonOperator(comparisonOperatorNode, input, env);
            case BooleanOperatorNode booleanOperatorNode:
                return operatorEvaluator.EvalBooleanOperator(booleanOperatorNode, input, env);
            case StringConcatenationNode stringConcatenationNode:
                return operatorEvaluator.EvalStringConcatenation(stringConcatenationNode, input, env);
            
            // Structural evaluations
            case RangeNode rangeNode:
                return structuralEvaluator.EvalRange(rangeNode, input, env);
            case ArrayNode arrayNode:
                return structuralEvaluator.EvalArray(arrayNode, input, env);
            case ObjectNode objectNode:
                return structuralEvaluator.EvalObject(objectNode, input, env);
            case PredicateNode predicateNode:
                return structuralEvaluator.EvalPredicate(predicateNode, input, env);
            case SortNode sortNode:
                return structuralEvaluator.EvalSort(sortNode, input, env);
            case GroupNode groupNode:
                return structuralEvaluator.EvalGroup(groupNode, input, env);
            
            // Control flow evaluations
            case BlockNode blockNode:
                return controlFlowEvaluator.EvalBlock(blockNode, input, env);
            case ConditionalNode conditionalNode:
                return controlFlowEvaluator.EvalConditional(conditionalNode, input, env);
            case AssignmentNode assignmentNode:
                return controlFlowEvaluator.EvalAssignment(assignmentNode, input, env);
            
            default:
                throw new NotImplementedException($"eval: unexpected node type {node.GetType().Name}: {node}");
        }
    }
}