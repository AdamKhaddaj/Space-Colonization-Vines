using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakePathContinuous : MonoBehaviour
{
    List<Node> growers;
    List<Attractor> attractors;

    Texture2D exclusionZone;
    bool inclusion, exclusion = false;

    List<Node>[,] grid;

    int resolution = 164;

    int depth = 0;
    
    float wiggleStrength = 1.5f;

    bool wiggleAttractionNodes = false;

    public void SetResolution(int resolution)
    {
        this.resolution = resolution;
    }

    public void SetWiggleStrength(float wiggleStrength)
    {
        this.wiggleStrength = wiggleStrength;
    }

    public void SetUp()
    {
        grid = new List<Node>[6, 6];
        for(int i = 0; i < 6; i++)
        {
            for(int j = 0; j < 6; j++)
            {
                grid[i, j] = new List<Node>();
            }
        }
        growers = new List<Node>();
        attractors = new List<Attractor>();
        PlaceRootNodes();
        PlaceAttractionPoints();
    }

    public void SetBoundaryTex(Texture2D tex)
    {
        exclusionZone = tex;
    }

    public void SetBoundaryType(bool inclusion, bool exclusion)
    {
        if (inclusion)
        {
            this.inclusion = true;
        }
        else if (exclusion)
        {
            this.exclusion = true;
        }
    }

    public void SetWiggle(bool wiggle)
    {
        wiggleAttractionNodes = wiggle;
    }

    private void PlaceRootNodes() 
    {
        if (inclusion || exclusion)
        {
            Color[] pixels = exclusionZone.GetPixels();
            for(int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].g == 1)
                {
                    int x = i % exclusionZone.width;
                    int y = (i - x) / exclusionZone.width;
                    // conver to coordinates of vine image
                    int correspondX = x * resolution / exclusionZone.width;
                    int correspondY = y * resolution / exclusionZone.width;

                    int gridX = Mathf.FloorToInt(correspondX / (resolution / 6));
                    int gridY = Mathf.FloorToInt(correspondY / (resolution / 6));
                    Grower g = new Grower(new Vector2(correspondX, correspondY), null, (gridX, gridY), null, 1, 0);
                    growers.Add(g);
                }
            }
        }
        else
        {
            Grower g = new Grower(new Vector2(0, resolution - 1), null, (0, 5), null, 1, 0);
            growers.Add(g);

            Grower g2 = new Grower(new Vector2(0, 0), null, (0, 0), null, 1, 0);
            growers.Add(g2);
        }
    }

    private void PlaceAttractionPoints() // arbitrary placement for now
    {
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                // make sure to scale coordinates to exclusion zone resolution when needed
                if (exclusion)
                {
                    int correspondX = j * exclusionZone.width / resolution;
                    int correspondY = i * exclusionZone.width / resolution;
                    if (exclusionZone.GetPixel(correspondX, correspondY).r == 0)
                    {
                        int bruh = UnityEngine.Random.Range(1, 4);
                        /*
                        int[] tags = new int[2];
                        int p1 = UnityEngine.Random.Range(1, 3);
                        int p2 = UnityEngine.Random.Range(1, 3);
                        */

                        if (bruh == 1)
                        {
                            int gridX = Mathf.FloorToInt(j / (resolution / 6));
                            int gridY = Mathf.FloorToInt(i / (resolution / 6));
                            Attractor a = new Attractor(new Vector2(j, i), null, (gridX, gridY), 5, 2, this, 1, 2);
                            attractors.Add(a);
                        }
                    }
                }
                else if (inclusion)
                {
                    int correspondX = j * exclusionZone.width / resolution;
                    int correspondY = i * exclusionZone.width / resolution;
                    if (exclusionZone.GetPixel(correspondX, correspondY).r != 0)
                    {
                        int bruh = UnityEngine.Random.Range(1, 4);
                        /*
                        int[] tags = new int[2];
                        int p1 = UnityEngine.Random.Range(1, 3);
                        int p2 = UnityEngine.Random.Range(1, 3);
                        */

                        if (bruh == 1)
                        {
                            int gridX = Mathf.FloorToInt(j / (resolution / 6));
                            int gridY = Mathf.FloorToInt(i / (resolution / 6));
                            Attractor a = new Attractor(new Vector2(j, i), null, (gridX, gridY), 5, 2, this, 1, 2);
                            attractors.Add(a);
                        }
                    }
                }
                else
                {
                    int bruh = UnityEngine.Random.Range(1, 4);
                    /*
                    int[] tags = new int[2];
                    int p1 = UnityEngine.Random.Range(1, 3);
                    int p2 = UnityEngine.Random.Range(1, 3);
                    */

                    if (bruh == 1)
                    {
                        int gridX = Mathf.FloorToInt(j / (resolution / 6));
                        int gridY = Mathf.FloorToInt(i / (resolution / 6));
                        Attractor a = new Attractor(new Vector2(j, i), null, (gridX, gridY), 5, 2, this, 1, 2);
                        attractors.Add(a);
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

            if (wiggleAttractionNodes)
            {
                a.pos.x += Mathf.Cos(Time.time);
                a.pos.y += Mathf.Sin(Time.time);
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
            int gridX = Mathf.FloorToInt(newGrowthPos.x / (resolution / 6));
            int gridY = Mathf.FloorToInt(newGrowthPos.y / (resolution / 6));
            Grower leaf = new Grower(new Vector2(newGrowthPos.x, newGrowthPos.y), null, (gridX, gridY), g, g.tag, 0);
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
        bool death = false;
        int checker = 0;
        float counter = 0.0f;

        //If after 10 iterations, no attraction points have died, stop growth
        while (true)
        {
            depth++;
            if(checker == 10)
            {
                checker = 0;
                if (!death)
                {
                    break;
                }
                else
                {
                    death = false;
                }
            }
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
                        death = true;
                    }
                    if(wiggleAttractionNodes)
                    {
                        counter++;
                        a.pos.x += Mathf.Cos(counter) * wiggleStrength;
                        a.pos.y += Mathf.Sin(counter) * wiggleStrength;
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

                // to fix bug where growth node is pulled between two attraction points
                if(g.parent != null)
                {
                    if (Vector2.Distance(newGrowthPos, g.parent.pos) < 0.1f)
                    {
                        g.growthDir = Vector2.zero;
                        g.numInfluencers = 0;
                        g.active = false;
                        continue;
                    }
                }

                // NOTE!!! this currently does not check for overlap

                int gridX = Mathf.FloorToInt(newGrowthPos.x / (resolution / 6));
                int gridY = Mathf.FloorToInt(newGrowthPos.y / (resolution / 6));
                Grower leaf = new Grower(new Vector2(newGrowthPos.x, newGrowthPos.y), null, (gridX, gridY), g, g.tag, depth);
                leaf.parent.child = leaf;
                growers.Add(leaf);
                g.ThicknessChange();

                g.growthDir = Vector2.zero;
                g.numInfluencers = 0;
                g.active = false;
            }
            checker++;
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
