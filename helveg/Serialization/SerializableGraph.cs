using System;
using System.Numerics;
using Helveg.Render;

namespace Helveg.Serialization
{
    public class SerializableGraph
    {
        public DateTime TimeStamp { get; set; }

        public Vector2[]? Positions { get; set; }

        public float[]? Weights { get; set; }

        public string[]? Labels { get; set; }

        public float[]? Sizes { get; set; }

        public string? Name { get; set; }

        public static SerializableGraph FromGraph(Graph graph, DateTime timeStamp)
        {
            return new SerializableGraph
            {
                Name = graph.Name,
                TimeStamp = timeStamp,
                Positions = graph.Positions,
                Weights = graph.Weights,
                Labels = graph.Labels,
                Sizes = graph.Sizes
            };
        }

        public Graph ToGraph()
        {
            if (TimeStamp == default || Positions is null || Weights is null || Labels is null || Sizes is null
                || Name is null)
            {
                throw new NullReferenceException();
            }

            return new Graph(Name, Positions, Weights, Labels, Sizes);
        }
    }
}
