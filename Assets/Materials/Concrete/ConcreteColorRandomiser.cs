using System.Linq;
using UnityEngine;

public class ConcreteColorRandomiser : MonoBehaviour
{
    private MaterialPropertyBlock propertyBlock;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material materialToChange;
    [SerializeField] private Gradient colorRange;

    private void Awake()
    {
        renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(renderer => renderer.sharedMaterial == materialToChange).ToArray();

        // Initialize the property block if it's the first time
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var renderer in renderers)
        {
            renderer.GetPropertyBlock(propertyBlock);
            // Set the color property (use "_Color" or the name defined in your shader)
            propertyBlock.SetColor("_Color", colorRange.Evaluate(Random.Range(0f,1f)));

            // Apply the property block to the renderer
            renderer.SetPropertyBlock(propertyBlock);
            //Debug.Log("AH");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
