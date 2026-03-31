using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TerrainTools;

public class PaintManager : MonoBehaviour
{
    public static PaintManager instance;

    public Shader texturePaint;

    private Material paintMaterial;

    CommandBuffer buffer;

    [Header("SFX")]
    [SerializeField] private List<AudioClip> paintSounds = new List<AudioClip>();
    [SerializeField] private float SFXVolume = 0.05f;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);

        paintMaterial = new Material(texturePaint);

        buffer = new CommandBuffer();
        buffer.name = "CommandBuffer - " + gameObject.name;
    }

    //public PaintManager GetInstance()
    //{
    //    if (instance == null)
    //        instance = new GameObject();
    //    return instance;
    //}    

    public void Paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? color = null)
    {
        Debug.Log("Attempted Paint");
        RenderTexture mask = paintable.getMask();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        paintMaterial.SetVector("_PainterPosition", pos);
        paintMaterial.SetFloat("_Hardness", hardness);
        paintMaterial.SetFloat("_Strength", strength);
        paintMaterial.SetFloat("_Radius", radius);
        paintMaterial.SetTexture("_MainTex", support);
        paintMaterial.SetColor("_PainterColor", color ?? Color.red);

        buffer.SetRenderTarget(mask);
        buffer.DrawRenderer(rend, paintMaterial, 0);

        buffer.SetRenderTarget(support);
        buffer.Blit(mask, support);
        //Debug.Log("Painted");

        Graphics.ExecuteCommandBuffer(buffer);
        buffer.Clear();

        if (paintSounds.Count > 0)
        {
            int rand = Random.Range(0, paintSounds.Count - 1);
            AudioSource.PlayClipAtPoint(paintSounds[rand], pos, 0.01f);
        }
    }

    public void initTextures(Paintable paintable)
    {
        RenderTexture mask = paintable.getMask();
        //RenderTexture uvIslands = paintable.getUVIslands();
        //RenderTexture extend = paintable.getExtend();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        buffer.SetRenderTarget(mask);
        //buffer.SetRenderTarget(extend);
        buffer.SetRenderTarget(support);

        //paintMaterial.SetFloat(prepareUVID, 1);
        //buffer.SetRenderTarget(uvIslands);
        buffer.DrawRenderer(rend, paintMaterial, 0);

        Graphics.ExecuteCommandBuffer(buffer);
        buffer.Clear();

        //Paint(paintable, Vector3.zero, 0f);
    }

    public bool CheckPaint(Paintable paintable, Vector3 pos)
    {
        Texture2D tex = new Texture2D(512, 512);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = paintable.getMask();
        tex.ReadPixels(new Rect(0, 0, paintable.getMask().width, paintable.getMask().height), 0, 0);
        tex.Apply();

        //pos.

        //tex.GetPixel()

        return false;
    }
}
