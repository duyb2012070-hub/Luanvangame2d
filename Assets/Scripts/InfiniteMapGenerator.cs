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

    [Header("Coin")]
    public GameObject coinPrefab;

    [Header("Checkpoint")]
    public GameObject checkpointPrefab;

    [Header("Background")]
    public GameObject[] backgroundPrefabs;
    public float backgroundWidth = 30f;

    [Header("Left Block")]
    public GameObject leftBlockPrefab;

    [Header("Map Settings")]
    public int startTiles = 12;
    public float spawnDistance = 25f;

    [Header("Gap")]
    public float minGap = 3.5f;
    public float maxGap = 6f;

    [Header("Height")]
    public float minHeight = -1f;
    public float maxHeight = 4f;
    public float heightVariation = 2f;

    float lastX;
    float lastY;
    float lastCheckpointX;
    float bgLastX;

    int trapChance = 30;
    int islandChance = 20;

    int trapStreak = 0;
    int maxTrapStreak = 2;

    int coinCooldown = 0;

    void Start()
    {
        Random.InitState(System.Environment.TickCount);

        SetDifficulty();

        lastX = player.position.x;
        lastY = player.position.y - 2f;
        lastCheckpointX = player.position.x;
        bgLastX = player.position.x;

        SpawnLeftBlock();
        SpawnStartGround();

        for (int i = 0; i < startTiles; i++)
        {
            GenerateTile();
        }

        for (int i = 0; i < 6; i++)
        {
            SpawnBackground();
        }
    }

    void Update()
    {
        if (player.position.x + spawnDistance > lastX)
        {
            GenerateTile();
        }

        if (player.position.x + spawnDistance > bgLastX)
        {
            SpawnBackground();
        }
    }

    void SetDifficulty()
    {
        int diff = GameManager.instance.difficulty;

        if (diff == 0)
        {
            trapChance = 10;
            minGap = 3f;
            maxGap = 4.5f;
            heightVariation = 1f;
        }

        if (diff == 1)
        {
            trapChance = 30;
            minGap = 4f;
            maxGap = 5.5f;
            heightVariation = 1.8f;
        }

        if (diff == 2)
        {
            trapChance = 55;
            minGap = 4.5f;
            maxGap = 6f;
            heightVariation = 2.3f;
        }
    }

    void GenerateTile()
    {
        float gap = Random.Range(minGap, maxGap);
        lastX += gap;

        float randomY = lastY + Random.Range(-heightVariation, heightVariation);

        float maxUp = 2f;
        float maxDown = 2.5f;

        randomY = Mathf.Clamp(randomY, lastY - maxDown, lastY + maxUp);
        randomY = Mathf.Clamp(randomY, minHeight, maxHeight);

        Vector3 spawnPos = new Vector3(lastX, randomY, 0);

        int type = Random.Range(0, 100);

        GameObject tile = null;
        bool isIsland = false;
        bool isGround = false;

        if (type < 40)
        {
            tile = SpawnPrefab(groundPrefabs, spawnPos);
            isGround = true;
        }
        else if (type < 70)
        {
            tile = SpawnPrefab(platformPrefabs, spawnPos);
        }
        else if (type < 70 + islandChance)
        {
            tile = SpawnPrefab(islandPrefabs, spawnPos + Vector3.up * 2.5f);
            isIsland = true;
        }
        else
        {
            tile = SpawnPrefab(platformPrefabs, spawnPos);
        }

        if (tile == null) return;

        Collider2D col = tile.GetComponent<Collider2D>();
        float topY = col.bounds.max.y;

        bool trapSpawned = false;

        if (!isIsland)
        {
            trapSpawned = SpawnTrap(tile, topY);
        }

        if (!isIsland && !trapSpawned)
        {
            SpawnCoinGroup(tile, topY);
        }

        SpawnCheckpoint(tile, isGround, trapSpawned, topY);

        lastY = randomY;
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

        if (trapStreak >= maxTrapStreak)
        {
            trapStreak = 0;
            return false;
        }

        if (Random.Range(0, 100) < trapChance)
        {
            GameObject trap = trapPrefabs[Random.Range(0, trapPrefabs.Length)];

            float offsetX = Random.Range(-0.4f, 0.4f);

            Vector3 pos = tile.transform.position;

            Vector3 trapPos = new Vector3(pos.x + offsetX, topY + 0.4f, 0);

            Instantiate(trap, trapPos, Quaternion.identity);

            trapStreak++;

            return true;
        }

        trapStreak = 0;

        return false;
    }

    void SpawnCoinGroup(GameObject tile, float topY)
    {
        if (coinPrefab == null) return;

        if (coinCooldown > 0)
        {
            coinCooldown--;
            return;
        }

        if (Random.Range(0, 100) > 35) return;

        Vector3 basePos = tile.transform.position;

        float height = topY + 1.5f;

        for (int i = 0; i < 3; i++)
        {
            Instantiate(
                coinPrefab,
                new Vector3(basePos.x - 0.8f + i * 0.8f, height, 0),
                Quaternion.identity
            );
        }

        coinCooldown = 2;
    }

    void SpawnCheckpoint(GameObject tile, bool isGround, bool trapSpawned, float topY)
    {
        if (checkpointPrefab == null) return;

        if (!isGround) return;
        if (trapSpawned) return;

        if (lastX - lastCheckpointX < 60f) return;

        Vector3 pos = new Vector3(tile.transform.position.x, topY + 1.1f, 0);

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

        float x = player.position.x - 6f;
        float y = player.position.y - 5f;

        GameObject block = Instantiate(leftBlockPrefab, new Vector3(x, y, 0), Quaternion.identity);

        block.transform.localScale = new Vector3(3f, 25f, 1f);
    }
}