using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attractor : Node
{
    int influenceRadius, killRadius;
    int resolution; // reference to resolution of full nodes array
    public bool dying = false;
    MakePathContinuous manager;
    int tag2;

    public Attractor(Vector2 pos, Node[,] nodes, (int,int) gridLocation, int influenceRadius, int killRadius, MakePathContinuous manager, int tag , int tag2) : base(pos, nodes, gridLocation)
    {
        this.influenceRadius = influenceRadius;
        this.killRadius = killRadius;
        if (manager == null)
        {
            resolution = (int)Mathf.Sqrt(nodes.Length);
        }
        else
        {
            resolution = manager.Resolution;
        }
        active = true;
        this.manager = manager;
        this.tag = tag;
        this.tag2 = tag2;
    }

    public void SetInfluenceRadius(int r)
    {
        influenceRadius = r;
    }
    public void SetKillRadius(int r)
    {
        killRadius = r;
    }

    public (Grower, bool) GetInfluencedNodeContinuous()
    {
        Grower closestNode = null;
        float closestDist = Mathf.Infinity;
        List<Node> growers = manager.GetGrowers();

        // For now, just iterate over all growth nodes, will implement quad tree optomization later
        for (int i = 0; i < growers.Count; i++)
        {
            Grower g = (Grower)growers[i];

            bool dontCheckKills = manager.GetDontCheckKills() ? true : g.numKills < manager.GetGrowerKillLimit();

            if (dontCheckKills)
            {
                // only look at grow nodes within 1 range in the grid
                //Debug.Log("COMPARING: (" + g.gridLocation.Item1 + "," + g.gridLocation.Item2 + ") and: (" + gridLocation.Item1 + "," + gridLocation.Item2 + ")");
                if (Mathf.Abs(g.gridLocation.Item1 - gridLocation.Item1) < 2 && Mathf.Abs(g.gridLocation.Item2 - gridLocation.Item2) < 2)
                {
                    if (g.tag == tag || g.tag == tag2)
                    {
                        float dist = Vector2.Distance(pos, g.pos);
                        if (dist <= influenceRadius)
                        {
                            if (dist <= killRadius)
                            {
                                dying = true;
                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    closestNode = g;
                                }
                            }
                            else
                            {
                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    closestNode = g;
                                }
                            }
                        }
                    }
                }
            }
        }
        return (closestNode, dying);
    }
}
