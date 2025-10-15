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
        //transform.up = Source.position - transform.position;
        //transform.Rotate(Vector3.up, 90);
        //transform.LookAt(Source, -transform.parent.up);
        //transform.Rotate(Vector3.right, 90);

        //transform.LookAt(Source, -Vector3.right);
        //transform.Rotate(Vector3.right, 90);
        //Vector3 angles = transform.localEulerAngles;
        //angles.y = _initialRotation.y;
        //transform.localEulerAngles = angles;
        //transform.Rotate(Vector3.up, Vector3.Dot(transform.parent.eulerAngles, transform.parent.up) - 90);

        Vector3 disp = Source.position - transform.position;
        float angleZ, angleX;
        {
            float x = Vector3.Dot(disp, -transform.parent.right);
            float y = Vector3.Dot(disp, transform.parent.up);
            angleZ = Vector2.SignedAngle(new Vector2(x, y), Vector2.up);
        }
        {
            float x = Vector3.Dot(disp, transform.parent.forward);
            float y = Vector3.Dot(disp, transform.parent.up);
            angleX = Vector2.SignedAngle(new Vector2(x, y), Vector2.up);
        }

        Vector3 angle = _initialRotation.eulerAngles;
        angle.x = angleX;
        angle.z = angleZ;
        transform.localEulerAngles = angle;

        transform.localRotation = Quaternion.Slerp(_initialRotation, transform.localRotation, Weight);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, Source.position);
        Gizmos.DrawRay(transform.position, transform.right * 0.01f);
    }
}
