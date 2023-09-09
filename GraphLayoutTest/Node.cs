using System.Numerics;
using System.Text.Json;
using Jering.Javascript.NodeJS;

namespace Test;

public class Node
{
    public List<Node> ConnectedNodes { get; set; } = new();
    public Vector2 Position { get; set; }
    public string Id { get; } = Guid.NewGuid().ToString();

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is Node node && node.Id == Id;
    }
}

public class Layouter
{
    private readonly List<Node> _nodes;

    public Layouter(List<Node> nodes)
    {
        _nodes = nodes;
    }

    public async Task Layout()
    {
        var nodes = _nodes.Select(x => new ParameterNode
        {
            Data = new ParameterNodeData
            {
                Id = x.Id
            }
        }).ToList();

        var edges = _nodes.SelectMany(x => x.ConnectedNodes.Select(y => new Connection(x, y)))
            .Distinct()
            .Select(x => new ParameterEdge
            {
                Data = new ParameterEdgeData
                {
                    Id = x.A.Id + x.B.Id,
                    Source = x.A.Id,
                    Target = x.B.Id
                }
            }).ToList();

        // Create a Parameter object and populate it with nodes and edges
        var parameter = new Parameter
        {
            Nodes = nodes,
            Edges = edges
        };

        var res = await StaticNodeJSService.InvokeFromFileAsync<List<ResultNode>>("index.js", args: new object[] { parameter } );

        foreach (var resultNode in res!)
        {
            _nodes.First(x => x.Id == resultNode.Id).Position = new Vector2((float)resultNode.X, (float)resultNode.Y);
        }
    }
}

public class Parameter
{
    public List<ParameterNode> Nodes { get; set; }
    public List<ParameterEdge> Edges { get; set; }
}

public class ParameterEdge
{
    public ParameterEdgeData Data { get; set; }
}

public class ParameterEdgeData
{
    public string Id { get; set; }
    public string Source { get; set; }
    public string Target { get; set; }
}

public class ParameterNode
{
    public ParameterNodeData Data { get; set; }
}

public class ParameterNodeData
{
    public string Id { get; set; }
}

public class ResultNode
{
    public string Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}
