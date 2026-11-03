/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace OmegaEngine.Graphics.Meshes;

/// <summary>
/// Reorders faces to make good use of the GPU's post-transform vertex cache.
/// </summary>
/// <remarks>Implements Tom Forsyth's "Linear-Speed Vertex Cache Optimisation".</remarks>
internal static class VertexCacheOptimizer
{
    private const int CacheSize = 32;
    private const float CacheDecayPower = 1.5f, LastFaceScore = 0.75f, ValenceBoostScale = 2.0f, ValenceBoostPower = 0.5f;

    /// <summary>
    /// Reorders faces in place.
    /// </summary>
    /// <param name="indexes">The vertex indexes of all faces in the mesh (three per face).</param>
    /// <param name="faces">The indexes of the faces to reorder.</param>
    public static void Optimize(int[] indexes, Span<int> faces)
    {
        int faceCount = faces.Length;
        if (faceCount < 2) return;

        // Map the vertexes used by these faces to a compact local range
        var localIds = new Dictionary<int, int>();
        var corners = new int[faceCount * 3];
        for (int f = 0; f < faceCount; f++)
        {
            for (int c = 0; c < 3; c++)
            {
                int vertex = indexes[faces[f] * 3 + c];
                if (!localIds.TryGetValue(vertex, out int local)) localIds.Add(vertex, local = localIds.Count);
                corners[f * 3 + c] = local;
            }
        }
        int vertexCount = localIds.Count;

        // Faces using each vertex
        var valence = new int[vertexCount];
        foreach (int vertex in corners) valence[vertex]++;
        var vertexFaceStart = new int[vertexCount + 1];
        for (int v = 0; v < vertexCount; v++) vertexFaceStart[v + 1] = vertexFaceStart[v] + valence[v];
        var vertexFaces = new int[corners.Length];
        var fill = (int[])vertexFaceStart.Clone();
        for (int i = 0; i < corners.Length; i++) vertexFaces[fill[corners[i]]++] = i / 3;

        var cachePosition = new int[vertexCount];
        Array.Fill(cachePosition, -1);
        var vertexScore = new float[vertexCount];
        for (int v = 0; v < vertexCount; v++) vertexScore[v] = Score(-1, valence[v]);

        var faceScore = new float[faceCount];
        for (int f = 0; f < faceCount; f++) faceScore[f] = vertexScore[corners[f * 3]] + vertexScore[corners[f * 3 + 1]] + vertexScore[corners[f * 3 + 2]];

        var added = new bool[faceCount];
        var cache = new List<int>(CacheSize + 3);
        var newCache = new List<int>(CacheSize + 3);
        var result = new int[faceCount];
        int nextUnadded = 0;

        int bestFace = -1;
        for (int outputCount = 0; outputCount < faceCount; outputCount++)
        {
            if (bestFace < 0)
            { // No candidate from the cache, so pick the best remaining face
                float bestScore = float.MinValue;
                while (added[nextUnadded]) nextUnadded++;
                for (int f = nextUnadded; f < faceCount; f++)
                {
                    if (!added[f] && faceScore[f] > bestScore)
                    {
                        bestScore = faceScore[f];
                        bestFace = f;
                    }
                }
            }

            result[outputCount] = faces[bestFace];
            added[bestFace] = true;

            // Put the face's vertexes at the front of the cache
            newCache.Clear();
            for (int c = 0; c < 3; c++)
            {
                int vertex = corners[bestFace * 3 + c];
                valence[vertex]--;
                if (!newCache.Contains(vertex)) newCache.Add(vertex);
            }
            foreach (int vertex in cache)
                if (!newCache.Contains(vertex)) newCache.Add(vertex);

            // Update scores of the cached vertexes and of those pushed out
            for (int i = 0; i < newCache.Count; i++)
            {
                int vertex = newCache[i];
                cachePosition[vertex] = i < CacheSize ? i : -1;
                vertexScore[vertex] = Score(cachePosition[vertex], valence[vertex]);
            }

            // Rescore the faces using these vertexes and pick the best one as the next candidate
            bestFace = -1;
            float bestCandidateScore = float.MinValue;
            foreach (int vertex in newCache)
            {
                for (int i = vertexFaceStart[vertex]; i < vertexFaceStart[vertex + 1]; i++)
                {
                    int f = vertexFaces[i];
                    if (added[f]) continue;
                    faceScore[f] = vertexScore[corners[f * 3]] + vertexScore[corners[f * 3 + 1]] + vertexScore[corners[f * 3 + 2]];
                    if (faceScore[f] > bestCandidateScore)
                    {
                        bestCandidateScore = faceScore[f];
                        bestFace = f;
                    }
                }
            }

            if (newCache.Count > CacheSize) newCache.RemoveRange(CacheSize, newCache.Count - CacheSize);
            (cache, newCache) = (newCache, cache);
        }

        result.CopyTo(faces);
    }

    private static float Score(int cachePosition, int valence)
    {
        if (valence <= 0) return -1; // No faces left to use this vertex

        float score = 0;
        if (cachePosition >= 0)
        {
            score = cachePosition < 3
                ? LastFaceScore // Used by the last face, so using it again directly gains little
                : MathF.Pow(1 - (cachePosition - 3) / (float)(CacheSize - 3), CacheDecayPower);
        }

        // Prefer vertexes with few remaining faces, to get rid of them early
        return score + ValenceBoostScale * MathF.Pow(valence, -ValenceBoostPower);
    }
}
