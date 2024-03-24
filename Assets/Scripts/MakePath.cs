using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.Mathematics;
using UnityEditor.ShaderKeywordFilter;
using Unity.VisualScripting;
using UnityEditor;
using TMPro;
using System.IO;
using System.Globalization;
public class MakePath : MonoBehaviour
{

    // will determine how large gaps between uv coord placements of nodes are
    int resolution = 200; //0.01 coord gaps

    int numAttractors = 6;
    //int tempNumAttractors = 6;
    int numRoots = 1;

    // We're gonna choose to store attractors and growers in a 2D array so that we may have their placement
    // line up with their position, allowing for some optomization
    Node[,] nodes;
    List<Node> attractors;
    List<Node> growers;

    public Node[,] Nodes
    {
        get { return nodes; }
    }

    public List<Node> Growers
    {
        get { return growers; }
    }

    public int Resolution
    {
        get { return resolution; }
    }

    void Start()
    {
        nodes = new Node[resolution,resolution];
        attractors = new List<Node>();
        growers = new List<Node>();
        PlaceRootNodes();
        PlaceAttractionPoints();
    }

    private void Update()
    {
        if (Input.GetKeyDown("g"))
        {
            CreatePath();
        }
        if (Input.GetKeyDown("p"))
        {
            PrintPath();
        }
        if (Input.GetKeyDown("c"))
        {
            CreateFullPath();
        }
        if (Input.GetKeyDown("w"))
        {
            WritePath();
        }
    }

    private void PlaceRootNodes() // for now, just start with only one root node in bottom left
    {
        Grower g = new Grower(new Vector2(0, resolution-1), nodes, null);
        nodes[0, resolution-1] = g;
        growers.Add(g);
    }

    private void PlaceAttractionPoints() // arbitrary placement for now
    {
        for(int i = 0; i < resolution; i++)
        {
            for(int j = 0; j < resolution; j++)
            {
                if (nodes[j,i] == null)
                {
                    int bruh = UnityEngine.Random.Range(1, 4);
                    if(bruh == 1)
                    {
                        Attractor a = new Attractor(new Vector2(j, i), nodes, 5, 2);
                        attractors.Add(a);
                        nodes[j, i] = a;
                    }
                }
            }
        }
    }

    private void CreatePath()
    {
        List<Grower> influencedNodes = new List<Grower>();
        foreach (Attractor a in attractors)
        {
            if (a.active)
            {
                // Find the closest growth node. If it's close enough to be in kill radius, kill this attractor.
                // If it's only in influence radius, affect that growth node's grow direction
                // If there are no nearby nodes, do nothing.

                (Grower, bool) info = a.GetInfluencedNode();
                Grower g = info.Item1;
                bool dying = info.Item2;
                if (g != null)
                {
                    Vector2 pushDir = new Vector2(a.pos.x - g.pos.x, a.pos.y - g.pos.y).normalized;
                    g.growthDir += pushDir;
                    g.numInfluencers++;

                    if (!g.active)
                    {
                        g.active = true;
                        influencedNodes.Add(g);
                    }
                }
                if (dying)
                {
                    nodes[(int)a.pos.x, (int)a.pos.y] = null;
                    a.active = false;
                    g.numKills++;
                }
            }
        }

        // Now we can iterate over the growers, and have them move in their grow directions
        foreach (Grower g in influencedNodes)
        {
            // Create new growth node in direction of growth direction
            //ANNOYING: round will not always round up when result is 0.5, so we gotta do this
            Vector2 finalGrowDir = new Vector2(g.growthDir.x / g.numInfluencers, g.growthDir.y / g.numInfluencers).normalized;
            int x = Mathf.RoundToInt(finalGrowDir.x);
            int y = Mathf.RoundToInt(finalGrowDir.y);
            if (g.growthDir.x / g.numInfluencers == 0.5)
            {
                x = 1;
            }
            if (g.growthDir.x / g.numInfluencers == -0.5)
            {
                x = -1;
            }
            if (g.growthDir.y / g.numInfluencers == 0.5)
            {
                y = 1;
            }
            if (g.growthDir.y / g.numInfluencers == -0.5)
            {
                y = -1;
            }
            if (x == 0 && y == 0)
            {
                Debug.Log("PROBLEM");
                Debug.Log(g.growthDir);
                Debug.Log(g.numInfluencers);
                Debug.Log(finalGrowDir);
            }

            Vector2 newGrowthPos = new Vector2(g.pos.x + x, g.pos.y + y);
            if (nodes[(int)newGrowthPos.x, (int)newGrowthPos.y] != null)
            {
                g.growthDir = Vector2.zero;
                g.numInfluencers = 0;
                g.active = false;
            }
            else
            {
                Grower leaf = new Grower(new Vector2(newGrowthPos.x, newGrowthPos.y), nodes, g);
                growers.Add(leaf);
                nodes[(int)leaf.pos.x, (int)leaf.pos.y] = leaf;

                g.growthDir = Vector2.zero;
                g.numInfluencers = 0;
                g.active = false;
            }
        }
        Debug.Log("Step");
    }

