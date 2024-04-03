using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MakePathContinuous))]
public class VineGrowthController : MonoBehaviour
{
    public enum DrawMode
    {
        None,
        PlaceGrowthNodes,
        DrawZones
    }

    public enum InclusionMode
    {
        None,
        Inclusion,
        Exclusion
    }

    [Header("General Settings")]
    [SerializeField]
    private Renderer baseObject; //game object that vine will grow on
    [SerializeField]
    private Vector2 textureResolution = new Vector2(1024, 1024);
    [SerializeField]
    private int vineResolution = 128;
    [SerializeField]
    private Color vineColor = new Color(114.0f / 255.0f, 92.0f / 255.0f, 66.0f / 255.0f);
    [SerializeField]
    private bool wiggleAttractionPoints;
    [Range(0.0f, 5.0f)]
    [SerializeField]
    private float wiggleAmount;

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
    private InclusionMode inclusionMode;
    [SerializeField]
    private DrawMode drawMode;
    [Range(0.0f, 0.05f)]
    [SerializeField]
    private float brushSize = 0.02f;
    [SerializeField]
    private Texture2D boundaryTexture;

    private Camera mainCam;

    private string zonesBrushShaderName = "Unlit/DrawCircle";
    private string drawVineShaderName = "Unlit/DrawLine"; //shader to use to draw shape for grower nodes
    private string drawLeafShaderName = "Unlit/DrawLeaf"; //shader to use to draw shape for grower nodes

    private Shader drawVineShader;
    private Shader drawLeafShader;

    private RenderTexture boundaryTex;
    private RenderTexture vineTexture; //final vine texture to be used on base object

    private Material zonesBrushMat;
    private Material baseObjectMat;

    private MakePath makePath;
    private MakePathContinuous makePathContinuous;

    private DrawVine drawVine;

    private bool vineGenerated;
    private RaycastHit hit;

    //saving texture settings
    private string textureName;

    public string TextureName
    {
        get { return textureName; }
        set { textureName = value; }
    }

    private void OnValidate()
    {
        //Debug.Log("My value changed to: " + myValue);
    }

    void Start()
    {
        makePath = this.GetComponent<MakePath>();
        makePathContinuous = this.GetComponent<MakePathContinuous>();

        zonesBrushMat = new Material(Shader.Find(zonesBrushShaderName));
        zonesBrushMat.SetColor("_Color", Color.red);
        zonesBrushMat.SetFloat("_Size", brushSize);

        drawLeafShader = Shader.Find(drawLeafShaderName);
        drawVineShader = Shader.Find(drawVineShaderName);

        baseObjectMat = baseObject.material;
        vineGenerated = false;

        mainCam = Camera.main;

        boundaryTex = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGBFloat);

        if(drawMode != DrawMode.None)
        {
            baseObjectMat.SetTexture("_VineTex", boundaryTex);
        }

        drawVine = new DrawVine((int)textureResolution.x, (int)textureResolution.y);

        makePathContinuous.SetResolution(vineResolution);

        SetBoundaryTexture();

        makePathContinuous.SetWiggle(wiggleAttractionPoints);
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0))
        {
            if(Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit))
            {
                Paint();
            }
        }
    }

    private void SetBoundaryTexture()
    {
        Texture2D boundTex = boundaryTexture;

        bool useInclusion = (inclusionMode == InclusionMode.Inclusion);
        bool useExclusion = (inclusionMode == InclusionMode.Exclusion);

        if(boundaryTexture == null)
        {

            boundTex = ConvertRenderTextureToTexture2D(boundaryTex);
        }

        boundTex = flipTex(boundTex);

        makePathContinuous.SetBoundaryTex(boundTex);
        makePathContinuous.SetBoundaryType(useInclusion, useExclusion);
    }
    private void Paint()
    {
        float tmpBrushSize = brushSize;

        if (drawMode == DrawMode.PlaceGrowthNodes)
        {
            tmpBrushSize = 0.002f;

            zonesBrushMat.SetColor("_Color", Color.green);
        }
        else
        {
            zonesBrushMat.SetColor("_Color", Color.red);
        }

        zonesBrushMat.SetFloat("_Size", tmpBrushSize);
        zonesBrushMat.SetVector("_Position", new Vector4(hit.textureCoord.x, hit.textureCoord.y, 0.0f, 0.0f));

        RenderTexture tmp = RenderTexture.GetTemporary(boundaryTex.width, boundaryTex.height, 0, boundaryTex.format);
        Graphics.Blit(boundaryTex, tmp);
        Graphics.Blit(tmp, boundaryTex, zonesBrushMat);
        RenderTexture.ReleaseTemporary(tmp);
    }

    public void GenerateVines()
    {

        makePathContinuous.SetResolution(vineResolution);

        SetBoundaryTexture();

        makePathContinuous.SetWiggle(wiggleAttractionPoints);

        makePathContinuous.SetWiggleStrength(wiggleAmount);

        makePathContinuous.SetUp();

        //create path of vines
        makePathContinuous.CreatePathFull();
        vineGenerated = true;
    }

    public void ResetBoundaryTexture()
    {
        boundaryTex.Release();
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

        Texture2D tex = ConvertRenderTextureToTexture2D(vineTexture);

        System.IO.File.WriteAllBytes("Assets\\" + textureName + ".png", tex.EncodeToPNG());
    }

    private Texture2D ConvertRenderTextureToTexture2D(RenderTexture renTex)
    {
        TextureFormat format;

        //convert rendertextureformat to textureformat
        switch (renTex.format)
        {
            case RenderTextureFormat.ARGBFloat:
                format = TextureFormat.ARGB32;
                break;
            default:
                format = TextureFormat.ARGB32;
                break;
        }

        Texture2D tex = new Texture2D(renTex.width, renTex.height, format, false, true);

        RenderTexture curRenTex = RenderTexture.active;
        RenderTexture.active = renTex;

        tex.ReadPixels(new Rect(0, 0, renTex.width, renTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = curRenTex;

        return tex;
    }

    private Texture2D flipTex(Texture2D tex)
    {
        Texture2D res = new Texture2D(tex.width, tex.height, tex.format, false, true);

        for (int i = 0; i < tex.height; i++)
        {
            for (int j = 0; j < tex.width; j++)
            {
                res.SetPixel(j, i, tex.GetPixel(j, tex.width - i));
            }
        }

        return res;
    }
}
