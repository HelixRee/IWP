using System.Collections;
using System.Collections.Generic;
using System.IO;
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



        InvokeRepeating("SavePNG", 0, 60 * 60);

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

    public void SavePNG()
    {
        StartCoroutine(SaveTextureToFile(maskRenderTexture, "SavedImage.png"));
    }

    IEnumerator SaveTextureToFile(RenderTexture rt, string fileName)
    {
        // Wait until the end of the frame to ensure rendering is complete
        yield return new WaitForEndOfFrame();

        // 1. Set the active RenderTexture to the one we want to read from
        RenderTexture.active = rt;

        // 2. Create a new Texture2D and read the pixels from the active RenderTexture
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply(); // Apply the pixels to the texture

        // 3. Reset the active RenderTexture
        RenderTexture.active = null;

        // 4. Encode the Texture2D to PNG bytes
        byte[] bytes = tex.EncodeToPNG();

        // 5. Destroy the temporary Texture2D to free up memory
        Destroy(tex);

        // 6. Define the save path (Application.persistentDataPath is a reliable writeable location at runtime)
        string path = Path.Combine(Application.persistentDataPath, fileName);

        // Ensure the directory exists if needed
        string dirPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        // 7. Write the bytes to the file
        File.WriteAllBytes(path, bytes);

        Debug.Log("Saved to " + path);

        // Optional: Refresh the Asset Database if running in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
