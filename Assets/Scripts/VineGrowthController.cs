using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MakePathContinuous))]
public class VineGrowthController : MonoBehaviour
{
    [SerializeField]
    private Color vineColor = new Color(114.0f / 255.0f, 92.0f / 255.0f, 66.0f / 255.0f);
    [SerializeField]
    private Renderer baseObject; //game object that vine will grow on
    [SerializeField]
    private Shader drawVineShader; //shader to use to draw shape for grower nodes
    [SerializeField]
    private Shader drawLeafShader; //shader to use to draw shape for grower nodes
    [SerializeField]
    private Texture leafTexture;
    
    private MakePath makePath;
    private MakePathContinuous makePathContinuous;
    private DrawVine drawVine;
    private Material baseObjectMat;
    private RenderTexture vineTexture; //final vine texture to be used on base object

    void Start()
    {
        makePath = this.GetComponent<MakePath>();
        makePathContinuous = this.GetComponent<MakePathContinuous>();
        drawVine = new DrawVine(1024, 1024);
        baseObjectMat = baseObject.material;
    }

    void Update()
    {
        if (Input.GetKeyDown("d")) //draw
        {
            CreateVines(false);
        }
        else if(Input.GetKeyDown("a")) //animate
        {
            CreateVines(true);
        }
    }


    /// <summary>
    /// Method generates vines, draws it to a texture and assigns the texture to the base object
    /// </summary>
    void CreateVines(bool animate)
    {

        //create path of vines
        makePathContinuous.CreatePathFull();

        if (animate)
        {
            drawVine.ClearTexture();

            vineTexture = drawVine.Result;
            StartCoroutine(drawVine.AnimateVines(0.001f, true, true, vineColor, drawVineShader, makePathContinuous.Growers, makePathContinuous.Resolution, drawLeafShader, leafTexture));
        }
        else
        {
            drawVine.ClearTexture();

            vineTexture = drawVine.DrawToRenderTexture(false, vineColor, drawVineShader, makePathContinuous.Growers, makePathContinuous.Resolution, drawLeafShader, leafTexture);
        }

        baseObjectMat.SetTexture("_VineTex", vineTexture);
    }
}
