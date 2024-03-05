using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
public class MakePath : MonoBehaviour
{

    struct Node
    {
        public bool root, attractor;
    }

    GameObject obj;
    Texture2D tex;

    Color[,] pixels; // use a 2d array for ease
    Node[,] nodes;

    int numAttractors = 50;

    void Start()
    {
        tex = (Texture2D)obj.GetComponent<MeshRenderer>().material.mainTexture;
        Color[] pixelTemp = tex.GetPixels();
        pixels = new Color[tex.width,tex.height];
        nodes = new Node[tex.width, tex.height];
        for (int i = 0; i < tex.width; i++)
        {
            for(int j = 0; j < tex.height; j++)
            {
                pixels[j, i] = pixelTemp[i * tex.width + j];
            }
        }
    }

    private void PlaceRootNodes(int r) // for now, just start with only one root node in bottom left
    {
        nodes[0, 0].root = true;
        nodes[0, 0].attractor = false;
    }

    private void PlaceAttractionPoints() // arbitrary placement for now
    {
        // using someone elses code for now
        List<Vector2> points = GeneratePoints(100, new Vector2(tex.width, tex.height), 30);

        for(int i = 0; i < points.Count; i++)
        {
            if (!nodes[(int)points[i].x, (int)points[i].y].root)
            {
                nodes[(int)points[i].x, (int)points[i].y].attractor = true;
            }
        }
    }

    private void CreatePath()
    {

    }

    // NOT MY CODE
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {
        float cellSize = radius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(sampleRegionSize / 4);
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = UnityEngine.Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + dir * UnityEngine.Random.Range(radius, 2 * radius);
                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }

        }

        return points;
    }

    static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
        {
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }
}
