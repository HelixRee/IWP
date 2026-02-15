using SmallHedge.AudioManager;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(BatterySocket))]
public class BatterySocketAddon : MonoBehaviour
{
    [SerializeField] private SplineAnimate _splineAnimator;
    [SerializeField] private List<CircuitComponent> _circuitGroup = new();
    private BatterySocket _socket;
    private bool _isPowered = false;
    private bool _soundStarted = false;


    private void Awake()
    {
        _socket = GetComponent<BatterySocket>();
        _socket.onAttachBattery.AddListener(OnBatteryAttach);
        _socket.onDetachBattery.AddListener(OnBatteryDetach);
    }

    private void OnBatteryAttach()
    {

        if (_splineAnimator == null)
            _isPowered = true;
        if (_splineAnimator != null)
        {
            _splineAnimator.gameObject.SetActive(false);
            _splineAnimator.Restart(false);
            _splineAnimator.gameObject.SetActive(true);
            _splineAnimator.Play();
        }

    }

    private void OnBatteryDetach()
    {
        if (_splineAnimator == null)
            _isPowered = false;

        if (_splineAnimator != null)
        {
            _splineAnimator.gameObject.SetActive(false);
            _splineAnimator.Restart(false);
            _splineAnimator.gameObject.SetActive(true);
        }

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
