using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grower : Node
{
    float influencable;
    public float thickness;
    public Grower parent, child;
    public Vector2 growthDir;
    public int numKills, numInfluencers;

    public Grower(Vector2 pos, Node[,] nodes, Grower parent) : base(pos, nodes)
    {
        active = false;
        numInfluencers = 0;
        this.parent = parent;
        growthDir = Vector2.zero;
    }

    public void ThicknessChange()
    {
        if (thickness >= 2500)
        {
            return;
        }
        if (parent == null)
        {
            thickness += 1;
            return;
        }
        else
        {
            thickness += 1;
            parent.ThicknessChange();
        }
    }
}