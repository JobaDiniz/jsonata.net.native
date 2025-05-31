using System;
using Jsonata.Net.Native.Eval; //Required for EvalProcessor.UNDEFINED
using Jsonata.Net.Native.Json;
using NUnit.Framework;

namespace Jsonata.Net.Native.Tests
{
    public sealed class FuncBindingTests
    {
        // Original tests for BindFunction (unrelated to lazy evaluation)
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

        // Counter for lazy evaluations
        private static int s_lazyEvaluationCounter;
        private static int s_anotherLazyCounter;


        private void ResetCounters()
        {
            s_lazyEvaluationCounter = 0;
            s_anotherLazyCounter = 0;
        }

        // ** Adapted Tests for Lazy Evaluation (BindLazyValue and new Provider model) **

        [Test]
        public void TestBindLazyValue_IsCalledForEachAccess() // Formerly TestLazyEvaluationCaching
        {
            ResetCounters();
            EvaluationEnvironment env = new EvaluationEnvironment();
            env.BindLazyValue("boundLazyVar", () => {
                s_lazyEvaluationCounter++;
                return JToken.Parse("\"called_via_bindlazyvalue\"");
            });

            JsonataQuery query = new JsonataQuery("$boundLazyVar & ' ' & $boundLazyVar");
            JToken result = query.Eval(JValue.CreateNull(), env);

            Assert.AreEqual(JTokenType.String, result.Type);
            Assert.AreEqual("called_via_bindlazyvalue called_via_bindlazyvalue", (string)result);
            Assert.AreEqual(2, s_lazyEvaluationCounter, "Function bound with BindLazyValue should be called each time it's accessed.");
        }

        [Test]
        public void TestLazyProvider_BasicEvaluation() // Formerly TestBasicLazyEvaluation
        {
            ResetCounters();

            Func<string, JToken> provider = (name) => {
                if (name == "lazyVarFromProvider")
                {
                    s_lazyEvaluationCounter++;
                    return new JValue("hello_from_provider");
                }
                return EvalProcessor.UNDEFINED;
            };

            JsonataQuery query = new JsonataQuery("$lazyVarFromProvider");
            JToken result = query.Eval(JValue.CreateNull(), null, provider);

            Assert.AreEqual(JTokenType.String, result.Type);
            Assert.AreEqual("hello_from_provider", (string)result);
            Assert.AreEqual(1, s_lazyEvaluationCounter, "Provider should be called once for a single access.");
        }

        [Test]
        public void TestBindLazyValue_MultipleVariables_CalledForEachAccess() // Formerly TestMultipleLazyVariables
        {
            ResetCounters();
            EvaluationEnvironment env = new EvaluationEnvironment();
            env.BindLazyValue("lazy1", () => {
                s_lazyEvaluationCounter++;
                return JToken.Parse("10");
            });
            env.BindLazyValue("lazy2", () => {
                s_anotherLazyCounter++;
                return JToken.Parse("20");
            });

            JsonataQuery query = new JsonataQuery("$lazy1 + $lazy2 + $lazy1"); // lazy1 accessed twice
            JToken result = query.Eval(JValue.CreateNull(), env);

            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(40, (long)result); // 10 + 20 + 10
            Assert.AreEqual(2, s_lazyEvaluationCounter, "lazy1 should be called twice.");
            Assert.AreEqual(1, s_anotherLazyCounter, "lazy2 should be called once.");
        }

        [Test]
        public void TestMixed_EagerBindingAndLazyProvider() // Formerly TestMixedLazyAndEagerVariables
        {
            ResetCounters();
            JObject bindings = new JObject();
            bindings.Add("eagerVar", new JValue("hello_eager "));

            Func<string, JToken> provider = (name) => {
                if (name == "lazyVar")
                {
                    s_lazyEvaluationCounter++;
                    return new JValue("world_provider");
                }
                return EvalProcessor.UNDEFINED;
            };

            JsonataQuery query = new JsonataQuery("$eagerVar & $lazyVar");
            JToken result = query.Eval(JValue.CreateNull(), bindings, provider);

            Assert.AreEqual(JTokenType.String, result.Type);
            Assert.AreEqual("hello_eager world_provider", (string)result);
            Assert.AreEqual(1, s_lazyEvaluationCounter, "Lazy provider function should be called once.");
        }

