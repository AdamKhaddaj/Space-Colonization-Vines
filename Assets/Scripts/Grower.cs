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
    public int depth;

    public Grower(Vector2 pos, Node[,] nodes, (int, int) gridLocation, Grower parent, int tag, int depth) : base(pos, nodes, gridLocation)
    {
        active = false;
        numInfluencers = 0;
        this.parent = parent;
        growthDir = Vector2.zero;
        this.tag = tag;
        this.depth = depth;
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