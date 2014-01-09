/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using Common;
using SlimDX;

namespace AlphaFramework.World.Paths
{
    /// <summary>
    /// A simple A* pathfinder.
    /// </summary>
    public class SimplePathfinder : IPathfinder<Vector2>
    {
        private struct Node
        {
            public int F, G, H;
            public Vector2 Position, Parent;
        }

        private static readonly Vector2[] _direction =
        {
            new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0),
            new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(-1, -1)
        };

        private readonly bool[,] _blockMap;
        private readonly List<Node> _openList = new List<Node>(), _closeList = new List<Node>();

        /// <summary>
        /// Initializes a new pathfinder.
        /// </summary>
        /// <param name="blockMap">A 2D map of blocked fields.</param>
        public SimplePathfinder(bool[,] blockMap)
        {
            _blockMap = blockMap;
        }

        /// <inheritdoc/>
        public IEnumerable<Vector2> FindPathPlayer(Vector2 start, Vector2 target)
        {
            start.X = (int)start.X;
            start.Y = (int)start.Y;
            target.X = (int)target.X;
            target.Y = (int)target.Y;
            var goneLockup = new int[_blockMap.GetLength(0), _blockMap.GetLength(1)];

            using (new TimedLogEvent("Calculating path"))
            {
                bool pathFound = false;
                var path = new List<Vector2>();

                _openList.Clear();
                _closeList.Clear();
                var parentNode = GetParentNode(start, target);
                _openList.Add(parentNode);

                Node nextNode;
                while (_openList.Count != 0)
                {
                    _openList.Remove(parentNode);
                    _closeList.Add(parentNode);

                    if (parentNode.Position == target)
                    {
                        pathFound = true;
                        break;
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        nextNode = new Node {Position = parentNode.Position + _direction[i]};
                        if ((nextNode.Position.X < 0) || (nextNode.Position.Y < 0)
                            || (nextNode.Position.X >= _blockMap.GetLength(0)) || (nextNode.Position.Y >= _blockMap.GetLength(1)))
                            continue;
                        if (_blockMap[(int)nextNode.Position.X, (int)nextNode.Position.Y])
                            continue;

                        nextNode.G = parentNode.G + (i > 3 ? 14 : 10);
                        nextNode.H = 10 * (int)(nextNode.Position - target).Length();
                        nextNode.F = nextNode.G + nextNode.H;
                        nextNode.Parent = parentNode.Position;

                        if (goneLockup[(int)nextNode.Position.X, (int)nextNode.Position.Y] != 0)
                        {
                            if (goneLockup[(int)nextNode.Position.X, (int)nextNode.Position.Y] > nextNode.G)
                            {
                                for (int x = 0; x < _openList.Count; x++)
                                {
                                    if (_openList[x].Position.Equals(nextNode.Position))
                                    {
                                        _openList[x] = nextNode;
                                        goneLockup[(int)nextNode.Position.X, (int)nextNode.Position.Y] = nextNode.G;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _openList.Add(nextNode);
                            goneLockup[(int)nextNode.Position.X, (int)nextNode.Position.Y] = nextNode.G;
                        }
                    }

                    if (_openList.Count == 0) continue;

                    parentNode = _openList[0];
                    for (int i = 1; i < _openList.Count; i++)
                    {
                        if (parentNode.F > _openList[i].F)
                            parentNode = _openList[i];
                    }
                }

                if (pathFound)
                {
                    nextNode = _closeList[_closeList.Count - 1];
                    while (!(nextNode.Parent.Equals(nextNode.Position)))
                    {
                        path.Add(nextNode.Position);
                        foreach (var node in _closeList)
                        {
                            if (node.Position.Equals(nextNode.Parent))
                            {
                                nextNode = node;
                                _closeList.Remove(nextNode);
                                break;
                            }
                        }
                    }
                    return path;
                }
                else return null;
            }
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
}