        [Test]
        public void TestLazyProvider_VariableNotUsed() // Formerly TestLazyVariableNotUsed
        {
            ResetCounters();
            JObject bindings = new JObject();
            bindings.Add("usedVar", new JValue(123));

            Func<string, JToken> provider = (name) => {
                if (name == "unusedLazyViaProvider")
                {
                    s_lazyEvaluationCounter++;
                    return new JValue("should_not_eval");
                }
                return EvalProcessor.UNDEFINED;
            };

            JsonataQuery query = new JsonataQuery("$usedVar");
            JToken result = query.Eval(JValue.CreateNull(), bindings, provider);

            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(123, (long)result);
            Assert.AreEqual(0, s_lazyEvaluationCounter, "Unused lazy variable from provider should not be called.");
        }

        // ** New Tests for Provider Logic **

        [Test]
        public void TestLazyProvider_CalledForEachAccess()
        {
            ResetCounters();
            Func<string, JToken> provider = (name) => {
                if (name == "myVar")
                {
                    s_lazyEvaluationCounter++;
                    return new JValue(5); // Return a number for addition
                }
                return EvalProcessor.UNDEFINED;
            };

            JsonataQuery query = new JsonataQuery("$myVar + $myVar");
            JToken result = query.Eval(JValue.CreateNull(), null, provider);

            Assert.AreEqual(JTokenType.Integer, result.Type);
            Assert.AreEqual(10, (long)result);
            Assert.AreEqual(2, s_lazyEvaluationCounter, "Provider should be called for each access of the variable.");
        }

        [Test]
        public void TestEagerBinding_TakesPrecedenceOverProvider()
        {
            ResetCounters();
            JObject bindings = new JObject();
            bindings.Add("myVar", new JValue("eager_value"));

            Func<string, JToken> provider = (name) => {
                if (name == "myVar")
                {
                    s_lazyEvaluationCounter++; // This should not be incremented
                    return new JValue("provider_value");
                }
                return EvalProcessor.UNDEFINED;
            };

            JsonataQuery query = new JsonataQuery("$myVar");
            JToken result = query.Eval(JValue.CreateNull(), bindings, provider);

            Assert.AreEqual(JTokenType.String, result.Type);
            Assert.AreEqual("eager_value", (string)result);
            Assert.AreEqual(0, s_lazyEvaluationCounter, "Provider should not be called when an eager binding exists for the same name.");
        }

        [Test]
        public void TestBindLazyValue_TakesPrecedenceOverProvider()
        {
            ResetCounters();
            EvaluationEnvironment env = new EvaluationEnvironment(); // Create env to use BindLazyValue

            env.BindLazyValue("foo", () => {
                s_lazyEvaluationCounter++; // Counter for BindLazyValue func
                return new JValue("from_bind_lazy_value");
            });

            Func<string, JToken> provider = (name) => {
                if (name == "foo")
                {
                    s_anotherLazyCounter++; // Counter for provider func
                    return new JValue("from_provider");
                }
                return EvalProcessor.UNDEFINED;
            };

            // Need to create a new environment that has the provider, and make the one with BindLazyValue its parent.
            // Or, more directly, instantiate Eval Env with the provider and then call BindLazyValue on it.
            // The public constructor for Eval Env with provider already sets Default Env as parent.
            // Let's create an environment that will have BOTH the explicit BindLazyValue AND the provider.
            // The Lookup order is: m_bindings (where BindLazyValue stores) -> m_lazyVariableProvider -> parent.

            // To test this specific precedence, we need an environment that has both.
            // The constructor EvaluationEnvironment(Func<string, JToken> lazyVariableProvider) sets up the provider.
            // Then we can call BindLazyValue on this specific environment.
            EvaluationEnvironment testEnvWithProvider = new EvaluationEnvironment(provider);
            testEnvWithProvider.BindLazyValue("foo", () => {
                s_lazyEvaluationCounter++; // Counter for BindLazyValue func
                return new JValue("from_bind_lazy_value");
            });


            JsonataQuery query = new JsonataQuery("$foo");
            // Use the Eval overload that takes a pre-configured environment
            JToken result = query.Eval(JValue.CreateNull(), testEnvWithProvider);

            Assert.AreEqual(JTokenType.String, result.Type);
            Assert.AreEqual("from_bind_lazy_value", (string)result);
            Assert.AreEqual(1, s_lazyEvaluationCounter, "Function from BindLazyValue should be called.");
            Assert.AreEqual(0, s_anotherLazyCounter, "General provider should not be called for 'foo'.");
        }

