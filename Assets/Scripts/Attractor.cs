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

    public Attractor(Vector2 pos, Node[,] nodes, int influenceRadius, int killRadius, MakePathContinuous manager, int tag , int tag2) : base(pos, nodes)
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

    public (Grower, bool) GetInfluencedNode()
    {
        int x = (int)base.pos[0];
        int y = (int)base.pos[1];
        Grower closestNode = null;
        float closestDist = Mathf.Infinity;
        for (int i = Mathf.Clamp(y-influenceRadius, 0, resolution); i < Mathf.Clamp(y+influenceRadius,0,resolution); i++)
        {
            for (int j = Mathf.Clamp(x - influenceRadius, 0, resolution); j < Mathf.Clamp(x + influenceRadius, 0, resolution); j++)
            {
                // Check if it's actually inside the radius circle
                if (Mathf.Pow(j-x,2) + Mathf.Pow(i-y,2) <= Mathf.Pow(influenceRadius, 2))
                {
                    // Now check if it's actually inside the kill radius circle
                    if (Mathf.Pow(j-x, 2) + Mathf.Pow(i-y, 2) <= Mathf.Pow(killRadius, 2))
                    {
                        if (nodes[j, i] != null)
                        {
                            if (nodes[j, i].GetType() == typeof(Grower))
                            {
                                Grower g = (Grower)nodes[j, i];
                                float dist = Vector2.Distance(pos, g.pos);
                                dying = true;
                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    closestNode = g;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (nodes[j, i] != null)
                        {
                            if (nodes[j, i].GetType() == typeof(Grower))
                            {
                                Grower g = (Grower)nodes[j, i];
                                float dist = Vector2.Distance(pos, g.pos);
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

    public (Grower, bool) GetInfluencedNodeContinuous()
    {
        Grower closestNode = null;
        float closestDist = Mathf.Infinity;
        List<Node> growers = manager.GetGrowers();

        // For now, just iterate over all growth nodes, will implement quad tree optomization later
        for (int i = 0; i < growers.Count; i++)
        {
            Grower g = (Grower)growers[i];
            if(g.child == null)
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
        return (closestNode, dying);
    }
}
