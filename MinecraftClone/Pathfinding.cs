using System.Collections;

namespace RayLib3dTest;

public unsafe class Node
{
    public Node(IntVector3 pos)
    {
        this.Position = pos;
    }

    public IntVector3 Position;
    public int GCost;
    public int HCost;
    public int FCost() => (int)(GCost + 1.5f * HCost);
    public Node? Parent;
}

public class Pathfinding
{

    /*
     * G: distance to start
     * H: distance to end
     * F: G + H
     */

    public static IntVector3[] PathFind(IntVector3 start, IntVector3 end)
    {
        if (start == end)
            return [];

        const int maxFCost = 50;

        World world = CurrentWorld;

        var openNodes = new Dictionary<IntVector3, Node>();
        var closedNodes = new Dictionary<IntVector3, Node>();

        openNodes.Add(start, new Node(start));

        while (true)
        {
            if (openNodes.Count == 0)
                return [];

            var current = openNodes.MinBy(x => x.Value.FCost()).Value;
            openNodes.Remove(current.Position);
            closedNodes.Add(current.Position, current);

            if (current.Position == end)
            {
                return GetFinalPath(current);
            }

            if (current.FCost() > 500)
            {
                return GetFinalPath(closedNodes.Where(x => x.Value.HCost != 0).MinBy(x => x.Value.HCost).Value);
            }

            foreach (var offset in PathFindingNeighbours)
            {
                var neighbourPos = current.Position + offset.Offset;
                if(!IsWalkable(current.Position, offset, world) || closedNodes.ContainsKey(neighbourPos))
                    continue;

                var neighbour = new Node(neighbourPos);

                var newGCost = current.GCost + GetDistance(current.Position, neighbour.Position);

                if (!openNodes.ContainsKey(neighbour.Position) || newGCost < neighbour.GCost)
                {
                    neighbour.GCost = newGCost;
                    neighbour.HCost = GetDistance(neighbour.Position, end);
                    neighbour.Parent = current;
                    if(!openNodes.ContainsKey(neighbour.Position))
                        openNodes.Add(neighbour.Position, neighbour);
                }
            }
        }
    }

    private static IntVector3[] GetFinalPath(Node current)
    {
        List<IntVector3> pathElements = [];
        while (current.Parent != null)
        {
            pathElements.Add(current.Position);
            current = current.Parent;
        }

        pathElements.Reverse();
        return pathElements.ToArray();
    }

    private static bool IsWalkable(IntVector3 origin, Neighbour neighbour, World world)
    {
        var pos = origin + neighbour.Offset;

        if(IsSolid(pos, world))
            return false;

        //check above
        if(IsSolid(pos with { Y = pos.Y + 1}, world))
            return false;

        //check below
        if(!IsSolid(pos with { Y = pos.Y - 1} , world))
            return false;

        if (neighbour.IsDiagonalHorizontal)
        {
            if(IsSolid(origin + neighbour.Offset with { X = 0}, world))
                return false;

            if(IsSolid(origin + neighbour.Offset with { Z = 0}, world))
                return false;
        }

        if (neighbour.Offset.Y == -1)
        {
            if(IsSolid(pos with { Y = pos.Y + 2}, world))
                return false;
        }

        if (neighbour.Offset.Y == +1)
        {
            if(IsSolid(origin with { Y = origin.Y + 2}, world))
                return false;
        }

        return true;
    }

    private static bool IsSolid(IntVector3 pos, World world)
    {
        var below = world.TryGetBlockAtPos(pos, out var belowFound);
        if(!belowFound || below.IsAir())
            return false;
        return true;
    }

    private static int GetDistance(IntVector3 a, IntVector3 b)
    {
        return (int)Vector3.Distance(a.ToVector3() * 10, b.ToVector3() * 10);
    }

    //left, right, forward, backwards,
    //forwardLeft, forwardRight, backwardsLeft, backwardRight
    //leftUp, rightUp, forwardUp, backwardsUp
    //leftDown, rightDown, forwardDown, backwardsDow
    private static Neighbour[] PathFindingNeighbours = [
        new Neighbour(new(0, 0, 1)), new (new(0, 0, -1)), new(new(1, 0, 0)), new(new(-1, 0, 0)),
        new Neighbour(new(1, 0, 1), true), new (new(1, 0, -1), true), new(new(-1, 0, 1), true), new(new(-1, 0, -1), true),
        new Neighbour(new(0, 1, 1)), new (new(0, 1, -1)), new(new(1, 1, 0)), new(new(-1, 1, 0)),
        new Neighbour(new(0, -1, 1)), new (new(0, -1, -1)), new(new(1, -1, 0)), new(new(-1, -1, 0)),
    ];

    public static void Visualize(IntVector3[] path)
    {
        DevTools.RenderActions.Add(() =>
        {
            foreach (var intVector3 in path)
            {
                DrawCubeWiresV(intVector3.ToVector3(), Vector3.One * 1.001f, Color.YELLOW);
            }
        });
    }
}

public record struct Neighbour(IntVector3 Offset, bool IsDiagonalHorizontal = false);