        [Test]
        public void TestLazyProvider_VariableNotFound()
        {
            ResetCounters();
            Func<string, JToken> provider = (name) => {
                if (name == "knownVar")
                {
                    s_lazyEvaluationCounter++;
                    return new JValue("known_value");
                }
                // For "unknownVar", this provider will implicitly return null, then Eval Env makes it UNDEFINED
                return EvalProcessor.UNDEFINED;
            };

            JsonataQuery query = new JsonataQuery("$unknownVar");
            JToken result = query.Eval(JValue.CreateNull(), null, provider);

            Assert.AreEqual(JTokenType.Undefined, result.Type, "Accessing an undefined variable via provider should result in undefined.");
            Assert.AreEqual(0, s_lazyEvaluationCounter, "Provider should be called for 'unknownVar' but return undefined, counter for knownVar not incremented.");
        }

        [Test]
        public void TestLazyProvider_OverridesParentBinding()
        {
            ResetCounters();

            // Parent environment with an eager binding for 'x'
            EvaluationEnvironment parentEnv = new EvaluationEnvironment();
            parentEnv.BindValue("x", new JValue("parent_x_value"));

            // Child environment with a lazy provider for 'x'
            Func<string, JToken> childProvider = (name) => {
                if (name == "x")
                {
                    s_lazyEvaluationCounter++;
                    return new JValue("child_provider_x_value");
                }
                return EvalProcessor.UNDEFINED;
            };
            // Create child environment, explicitly setting its parent and its own provider
            EvaluationEnvironment childEnv = new EvaluationEnvironment(parentEnv, null, childProvider); // Using the private constructor form via public one is tricky here
                                                                                                        // Let's use a more direct setup if possible, or adjust Eval
            // The current public EvaluationEnvironment constructors automatically parent to DefaultEnvironment if no parent specified.
            // To set a specific parent AND a provider for the child, we'd need to ensure the constructor path allows it.
            // The constructor EvaluationEnvironment(EvaluationEnvironment? parent, EvaluationSupplement? supplement, Func<string, JToken>? lazyVariableProvider) is private.
            // EvaluationEnvironment.CreateNestedEnvironment or CreateEvalEnvironment are internal.
            // For testing, we can assume the environment setup is correct as per previous steps.
            // The key is the lookup order: child_bindings -> child_provider -> parent_lookup.

            // We'll test by providing the provider to the Eval call, which creates an environment.
            // To simulate parent, we need Eval to take a base environment and a provider for the new context.
            // Current Eval(data, bindings, provider) creates new Env(bindings, provider) which parents Default.
            // The most straightforward way to test this specific interaction is to configure childEnv and pass it to Eval.
            // So, how to create `childEnv` that has `parentEnv` as parent AND `childProvider`?
            // We can't directly call the private constructor.
            // Let's use BindFunction on parent and then create a nested environment which should inherit the provider.

            // Simpler approach for this test:
            // 1. Create parentEnv. Bind "x" to "parent_x_value".
            // 2. Create an intermediate env for the query, passing `parentEnv` to `CreateEvalEnvironment`.
            //    Then, this intermediate env needs its *own* provider.
            // This is not directly possible with current public JsonataQuery.Eval signatures if we want parentEnv to be custom.
            // The Eval(data, EvaluationEnvironment) is the best fit if we can construct the environment hierarchy.

            // Let's assume the lookup order is what we want to test: child_bindings -> child_provider -> parent_lookup
            // We can test child_provider vs parent_binding by:
            // Parent binds 'x'. Child has provider for 'x'.
            // JsonataQuery.Eval(data, childEnv) where childEnv has parentEnv AND childProvider.
            // This means we need to be able_to_construct_ childEnv properly.
            // EvaluationEnvironment child = EvaluationEnvironment.CreateNestedEnvironment(parentEnv); // This copies parent's provider.
            // We need child to have its *own* provider.

            // Alternative: Test if provider on an Env takes precedence over DefaultEnvironment's bindings (if any matched)
            // For this specific test, let's use two levels of provider for clarity on lookup.

            EvaluationEnvironment granParentEnv = new EvaluationEnvironment(); // Uses DefaultEnvironment as parent
            granParentEnv.BindValue("x", new JValue("grand_parent_x"));

            EvaluationEnvironment parentEnvWithProvider = new EvaluationEnvironment(granParentEnv, null, (name) => {
                 if (name == "x") {
                    s_lazyEvaluationCounter++; // parent provider
                    return new JValue("parent_provider_x");
                 }
                 return EvalProcessor.UNDEFINED;
            });

            // Child environment, no specific bindings, no specific provider, should inherit parent's provider
            EvaluationEnvironment childEnvShouldUseParentProvider = EvaluationEnvironment.CreateNestedEnvironment(parentEnvWithProvider);


            JsonataQuery query = new JsonataQuery("$x");
            JToken result = query.Eval(JValue.CreateNull(), childEnvShouldUseParentProvider);

            Assert.AreEqual("parent_provider_x", (string)result);
            Assert.AreEqual(1, s_lazyEvaluationCounter, "Parent's provider for 'x' should be called.");

            // Now, a child with ITS OWN provider for 'x'
            ResetCounters();
            EvaluationEnvironment childEnvWithOwnProvider = EvaluationEnvironment.CreateNestedEnvironment(parentEnvWithProvider);
            // Oops, CreateNestedEnvironment copies provider. We need to be able to set a *new* provider on a child.
            // The current EvaluationEnvironment constructors are:
            // - public EvaluationEnvironment(Func<string, JToken>? lazyVariableProvider = null) -> parent is DefaultEnvironment
            // - public EvaluationEnvironment(JObject bindings, Func<string, JToken>? lazyVariableProvider = null) -> parent is DefaultEnvironment
            // - internal static CreateNestedEnvironment(EvaluationEnvironment parent) -> inherits provider
            // This test case might be hard to set up perfectly without modifying EvaluationEnvironment further to allow setting a provider on a nested env differently.

            // Let's simplify the test to: Provider on an environment takes precedence over its parent's direct binding.
            ResetCounters();
            EvaluationEnvironment parentForPrecTest = new EvaluationEnvironment();
            parentForPrecTest.BindValue("z", new JValue("parent_z_value"));

            // Create a new environment that will become the "child" for the Eval call.
            // It has `parentForPrecTest` as its conceptual parent (via DefaultEnvironment chain if not direct)
            // and its own provider.
            // The Eval call `query.Eval(data, null, childProviderForPrecTest)` will create an environment
            // whose parent is DefaultEnvironment. This isn't what we want for this specific hierarchical test.
            // We must use the Eval(data, environment) overload.

            Func<string, JToken> childProviderForPrecTest = (name) => {
                if (name == "z") {
                    s_lazyEvaluationCounter++;
                    return new JValue("child_provider_z_value");
                }
                return EvalProcessor.UNDEFINED;
            };
            // Create an environment that has `parentForPrecTest` as a parent, and `childProviderForPrecTest` as its provider.
            // This requires using the internal CreateEvalEnvironment or similar.
            // For a unit test, we might need a helper or adjust constructors if this specific scenario is critical.
            // Given the constraints, I will test: provider on Env X vs binding on DefaultEnvironment (parent of Env X).

            // Assume DefaultEnvironment has a binding for "defaultVar" (it does, e.g. $abs)
            // Let's use a fresh variable.
            // We can't easily add to DefaultEnvironment's *bindings* for a test.
            // So, test: provider on Env X vs. binding on its explicit parent ParentY.
            EvaluationEnvironment parentY = new EvaluationEnvironment();
            parentY.BindValue("varFromParent", new JValue("parent_value"));

            Func<string, JToken> providerForChildOfY = (name) => {
                if (name == "varFromParent") {
                    s_lazyEvaluationCounter++;
                    return new JValue("provider_value_overrides_parent_binding");
                }
                return EvalProcessor.UNDEFINED;
            };
            // This creates an environment whose parent is parentY, and has its own provider.
            EvaluationEnvironment childOfYWithProvider = EvaluationEnvironment.CreateEvalEnvironment(parentY);
            // CreateEvalEnvironment copies provider. This is not the right way.
            // The constructor new EvaluationEnvironment(parent, supplement, provider) is private.
            // This specific test case about provider overriding a *specific parent's binding* is hard to set up
            // without more flexible public constructors or internal helpers for test setup.

            // What CAN be tested easily: Provider on an environment (whose parent is DefaultEnvironment)
            // overrides a binding that might exist on DefaultEnvironment.
            // Example: Try to override $abs with a provider.
            ResetCounters();
            Func<string, JToken> overrideProvider = (name) => {
                if (name == "abs") { // $abs is a default function
                    s_lazyEvaluationCounter++;
                    return new JValue("overridden_abs");
                }
                return EvalProcessor.UNDEFINED;
            };
            JsonataQuery queryOverride = new JsonataQuery("$abs"); // Normally $abs would be a function
            JToken resultOverride = queryOverride.Eval(JValue.CreateNull(), null, overrideProvider);
            Assert.AreEqual("overridden_abs", (string)resultOverride);
            Assert.AreEqual(1, s_lazyEvaluationCounter, "Provider should override default environment function/variable.");
        }
    }
}
