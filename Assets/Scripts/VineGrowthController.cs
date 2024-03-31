using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MakePathContinuous))]
public class VineGrowthController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField]
    private Vector2 textureResolution = new Vector2(1024, 1024);
    [SerializeField]
    private Color vineColor = new Color(114.0f / 255.0f, 92.0f / 255.0f, 66.0f / 255.0f);
    [SerializeField]
    private Renderer baseObject; //game object that vine will grow on
    [SerializeField]
    private Shader drawVineShader; //shader to use to draw shape for grower nodes
    [SerializeField]
    private Shader drawLeafShader; //shader to use to draw shape for grower nodes

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

    private MakePath makePath;
    private MakePathContinuous makePathContinuous;
    private DrawVine drawVine;
    private Material baseObjectMat;
    private RenderTexture vineTexture; //final vine texture to be used on base object
    private bool vineGenerated;

    //saving texture settings
    private string textureName;

    public string TextureName
    {
        get { return textureName; }
        set { textureName = value; }
    }

    void Start()
    {
        makePath = this.GetComponent<MakePath>();
        makePathContinuous = this.GetComponent<MakePathContinuous>();
        drawVine = new DrawVine((int)textureResolution.x, (int)textureResolution.y);
        baseObjectMat = baseObject.material;
        vineGenerated = false;
    }

    void Update()
    {
        if (Input.GetKeyDown("g")) //generate vines
        {
            GenerateVines();
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

    public void GenerateVines()
    {
        //create path of vines
        makePathContinuous.CreatePathFull();
        vineGenerated = true;
    }

    /// <summary>
    /// Method generates vines, draws it to a texture and assigns the texture to the base object
    /// </summary>
    public void DrawVines(bool animate)
    {
        if (!vineGenerated)
            GenerateVines();

        if (animate)
        {
            drawVine.ClearTexture();

            vineTexture = drawVine.Result;
            StartCoroutine(drawVine.DrawToRenderTextureAnim(drawLeaves, drawLeavesAtEndsOnly, vineColor, drawVineShader, makePathContinuous.Growers, makePathContinuous.Resolution, drawLeafShader, leafTexture, leafDensity, leafGravityScale, leafScale, randomLeafScaleOffset, randomLeafRotOffset));
        }
        else
        {
            drawVine.ClearTexture();

            vineTexture = drawVine.DrawToRenderTexture(drawLeaves, drawLeavesAtEndsOnly, vineColor, drawVineShader, makePathContinuous.Growers, makePathContinuous.Resolution, drawLeafShader, leafTexture, leafDensity, leafGravityScale, leafScale, randomLeafScaleOffset, randomLeafRotOffset);
        }

        baseObjectMat.SetTexture("_VineTex", vineTexture);
    }

    public void SaveTexture()
    {
        if (vineTexture == null)
        {
            Debug.LogWarning("Generate and draw a texture first before saving.");
            return;
        }

        TextureFormat format;

        //convert rendertextureformat to textureformat
        switch(vineTexture.format)
        {
            case RenderTextureFormat.ARGBFloat:
                format = TextureFormat.ARGB32;
                break;
            default:
                format = TextureFormat.ARGB32;
                break;
        }

        Texture2D tex = new Texture2D(vineTexture.width, vineTexture.height, format, false, true);

        RenderTexture curRenTex = RenderTexture.active;
        RenderTexture.active = vineTexture;

        tex.ReadPixels(new Rect(0, 0, vineTexture.width, vineTexture.height), 0, 0);
        tex.Apply();

        RenderTexture.active = curRenTex;

        System.IO.File.WriteAllBytes("Assets\\" + textureName + ".png", tex.EncodeToPNG());
    }
}
