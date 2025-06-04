namespace Jsonata.Net.Native.Json;

/** an analogue of 
 Object.defineProperty(result, 'cons', {
                    enumerable: false,
                    configurable: false,
                    value: true
                })
*/
internal sealed class ExplicitArray : JArray
{
    public ExplicitArray()
    {
    }

    protected override JArray DeepCloneArrayNoChildren()
    {
        return new ExplicitArray();
    }
}
