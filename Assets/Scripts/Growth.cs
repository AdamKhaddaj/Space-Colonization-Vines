using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Growth : MonoBehaviour
{
    public GameObject growTarget; //regular quad for now
    private Material objectMat; //thing that will be grown onto
    private RenderTexture growTex;


    private void Start()
    {
        objectMat = growTarget.GetComponent<MeshRenderer>().material;
        Texture tex = objectMat.mainTexture;
        growTex = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGBFloat);
        RenderTexture.active = growTex;
        Graphics.Blit(tex, growTex);
    }

    private void Update()
    {
        
    }

}


