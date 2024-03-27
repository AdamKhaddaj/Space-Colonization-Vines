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

    public RenderTexture DrawToRenderTexture(bool drawLeaves, Color circleColor, Shader drawVineShader, List<Node> nodes, int nodeGridSize, Shader drawLeafShader, Texture leafTexture)
    {
        RenderTexture temp = RenderTexture.GetTemporary(result.width, result.height, 0, result.format);
        Material drawVineMaterial = new Material(drawVineShader);
        Material drawLeafMaterial = new Material(drawLeafShader);

        drawVineMaterial.SetVector("_Color", circleColor);
        drawLeafMaterial.SetTexture("_LeafTex", leafTexture);

        float maxThickness = 1000;
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

        }

        //add leaf every n nodes
        //change it so it is within the same vine
        int leafDensity = 5;
        int curInterval = leafDensity;

        if (drawLeaves)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Grower cur = (Grower)nodes[i];

                Vector2 pos = cur.pos / nodeGridSize;

                if (curInterval <= 0)
                {
                    drawLeafMaterial.SetVector("_Position", new Vector4(pos.x, pos.y, 0.0f, 0.0f));
                    drawLeafMaterial.SetFloat("_Rotation", Random.Range(-25.0f, 25.0f));

                    float scaleOff = Random.Range(-5.0f, 5.0f);
                    drawLeafMaterial.SetVector("_Scale", new Vector4(45.0f, 45.0f, 0.0f, 0.0f));

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

    public IEnumerator AnimateVines(float animSpeed, bool drawLeaves, bool animateLeaves, Color circleColor, Shader drawVineShader, List<Node> nodes, int nodeGridSize, Shader drawLeafShader, Texture leafTexture)
    {
        RenderTexture temp = RenderTexture.GetTemporary(result.width, result.height, 0, result.format);
        Material drawVineMaterial = new Material(drawVineShader);
        Material drawLeafMaterial = new Material(drawLeafShader);

        drawVineMaterial.SetVector("_Color", circleColor);
        drawLeafMaterial.SetTexture("_LeafTex", leafTexture);

        float maxThickness = 1000;
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

            yield return new WaitForSeconds(animSpeed);
        }

        //add leaf every n nodes
        //change it so it is within the same vine
        int leafDensity = 1;
        int curInterval = leafDensity;

        if (drawLeaves)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Grower cur = (Grower)nodes[i];

                Vector2 pos = cur.pos / nodeGridSize;

                if (curInterval <= 0)
                {
                    drawLeafMaterial.SetVector("_Position", new Vector4(pos.x, pos.y, 0.0f, 0.0f));
                    drawLeafMaterial.SetFloat("_Rotation", Random.Range(-25.0f, 25.0f));

                    float scaleOff = Random.Range(-5.0f, 5.0f);
                    drawLeafMaterial.SetVector("_Scale", new Vector4(30.0f + scaleOff, 30.0f + scaleOff, 0.0f, 0.0f));

                    Graphics.Blit(result, temp);
                    Graphics.Blit(temp, result, drawLeafMaterial);

                    curInterval = leafDensity;

                    if(animateLeaves)
                        yield return new WaitForSeconds(animSpeed);
                }
                else
                    curInterval--;
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

}