    // This is pretty much the same function as above, it just does it all at once
    public void CreateFullPath()
    {
        int growing = 0;
        while (growing < 200) //TEMPORRARY SET 
        {
            growing++;
            List<Grower> influencedNodes = new List<Grower>();
            foreach (Attractor a in attractors)
            {
                if (a.active)
                {
                    // Find the closest growth node. If it's close enough to be in kill radius, kill this attractor.
                    // If it's only in influence radius, affect that growth node's grow direction
                    // If there are no nearby nodes, do nothing.

                    (Grower, bool) info = a.GetInfluencedNode();
                    Grower g = info.Item1;
                    bool dying = info.Item2;
                    if (g != null)
                    {
                        Vector2 pushDir = new Vector2(a.pos.x - g.pos.x, a.pos.y - g.pos.y).normalized;
                        g.growthDir += pushDir;
                        g.numInfluencers++;

                        if (!g.active)
                        {
                            g.active = true;
                            influencedNodes.Add(g);
                        }
                    }
                    if (dying)
                    {
                        nodes[(int)a.pos.x, (int)a.pos.y] = null;
                        a.active = false;
                        g.numKills++;
                    }
                }
            }

            // Now we can iterate over the growers, and have them move in their grow directions
            foreach (Grower g in influencedNodes)
            {
                // Create new growth node in direction of growth direction
                //ANNOYING: round will not always round up when result is 0.5, so we gotta do this
                Vector2 finalGrowDir = new Vector2(g.growthDir.x / g.numInfluencers, g.growthDir.y / g.numInfluencers).normalized;
                int x = Mathf.RoundToInt(finalGrowDir.x);
                int y = Mathf.RoundToInt(finalGrowDir.y);
                if (g.growthDir.x / g.numInfluencers == 0.5)
                {
                    x = 1;
                }
                if (g.growthDir.x / g.numInfluencers == -0.5)
                {
                    x = -1;
                }
                if (g.growthDir.y / g.numInfluencers == 0.5)
                {
                    y = 1;
                }
                if (g.growthDir.y / g.numInfluencers == -0.5)
                {
                    y = -1;
                }
                if (x == 0 && y == 0)
                {
                    Debug.Log("PROBLEM");
                    Debug.Log(g.growthDir);
                    Debug.Log(g.numInfluencers);
                    Debug.Log(finalGrowDir);
                }

                Vector2 newGrowthPos = new Vector2(g.pos.x + x, g.pos.y + y);
                if (nodes[(int)newGrowthPos.x, (int)newGrowthPos.y] != null)
                {
                    g.growthDir = Vector2.zero;
                    g.numInfluencers = 0;
                    g.active = false;
                }
                else
                {
                    Grower leaf = new Grower(new Vector2(newGrowthPos.x, newGrowthPos.y), nodes, g);
                    growers.Add(leaf);
                    nodes[(int)leaf.pos.x, (int)leaf.pos.y] = leaf;

                    g.growthDir = Vector2.zero;
                    g.numInfluencers = 0;
                    g.active = false;
                }
            }
        }
        Debug.Log("Done making path");
        
    }

    // temporary path printer
    private void PrintPath()
    {
        string[,] tempPath = new string[resolution, resolution];
        for(int i = 0; i < resolution; i++)
        {
            for(int j = 0; j < resolution; j++)
            {
                if (nodes[j,i] != null)
                {
                    if(nodes[j, i].GetType() == typeof(Grower))
                    {
                        tempPath[j, i] = "G";
                    }
                    else if(nodes[j, i].GetType() == typeof(Attractor))
                    {
                        tempPath[j, i] = "A";
                    }
                }
                else
                {
                    tempPath[j, i] = "0";
                }
            }
        }

        for (int i = 0; i < resolution; i++)
        {
            string line = "";
            for (int j = 0; j < resolution; j++)
            {
                line += tempPath[j, i].ToString().PadLeft(4) + " ";
            }
            Debug.Log(line);
        }
    }

    private void WritePath()
    {
        string[,] tempPath = new string[resolution, resolution];
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                if (nodes[j, i] != null)
                {
                    if (nodes[j, i].GetType() == typeof(Grower))
                    {
                        tempPath[j, i] = "G";
                    }
                    else if (nodes[j, i].GetType() == typeof(Attractor))
                    {
                        tempPath[j, i] = "A";
                    }
                }
                else
                {
                    tempPath[j, i] = "0";
                }
            }
        }
        string finalOutput = "";
        string filePath = "C:\\Users\\khadd\\UnityProjects\\VineGrowthTest\\Growth5.txt";
        StreamWriter writer = new StreamWriter(filePath, true);
        for (int i = 0; i < resolution; i++)
        {
            string line = "";
            for (int j = 0; j < resolution; j++)
            {
                line += tempPath[j, i].ToString().PadLeft(4);
            }
            writer.WriteLine(line);
        }
        writer.Close();
        Debug.Log("Path Written");
    }
}
