using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakePathContinuous : MonoBehaviour
{
    List<Node> growers;
    List<Attractor> attractors;

    int resolution = 100;

    private void Start()
    {
        growers = new List<Node>();
        attractors = new List<Attractor>();
        PlaceRootNodes();
        PlaceAttractionPoints();
    }

    private void PlaceRootNodes() // for now, just start with only one root node in bottom left
    {
        Grower g = new Grower(new Vector2(0, resolution - 1), null, null);
        growers.Add(g);
    }

    private void PlaceAttractionPoints() // arbitrary placement for now
    {
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                int bruh = UnityEngine.Random.Range(1, 4);
                if (bruh == 1)
                {
                    Attractor a = new Attractor(new Vector2(j, i), null, 5, 2, this);
                    attractors.Add(a);
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

                (Grower, bool) info = a.GetInfluencedNodeContinuous();
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
                    a.active = false;
                    g.numKills++;
                }
            }
        }

        // Now we can iterate over the growers, and have them move in their grow directions
        foreach (Grower g in influencedNodes)
        {
            // Create new growth node in direction of growth direction
            Vector2 finalGrowDir = new Vector2(g.growthDir.x / g.numInfluencers, g.growthDir.y / g.numInfluencers).normalized;
            float x = finalGrowDir.x;
            float y = finalGrowDir.y;
            Vector2 newGrowthPos = new Vector2(g.pos.x + x, g.pos.y + y);

            // NOTE!!! this currently does not check for overlap

            Grower leaf = new Grower(new Vector2(newGrowthPos.x, newGrowthPos.y), null, g);
            growers.Add(leaf);
            g.ThicknessChange();

            g.growthDir = Vector2.zero;
            g.numInfluencers = 0;
            g.active = false;
        }
        Debug.Log("Step");
    }

    public void CreatePathFull()
    {
        int growing = 0;
        while (growing < 150)
        {
            List<Grower> influencedNodes = new List<Grower>();
            foreach (Attractor a in attractors)
            {
                if (a.active)
                {
                    // Find the closest growth node. If it's close enough to be in kill radius, kill this attractor.
                    // If it's only in influence radius, affect that growth node's grow direction
                    // If there are no nearby nodes, do nothing.

                    (Grower, bool) info = a.GetInfluencedNodeContinuous();
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
                        a.active = false;
                        g.numKills++;
                    }
                }
            }

            // Now we can iterate over the growers, and have them move in their grow directions
            foreach (Grower g in influencedNodes)
            {
                // Create new growth node in direction of growth direction
                Vector2 finalGrowDir = new Vector2(g.growthDir.x / g.numInfluencers, g.growthDir.y / g.numInfluencers).normalized;
                float x = finalGrowDir.x;
                float y = finalGrowDir.y;
                Vector2 newGrowthPos = new Vector2(g.pos.x + x, g.pos.y + y);

                // NOTE!!! this currently does not check for overlap

                Grower leaf = new Grower(new Vector2(newGrowthPos.x, newGrowthPos.y), null, g);
                growers.Add(leaf);
                g.ThicknessChange();

                g.growthDir = Vector2.zero;
                g.numInfluencers = 0;
                g.active = false;
            }
            growing++;
        }
        
        Debug.Log("done");
    }

    public List<Node> Growers
    {
        get { return growers; }
    }

    public int Resolution
    {
        get { return resolution; }
    }

    public List<Node> GetGrowers()
    {
        return growers;
    }
}
