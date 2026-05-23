using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Time")]
    [SerializeField] private float minimumSpawnTime = 1f;

    [SerializeField] private float maximumSpawnTime = 3f;

    private float spawnTimer;
    private GameObject currentEnemy;

    private void OnEnable()
    {
        SetSpawnTimer();
    }

    private void Update()
    {
        if (currentEnemy != null)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        SpawnEnemy();

        SetSpawnTimer();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        int randomIndex =
            Random.Range(0, enemyPrefabs.Length);

        GameObject prefab =
            enemyPrefabs[randomIndex];

        if (prefab == null)
            return;

        currentEnemy =
            Instantiate(
                prefab,
                transform.position,
                Quaternion.identity
            );

        // Daftarkan spawner ini ke EnemyHealth agar bisa
        // menerima notifikasi saat enemy mati dan spawn lagi
        EnemyHealth health =
            currentEnemy.GetComponentInChildren<EnemyHealth>();

        if (health != null)
            health.SetOwnerSpawn(this);
    }

    private void SetSpawnTimer()
    {
        spawnTimer =
            Random.Range(
                minimumSpawnTime,
                maximumSpawnTime
            );
    }

    // Dipanggil enemy saat benar-benar destroy
    public void ClearCurrentEnemy(GameObject enemy)
    {
        if (currentEnemy == enemy)
        {
            currentEnemy = null;
        }
    }
}