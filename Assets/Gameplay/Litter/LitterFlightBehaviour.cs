using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class LitterFlightBehaviour : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Spline path;
    public Vector3 startOffset = Vector3.zero;
    public Vector3 activeOffset = Vector3.zero;

    public PIDVec3 pid;
    public float correctionalSpeed = 2;
    private Rigidbody _rb;
    public Transform target;

    private float startTime;

    public bool isSimulated = false;
    public GameObject simulatedObject;
    private void Start()
    {
        startOffset = target.position;


        startTime = Time.time;

        _rb = GetComponent<Rigidbody>();

        path = new Spline();
        BezierKnot knot = new BezierKnot();
        path.SetTangentMode(TangentMode.AutoSmooth);
        knot.Position = transform.position;
        knot.TangentOut = Vector3.up * 1f;
        path.Add(knot);

        knot.Position = target.position;
        knot.TangentIn = Vector3.up * 1f;
        path.Add(knot);

    }
    private void LateUpdate()
    {
        if (target == null) return;

        activeOffset = target.position - startOffset;

        path.Evaluate(Time.time - startTime, out var position, out var tangent, out var normal);
        transform.position = (Vector3)position + activeOffset;

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            enabled = false;
            inventoryManager.CreateObject(gameObject);
            Collider collider = GetComponent<Collider>();
            collider.enabled = false;
        }


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (_rb)
            Gizmos.DrawRay(_rb.position, _rb.linearVelocity);
        if (Time.time - startTime < 2)
            for (int i = 0; i < 20 - 1; i++)
            {
                path.Evaluate(i / 20f, out var position1, out var tangent1, out var normal1);
                path.Evaluate((i + 1) / 20f, out var position2, out var tangent2, out var normal2);
                Gizmos.DrawLine((Vector3)position1 + activeOffset, (Vector3)position2 + activeOffset);
            }
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(target.position, _rb.position + _rb.linearVelocity);
    }
}
