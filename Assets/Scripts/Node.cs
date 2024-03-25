using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node
{
    public Vector2 pos;
    public Node[,] nodes; // reference to node array 
    public bool active;

    public Node(Vector2 pos, Node[,] nodes)
    {
        this.pos = pos;
        this.nodes = nodes;
    }
}
