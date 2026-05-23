using UnityEngine;

public class HealthItemSpawn : MonoBehaviour
{
    [SerializeField] GameObject[] _enemyPrefab;
    [SerializeField] float spawnItem;
    [SerializeField] private float respawnItem; 
    private float _timeUtilSpawn;
    [Header("Spawn Options")]
    [SerializeField] private bool onlyOneAtOnce = false;
    [SerializeField] private bool preventDuplicateType = true;
    private struct ActiveInstance
    {
        public GameObject go;
        public int prefabIndex;
    }

    private System.Collections.Generic.List<ActiveInstance> activeInstances = new System.Collections.Generic.List<ActiveInstance>();
    void Awake()
    {
        SetTimeUtilSpawn();
    }

    void Update()
    {
        _timeUtilSpawn -= Time.deltaTime;

        if (_timeUtilSpawn <= 0f)
        {
            // clean up dead/null instances from tracking
            CleanupActiveInstances();

            if (onlyOneAtOnce && activeInstances.Count > 0)
            {
                // wait until existing enemies are gone
                SetTimeUtilSpawn();
                return;
            }

            int randomIndex = GetSpawnIndex();
            if (randomIndex >= 0)
            {
                GameObject spawned = Instantiate(_enemyPrefab[randomIndex], transform.position, Quaternion.identity);
                activeInstances.Add(new ActiveInstance { go = spawned, prefabIndex = randomIndex });
            }

            SetTimeUtilSpawn();
        }
    }

    private int GetSpawnIndex()
    {
        if (_enemyPrefab == null || _enemyPrefab.Length == 0) return -1;

        if (!preventDuplicateType)
        {
            return Random.Range(0, _enemyPrefab.Length);
        }

        int attempts = _enemyPrefab.Length;
        for (int i = 0; i < attempts; i++)
        {
            int idx = Random.Range(0, _enemyPrefab.Length);
            if (!IsPrefabActive(idx))
                return idx;
        }
        return -1;
    }

    private bool IsPrefabActive(int prefabIndex)
    {
        if (_enemyPrefab == null || prefabIndex < 0 || prefabIndex >= _enemyPrefab.Length) return false;
        string prefabName = _enemyPrefab[prefabIndex].name;
        foreach (var ai in activeInstances)
        {
            var go = ai.go;
            if (go == null) continue;
            if (go.name.StartsWith(prefabName))
                return true;
        }
        return false;
    }

    private void CleanupActiveInstances()
    {
        for (int i = activeInstances.Count - 1; i >= 0; i--)
        {
            var ai = activeInstances[i];
            var go = ai.go;
            if (go == null || !go.activeInHierarchy)
            {
                int prefabIdx = ai.prefabIndex;
                activeInstances.RemoveAt(i);
                if (respawnItem > 0f)
                {
                    StartCoroutine(RespawnAfterDelay(prefabIdx, respawnItem));
                }
            }
        }
    }

    private System.Collections.IEnumerator RespawnAfterDelay(int prefabIndex, float delay)
    {
        yield return new WaitForSeconds(delay);

        // If only one allowed and something else exists, skip respawn
        if (onlyOneAtOnce && activeInstances.Count > 0)
            yield break;

        if (preventDuplicateType && IsPrefabActive(prefabIndex))
            yield break;

        if (_enemyPrefab == null || prefabIndex < 0 || prefabIndex >= _enemyPrefab.Length)
            yield break;

        GameObject spawned = Instantiate(_enemyPrefab[prefabIndex], transform.position, Quaternion.identity);
        activeInstances.Add(new ActiveInstance { go = spawned, prefabIndex = prefabIndex });
    }

    private void SetTimeUtilSpawn()
    {
        // use fixed spawn delay so we know exactly when next spawn appears
        _timeUtilSpawn = Mathf.Max(0f, spawnItem);
   }
}