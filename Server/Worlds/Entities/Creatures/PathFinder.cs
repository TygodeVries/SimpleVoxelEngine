using Shared.Mathf;
using Shared.Worlds;

namespace Server.Worlds;

public class PathFinder
{
    private readonly World world;

    public PathFinder(World world)
    {
        this.world = world;
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 goal)
    {
        Vector3 startBlock = FloorVector(start);
        Vector3 goalBlock = FloorVector(goal);

        var open = new PriorityQueue<Node, int>();
        var nodes = new Dictionary<Vector3, Node>();

        // If the start isn't walkable, find the closest walkable block.
        if (!CanWalk(startBlock))
        {
            Vector3? closestStart = FindClosestWalkable(startBlock);

            if (closestStart == null)
                return new List<Vector3>();

            startBlock = closestStart.Value;
        }

        Node startNode = new(
            startBlock,
            null,
            0,
            GetHeuristic(startBlock, goalBlock)
        );

        open.Enqueue(startNode, startNode.F);
        nodes[startBlock] = startNode;

        // Always keep track of the node closest to the goal.
        Node closestNode = startNode;

        while (open.Count > 0)
        {
            Node current = open.Dequeue();

            // Ignore stale nodes.
            if (current.G > nodes[current.Position].G)
                continue;

            // Track the node closest to the goal.
            if (current.H < closestNode.H)
            {
                closestNode = current;
            }

            // Goal reached.
            if (current.Position == goalBlock)
                return BuildPath(current);

            foreach (Vector3 neighborPosition in GetNeighbors(current.Position))
            {
                if (!CanWalk(neighborPosition))
                    continue;

                int cost = current.G + 1;

                if (nodes.TryGetValue(neighborPosition, out Node? existing) &&
                    cost >= existing.G)
                {
                    continue;
                }

                Node neighbor = new(
                    neighborPosition,
                    current,
                    cost,
                    GetHeuristic(neighborPosition, goalBlock)
                );

                nodes[neighborPosition] = neighbor;
                open.Enqueue(neighbor, neighbor.F);
            }
        }

        // Goal was unreachable.
        // Return the path to the closest reachable point instead.
        return BuildPath(closestNode);
    }

    private bool CanWalk(Vector3 position)
    {
        var blockAtFeet = world.GetBlockAt(position);
        var blockAtHead = world.GetBlockAt(
            position + new Vector3(0, 1, 0)
        );
        var blockBelowFeet = world.GetBlockAt(
            position + new Vector3(0, -1, 0)
        );

        return !blockAtFeet.Solid &&
               !blockAtHead.Solid &&
               blockBelowFeet.Solid;
    }

    private Vector3? FindClosestWalkable(Vector3 start)
    {
        // Search outward in increasing radius.
        const int maxRadius = 16;

        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        // Only check the outer shell of this radius.
                        if (Math.Abs(x) != radius &&
                            Math.Abs(y) != radius &&
                            Math.Abs(z) != radius)
                        {
                            continue;
                        }

                        Vector3 position = start + new Vector3(x, y, z);

                        if (CanWalk(position))
                            return position;
                    }
                }
            }
        }

        return null;
    }

    private IEnumerable<Vector3> GetNeighbors(Vector3 position)
    {
        Vector3[] directions =
        {
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1)
    };

        foreach (Vector3 direction in directions)
        {
            // Normal movement.
            Vector3 flat = position + direction;

            if (CanWalk(flat))
                yield return flat;

            // Step up one block.
            Vector3 stepUp = position + direction + new Vector3(0, 1, 0);

            if (CanStepUp(position, direction))
                yield return stepUp;

            // Step down one block.
            Vector3 stepDown = position + direction + new Vector3(0, -1, 0);

            if (CanStepDown(position, direction))
                yield return stepDown;
        }
    }
    private bool CanStepUp(Vector3 position, Vector3 direction)
    {
        // Block directly in front of us must be solid.
        Vector3 blockToStepOnto = position + direction;

        // The destination must have empty feet/head space.
        Vector3 destination = position + direction + new Vector3(0, 1, 0);

        Vector3 destinationHead = destination + new Vector3(0, 1, 0);

        var stepBlock = world.GetBlockAt(blockToStepOnto);
        var destinationBlock = world.GetBlockAt(destination);
        var destinationHeadBlock = world.GetBlockAt(destinationHead);

        return stepBlock.Solid &&
               !destinationBlock.Solid &&
               !destinationHeadBlock.Solid;
    }

    private bool CanStepDown(Vector3 position, Vector3 direction)
    {
        Vector3 destination = position + direction + new Vector3(0, -1, 0);

        return CanWalk(destination);
    }


    private static int GetHeuristic(Vector3 a, Vector3 b)
    {
        int x = (int)Math.Abs(a.X - b.X);
        int y = (int)Math.Abs(a.Y - b.Y);
        int z = (int)Math.Abs(a.Z - b.Z);

        return x + (y * 4) + z;
    }


    private static Vector3 FloorVector(Vector3 v)
    {
        return new Vector3(
            MathF.Floor(v.X),
            MathF.Floor(v.Y),
            MathF.Floor(v.Z)
        );
    }

    private static List<Vector3> BuildPath(Node node)
    {
        var path = new List<Vector3>();

        while (node != null)
        {
            path.Add(
                node.Position +
                new Vector3(0.5f, 0f, 0.5f)
            );

            node = node.Parent;
        }

        path.Reverse();
        return path;
    }

    private sealed class Node
    {
        public Vector3 Position { get; }
        public Node? Parent { get; }
        public int G { get; }
        public int H { get; }
        public int F => G + H;

        public Node(
            Vector3 position,
            Node? parent,
            int g,
            int h)
        {
            Position = position;
            Parent = parent;
            G = g;
            H = h;
        }
    }
}
