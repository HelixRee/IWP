using UnityEngine;

public class FollowObject : BaseRalphAnimator
{
    [Header("Behaviour")]
    [SerializeField] private bool _unparentOnAwake = true;

    [Header("Target")]
    public Transform Target;
    public bool LockPosition = false;
    public float Frequency = 1.0f;
    public float Damping = 0.5f;
    public float Readiness = 2f;
    public float MaxDistance = 0.1f;
    private SODVec3 smoothedPosition;

    [Header("Distance Anchor")]
    public Transform DistanceAnchor;
    public float AnchorMaxDistance = 0.1f;

    public override void ManualInit()
    {
        if (_unparentOnAwake)
            Unparent();

        smoothedPosition = new SODVec3(transform.position, Frequency, Damping, Readiness);
    }

    public override void ManualUpdate()
    {
        if (LockPosition)
        {
            transform.position = Target.position;
            return;
        }

        Vector3 newPos = smoothedPosition.Update(Time.deltaTime, Target.position + (UseGravity ? Vector3.down * MaxDistance : Vector3.zero));

        CalculateDistanceAnchor(Target, MaxDistance, ref newPos);
        if (DistanceAnchor)
            CalculateDistanceAnchor(DistanceAnchor, AnchorMaxDistance, ref newPos);

        smoothedPosition.Value = newPos;
        transform.position = newPos;
    }

    private void CalculateDistanceAnchor(Transform reference, float maxDistance, ref Vector3 newPos)
    {
        Vector3 direction = (newPos - reference.position).normalized;
        float displacement = Vector3.Distance(newPos, reference.position);
        displacement = Mathf.Min(displacement, maxDistance);

        newPos = reference.position + direction * displacement;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (Target)
            Gizmos.DrawWireSphere(Target.position, MaxDistance);
        if (DistanceAnchor)
            Gizmos.DrawWireSphere(DistanceAnchor.position, AnchorMaxDistance);

    }
    void Unparent()
    {
        Transform root = transform.root ?? GameObject.Find("Follow Objects").transform;

        if (root == null)
        {
            GameObject newRoot = new GameObject("Follow Objects");
            root = newRoot.transform;
        }

        if (root)
        {
            GameObject wrapperObject = GameObject.Find(root.name + " Follow Objects");
            if (wrapperObject == null && root.name.Contains(" Follow Objects")) wrapperObject = root.gameObject;
            if (wrapperObject != null)
            {
                root = wrapperObject.transform;
            }
            else
            {
                GameObject newRoot = new GameObject(root.name + " Follow Objects");
                newRoot.transform.SetSiblingIndex(root.GetSiblingIndex() + 1);
                root = newRoot.transform;
            }
        }

        transform.SetParent(root, true);
    }

    private void OnDrawGizmos()
    {
        if (!Target) return;

        float distance = Vector3.Distance(transform.position, Target.position);

        Gizmos.color = Color.Lerp(Color.green, Color.red, distance / MaxDistance);
        Gizmos.DrawLine(transform.position, Target.position);

        if (!DistanceAnchor) return;
        float anchorDistance = Vector3.Distance(transform.position, DistanceAnchor.position);

        Gizmos.color = Color.Lerp(Color.green, Color.red, anchorDistance / AnchorMaxDistance);
        Gizmos.DrawLine(transform.position, DistanceAnchor.position);
    }
}
