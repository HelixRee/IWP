using SmallHedge.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class BatterySocket : MonoBehaviour
{
    [SerializeField] private Transform _attachTransform;
    [SerializeField] private SplineAnimate _splineAnimator;
    [SerializeField] private List<CircuitComponent> _circuitGroup = new();
    [SerializeField] private GameObject _UI;

    private Battery _attachedBattery;
    public bool _isPowered = false;
    private bool _socketActive = true;
    private bool _soundStarted = false;

    [HideInInspector] public UnityEvent onAttachBattery;
    [HideInInspector] public UnityEvent onDetachBattery;
    private void Start()
    {
        _UI.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!_socketActive) return;
        if (!other.TryGetComponent(out Battery battery)) return;
        if (!other.CompareTag("Litter")) return;


        AttachBattery(battery);
    }

    private void AttachBattery(Battery battery)
    {
        if (_attachedBattery != null) return;

        battery.tag = "Untagged";
        _attachedBattery = battery;
        battery.GetComponent<Collider>().enabled = false;
        battery.GetComponent<Rigidbody>().isKinematic = true;

        if (_splineAnimator == null)
            _isPowered = true;
        if (_splineAnimator != null)
        {
            _splineAnimator.gameObject.SetActive(false);
            _splineAnimator.Restart(false);
            _splineAnimator.gameObject.SetActive(true);
            _splineAnimator.Play();
        }

        onAttachBattery.Invoke();
    }
    public void DetachBattery()
    {
        if (_attachedBattery == null) return;

        _attachedBattery.tag = "Litter";
        _attachedBattery.GetComponent<Collider>().enabled = true;
        _attachedBattery.GetComponent<Rigidbody>().isKinematic = false;

        _attachedBattery = null;
        if (_splineAnimator == null)
            _isPowered = false;
        _socketActive = false;
        StartCoroutine(WaitAndReenable());

        if (_splineAnimator != null)
        {
            _splineAnimator.gameObject.SetActive(false);
            _splineAnimator.Restart(false);
            _splineAnimator.gameObject.SetActive(true);
        }

        onDetachBattery.Invoke();
    }

    // Change to blacklist system eventually
    // minecraft nether portal logic
    private IEnumerator WaitAndReenable()
    {
        yield return new WaitForSeconds(1f);
        _socketActive = true;
    }

    public void RefreshUI()
    {
        if (_attachedBattery == null)
            _UI.SetActive(false);
        else
            _UI.SetActive(true);
    }

    public void DisableUI()
    {
        _UI.SetActive(false);
    }

    private Coroutine _soundCoroutine = null;
    private Coroutine _auxSoundCoroutine = null;
    private void Update()
    {
        if (_splineAnimator != null)
        {
            _isPowered = _splineAnimator.NormalizedTime >= 1;

            if (_splineAnimator.NormalizedTime > 0.05f && _splineAnimator.NormalizedTime < 0.95f && !_soundStarted)
            {
                _soundCoroutine = StartCoroutine(LoopCracklingSound());
                _auxSoundCoroutine = StartCoroutine(StartLoopAuxCracklingSound());
                _soundStarted = true;
            }

            if (_splineAnimator.NormalizedTime <= 0.05f || _splineAnimator.NormalizedTime >= 0.95f)
            {
                if (_soundCoroutine != null)
                    StopCoroutine(_soundCoroutine);
                if (_auxSoundCoroutine != null)
                    StopCoroutine(_auxSoundCoroutine);
                _soundStarted = false;
            }
        }


        _circuitGroup.ForEach(comp => { if (comp != null) comp.isPowered = _isPowered; });
        if (_attachedBattery == null) return;

        _attachedBattery.transform.position = Vector3.Lerp(_attachedBattery.transform.position, _attachTransform.position, 12f * Time.deltaTime);
        _attachedBattery.transform.rotation = Quaternion.Slerp(_attachedBattery.transform.rotation, _attachTransform.rotation, 12f * Time.deltaTime);
    }

    private IEnumerator LoopCracklingSound()
    {
        if (_splineAnimator)
            AudioManager.PlaySound(ClipType.Electric_Crackling, _splineAnimator.GetComponent<AudioSource>());
        yield return new WaitForSeconds(0.7f);

        _soundCoroutine = StartCoroutine(LoopCracklingSound());
    }
    private IEnumerator StartLoopAuxCracklingSound()
    {
        yield return new WaitForSeconds(0.35f);

        _auxSoundCoroutine = StartCoroutine(LoopAuxCracklingSound());
    }
    private IEnumerator LoopAuxCracklingSound()
    {
        if (_splineAnimator)
            AudioManager.PlaySound(ClipType.Electric_Crackling, _splineAnimator.GetComponentsInChildren<AudioSource>()[1]);
        yield return new WaitForSeconds(0.7f);

        _auxSoundCoroutine = StartCoroutine(LoopAuxCracklingSound());
    }
}
