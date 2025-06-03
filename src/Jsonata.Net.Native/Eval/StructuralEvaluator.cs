using Jsonata.Net.Native.Json;
using Jsonata.Net.Native.Dom;
using Jsonata.Net.Native.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Jsonata.Net.Native.Eval;

internal sealed class StructuralEvaluator
{
    private readonly EvalProcessor evalProcessor;

    internal StructuralEvaluator(EvalProcessor evalProcessor)
    {
        this.evalProcessor = evalProcessor;
    }

    internal JToken EvalRange(RangeNode rangeNode, JToken input, EvaluationEnvironment env)
    {
        JToken lhs = evalProcessor.Eval(rangeNode.lhs, input, env);
        JToken rhs = evalProcessor.Eval(rangeNode.rhs, input, env);

        if (lhs.Type != JTokenType.Undefined && lhs.Type != JTokenType.Integer)
        {
            throw new JsonataException("T2003", $"The left side of the range operator (..) must evaluate to an integer, got {lhs.Type}");
        }
        else if (rhs.Type != JTokenType.Undefined && rhs.Type != JTokenType.Integer)
        {
            throw new JsonataException("T2004", $"The right side of the range operator (..) must evaluate to an integer, got {rhs.Type}");
        }
        else if (lhs.Type == JTokenType.Undefined || rhs.Type == JTokenType.Undefined)
        {
            // if either side is undefined, the result is undefined
            return EvalProcessor.UNDEFINED;
        }
        ;

        long lhsValue = (long)lhs;
        long rhsValue = (long)rhs;

        if (lhsValue > rhsValue)
        {
            // if the lhs is greater than the rhs, return undefined
            return EvalProcessor.UNDEFINED;
        }
        ;

        // limit the size of the array to ten million entries (1e7)
        // this is an implementation defined limit to protect against
        // memory and performance issues.  This value may increase in the future.
        long size = rhsValue - lhsValue + 1;
        if (size > 1e7)
        {
            throw new JsonataException("D2014", $"The size of the sequence allocated by the range operator (..) must not exceed 1e7.  Attempted to allocate {size}.");
        }
        ;

        JArray result = new Sequence();
        for (long value = lhsValue; value <= rhsValue; ++value)
        {
            result.Add(new JValue(value));
        }
        return result;
    }

    internal JToken EvalArray(ArrayNode arrayNode, JToken input, EvaluationEnvironment env)
    {
        JArray result = new ExplicitArray();
        foreach (Node node in arrayNode.items)
        {
            JToken res = evalProcessor.Eval(node, input, env);
            switch (res.Type)
            {
                case JTokenType.Undefined:
                    break;
                case JTokenType.Array:
                    if (node is ArrayNode)
                    {
                        result.Add(res);
                    }
                    else if (res.Type == JTokenType.Array
                        && (!(res is Sequence sequence) || !sequence.keepSingletons)
                    )
                    {
                        result.AddRange(((JArray)res).ChildrenTokens);
                    }
                    else
                    {
                        result.Add(res);
                    }
                    break;
                default:
                    result.Add(res);
                    break;
            }
        }
        return result;
    }

