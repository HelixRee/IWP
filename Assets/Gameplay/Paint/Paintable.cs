using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Paintable : MonoBehaviour
{
    const int TEXTURE_SIZE = 512;
    const int TEXTURE_WIDTH = 2560;
    const int TEXTURE_HEIGHT = 1080;

    public RenderTexture maskRenderTexture;
    public RenderTexture supportTexture;
    //public RenderTexture baseTexture;
    //[SerializeField] private Material baseMaterial;

    Renderer rend;
    int maskTextureID = Shader.PropertyToID("_MaskTexture");

    public RenderTexture getMask() => maskRenderTexture;
    public RenderTexture getSupport() => supportTexture;
    public Renderer getRenderer() => rend;
    

    // Start is called before the first frame update
    void Start()
    {
        maskRenderTexture = new RenderTexture(TEXTURE_WIDTH, TEXTURE_HEIGHT, 0, RenderTextureFormat.ARGB32);
        maskRenderTexture.filterMode = FilterMode.Bilinear;

        supportTexture = new RenderTexture(TEXTURE_WIDTH, TEXTURE_HEIGHT, 0, RenderTextureFormat.ARGB32);
        supportTexture.filterMode = FilterMode.Bilinear;
        //baseTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0, RenderTextureFormat.ARGB32);
        //baseTexture.filterMode = FilterMode.Bilinear;


        rend = GetComponent<Renderer>();
        rend.material.SetTexture(maskTextureID, maskRenderTexture);

        //CommandBuffer buffer = new CommandBuffer();
        //buffer.SetRenderTarget(baseTexture);
        //buffer.DrawRenderer(rend, baseMaterial, 0);
        //Graphics.ExecuteCommandBuffer(buffer);
        //buffer.Clear();


        //PaintManager.instance.initTextures(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        //GUI.DrawTexture(new Rect(0, 0, 512, 512), maskRenderTexture);
        //GUI.DrawTexture(new Rect(512, 0, 512, 512), supportTexture);
    }
}
