using UnityEngine;

public class FollowObject : RalphAnimator
{
    [Header("Behaviour")]
    [SerializeField] private bool _unparentOnAwake = true;

    [Header("Target")]
    public Transform Target;
    public float Frequency = 1.0f;
    public float Damping = 0.5f;
    public float Readiness = 2f;
    public float MaxDistance = 0.1f;
    private SecondOrderDyanmics smoothedPosition;


    public override void ManualInit()
    {
        if (_unparentOnAwake)
            Unparent();

        smoothedPosition = new SecondOrderDyanmics(transform.position, Frequency, Damping, Readiness);
    }

    public override void ManualUpdate()
    {
        Vector3 newPos = smoothedPosition.Update(Time.deltaTime, Target.position);

        Vector3 direction = (newPos - Target.position).normalized;
        float displacement = Vector3.Distance(newPos, Target.position);
        displacement = Mathf.Min(displacement, MaxDistance);

        newPos = Target.position + direction * displacement;
        smoothedPosition.ForceSetValue(newPos);
        transform.position = newPos;

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Target.position, MaxDistance);
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


}
