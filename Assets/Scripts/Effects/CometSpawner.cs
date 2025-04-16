using UnityEngine;

public class CometSpawner : MonoBehaviour
{
    [Header("Prefab & Timing")]
    [SerializeField] private GameObject cometPrefab;
    [SerializeField] private float spawnIntervalMin = 8f;
    [SerializeField] private float spawnIntervalMax = 20f;

    [Header("Spawning & Speed")]
    [SerializeField] private float spawnDistance = 60f;
    [SerializeField] private float cometSpeed = 30f;

    private float nextSpawnTime;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnComet();
            SetNextSpawnTime();
        }
    }

    void SetNextSpawnTime()
    {
        nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void SpawnComet()
    {
        Vector2 angle = Random.insideUnitCircle.normalized;
        Vector3 dir = (Vector3)angle.normalized;

        Vector3 spawnPos = transform.position - dir * spawnDistance;
        GameObject c = Instantiate(cometPrefab, spawnPos, Quaternion.LookRotation(dir));

        Comet comet = c.GetComponent<Comet>();
        comet.Initialize(dir, cometSpeed);
    }
}
