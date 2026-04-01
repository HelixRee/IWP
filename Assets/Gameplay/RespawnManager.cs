using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private float _playerLifespan = 240f;
    private float _playerRemainingLifespan = 0f;
    private float _smoothedChargeAmt = 1f;

    public bool _ragdollStarted = false;
    [SerializeField] private GameObject _activePlayer;
    private RalphMaterialController _playerMaterial;
    private RalphRagdollController _playerRagdoll;

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private UIBatteryCharge _batteryCharge;
    [SerializeField] private InventoryManager _inventoryManager;

    private Queue<GameObject> _playersInWorld = new();
    public static RespawnManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Optional: Persist across scene loads
        }
    }
    private void Start()
    {
        if (_activePlayer == null)
            Respawn();
        else
            InitPlayer();
    }
    private void InitPlayer()
    {
        _playerRemainingLifespan = _playerLifespan;
        _playerMaterial = _activePlayer.GetComponent<RalphMaterialController>();
        _playerRagdoll = _activePlayer.GetComponent<RalphRagdollController>();
        _ragdollStarted = false;

        _smoothedChargeAmt = 0f;

        _playersInWorld.Enqueue(_activePlayer);
        if (_playersInWorld.Count > 3)
            Destroy(_playersInWorld.Dequeue());
    }
    public void Respawn()
    {
        // Disable prev input
        _activePlayer.GetComponent<PlayerInput>().enabled = false;

        _activePlayer = Instantiate(_playerPrefab);
        _activePlayer.transform.position = _spawnPoint.position;
        _activePlayer.transform.rotation = _spawnPoint.rotation;

        _inventoryManager.SetRefPoint(_activePlayer.GetComponentInChildren<RalphPackMountAnimator>().RefPoint);

        Transform cameraTarget = _activePlayer.GetComponent<RalphCameraController>().CinemachineCameraTarget.transform;
        _virtualCamera.Follow = cameraTarget;
        _virtualCamera.LookAt = cameraTarget;

        InitPlayer();

        var spawners = FindObjectsByType<PaintballSpawner>(FindObjectsSortMode.None);
        foreach (var spawner in spawners)
        {
            spawner.ClearBalls();
        }
        //Debug.Log("respawn");

        ResetBattery();
    }
    private void Update()
    {
        if (_playerRemainingLifespan > 0)
            _playerRemainingLifespan -= Time.deltaTime;
        if (_playerRemainingLifespan <= 0 && !_ragdollStarted)
        {
            _playerRagdoll.StartRagdoll();
            _ragdollStarted = true;
        }
        if (Input.GetKeyDown(KeyCode.R))
            _playerRemainingLifespan = 0f;

        float fillAmt = _playerRemainingLifespan / _playerLifespan;

        _playerMaterial.headlightFillAmt = fillAmt;
        if (fillAmt > 0f)
            _smoothedChargeAmt = Mathf.Lerp(_smoothedChargeAmt, 1f, 1f * Time.deltaTime);
        _batteryCharge.fillAmount = Mathf.Min(fillAmt, _smoothedChargeAmt);
    }
    public RalphMaterialController GetActivePlayerMaterial()
    {
        return _playerMaterial;
    }
    public void ForceRagdoll()
    {
        _playerRagdoll.StartRagdoll();
    }

    public BatterySocket batterySocket;
    public GameObject battery;

    void ResetBattery()
    {
        if (batterySocket._isPowered) return;
        battery.transform.position = batterySocket.transform.position;
    }
}
