using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MakePathContinuous))]
public class VineGrowthController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField]
    private Color vineColor = new Color(114.0f / 255.0f, 92.0f / 255.0f, 66.0f / 255.0f);
    [SerializeField]
    private Renderer baseObject; //game object that vine will grow on
    [SerializeField]
    private Shader drawVineShader; //shader to use to draw shape for grower nodes
    [SerializeField]
    private Shader drawLeafShader; //shader to use to draw shape for grower nodes
    [SerializeField]
    private bool wiggleAttractionPoints;

    [Header("Leaf Settings")]
    [SerializeField]
    private bool drawLeaves = true;
    [SerializeField]
    private bool drawLeavesAtEndsOnly = true;
    [SerializeField]
    private Texture leafTexture;
    [Range(0.0f, 1.0f)]
    [SerializeField]
    private float leafGravityScale = 0.0f;
    [Range(2, 100)]
    [SerializeField]
    private int leafDensity = 0;
    [SerializeField]
    private float leafScale = 30.0f;
    [SerializeField]
    private float randomLeafRotOffset = 0.0f;
    [SerializeField]
    private float randomLeafScaleOffset = 0.0f;

    [Header("Exclusion / Inclusion Zones")]
    [SerializeField]
    private bool useInclusion = false;
    [SerializeField]
    private bool useExclusion = false;
    [SerializeField]
    private Texture2D boundaryTexture;

    private MakePath makePath;
    private MakePathContinuous makePathContinuous;
    private DrawVine drawVine;
    private Material baseObjectMat;
    private RenderTexture vineTexture; //final vine texture to be used on base object
    private bool vineGenerated;

    void Start()
    {
        makePath = this.GetComponent<MakePath>();
        makePathContinuous = this.GetComponent<MakePathContinuous>();
        makePathContinuous.SetBoundaryTex(boundaryTexture);
        makePathContinuous.SetBoundaryType(useInclusion, useExclusion);
        makePathContinuous.wiggleAttractionNodes = wiggleAttractionPoints;
        makePathContinuous.SetBoundaryType(useInclusion, useExclusion);
        drawVine = new DrawVine(1024, 1024);
        baseObjectMat = baseObject.material;
        vineGenerated = false;
    }

    void Update()
    {
        if (Input.GetKeyDown("g")) //generate vines
        {
            //create path of vines
            makePathContinuous.CreatePathFull();
            vineGenerated = true;
        }

        if (Input.GetKeyDown("d")) //draw
        {
            DrawVines(false);
        }
        else if(Input.GetKeyDown("a")) //animate
        {
            DrawVines(true);
        }

    }


    /// <summary>
    /// Method generates vines, draws it to a texture and assigns the texture to the base object
    /// </summary>
    void DrawVines(bool animate)
    {
        if (animate)
        {
            drawVine.ClearTexture();

            vineTexture = drawVine.Result;
            StartCoroutine(drawVine.DrawToRenderTextureAnim(true, true, vineColor, drawVineShader, makePathContinuous.Growers, makePathContinuous.Resolution, drawLeafShader, leafTexture));
        }
        else
        {
            drawVine.ClearTexture();

            vineTexture = drawVine.DrawToRenderTexture(drawLeaves, drawLeavesAtEndsOnly, vineColor, drawVineShader, makePathContinuous.Growers, makePathContinuous.Resolution, drawLeafShader, leafTexture, leafDensity, leafGravityScale, leafScale, randomLeafScaleOffset, randomLeafRotOffset);
        }

        baseObjectMat.SetTexture("_VineTex", vineTexture);
    }
}
