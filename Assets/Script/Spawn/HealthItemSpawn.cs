using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthItemSpawn : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnItem = 5f;

    [SerializeField] private float respawnItem = 3f;

    private float timeUntilSpawn;

    private struct ActiveInstance
    {
        public GameObject go;
        public int prefabIndex;
    }

    private readonly List<ActiveInstance> activeInstances =
        new List<ActiveInstance>();

    private void Awake()
    {
        SetSpawnTimer();
    }

    private void Update()
    {
        timeUntilSpawn -= Time.deltaTime;

        if (timeUntilSpawn > 0f)
            return;

        CleanupActiveInstances();

        SpawnRandomItem();

        SetSpawnTimer();
    }

    private void SpawnRandomItem()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
            return;

        int randomIndex =
            Random.Range(0, itemPrefabs.Length);

        GameObject prefab = itemPrefabs[randomIndex];

        if (prefab == null)
            return;

        GameObject spawned =
            Instantiate(
                prefab,
                transform.position,
                Quaternion.identity
            );

        activeInstances.Add(
            new ActiveInstance
            {
                go = spawned,
                prefabIndex = randomIndex
            }
        );
    }

    private void CleanupActiveInstances()
    {
        for (int i = activeInstances.Count - 1; i >= 0; i--)
        {
            ActiveInstance ai = activeInstances[i];

            if (ai.go == null || !ai.go.activeInHierarchy)
            {
                int prefabIndex = ai.prefabIndex;

                activeInstances.RemoveAt(i);

                if (respawnItem > 0f)
                {
                    StartCoroutine(
                        RespawnAfterDelay(
                            prefabIndex,
                            respawnItem
                        )
                    );
                }
            }
        }
    }

    private IEnumerator RespawnAfterDelay(
        int prefabIndex,
        float delay
    )
    {
        yield return new WaitForSeconds(delay);

        if (itemPrefabs == null)
            yield break;

        if (prefabIndex < 0 ||
            prefabIndex >= itemPrefabs.Length)
            yield break;

        GameObject prefab =
            itemPrefabs[prefabIndex];

        if (prefab == null)
            yield break;

        GameObject spawned =
            Instantiate(
                prefab,
                transform.position,
                Quaternion.identity
            );

        activeInstances.Add(
            new ActiveInstance
            {
                go = spawned,
                prefabIndex = prefabIndex
            }
        );
    }

    private void SetSpawnTimer()
    {
        timeUntilSpawn = Mathf.Max(0f, spawnItem);
    }
}