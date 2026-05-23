using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] GameObject[] _enemyPrefab;
    [SerializeField] float _minimumSpawnTime;
    [SerializeField] float _maximumSpawnTime;
    private float _timeUtilSpawn;
    [Header("Spawn Options")]
    [SerializeField] private bool onlyOneAtOnce = false;
    [SerializeField] private bool preventDuplicateType = true;

    // track active instances we've spawned
    private System.Collections.Generic.List<GameObject> activeInstances = new System.Collections.Generic.List<GameObject>();

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
                activeInstances.Add(spawned);
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
        foreach (var go in activeInstances)
        {
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
            var go = activeInstances[i];
            if (go == null || !go.activeInHierarchy)
            {
                activeInstances.RemoveAt(i);
            }
        }
    }

    private void SetTimeUtilSpawn()
    {
        _timeUtilSpawn = Random.Range(_minimumSpawnTime, _maximumSpawnTime);
   }
}