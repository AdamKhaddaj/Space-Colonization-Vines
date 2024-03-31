using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node
{
    public Vector2 pos;
    public Node[,] nodes; // reference to node array 
    public bool active;
    public int tag;
    public (int, int) gridLocation;

    public Node(Vector2 pos, Node[,] nodes, (int,int) gridLocation, int tag = 1)
    {
        this.pos = pos;
        this.nodes = nodes;
        this.gridLocation = gridLocation;
        this.tag = tag;
    }
}
