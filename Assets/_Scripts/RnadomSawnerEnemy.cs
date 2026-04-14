// RandomSpawnerEnemy.cs (Com Spawns Direcionais, Ondas e Altura Y Configurável - SEM DUPLICATAS)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomSpawnerEnemy : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName = "Onda";
        public int numberOfEnemies = 5;
        public GameObject[] enemyPrefabs;
        public float spawnInterval = 1.0f;
    }
    
    
    public StageAnnouncementUI announcementUI;
    public int levelNumber = 1;
    [Header("Configuração das Ondas")]
    public List<Wave> waves;
    public float timeBetweenWaves = 5.0f;
    public float initialSpawnDelay = 3.0f;

    [Header("Área de Spawn")]
    public Vector2 mapCenter = Vector2.zero;
    public Vector2 mapSize = new Vector2(40f, 40f);
    public float spawnEdgeMargin = 2.0f;
    public float spawnHeightY = 0.5f; // Altura Y do spawn

    private int currentWaveIndex = -1;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawningWave = false;
    private bool allWavesCompleted = false;

    public PlantGrowthController plant;

    void Start()
{
    if (waves == null || waves.Count == 0)
    {
        Debug.LogError("Spawner: Nenhuma onda definida!");
        enabled = false;
        return;
    }
    if (announcementUI == null)
{
    announcementUI = FindObjectOfType<StageAnnouncementUI>();
}

if (announcementUI != null)
{
    announcementUI.ShowLevelIntro(levelNumber);
}

    if (mapSize.x <= spawnEdgeMargin * 2 || mapSize.y <= spawnEdgeMargin * 2)
    {
        Debug.LogError("Spawner: Tamanho do mapa muito pequeno!");
        enabled = false;
        return;
    }

    plant = FindObjectOfType<PlantGrowthController>();

    if (announcementUI == null)
    {
        announcementUI = FindObjectOfType<StageAnnouncementUI>();
    }

    if (announcementUI != null)
    {
        announcementUI.ShowLevelIntro(levelNumber);
    }

    StartCoroutine(WaveController());
}

    IEnumerator WaveController()
{
    yield return new WaitForSeconds(initialSpawnDelay);

    while (currentWaveIndex < waves.Count - 1)
    {
        if (!isSpawningWave && activeEnemies.Count == 0)
        {
            if (currentWaveIndex >= 0)
            {
                if (announcementUI != null)
                {
                    announcementUI.ShowWaveIntro(currentWaveIndex + 2);
                }

                Debug.Log($"Esperando {timeBetweenWaves}s para a próxima onda...");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
            else
            {
                if (announcementUI != null)
                {
                    announcementUI.ShowWaveIntro(1);
                }

                yield return new WaitForSeconds(2f);
            }

            currentWaveIndex++;
            Wave currentWave = waves[currentWaveIndex];
            Debug.Log($"Iniciando Onda {currentWaveIndex + 1}/{waves.Count}: {currentWave.waveName}");
            StartCoroutine(SpawnWave(currentWave));
        }

        yield return new WaitForSeconds(1.0f);
    }

    while (activeEnemies.Count > 0)
    {
        yield return new WaitForSeconds(1.0f);
    }

    if (!allWavesCompleted)
    {
        allWavesCompleted = true;
        Debug.Log("****** TODAS AS ONDAS COMPLETADAS! ******");

        if (announcementUI != null)
        {
            announcementUI.ShowFinalObjective();
        }

        Debug.Log("Mandando a planta crescer!");
        plant.GrowPlant();
    }
}

    IEnumerator SpawnWave(Wave wave)
    {
        isSpawningWave = true; // Indica que o spawn começou
        Debug.Log($"Spawnando {wave.numberOfEnemies} inimigos para Onda {currentWaveIndex + 1}...");

        float minX = mapCenter.x - mapSize.x / 2f + spawnEdgeMargin;
        float maxX = mapCenter.x + mapSize.x / 2f - spawnEdgeMargin;
        float minZ = mapCenter.y - mapSize.y / 2f + spawnEdgeMargin;
        float maxZ = mapCenter.y + mapSize.y / 2f - spawnEdgeMargin;

        int enemiesSpawned = 0;
        List<Vector3> usedPoints = new List<Vector3>(); // Mantido, mas não usado para cardinal

        // Spawn Direcional (se aplicável)
        if (wave.numberOfEnemies >= 4)
        {
            SpawnEnemyAt(GetDirectionalSpawnPosition(minX, maxX, minZ, maxZ, 'N'), wave.enemyPrefabs, usedPoints); enemiesSpawned++; yield return new WaitForSeconds(wave.spawnInterval);
            SpawnEnemyAt(GetDirectionalSpawnPosition(minX, maxX, minZ, maxZ, 'S'), wave.enemyPrefabs, usedPoints); enemiesSpawned++; yield return new WaitForSeconds(wave.spawnInterval);
            SpawnEnemyAt(GetDirectionalSpawnPosition(minX, maxX, minZ, maxZ, 'E'), wave.enemyPrefabs, usedPoints); enemiesSpawned++; yield return new WaitForSeconds(wave.spawnInterval);
            SpawnEnemyAt(GetDirectionalSpawnPosition(minX, maxX, minZ, maxZ, 'W'), wave.enemyPrefabs, usedPoints); enemiesSpawned++; yield return new WaitForSeconds(wave.spawnInterval);
        }

        // Spawn Aleatório Restante
        while (enemiesSpawned < wave.numberOfEnemies)
        {
            SpawnEnemyAt(GetRandomSpawnPosition(minX, maxX, minZ, maxZ), wave.enemyPrefabs, usedPoints);
            enemiesSpawned++;
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        isSpawningWave = false; // Indica que o spawn terminou
        Debug.Log($"Spawn da Onda {currentWaveIndex + 1} completo.");
    }

    Vector3 GetDirectionalSpawnPosition(float minX, float maxX, float minZ, float maxZ, char direction)
    {
        float spawnX = Random.Range(minX, maxX);
        float spawnZ = Random.Range(minZ, maxZ);
        switch (direction)
        {
            case 'N': spawnZ = maxZ; break;
            case 'S': spawnZ = minZ; break;
            case 'E': spawnX = maxX; break;
            case 'W': spawnX = minX; break;
        }
        return new Vector3(spawnX, spawnHeightY, spawnZ); // Usa altura Y configurada
    }

    Vector3 GetRandomSpawnPosition(float minX, float maxX, float minZ, float maxZ)
    {
        float spawnX = Random.Range(minX, maxX);
        float spawnZ = Random.Range(minZ, maxZ);
        return new Vector3(spawnX, spawnHeightY, spawnZ); // Usa altura Y configurada
    }

    void SpawnEnemyAt(Vector3 position, GameObject[] allowedPrefabs, List<Vector3> usedPoints)
    {
        if (allowedPrefabs == null || allowedPrefabs.Length == 0) { Debug.LogError("Spawn sem prefabs definidos!"); return; }

        GameObject prefabToSpawn = allowedPrefabs[Random.Range(0, allowedPrefabs.Length)];
        GameObject spawnedEnemy = Instantiate(prefabToSpawn, position, Quaternion.identity);
        activeEnemies.Add(spawnedEnemy);

        HealthSystem health = spawnedEnemy.GetComponent<HealthSystem>();
        if (health != null)
        {
            // Adiciona listener para notificar morte
            EnemyDeathListener listener = spawnedEnemy.AddComponent<EnemyDeathListener>();
            listener.spawner = this;
        } else {
             Debug.LogError($"Prefab {prefabToSpawn.name} não tem HealthSystem!");
        }
    }

    // Chamada pelo EnemyDeathListener quando um inimigo morre
    public void ReportEnemyDeath(GameObject deadEnemy)
    {
        if (activeEnemies.Contains(deadEnemy))
        {
            activeEnemies.Remove(deadEnemy);
            Debug.Log($"Inimigo removido. Restantes na onda: {activeEnemies.Count}");
            // A lógica de iniciar a próxima onda está no WaveController

        }

      
    }
    void OnDrawGizmosSelected()
{
    // Área total do mapa
    Gizmos.color = Color.white;
    Vector3 center = new Vector3(mapCenter.x, spawnHeightY, mapCenter.y);
    Vector3 size = new Vector3(mapSize.x, 0.1f, mapSize.y);
    Gizmos.DrawWireCube(center, size);

    // Área válida de spawn (com margem)
    float innerWidth = mapSize.x - (spawnEdgeMargin * 2f);
    float innerHeight = mapSize.y - (spawnEdgeMargin * 2f);

    Gizmos.color = Color.green;
    Vector3 innerSize = new Vector3(innerWidth, 0.1f, innerHeight);
    Gizmos.DrawWireCube(center, innerSize);

    // Pontos direcionais (N, S, E, W)
    float minX = mapCenter.x - mapSize.x / 2f + spawnEdgeMargin;
    float maxX = mapCenter.x + mapSize.x / 2f - spawnEdgeMargin;
    float minZ = mapCenter.y - mapSize.y / 2f + spawnEdgeMargin;
    float maxZ = mapCenter.y + mapSize.y / 2f - spawnEdgeMargin;

    Gizmos.color = Color.red;

    // Norte
    Gizmos.DrawSphere(new Vector3(0f, spawnHeightY, maxZ), 0.5f);

    // Sul
    Gizmos.DrawSphere(new Vector3(0f, spawnHeightY, minZ), 0.5f);

    // Leste
    Gizmos.DrawSphere(new Vector3(maxX, spawnHeightY, 0f), 0.5f);

    // Oeste
    Gizmos.DrawSphere(new Vector3(minX, spawnHeightY, 0f), 0.5f);
}
}