using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MakePath))]
public class VineGrowthController : MonoBehaviour
{
    [SerializeField]
    private Renderer baseObject;
    [SerializeField]
    private Shader drawShader;

    private MakePath makePath;
    private DrawVine drawVine;
    private Material baseObjectMat;

    void Start()
    {
        makePath = this.GetComponent<MakePath>();
        drawVine = new DrawVine(1024, 1024);
        baseObjectMat = baseObject.material;
    }

    void Update()
    {
        if (Input.GetKeyDown("d"))
        {
            CreateVines();
        }
    }

    void CreateVines()
    {
        makePath.CreateFullPath();

        RenderTexture vineTexture = drawVine.DrawToRenderTexture(Color.green, drawShader, makePath.Growers, makePath.Resolution);

        baseObjectMat.SetTexture("_VineTex", vineTexture);
    }
}
