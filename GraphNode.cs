using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SchoolBusRoute
{
    class GraphNode : IEquatable<GraphNode>
    {
        public int Value { get; private set; }//stores the node's number
        public int Cost { get; private set; }
        public GraphNode(int value, int cost)
        {
            Value = value;
            Cost = cost;
        }
        public GraphNode(int node)
        {
            Value = node;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GraphNode);
        }

        public bool Equals(GraphNode other)
        {
            return other != null &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
}
