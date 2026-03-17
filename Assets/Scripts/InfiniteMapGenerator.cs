using UnityEngine;

public class InfiniteMapGenerator : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Ground")]
    public GameObject[] groundPrefabs;

    [Header("Platform")]
    public GameObject[] platformPrefabs;

    [Header("Island")]
    public GameObject[] islandPrefabs;

    [Header("Trap")]
    public GameObject[] trapPrefabs;

    [Header("Enemy")]
    public GameObject[] enemyPrefabs;

    [Header("Coin")]
    public GameObject coinPrefab;

    [Header("Heart")]
    public GameObject heartPrefab;

    [Header("Checkpoint")]
    public GameObject checkpointPrefab;

    [Header("Background")]
    public GameObject[] backgroundPrefabs;
    public float backgroundWidth = 30f;

    [Header("Left Block")]
    public GameObject leftBlockPrefab;

    [Header("Map Settings")]
    public int startTiles = 12;
    public float spawnDistance = 30f;

    float minGap;
    float maxGap;

    float lastX;
    float lastY;
    float lastCheckpointX;
    float bgLastX;

    int trapChance;
    int enemyChance;
    int islandChance;
    int coinChance;

    int trapCooldown;
    int enemyCooldown;

    bool mapStarted = false;

    void Start()
    {
        ApplyDifficulty();
        StartMap();
    }

    void Update()
    {
        if (!mapStarted) return;

        if (player.position.x + spawnDistance > lastX)
        {
            GenerateTile();
        }

        if (player.position.x + spawnDistance > bgLastX)
        {
            SpawnBackground();
        }
    }

    void ApplyDifficulty()
    {
        int d = DifficultyManager.difficulty;

        if (d == 0)
        {
            minGap = 2.5f;
            maxGap = 4f;

            trapChance = 8;
            enemyChance = 15;
            islandChance = 10;
            coinChance = 85;
        }
        else if (d == 1)
        {
            minGap = 3.5f;
            maxGap = 5.5f;

            trapChance = 25;
            enemyChance = 30;
            islandChance = 20;
            coinChance = 60;
        }
        else
        {
            minGap = 4.5f;
            maxGap = 6f;

            trapChance = 65;
            enemyChance = 55;
            islandChance = 40;
            coinChance = 40;
        }
    }

    public void StartMap()
    {
        lastX = player.position.x;
        lastY = player.position.y - 2f;
        lastCheckpointX = player.position.x;

        SpawnStartGround();
        SpawnLeftBlock();

        for (int i = 0; i < startTiles; i++)
        {
            GenerateTile();
        }

        bgLastX = player.position.x;

        for (int i = 0; i < 6; i++)
        {
            SpawnBackground();
        }

        mapStarted = true;
    }

    void GenerateTile()
    {
        float gap = Random.Range(minGap, maxGap);
        lastX += gap;

        float height = lastY + Random.Range(-1.5f, 1.5f);
        height = Mathf.Clamp(height, -1f, 3f);

        Vector3 spawnPos = new Vector3(lastX, height, 0);

        int type = Random.Range(0, 100);

        GameObject tile = null;
        bool isGround = false;

        if (type < 50)
        {
            tile = SpawnPrefab(groundPrefabs, spawnPos);
            isGround = true;
        }
        else if (type < 80)
        {
            tile = SpawnPrefab(platformPrefabs, spawnPos);
        }
        else if (type < 80 + islandChance)
        {
            spawnPos.y += 2.5f;
            tile = SpawnPrefab(islandPrefabs, spawnPos);
        }
        else
        {
            tile = SpawnPrefab(platformPrefabs, spawnPos);
        }

        if (tile == null) return;

        Collider2D col = tile.GetComponent<Collider2D>();
        float topY = col.bounds.max.y;

        bool trapSpawned = SpawnTrap(tile, topY);

        if (!trapSpawned)
        {
            SpawnEnemy(tile, topY);
            SpawnCoinPattern(tile, topY);
        }

        SpawnHeart(tile, topY);
        SpawnCheckpoint(tile, isGround, trapSpawned, topY);

        lastY = height;
    }

    GameObject SpawnPrefab(GameObject[] list, Vector3 pos)
    {
        if (list.Length == 0) return null;

        GameObject prefab = list[Random.Range(0, list.Length)];

        return Instantiate(prefab, pos, Quaternion.identity);
    }

    bool SpawnTrap(GameObject tile, float topY)
    {
        if (trapPrefabs.Length == 0) return false;

        if (trapCooldown > 0)
        {
            trapCooldown--;
            return false;
        }

        if (Random.Range(0, 100) > trapChance) return false;

        Collider2D tileCol = tile.GetComponent<Collider2D>();

        float spawnX = Random.Range(
            tileCol.bounds.min.x + 1f,
            tileCol.bounds.max.x - 1f
        );

        float spawnY = topY + Random.Range(5f, 7f);

        GameObject trapPrefab = trapPrefabs[Random.Range(0, trapPrefabs.Length)];

        Instantiate(trapPrefab, new Vector3(spawnX, spawnY, 0), Quaternion.identity);

        trapCooldown = 2;

        return true;
    }

    void SpawnEnemy(GameObject tile, float topY)
    {
        if (enemyPrefabs.Length == 0) return;

        if (enemyCooldown > 0)
        {
            enemyCooldown--;
            return;
        }

        if (Random.Range(0, 100) > enemyChance) return;

        Collider2D tileCol = tile.GetComponent<Collider2D>();

        float spawnX = Random.Range(
            tileCol.bounds.min.x + 1f,
            tileCol.bounds.max.x - 1f
        );

        float spawnY = topY + Random.Range(5f, 7f);

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Instantiate(enemyPrefab, new Vector3(spawnX, spawnY, 0), Quaternion.identity);

        enemyCooldown = 2;
    }

    void SpawnCoinPattern(GameObject tile, float topY)
    {
        if (coinPrefab == null) return;

        if (Random.Range(0, 100) > coinChance) return;

        Collider2D col = tile.GetComponent<Collider2D>();

        float centerX = col.bounds.center.x;
        float baseY = topY + 2.8f;

        int pattern = Random.Range(0, 4);

        switch (pattern)
        {
            case 0:
                SpawnCoinLine(centerX, baseY);
                break;

            case 1:
                SpawnCoinArc(centerX, baseY);
                break;

            case 2:
                SpawnCoinTriangle(centerX, baseY);
                break;

            case 3:
                SpawnCoinDiamond(centerX, baseY);
                break;
        }
    }

    void SpawnCoinLine(float x, float y)
    {
        float space = 0.8f;

        for (int i = -3; i <= 3; i++)
        {
            Instantiate(coinPrefab, new Vector3(x + i * space, y, 0), Quaternion.identity);
        }
    }

    void SpawnCoinArc(float x, float y)
    {
        float space = 0.8f;

        for (int i = -3; i <= 3; i++)
        {
            float arcY = y + Mathf.Abs(i) * 0.6f;

            Instantiate(coinPrefab, new Vector3(x + i * space, arcY, 0), Quaternion.identity);
        }
    }

    void SpawnCoinTriangle(float x, float y)
    {
        float space = 0.8f;

        Instantiate(coinPrefab, new Vector3(x, y + 2f, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(x - space, y + 1f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x + space, y + 1f, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(x - space * 2, y, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x, y, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x + space * 2, y, 0), Quaternion.identity);
    }

    void SpawnCoinDiamond(float x, float y)
    {
        float space = 0.8f;

        Instantiate(coinPrefab, new Vector3(x, y + 2f, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(x - space, y + 1f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x + space, y + 1f, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(x, y, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(x - space, y - 1f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x + space, y - 1f, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(x, y - 2f, 0), Quaternion.identity);
    }

    void SpawnHeart(GameObject tile, float topY)
    {
        if (heartPrefab == null) return;

        if (Random.Range(0, 100) > 20) return;

        Collider2D col = tile.GetComponent<Collider2D>();

        float spawnX = Random.Range(
            col.bounds.min.x + 1f,
            col.bounds.max.x - 1f
        );

        Vector3 pos = new Vector3(spawnX, topY + 3f, 0);

        Instantiate(heartPrefab, pos, Quaternion.identity);
    }

    void SpawnCheckpoint(GameObject tile, bool isGround, bool trapSpawned, float topY)
    {
        if (checkpointPrefab == null) return;

        if (!isGround) return;
        if (trapSpawned) return;

        if (lastX - lastCheckpointX < 60f) return;

        Vector3 pos = new Vector3(tile.transform.position.x, topY + 1.2f, 0);

        Instantiate(checkpointPrefab, pos, Quaternion.identity);

        lastCheckpointX = lastX;
    }

    void SpawnBackground()
    {
        if (backgroundPrefabs.Length == 0) return;

        GameObject bg = backgroundPrefabs[Random.Range(0, backgroundPrefabs.Length)];

        float spawnX = bgLastX + backgroundWidth;

        GameObject mainBG = Instantiate(bg, new Vector3(spawnX, 0, 10), Quaternion.identity);

        float height = mainBG.GetComponent<SpriteRenderer>().bounds.size.y;

        Instantiate(bg, new Vector3(spawnX, height, 10), Quaternion.identity);
        Instantiate(bg, new Vector3(spawnX, -height, 10), Quaternion.identity);

        bgLastX = spawnX;
    }

    void SpawnStartGround()
    {
        Vector3 pos = new Vector3(player.position.x, player.position.y - 2f, 0);

        GameObject ground = groundPrefabs[Random.Range(0, groundPrefabs.Length)];

        Instantiate(ground, pos, Quaternion.identity);

        lastX = pos.x;
        lastY = pos.y;
    }

    void SpawnLeftBlock()
    {
        if (leftBlockPrefab == null) return;

        Vector3 spawnPos = new Vector3(
            player.position.x - 3.5f,
            player.position.y - 2f,
            0
        );

        GameObject block = Instantiate(leftBlockPrefab, spawnPos, Quaternion.identity);

        block.transform.localScale = new Vector3(2f, 25f, 1f);
    }
}