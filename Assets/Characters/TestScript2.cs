using UnityEngine;

public class TestScript2 : MonoBehaviour
{
    public Transform Source;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 angles = new Vector3(Source.localEulerAngles.z + 90, 90, -90);
        transform.localEulerAngles = angles;
    }
}
