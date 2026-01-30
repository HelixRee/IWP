using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;
using UnityEngine.Windows;

public class ConcreteColorRandomiser : MonoBehaviour
{
    private MaterialPropertyBlock propertyBlock;
    public Renderer[] renderers;
    [SerializeField] private Material materialToChange;
    [SerializeField] private Gradient colorRange;

    private void Awake()
    {
        //renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(renderer => renderer.sharedMaterial == materialToChange).ToArray();
        //renderers = GetComponentsInChildren<Renderer>().Where(renderer =>  renderer.sharedMaterial == materialToChange).ToArray();
        GetRenderers();

        // Initialize the property block if it's the first time
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }
    public void GetRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>().Where(renderer => renderer.sharedMaterial == materialToChange).ToArray();
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
}

#if UNITY_EDITOR
[CustomEditor(typeof(ConcreteColorRandomiser))]
public class ConcreteColorRandomiserEditor : Editor
{
    public float gridSpacing = 0.125f;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ConcreteColorRandomiser randomiser = (ConcreteColorRandomiser)target;
        if (GUILayout.Button("Snap to grid"))
        {
            Undo.RecordObjects(randomiser.renderers.Select(renderer => renderer.gameObject).ToArray(), "Snapped objects to grid");
            foreach(Renderer renderer in randomiser.renderers)
            {
                Vector3 objectPos = renderer.transform.position;
                objectPos = RoundTo(objectPos, gridSpacing);
            }
            Debug.Log(randomiser.renderers.Length + " Objects snapped");
        }
        if (GUILayout.Button("Refresh Renderers"))
        {
            randomiser.GetRenderers();
        }
    }
    private Vector3 RoundTo(Vector3 input, float fraction, float offset = 0f)
    {
        return new Vector3(RoundTo(input.x, fraction, offset), RoundTo(input.y, fraction, offset), RoundTo(input.z, fraction, offset)); 
    }
    private float RoundTo(float input, float fraction, float offset = 0f)
    {
        float output = ((Mathf.Round((input - offset) * fraction)) / fraction) + offset;
        return output;
    }
}
#endif