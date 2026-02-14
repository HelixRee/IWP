using SmallHedge.AudioManager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CircuitLockDisplay : CircuitComponent
{
    [SerializeField] private RectTransform _lockShackle;
    [SerializeField] private Color _poweredOnColor;
    [SerializeField] private Color _poweredOffColor;
    private Color _activeColor;
    private List<Image> _images; 
    private SODFloat _yOffset;
    private float _initialYOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _images = GetComponentsInChildren<Image>().ToList();
        _yOffset = new(0, 3,0.5f,0);
        _initialYOffset = _lockShackle.localPosition.y;

        _activeColor = _poweredOffColor;
    }

    protected override void Update()
    {
        base.Update();
        if (isPowered)
        {
            _yOffset.Update(Time.deltaTime, 20);
            _activeColor = Color.Lerp(_activeColor, _poweredOnColor, 6f * Time.deltaTime);
        }
        else
        {
            _yOffset.Update(Time.deltaTime, 0);
            _activeColor = Color.Lerp(_activeColor, _poweredOffColor, 6f * Time.deltaTime);
        }

        Vector3 pos = _lockShackle.localPosition;
        pos.y = _initialYOffset + _yOffset.Value;
        _lockShackle.localPosition = pos;

        _images.ForEach(image => image.color = _activeColor);
    }

    protected override void OnPowerOn()
    {
        base.OnPowerOn();
        AudioManager.PlaySound(ClipType.Unlock, GetComponent<AudioSource>());
    }

    protected override void OnPowerOff()
    {
        base.OnPowerOff();
        AudioManager.PlaySound(ClipType.Lock, GetComponent<AudioSource>());
    }
}
