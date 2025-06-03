using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jsonata.Net.Native.Dom;

internal abstract class NumberNode : Node
{
    public abstract int GetIntValue();
}
