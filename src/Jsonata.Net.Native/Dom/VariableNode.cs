using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jsonata.Net.Native.Dom
{
    /// <summary>
    /// Represents a variable reference expression in a JSONata query.
    /// Contains the variable name to be resolved from the evaluation context.
    /// </summary>
    internal sealed class VariableNode : Node
    {
        public string name { get; }

        public VariableNode(string name)
        {
            this.name = name;
        }

        internal override Node optimize()
        {
            return this;
        }

        public override string ToString()
        {
            return "$" + this.name;
        }

        protected override bool EqualsSpecific(Node other)
        {
            VariableNode otherNode = (VariableNode)other;
            return otherNode.name == this.name;
        }
    }
}
