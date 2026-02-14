using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SmallHedge.AudioManager;

public class PlayClipOnClick : MonoBehaviour
{
    [SerializeField] private ClipType audioType;
    private Button button;
    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlaySound);
        }
    }

    private void PlaySound()
    {
        AudioManager.PlaySound(audioType);
    }
}
