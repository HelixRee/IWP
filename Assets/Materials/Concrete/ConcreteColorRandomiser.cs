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
    public float gridSpacing = 1 / 0.125f;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ConcreteColorRandomiser randomiser = (ConcreteColorRandomiser)target;
        if (GUILayout.Button("Snap to grid"))
        {
            int i = 0;
            Undo.RecordObjects(randomiser.renderers.Select(renderer => renderer.gameObject).ToArray(), "Snapped objects to grid");
            foreach(Renderer renderer in randomiser.renderers)
            {
                Vector3 objectPos = renderer.transform.position;
                if (RoundTo(ref objectPos, gridSpacing))
                    continue;

                Vector3 objectScale = renderer.transform.localScale;
                if (RoundTo(ref objectScale, gridSpacing))
                    continue;

                i++;
                renderer.transform.position = objectPos;
                renderer.transform.localScale = objectScale;
            }
            Debug.Log(i + " Objects snapped");
        }
        if (GUILayout.Button("Refresh Renderers"))
        {
            randomiser.GetRenderers();
        }
    }
    private bool RoundTo(ref Vector3 input, float fraction, float offset = 0f)
    {
        // return true if out of threshold
        bool outOfSpec = false;
        if (RoundTo(ref input.x, fraction, offset))
            outOfSpec = true;
        if (RoundTo(ref input.y, fraction, offset))
            outOfSpec = true;
        if (RoundTo(ref input.z, fraction, offset))
            outOfSpec = true;

        return outOfSpec;
    }
    private bool RoundTo(ref float input, float fraction, float offset = 0f)
    {
        float roundedVal = Mathf.Round((input - offset) * fraction);
        float rawVal = (input - offset) * fraction;


        input = (roundedVal / fraction) + offset;
        return (Mathf.Abs(rawVal - roundedVal) > 0.05f * fraction);
    }
}
#endif