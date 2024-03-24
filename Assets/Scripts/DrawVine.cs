using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrawVine
{
    private RenderTexture result;

    public DrawVine(int resX, int resY, RenderTextureFormat format = RenderTextureFormat.ARGBFloat)
    {
        result = new RenderTexture(resX, resY, 0, format);
    }

    public RenderTexture DrawToRenderTexture(Color circleColor, Shader drawShader, List<Node> nodes, int nodeGridSize)
    {
        RenderTexture temp = RenderTexture.GetTemporary(result.width, result.height, 0, result.format);
        Material drawMaterial = new Material(drawShader);

        drawMaterial.SetVector("_Color", circleColor);

        for (int i = 0; i < nodes.Count; i++)
        {
            Grower cur = (Grower)nodes[i];

            if (cur.parent == null)
                continue;

            Vector2 startPos = cur.parent.pos / nodeGridSize;
            Vector2 endPos = cur.pos / nodeGridSize;

            drawMaterial.SetVector("_StartPosition", new Vector4(startPos.x, startPos.y, 0.0f, 0.0f));
            drawMaterial.SetVector("_EndPosition", new Vector4(endPos.x, endPos.y, 0.0f, 0.0f));
            drawMaterial.SetFloat("_Thickness", 0.003f);
            
            Graphics.Blit(result, temp);
            Graphics.Blit(temp, result, drawMaterial);
        }

        RenderTexture.ReleaseTemporary(temp);

        return result;
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
