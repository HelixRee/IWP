using UnityEngine;

public class RalphAntennaLookAt : BaseRalphAnimator
{
    public Transform Source;

    [Range(0,1)]
    public float Weight = 1f;

    private Quaternion _initialRotation;
    public override void ManualInit()
    {
        _initialRotation = transform.localRotation;
    }

    public override void ManualUpdate()
    {
        transform.up = Source.position - transform.position;
        transform.Rotate(Vector3.up, 90);
         
        transform.localRotation = Quaternion.Slerp(_initialRotation, transform.localRotation, Weight);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, Source.position);
    }
}
