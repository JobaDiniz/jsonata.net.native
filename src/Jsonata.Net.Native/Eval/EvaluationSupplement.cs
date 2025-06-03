using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jsonata.Net.Native.Eval;
//created once for each EvalProcessor.EvaluateJson call
public sealed class EvaluationSupplement
{
    private readonly Lazy<Random> random = new Lazy<Random>();
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;

    internal Random Random => this.random.Value;
    internal DateTimeOffset Now => this.now;
}
