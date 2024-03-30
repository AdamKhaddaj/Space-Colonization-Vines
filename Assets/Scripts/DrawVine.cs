using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrawVine
{
    private RenderTexture result;

    public RenderTexture Result
    {
        get { return result; }
    }

    public DrawVine(int resX, int resY, RenderTextureFormat format = RenderTextureFormat.ARGBFloat)
    {
        result = new RenderTexture(resX, resY, 0, format);
    }

    public void ClearTexture()
    {
        result.Release();
    }

    public RenderTexture DrawToRenderTexture(bool drawLeaves, bool leavesAtRoot, Color circleColor, Shader drawVineShader, List<Node> nodes, int nodeGridSize, Shader drawLeafShader, Texture leafTexture, int leafDensity = 5, float leafGravityScale = 0.0f, float leafScale = 30.0f, float randomLeafScaleOffset = 0.0f, float randomLeafRotOffset = 0.0f)
    {
        float maxThickness = 1000;
        float minThickness = float.MaxValue;

        RenderTexture temp = RenderTexture.GetTemporary(result.width, result.height, 0, result.format);
        Material drawVineMaterial = new Material(drawVineShader);
        Material drawLeafMaterial = new Material(drawLeafShader);

        drawVineMaterial.SetVector("_Color", circleColor);
        drawLeafMaterial.SetTexture("_LeafTex", leafTexture);

        for (int i = 0; i < nodes.Count; i++)
        {
            Grower g = (Grower)nodes[i];
            if (g.parent == null)
            {
                maxThickness = g.thickness;
            }

            if (g.thickness < minThickness)
                minThickness = g.thickness;
        }

        drawVineMaterial.SetFloat("_MaxThickness", maxThickness);

        for (int i = 0; i < nodes.Count; i++)
        {
            Grower cur = (Grower)nodes[i];

            if (cur.parent == null)
                continue;

            Vector2 startPos = cur.parent.pos / nodeGridSize;
            Vector2 endPos = cur.pos / nodeGridSize;

            drawVineMaterial.SetVector("_StartPosition", new Vector4(startPos.x, startPos.y, 0.0f, 0.0f));
            drawVineMaterial.SetVector("_EndPosition", new Vector4(endPos.x, endPos.y, 0.0f, 0.0f));
            drawVineMaterial.SetFloat("_Thickness", cur.thickness);

            Graphics.Blit(result, temp);
            Graphics.Blit(temp, result, drawVineMaterial);

        }

        //add leaf every n nodes
        //change it so it is within the same vine
        int curInterval = leafDensity;

        if (drawLeaves)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Grower cur = (Grower)nodes[i];

                Vector2 pos = cur.pos / nodeGridSize;

                //offset used to have origin of leaf at tip of root instead of center of leaf texture
                float leafOffset = -0.01f;

                bool checkChildNode = cur.child == null;

                float thicknessRatio = 1.0f - ((cur.thickness - minThickness) / (maxThickness - minThickness));

                thicknessRatio = Mathf.Clamp(thicknessRatio, 0.75f, 1.0f);

                if (!leavesAtRoot)
                    checkChildNode = true;

                if (checkChildNode && curInterval <= 0)
                {
                    float rotation = CalculateLeafRotation(cur, true, true, leafGravityScale, randomLeafRotOffset);

                    drawLeafMaterial.SetVector("_Position", new Vector4(pos.x, pos.y, 0.0f, 0.0f));
                    drawLeafMaterial.SetFloat("_Rotation", rotation);
                    drawLeafMaterial.SetFloat("_PositionOffset", leafOffset);

                    float scaleOff = Random.Range(-randomLeafScaleOffset, randomLeafScaleOffset);
                    drawLeafMaterial.SetVector("_Scale", new Vector4(thicknessRatio * (leafScale + scaleOff), thicknessRatio * (leafScale + scaleOff), 0.0f, 0.0f));

                    Graphics.Blit(result, temp);
                    Graphics.Blit(temp, result, drawLeafMaterial);

                    curInterval = leafDensity;
                }
                else
                    curInterval--;

            }
        }

        RenderTexture.ReleaseTemporary(temp);

        return result;
    }

    public IEnumerator DrawToRenderTextureAnim(bool drawLeaves, bool leavesAtRoot, Color circleColor, Shader drawVineShader, List<Node> nodes, int nodeGridSize, Shader drawLeafShader, Texture leafTexture)
    {
        float maxThickness = 1000;
        int leafDensity = 5;

        RenderTexture temp = RenderTexture.GetTemporary(result.width, result.height, 0, result.format);
        Material drawVineMaterial = new Material(drawVineShader);
        Material drawLeafMaterial = new Material(drawLeafShader);

        drawVineMaterial.SetVector("_Color", circleColor);
        drawLeafMaterial.SetTexture("_LeafTex", leafTexture);

        for (int i = 0; i < nodes.Count; i++)
        {
            Grower g = (Grower)nodes[i];
            if (g.parent == null)
            {
                maxThickness = g.thickness;
            }
        }

        drawVineMaterial.SetFloat("_MaxThickness", maxThickness);

        for (int i = 0; i < nodes.Count; i++)
        {
            Grower cur = (Grower)nodes[i];

            if (cur.parent == null)
                continue;

            Vector2 startPos = cur.parent.pos / nodeGridSize;
            Vector2 endPos = cur.pos / nodeGridSize;

            drawVineMaterial.SetVector("_StartPosition", new Vector4(startPos.x, startPos.y, 0.0f, 0.0f));
            drawVineMaterial.SetVector("_EndPosition", new Vector4(endPos.x, endPos.y, 0.0f, 0.0f));
            drawVineMaterial.SetFloat("_Thickness", cur.thickness);

            Graphics.Blit(result, temp);
            Graphics.Blit(temp, result, drawVineMaterial);

            yield return null;

        }

        //add leaf every n nodes
        //change it so it is within the same vine
        int curInterval = leafDensity;

        if (drawLeaves)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Grower cur = (Grower)nodes[i];

                Vector2 pos = cur.pos / nodeGridSize;

                //offset used to have origin of leaf at tip of root instead of center of leaf texture
                float leafOffset = -0.01f;

                bool checkChildNode = cur.child == null;

                if (!leavesAtRoot)
                    checkChildNode = true;

                if (checkChildNode)
                {
                    float rotation = CalculateLeafRotation(cur, true);

                    drawLeafMaterial.SetVector("_Position", new Vector4(pos.x, pos.y, 0.0f, 0.0f));
                    drawLeafMaterial.SetFloat("_Rotation", rotation);
                    drawLeafMaterial.SetFloat("_PositionOffset", leafOffset);

                    float scaleOff = Random.Range(-5.0f, 5.0f);
                    drawLeafMaterial.SetVector("_Scale", new Vector4(30.0f + scaleOff, 30.0f + scaleOff, 0.0f, 0.0f));

                    Graphics.Blit(result, temp);
                    Graphics.Blit(temp, result, drawLeafMaterial);

                    curInterval = leafDensity;
                }
                else
                    curInterval--;

                yield return null;
            }
        }

        RenderTexture.ReleaseTemporary(temp);
    }

    public Texture2D DrawToTexture2D(Node[,] nodes)
    {
        Texture2D result = new Texture2D(nodes.GetLength(1), nodes.GetLength(0));

        result.filterMode = FilterMode.Point;

        Color[] pixels = result.GetPixels();

        for (int i = 0; i < nodes.GetLength(1); i++)
        {
            for (int j = 0; j < nodes.GetLength(0); j++)
            {
                int index = (j * nodes.GetLength(0)) + i;

                pixels[index] = new Color(0, 0, 0, 0);

                if (nodes[i, j] != null)
                {
                    if (nodes[i, j].GetType() == typeof(Grower))
                    {
                        pixels[index] = Color.green;
                    }
                }
            }
        }

        result.SetPixels(pixels);

        result.Apply();

        return result;
    }

    /*
     * returns rotation of leaf that is perpendicular to the vine it is growing on
     */
    private float CalculateLeafRotation(Grower node, bool addRotOffset, bool addGravity = false, float gravityScale = 0.0f, float randomLeafRotOffset = 0.0f)
    {
        int segmentLength = 5;

        Vector2 startPoint;
        Vector2 endPoint;

        Vector2 gravityVec = new Vector2(0.0f, -2.5f);

        gravityVec *= gravityScale;

        if (node == null)
            return 0.0f;

        endPoint = node.pos;
        startPoint = node.pos;

        Grower curNode = node;

        //find start point until end of segment length defined or at root node
        for (int i = 0; i <= Mathf.FloorToInt(segmentLength/2); i++)
        {
            if (curNode != null)
                startPoint = curNode.pos;
            else
                break;

            curNode = curNode.parent;
        }

        curNode = node;

        //find end point until end of segment length defined or at root node
        for (int i = 0; i <= Mathf.FloorToInt(segmentLength / 2); i++)
        {
            if (curNode != null)
                endPoint = curNode.pos;
            else
                break;

            curNode = curNode.child;
        }

        Vector2 segmentDir = startPoint - endPoint;

        if (segmentDir.magnitude == 0.0f)
            return 0.0f;

        segmentDir.Normalize();

        if (addGravity)
        {
            segmentDir = (segmentDir + gravityVec) / Vector2.Distance(new Vector2(0.0f, 0.0f), segmentDir + gravityVec);

            segmentDir.Normalize();
        }

        float angle = Mathf.Acos(Vector2.Dot(new Vector2(0f, -1f), segmentDir));

        angle *= Mathf.Rad2Deg;

        if(segmentDir.x < 0.0f)
            angle *= -1;

        if(addRotOffset)
            angle += Random.Range(-randomLeafRotOffset, randomLeafRotOffset);
        
        return angle;
    }

}
