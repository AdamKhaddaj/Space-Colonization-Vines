using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor : Node
{
    int influenceRadius, killRadius;
    int resolution; // reference to resolution of full nodes array
    public bool dying = false;

    public Attractor(Vector2 pos, Node[,] nodes, int influenceRadius, int killRadius) : base(pos, nodes)
    {
        this.influenceRadius = influenceRadius;
        this.killRadius = killRadius;
        resolution = (int)Mathf.Sqrt(nodes.Length);
        active = true;
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
}