    internal JToken EvalObject(ObjectNode objectNode, JToken input, EvaluationEnvironment env)
    {
        JArray inputArray;
        if (input.Type == JTokenType.Array)
        {
            inputArray = (JArray)input;
        }
        else
        {
            Sequence inputSequence = new Sequence();
            inputSequence.Add(input);
            inputArray = inputSequence;
        }
        ;

        /* //does not seem to have any positive effects, but causes this issue https://github.com/mikhail-barg/jsonata.net.native/issues/9

        // if the array is empty, add an undefined entry to enable literal JSON object to be generated
        if (inputArray.Count == 0)
        {
            inputArray.Add(EvalProcessor.UNDEFINED);
        }
        */


        Dictionary<string, KeyIndex> itemsGroupedByKey = new Dictionary<string, KeyIndex>();

        foreach (JToken item in inputArray.ChildrenTokens)
        {
            for (int pairIndex = 0; pairIndex < objectNode.pairs.Count; ++pairIndex)
            {
                Node keyNode = objectNode.pairs[pairIndex].Item1;
                JToken keyToken = evalProcessor.Eval(keyNode, item, env);
                if (keyToken.Type != JTokenType.String)
                {
                    throw new JsonataException("T1003", $"Object key should be String. Expression evaluated to {keyToken.Type} '{keyToken.ToFlatString()}'");
                }
                ;
                string key = (string)keyToken!;
                if (itemsGroupedByKey.TryGetValue(key, out KeyIndex? keyIndex))
                {
                    if (keyIndex.pairIndex != pairIndex)
                    {
                        // this key has been generated by another expression in this group
                        // when multiple key expressions evaluate to the same key, then error D1009 must be thrown
                        throw new JsonataException("D1009", $"Duplicate object key '{key}'");
                    }
                    else
                    {
                        keyIndex.inputs.Add(item);
                    }
                }
                else
                {
                    itemsGroupedByKey.Add(key, new KeyIndex(pairIndex, item));
                }
            }
        }

        JObject result = new JObject();
        // iterate over the groups to evaluate the 'value' expression
        foreach (KeyValuePair<string, KeyIndex> keyPair in itemsGroupedByKey)
        {
            string key = keyPair.Key;
            Node rhs = objectNode.pairs[keyPair.Value.pairIndex].Item2;
            JToken context = keyPair.Value.inputs;
            JToken value = evalProcessor.Eval(rhs, context, env);
            if (value.Type != JTokenType.Undefined)
            {
                result.Add(key, value);
            }
        }
        return result;
    }

    internal JToken EvalPredicate(PredicateNode predicateNode, JToken input, EvaluationEnvironment env)
    {
        JToken itemsToken = evalProcessor.Eval(predicateNode.expr, input, env);
        if (itemsToken.Type == JTokenType.Undefined)
        {
            return EvalProcessor.UNDEFINED;
        }
        ;

        JArray itemsArray;
        if (itemsToken.Type == JTokenType.Array)
        {
            itemsArray = (JArray)itemsToken;

            foreach (JToken item in itemsArray.ChildrenTokens)
            {
                if (item.parent == null)
                {
                    item.parent = itemsToken.parent;
                }
            }
        }
        else
        {
            itemsArray = new Sequence();
            itemsArray.Add(itemsToken);
        }
        ;

        foreach (Node filter in predicateNode.filters)
        {
            itemsArray = EvalFilter(filter, itemsArray, env);
            if (itemsArray.Count == 0)
            {
                return EvalProcessor.UNDEFINED;
            }
        }

        if (itemsArray is Sequence sequence)
        {
            return sequence.Simplify();
        }
        return itemsArray;
    }

