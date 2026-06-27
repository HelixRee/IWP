using UnityEditor;
using UnityEngine;

public class FirstOrderShowcase : MonoBehaviour
{
    public bool enableFirstOrder = false;
    public Transform target;

    // Normal Lerp
    public float t = 1;
    public Vector3 startPos = Vector3.zero;
    public Vector3 targetPos = Vector3.zero;

    // First Order
    public float damping = 12f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (t < 1 && t != 1) t += Time.deltaTime;
        if (t >= 1) t = 1;

        if (!enableFirstOrder)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, t);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, target.position, damping * Time.deltaTime);
        }
    }

    public void StartLerp()
    {
        startPos = transform.position;
        targetPos = target.position;

        t = 0;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FirstOrderShowcase))]
public class FirstOrderShowcaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        FirstOrderShowcase controller = (FirstOrderShowcase)target;
        if (GUILayout.Button("Start Lerp"))
        {
            controller.StartLerp();
        }
    }
}
#endif