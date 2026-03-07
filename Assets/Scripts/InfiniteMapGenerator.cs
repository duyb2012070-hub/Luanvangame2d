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

    [Header("Background")]
    public GameObject[] backgroundPrefabs;

    [Header("Map Settings")]
    public float tileWidth = 4f;
    public int startTiles = 12;
    public float spawnDistance = 20f;

    [Header("Background Settings")]
    public float backgroundWidth = 30f;

    [Header("Coin Settings")]
    public float coinStartDistance = 15f;

    [Header("Height Settings")]
    public float minHeight = -1f;
    public float maxHeight = 2f;

    float lastX;
    float lastY;
    float bgLastX;

    void Start()
    {
        lastX = player.position.x;
        lastY = player.position.y - 2f;

        bgLastX = player.position.x - backgroundWidth;

        SpawnStartGround();

        for (int i = 0; i < startTiles; i++)
        {
            GenerateTile();
        }

        // spawn background ban đầu
        for (int i = 0; i < 5; i++)
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

        SpawnBackground();
    }

    void SpawnStartGround()
    {
        Vector3 pos = new Vector3(player.position.x, player.position.y - 2f, 0);

        GameObject ground = groundPrefabs[Random.Range(0, groundPrefabs.Length)];
        Instantiate(ground, pos, Quaternion.identity);

        lastX = pos.x;
        lastY = pos.y;
    }

    void GenerateTile()
    {
        lastX += tileWidth;

        float randomY = lastY + Random.Range(-0.7f, 0.7f);
        randomY = Mathf.Clamp(randomY, minHeight, maxHeight);

        Vector3 spawnPos = new Vector3(lastX, randomY, 0);

        int randomType = Random.Range(0, 100);

        GameObject tile = null;

        if (randomType < 40 && groundPrefabs.Length > 0)
            tile = SpawnPrefab(groundPrefabs, spawnPos);

        else if (randomType < 70 && platformPrefabs.Length > 0)
            tile = SpawnPrefab(platformPrefabs, spawnPos);

        else if (randomType < 90 && islandPrefabs.Length > 0)
            tile = SpawnPrefab(islandPrefabs, spawnPos + Vector3.up * 2f);

        else
            tile = SpawnPrefab(groundPrefabs, spawnPos);

        // spawn coin
        if (tile != null && lastX > coinStartDistance)
        {
            int chance = Random.Range(0, 100);

            if (tile.CompareTag("Island"))
            {
                if (chance < 10)
                    SpawnCoinGroup(tile);
            }
            else
            {
                if (chance < 40)
                    SpawnCoinGroup(tile);
            }
        }

        SpawnTrap(spawnPos);

        lastY = randomY;
    }

    GameObject SpawnPrefab(GameObject[] list, Vector3 pos)
    {
        GameObject prefab = list[Random.Range(0, list.Length)];
        return Instantiate(prefab, pos, Quaternion.identity);
    }

    void SpawnTrap(Vector3 pos)
    {
        if (trapPrefabs.Length == 0) return;

        if (Random.Range(0, 100) < 20)
        {
            GameObject trap = trapPrefabs[Random.Range(0, trapPrefabs.Length)];

            Vector3 trapPos = pos + new Vector3(0, 0.5f, 0);

            Instantiate(trap, trapPos, Quaternion.identity);
        }
    }

    void SpawnCoinGroup(GameObject tile)
    {
        if (coinPrefab == null) return;

        Vector3 basePos = tile.transform.position;

        float baseHeight = basePos.y + 2.8f;

        int pattern = Random.Range(0, 4);

        switch (pattern)
        {
            case 0:
                SpawnLineCoins(basePos, baseHeight);
                break;

            case 1:
                SpawnArcCoins(basePos, baseHeight);
                break;

            case 2:
                SpawnTriangleCoins(basePos, baseHeight);
                break;

            case 3:
                SpawnSquareCoins(basePos, baseHeight);
                break;
        }
    }

    void SpawnLineCoins(Vector3 basePos, float height)
    {
        int count = Random.Range(3, 5);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(basePos.x - 1 + i * 1f, height, 0);
            Instantiate(coinPrefab, pos, Quaternion.identity);
        }
    }

    void SpawnArcCoins(Vector3 basePos, float height)
    {
        int count = 5;

        for (int i = 0; i < count; i++)
        {
            float x = basePos.x - 2 + i * 1f;
            float y = height + Mathf.Sin(i * Mathf.PI / (count - 1)) * 1f;

            Instantiate(coinPrefab, new Vector3(x, y, 0), Quaternion.identity);
        }
    }

    void SpawnTriangleCoins(Vector3 basePos, float height)
    {
        Instantiate(coinPrefab, new Vector3(basePos.x, height + 1f, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(basePos.x - 0.8f, height, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(basePos.x + 0.8f, height, 0), Quaternion.identity);
    }

    void SpawnSquareCoins(Vector3 basePos, float height)
    {
        Instantiate(coinPrefab, new Vector3(basePos.x - 0.8f, height, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(basePos.x + 0.8f, height, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(basePos.x - 0.8f, height + 0.8f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(basePos.x + 0.8f, height + 0.8f, 0), Quaternion.identity);
    }

    void SpawnBackground()
    {
        if (backgroundPrefabs.Length == 0) return;

        if (player.position.x + spawnDistance > bgLastX)
        {
            GameObject bg = backgroundPrefabs[Random.Range(0, backgroundPrefabs.Length)];

            Vector3 pos = new Vector3(bgLastX + backgroundWidth, 0, 10);

            Instantiate(bg, pos, Quaternion.identity);

            bgLastX += backgroundWidth;
        }
    }
}