    internal JToken EvalSort(SortNode sortNode, JToken input, EvaluationEnvironment env)
    {
        JToken items = evalProcessor.Eval(sortNode.expr, input, env);
        switch (items.Type)
        {
            case JTokenType.Undefined:
                return EvalProcessor.UNDEFINED;
            case JTokenType.Array:
                break;
            default:
                return items;
        }
        List<JToken> itemsList = ((JArray)items).ChildrenTokens.ToList();

        try
        {
            itemsList.Sort(comparison);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
            else
            {
                throw;
            }
        }

        JArray result = new JArray(itemsList.Count);
        foreach (JToken item in itemsList)
        {
            result.Add(item);
        }
        return result;

        int comparison(JToken a, JToken b)
        {
            foreach (SortNode.Term term in sortNode.terms)
            {
                //evaluate the sort term in the context of a
                JToken aa = evalProcessor.Eval(term.expr, a, env);
                //evaluate the sort term in the context of b
                JToken bb = evalProcessor.Eval(term.expr, b, env);

                // undefined should be last in sort order
                if (aa.Type == JTokenType.Undefined)
                {
                    if (bb.Type == JTokenType.Undefined)
                    {
                        continue;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (bb.Type == JTokenType.Undefined)
                {
                    return -1;
                }

                if (aa.Type != JTokenType.String && aa.Type != JTokenType.Integer && aa.Type != JTokenType.Float)
                {
                    throw new JsonataException("T2008", $"The expressions within an order-by clause must evaluate to numeric or string values. Got {aa.Type} ({aa.ToFlatString()})");
                }
                ;
                if (bb.Type != JTokenType.String && bb.Type != JTokenType.Integer && bb.Type != JTokenType.Float)
                {
                    throw new JsonataException("T2008", $"The expressions within an order-by clause must evaluate to numeric or string values. Got {bb.Type} ({bb.ToFlatString()})");
                }
                ;

                if ((aa.Type == JTokenType.String) != (bb.Type == JTokenType.String))
                {
                    throw new JsonataException("T2007", $"Type mismatch when comparing values {aa.Type}({aa.ToFlatString()}) and {bb.Type}({bb.ToFlatString()}) in order-by clause");
                }

                int comp;

                if (aa.Type == JTokenType.String)
                {
                    comp = String.Compare((string)aa!, (string)bb!);
                }
                else
                {
                    double aValue = aa.Type == JTokenType.Float ? (double)aa : (long)aa;
                    double bValue = bb.Type == JTokenType.Float ? (double)bb : (long)bb;
                    comp = aValue.CompareTo(bValue);
                }
                ;

                if (term.dir == SortNode.Direction.Descending)
                {
                    comp = -comp;
                }
                ;

                if (comp != 0)
                {
                    return comp;
                }
            }
            ;
            return 0;
        }
    }

    internal JToken EvalGroup(GroupNode groupNode, JToken input, EvaluationEnvironment env)
    {
        JToken items = evalProcessor.Eval(groupNode.expr, input, env);
        return EvalObject(groupNode.objectNode, items, env);
    }

    private JArray EvalFilter(Node filter, JArray itemsArray, EvaluationEnvironment env)
    {
        if (filter is NumberNode numberNode)
        {
            int index = numberNode.GetIntValue();
            JToken resultToken = GetArrayElementByIndex(itemsArray, index);
            if (resultToken.Type == JTokenType.Array)
            {
                return (JArray)resultToken;
            }
            else
            {
                Sequence result = new Sequence();
                result.Add(resultToken);
                return result;
            }
        }
        else
        {
            Sequence result = new Sequence();
            for (int index = 0; index < itemsArray.Count; ++index)
            {
                JToken item = itemsArray.ChildrenTokens[index];
                JToken res = evalProcessor.Eval(filter, item, env);
                if (res.Type == JTokenType.Integer || res.Type == JTokenType.Float)
                {
                    CheckAppendToken(result, item, index, res);
                }
                else if (res.IsArrayOfNumbers())
                {
                    foreach (JToken subtoken in ((JArray)res).ChildrenTokens)
                    {
                        CheckAppendToken(result, item, index, subtoken);
                    }
                }
                else if (res.Booleanize())
                {
                    result.Add(item);
                }
            }
            return result;
        }

        int WrapArrayIndex(JArray array, int index)
        {
            if (index < 0)
            {
                index = array.Count + index;
            }
            ;
            return index;
        }

        JToken GetArrayElementByIndex(JArray array, int index)
        {
            index = WrapArrayIndex(array, index);
            if (index < 0 || index >= array.Count)
            {
                return EvalProcessor.UNDEFINED;
            }
            else
            {
                return array.ChildrenTokens[index];
            }
        }

        void CheckAppendToken(Sequence result, JToken item, int itemIndex, JToken indexToken)
        {
            if (indexToken.Type == JTokenType.Integer)
            {
                int indexTokenValue = WrapArrayIndex(itemsArray, (int)(long)indexToken);
                if (indexTokenValue == itemIndex)
                {
                    result.Add(item);
                }
            }
            else if (indexToken.Type == JTokenType.Float)
            {
                int indexTokenValue = WrapArrayIndex(itemsArray, (int)(double)indexToken);
                if (indexTokenValue == itemIndex)
                {
                    result.Add(item);
                }
            }
        }
    }

    private sealed class KeyIndex
    {
        internal readonly int pairIndex;
        internal readonly Sequence inputs = new Sequence();

        internal KeyIndex(int pairIndex, JToken firstInput)
        {
            this.pairIndex = pairIndex;
            this.inputs.Add(firstInput);
        }
    }
}