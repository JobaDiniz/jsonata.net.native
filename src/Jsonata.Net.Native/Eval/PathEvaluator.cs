using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Dom;
using Jsonata.Net.Native.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jsonata.Net.Native.Eval;

internal sealed class PathEvaluator
{
    private readonly EvalProcessor evalProcessor;

    internal PathEvaluator(EvalProcessor evalProcessor)
    {
        this.evalProcessor = evalProcessor;
    }

    internal JToken EvalParent(ParentNode parentNode, JToken input, EvaluationEnvironment env)
    {
        if (input.parent == null)
        {
            throw new JsonataException("S0217", "The object representing the 'parent' cannot be derived from this expression: " + input.ToFlatString());
        }
        else
        {
            return input.parent;
        }
    }

    internal JToken EvalName(FieldNameNode nameNode, JToken data, EvaluationEnvironment env)
    {
        switch (data)
        {
            case JObject obj:
                {
                    if (!obj.Properties.TryGetValue(nameNode.value, out JToken? result))
                    {
                        return EvalProcessor.UNDEFINED;
                    }
                    result.parent = data;
                    return result;
                }
            case JArray array:
                {
                    Sequence result = new Sequence();
                    foreach (JToken obj in array.ChildrenTokens)
                    {
                        JToken res = EvalName(nameNode, obj, env);
                        switch (res.Type)
                        {
                            case JTokenType.Undefined:
                                //ignore
                                break;
                            case JTokenType.Array:
                                result.AddRange(((JArray)res).ChildrenTokens);
                                break;
                            default:
                                res.parent = data;
                                result.Add(res);
                                break;
                        }
                    }
                    //return result.Simplify();r
                    return result;
                }
            default:
                return EvalProcessor.UNDEFINED;
        }
    }

    internal JToken EvalWildcard(WildcardNode wildcardNode, JToken input, EvaluationEnvironment env)
    {
        if (input is Sequence inputSequence
            && inputSequence.Count > 0
            && inputSequence.outerWrapper
        )
        {
            input = inputSequence.ChildrenTokens[0];
        }

        Sequence result = new Sequence();
        switch (input.Type)
        {
            case JTokenType.Object:
                JObject obj = (JObject)input;
                foreach (JToken value in obj.Properties.Values)
                {
                    result.AddRange(FlattenArray(value));
                }
                break;
            case JTokenType.Array:
                foreach (JToken value in ((JArray)input).ChildrenTokens)
                {
                    result.AddRange(FlattenArray(value));
                }
                break;
            default:
                break;
        }
        return result;
    }

    internal JToken EvalDescendent(DescendantNode descendentNode, JToken input, EvaluationEnvironment env)
    {
        Sequence result = new Sequence();
        if (input.Type != JTokenType.Undefined)
        {
            RecurseDescendents(result, input);
        }
        return result.Simplify();
    }

    internal JToken EvalPath(PathNode node, JToken data, EvaluationEnvironment env)
    {
        if (node.steps.Count == 0)
        {
            return EvalProcessor.UNDEFINED;
        }

        // if the first step is a variable reference ($...), including root reference ($$),
        //   then the path is absolute rather than relative
        bool isVar = node.steps[0] switch
        {
            VariableNode => true,
            PredicateNode predicateNode => predicateNode.expr is VariableNode,
            _ => false,
        };

        JArray array;
        if (data.Type == JTokenType.Array && !isVar)
        {
            //already an array
            array = (JArray)data;
        }
        else
        {
            // if input is not an array, make it so
            Sequence sequence = new Sequence();
            sequence.Add(data);
            array = sequence;
        }
        ;

        int lastIndex = node.steps.Count - 1;
        for (int stepIndex = 0; stepIndex < node.steps.Count; ++stepIndex)
        {
            Node step = node.steps[stepIndex];
            // if the first step is an explicit array constructor, then just evaluate that (i.e. don't iterate over a context array)
            if (stepIndex == 0 && step is ArrayNode arrayStepNode)
            {
                array = (JArray)evalProcessor.Eval(arrayStepNode, array, env);
            }
            else
            {
                array = EvalPathStep(step, array, env, stepIndex == lastIndex);
            }
            ;

            if (array.Count == 0)
            {
                break;
            }
        }

        if (node.keepArrays)
        {
            if (array is Sequence arraySequence)
            {
                arraySequence.keepSingletons = true;
            }
            else if (array is ExplicitArray)
            {
                // if the array is explicitly constructed in the expression and marked to promote singleton sequences to array
                Sequence resultSequence = new Sequence()
                {
                    keepSingletons = true
                };
                resultSequence.Add(array);
                array = resultSequence;
            }
            else
            {
                //this case is not explicitly defined in jsonata-js, because only sequences are expected to have keepSingletons, but still..
                //maybe we'll need to convert current array to sequence to set keepSingletons
            }
        }
        return array;
    }

    private JArray EvalPathStep(Node step, JArray array, EvaluationEnvironment env, bool lastStep)
    {
        List<JToken> result = new List<JToken>(array.Count);
        foreach (JToken obj in array.ChildrenTokens)
        {
            JToken resultToken = evalProcessor.Eval(step, obj, env);
            if (resultToken.Type != JTokenType.Undefined)
            {
                result.Add(resultToken);
            }
        }
        ;

        if (lastStep
            && result.Count == 1
            && result[0].Type == JTokenType.Array
            && !(result[0] is Sequence)
        )
        {
            return (JArray)result[0];
        }
        ;

        // flatten the sequence
        //see also http://docs.jsonata.org/processing#sequences
        Sequence resultSequence = new Sequence();
        bool isArrayConstructor = step is ArrayNode;
        foreach (JToken resultToken in result)
        {
            if (resultToken.Type != JTokenType.Array   // <=  !Array.isArray(res)
                || isArrayConstructor                  // <=  res.cons
            )
            {
                // it's not an array - just push into the result sequence
                resultSequence.Add(resultToken);
            }
            else
            {
                // res is a sequence - flatten it into the parent sequence
                JArray resultArray = (JArray)resultToken;
                foreach (JToken subResult in resultArray.ChildrenTokens)
                {
                    if (subResult.parent == null)
                    {
                        subResult.parent = resultArray.parent;
                    }
                    resultSequence.Add(subResult);
                }
            }
        }

        return resultSequence;
    }

    private void RecurseDescendents(Sequence result, JToken input)
    {
        switch (input.Type)
        {
            case JTokenType.Array:
                foreach (JToken child in ((JArray)input).ChildrenTokens)
                {
                    RecurseDescendents(result, child);
                }
                break;
            case JTokenType.Object:
                result.Add(input);
                foreach (JToken child in ((JObject)input).Properties.Values)
                {
                    RecurseDescendents(result, child);
                }
                break;
            default:
                result.Add(input);
                break;
        }
    }

    private IEnumerable<JToken> FlattenArray(JToken input)
    {
        switch (input.Type)
        {
            case JTokenType.Array:
                foreach (JToken child in ((JArray)input).ChildrenTokens)
                {
                    foreach (JToken result in FlattenArray(child))
                    {
                        if (result.Type != JTokenType.Undefined)
                        {
                            yield return result;
                        }
                    }
                }
                break;
            case JTokenType.Undefined:
                break;
            default:
                yield return input;
                break;
        }
    }
}