
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PaintballSpawner : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private GameObject _paintBallPrefab;

    private List<GameObject> _storedBalls = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("AttemptSpawnBall", 0, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AttemptSpawnBall()
    {
        if (_storedBalls.Count < 3)
            SpawnBall();
    }

    void SpawnBall()
    {
        GameObject createdBall = Instantiate(_paintBallPrefab, _spawnPoint.position + Random.insideUnitSphere * 0.05f, Quaternion.identity, transform);
        PaintballBehaviour paintball = createdBall.GetComponent<PaintballBehaviour>();
        paintball.InitMaterial(RespawnManager.Instance.GetActivePlayerMaterial().GetActiveMaterialHueShift());

        paintball.onExplode.AddListener(RecountBalls);
        _storedBalls.Add(createdBall);
    }

    void RecountBalls(GameObject deletedBall)
    {
        _storedBalls.Remove(deletedBall);
    }
}
