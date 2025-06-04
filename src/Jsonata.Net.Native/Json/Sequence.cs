namespace Jsonata.Net.Native.Json;

internal sealed class Sequence : JArray
{
    public bool keepSingletons;
    public bool outerWrapper;

    public Sequence()
    {
    }

    public JToken Simplify()
    {
        if (this.ChildrenTokens.Count == 0)
        {
            return JValue.Undefined;
        }
        else if (this.ChildrenTokens.Count == 1 && !this.keepSingletons)
        {
            return this.ChildrenTokens[0];
        }
        else
        {
            return this;
        }
    }

    protected override JArray DeepCloneArrayNoChildren()
    {
        return new Sequence()
        {
            keepSingletons = this.keepSingletons,
            outerWrapper = this.outerWrapper
        };
    }
}
