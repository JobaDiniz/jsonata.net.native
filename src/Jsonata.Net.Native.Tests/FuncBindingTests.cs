using System;
using Jsonata.Net.Native;
using Jsonata.Net.Native.Json;
using NUnit.Framework;

namespace Jsonata.Net.Native.Tests
{
    public sealed class FuncBindingTests
    {
        [Test]
        public void ViaMethodInfo()
        {
            EvaluationEnvironment env = new EvaluationEnvironment();
            env.BindFunction(typeof(FuncBindingTests).GetMethod(nameof(Mult3))!);
            JsonataQuery query = new JsonataQuery("( $Mult3(1, 2, 3); )");
            JToken result = query.Eval(JValue.CreateNull(), env);
            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(6, (int)result);
        }

        [Test]
        public void ViaFuncStatic()
        {
            EvaluationEnvironment env = new EvaluationEnvironment();
            Func<int, int, int, int> func = Mult3;
            env.BindFunction(nameof(Mult3), func);
            JsonataQuery query = new JsonataQuery("( $Mult3(1, 2, 3); )");
            JToken result = query.Eval(JValue.CreateNull(), env);
            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(6, (int)result);
        }

        [Test]
        public void ViaInplaceStatic()
        {
            EvaluationEnvironment env = new EvaluationEnvironment();
            env.BindFunction(nameof(Mult3), Mult3);
            JsonataQuery query = new JsonataQuery("( $Mult3(1, 2, 3); )");
            JToken result = query.Eval(JValue.CreateNull(), env);
            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(6, (int)result);
        }

        [Test]
        public void ViaFuncLambda()
        {
            EvaluationEnvironment env = new EvaluationEnvironment();
            Func<int, int, int, int> func = (int a, int b, int c) => a * b * c;
            env.BindFunction(nameof(Mult3), func);
            JsonataQuery query = new JsonataQuery("( $Mult3(1, 2, 3); )");
            JToken result = query.Eval(JValue.CreateNull(), env);
            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(6, (int)result);
        }

        [Test]
        public void ViaInplaceLambda()
        {
            EvaluationEnvironment env = new EvaluationEnvironment();
            env.BindFunction(nameof(Mult3), (int a, int b, int c) => a * b * c);
            JsonataQuery query = new JsonataQuery("( $Mult3(1, 2, 3); )");
            JToken result = query.Eval(JValue.CreateNull(), env);
            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(6, (int)result);
        }

        public static int Mult3(int a, int b, int c)
        {
            return a * b * c;
        }

        private static int s_lazyEvaluationCounter;

        private void ResetLazyCounter()
        {
            s_lazyEvaluationCounter = 0;
        }

        private JToken CreateLazyValue(string json, bool incrementCounter = true)
        {
            if (incrementCounter)
            {
                s_lazyEvaluationCounter++;
            }
            return JToken.Parse(json);
        }

        [Test]
        public void TestBasicLazyEvaluation()
        {
            ResetLazyCounter();
            JObject bindings = new JObject();
            bindings.Add("lazyVar", new JValue("LAZY:\"hello\""));

            JsonataQuery query = new JsonataQuery("$lazyVar");
            JToken result = query.Eval(JValue.CreateNull(), bindings);

            Assert.AreEqual(JTokenType.String, result.Type);
            Assert.AreEqual("hello", (string)result);
        }

        [Test]
        public void TestLazyEvaluationCaching()
        {
            ResetLazyCounter();
            JObject bindings = new JObject();
            //This binding will use a lambda that increments the counter
            bindings.Add("cachedVar", new JValue("LAZY:\"cached_value\""));

            EvaluationEnvironment env = new EvaluationEnvironment();
            env.BindLazyValue("cachedVarWithCounter", () => {
                s_lazyEvaluationCounter++;
                return JToken.Parse("\"cached_value_counted\"");
            });

            JsonataQuery query = new JsonataQuery("$cachedVarWithCounter & $cachedVarWithCounter");
            JToken result = query.Eval(JValue.CreateNull(), env); // Use the env with the counter logic

            Assert.AreEqual(JTokenType.String, result.Type);
            Assert.AreEqual("cached_value_countedcached_value_counted", (string)result);
            Assert.AreEqual(1, s_lazyEvaluationCounter, "Lazy function should only be called once due to caching.");
        }

        [Test]
        public void TestMultipleLazyVariables()
        {
            ResetLazyCounter();
            EvaluationEnvironment env = new EvaluationEnvironment();
            env.BindLazyValue("lazy1", () => {
                s_lazyEvaluationCounter++;
                return JToken.Parse("10");
            });
            env.BindLazyValue("lazy2", () => {
                s_lazyEvaluationCounter++;
                return JToken.Parse("20");
            });

            JsonataQuery query = new JsonataQuery("$lazy1 + $lazy2");
            JToken result = query.Eval(JValue.CreateNull(), env);

            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(30, (long)result);
            Assert.AreEqual(2, s_lazyEvaluationCounter, "Both lazy functions should be called once.");
        }

        [Test]
        public void TestMixedLazyAndEagerVariables()
        {
            ResetLazyCounter();
            EvaluationEnvironment env = new EvaluationEnvironment();
            env.BindValue("eagerVar", new JValue("hello "));
            env.BindLazyValue("lazyVar", () => {
                s_lazyEvaluationCounter++;
                return JToken.Parse("\"world\"");
            });

            JsonataQuery query = new JsonataQuery("$eagerVar & $lazyVar");
            JToken result = query.Eval(JValue.CreateNull(), env);

            Assert.AreEqual(JTokenType.String, result.Type);
            Assert.AreEqual("hello world", (string)result);
            Assert.AreEqual(1, s_lazyEvaluationCounter, "Lazy function should be called once.");
        }

        [Test]
        public void TestLazyVariableNotUsed()
        {
            ResetLazyCounter();
            EvaluationEnvironment env = new EvaluationEnvironment();
            env.BindLazyValue("unusedLazy", () => {
                s_lazyEvaluationCounter++;
                return JToken.Parse("\"should_not_eval\"");
            });
            env.BindValue("usedVar", new JValue(123));

            JsonataQuery query = new JsonataQuery("$usedVar");
            JToken result = query.Eval(JValue.CreateNull(), env);

            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(123, (long)result);
            Assert.AreEqual(0, s_lazyEvaluationCounter, "Unused lazy function should not be called.");
        }
    }
}
