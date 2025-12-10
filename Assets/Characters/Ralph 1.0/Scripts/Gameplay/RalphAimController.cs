using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class RalphAimController : MonoBehaviour
{
    public bool IsAiming = false;
    public bool WasAiming = false;
    private Vector3 _aimDirection = Vector3.zero;

    public Spline path;
    public float startTime = 0f;
    public Vector3 startOffset = Vector3.zero;
    public Vector3 activeOffset = Vector3.zero;

    [Header("References")]
    [SerializeField] private StarterAssetsInputs _input;
    [SerializeField] private Transform _litterAttachPoint;
    [SerializeField] private float throwDelay;
    [SerializeField] private float throwPower = 2;

    private LitterBehaviour _activeLitter;
    private void Update()
    {
        IsAiming = _input.aiming;
        if (!IsAiming && WasAiming)
        {
            if (_activeLitter != null)
            {
                _aimDirection = Camera.main.transform.forward;

                if (Vector3.Distance(_activeLitter.transform.position, _litterAttachPoint.position) < 0.05f)
                {
                    IEnumerator coroutine = WaitAndThrowLitter();
                    StartCoroutine(coroutine);
                }
                else
                {
                    DropLitter();
                }
            }
        }
    }
    private void LateUpdate()
    {
        if (IsAiming && !WasAiming)
        {
            if (_activeLitter == null)
            {
                LitterBehaviour litterScript = InventoryManager.Instance.RemoveTopLitterObject();
                if (litterScript != null)
                {
                    _activeLitter = litterScript;
                    SetupSpline();
                    startOffset = _litterAttachPoint.position;
                    startTime = Time.time;


                    Rigidbody rb = _activeLitter.GetComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
        }
        if (_activeLitter != null)
        {
            activeOffset = _litterAttachPoint.position - startOffset;

            path.Evaluate((Time.time - startTime) * 5f, out var position, out var tangent, out var normal);
            _activeLitter.transform.position = (Vector3)position + activeOffset;
        }



        WasAiming = IsAiming;
    }

    private void SetupSpline()
    {
        path = new Spline();
        BezierKnot knot = new BezierKnot();
        path.SetTangentMode(TangentMode.AutoSmooth);
        knot.Position = _activeLitter.transform.position;
        knot.TangentOut = Vector3.up * 0.3f;
        path.Add(knot);

        knot.Position = _litterAttachPoint.position;
        knot.TangentIn = Vector3.up * 0.3f;
        path.Add(knot);
    }
    private IEnumerator WaitAndThrowLitter()
    {
        yield return new WaitForSeconds(throwDelay);
        ThrowLitter();
    }
    private void ThrowLitter()
    {
        if (_activeLitter == null) return;
        Rigidbody rb = _activeLitter.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.AddForce(_aimDirection * throwPower, ForceMode.Impulse);

        IEnumerator coroutine;
        coroutine = InventoryManager.Instance.WaitAndEnable(_activeLitter);
        StartCoroutine(coroutine);
        _activeLitter = null;
    }

    private void DropLitter()
    {
        if (_activeLitter == null) return;
        Rigidbody rb = _activeLitter.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        IEnumerator coroutine;
        coroutine = InventoryManager.Instance.WaitAndEnable(_activeLitter);
        StartCoroutine(coroutine);
        _activeLitter = null;
    }
}
