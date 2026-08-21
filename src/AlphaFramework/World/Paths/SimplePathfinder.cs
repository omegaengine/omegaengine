/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SlimDX;

namespace AlphaFramework.World.Paths;

/// <summary>
/// A simple A* pathfinder.
/// </summary>
public class SimplePathfinder : IPathfinder<Vector2>
{
    private record struct Node
    {
        public int F, G, H;
        public Vector2 Position, Parent;
    }

    private static readonly Vector2[] _direction =
    [
        new(0, -1), new(1, 0), new(0, 1), new(-1, 0),
        new(1, -1), new(1, 1), new(-1, 1), new(-1, -1)
    ];

    private readonly bool[,] _obstructionMap;

    /// <summary>
    /// Initializes a new pathfinder.
    /// </summary>
    /// <param name="obstructionMap">A 2D map of obstructed (untraversable) fields.</param>
    public SimplePathfinder(bool[,] obstructionMap)
    {
        _obstructionMap = obstructionMap ?? throw new ArgumentNullException(nameof(obstructionMap));
    }

    /// <inheritdoc/>
    public IEnumerable<Vector2>? FindPath(Vector2 start, Vector2 target)
    {
        var roundedStart = new Vector2((int)start.X, (int)start.Y);
        var roundedTarget = new Vector2((int)target.X, (int)target.Y);
        var goneLockup = new int[_obstructionMap.GetLength(0), _obstructionMap.GetLength(1)];

        // Positions outside the map are unreachable
        if (roundedStart.X < 0 || roundedStart.Y < 0
         || roundedStart.X >= _obstructionMap.GetLength(0) || roundedStart.Y >= _obstructionMap.GetLength(1)
         || roundedTarget.X < 0 || roundedTarget.Y < 0
         || roundedTarget.X >= _obstructionMap.GetLength(0) || roundedTarget.Y >= _obstructionMap.GetLength(1))
            return null;

        if (_obstructionMap[(int)roundedTarget.X, (int)roundedTarget.Y]) return null;

        bool pathFound = false;
        var path = new Stack<Vector2>();

        // Keep the working lists local, so that concurrent or nested calls don't corrupt each other
        List<Node> openList = [], closeList = [];

        var parentNode = GetParentNode(roundedStart, roundedTarget);
        openList.Add(parentNode);

        Node nextNode;
        while (openList.Count != 0)
        {
            openList.Remove(parentNode);
            closeList.Add(parentNode);

            if (parentNode.Position == roundedTarget)
            {
                pathFound = true;
                break;
            }

            for (int i = 0; i < 8; i++)
            {
                nextNode = new() {Position = parentNode.Position + _direction[i]};
                if ((nextNode.Position.X < 0) || (nextNode.Position.Y < 0)
                                              || (nextNode.Position.X >= _obstructionMap.GetLength(0)) || (nextNode.Position.Y >= _obstructionMap.GetLength(1)))
                    continue;
                if (_obstructionMap[(int)nextNode.Position.X, (int)nextNode.Position.Y])
                    continue;

                nextNode.G = parentNode.G + (i > 3 ? 14 : 10);
                nextNode.H = 10 * (int)(nextNode.Position - roundedTarget).Length();
                nextNode.F = nextNode.G + nextNode.H;
                nextNode.Parent = parentNode.Position;

                if (goneLockup[(int)nextNode.Position.X, (int)nextNode.Position.Y] != 0)
                {
                    if (goneLockup[(int)nextNode.Position.X, (int)nextNode.Position.Y] > nextNode.G)
                    {
                        for (int x = 0; x < openList.Count; x++)
                        {
                            if (openList[x].Position.Equals(nextNode.Position))
                            {
                                openList[x] = nextNode;
                                goneLockup[(int)nextNode.Position.X, (int)nextNode.Position.Y] = nextNode.G;
                            }
                        }
                    }
                }
                else
                {
                    openList.Add(nextNode);
                    goneLockup[(int)nextNode.Position.X, (int)nextNode.Position.Y] = nextNode.G;
                }
            }

            if (openList.Count == 0) continue;

            parentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (parentNode.F > openList[i].F)
                    parentNode = openList[i];
            }
        }

        if (pathFound)
        {
            path.Push(target);

            nextNode = closeList[^1];
            while (!(nextNode.Parent.Equals(nextNode.Position)))
            {
                path.Push(nextNode.Position);

                int previousCount = closeList.Count;
                foreach (var node in closeList)
                {
                    if (node.Position.Equals(nextNode.Parent))
                    {
                        nextNode = node;
                        closeList.Remove(nextNode);
                        break;
                    }
                }

                // Give up instead of looping forever if the chain of parent nodes is broken
                if (closeList.Count == previousCount) return null;
            }
            return path;
        }
        else return null;
    }

    private static Node GetParentNode(Vector2 start, Vector2 end)
    {
        var parentNode = new Node {G = 1, H = 10 * (int)(Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y))};
        parentNode.F = parentNode.G + parentNode.H;
        parentNode.Position = start;
        parentNode.Parent = start;
        return parentNode;
    }
}
