using Cinemachine;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
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

    public void Respawn()
    {
        GameObject player = Instantiate(_playerPrefab);
        player.transform.position = _spawnPoint.position;
        player.transform.rotation = _spawnPoint.rotation;

        Transform cameraTarget = player.GetComponent<RalphCameraController>().CinemachineCameraTarget.transform;
        _virtualCamera.Follow = cameraTarget;
        _virtualCamera.LookAt = cameraTarget;

        Debug.Log("respawn");
    }
}
