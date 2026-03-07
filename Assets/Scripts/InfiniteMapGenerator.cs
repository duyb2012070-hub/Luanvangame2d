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

    [Header("Background")]
    public GameObject[] backgroundPrefabs;

    [Header("Map Settings")]
    public float tileWidth = 4f;
    public int startTiles = 12;
    public float spawnDistance = 20f;

    [Header("Height Settings")]
    public float minHeight = -1f;
    public float maxHeight = 2f;

    float lastX;
    float lastY;

    float bgLastX = -100;

    void Start()
    {
        lastX = player.position.x;
        lastY = player.position.y - 1;

        SpawnStartGround();

        for (int i = 0; i < startTiles; i++)
        {
            GenerateTile();
        }
    }

    void Update()
    {
        if (player.position.x + spawnDistance > lastX)
        {
            GenerateTile();
        }
    }

    void SpawnStartGround()
    {
        Vector3 pos = new Vector3(player.position.x, player.position.y - 1.5f, 0);

        GameObject ground = groundPrefabs[Random.Range(0, groundPrefabs.Length)];

        Instantiate(ground, pos, Quaternion.identity);

        lastX = pos.x;
        lastY = pos.y;
    }

    void GenerateTile()
    {
        lastX += tileWidth;

        float randomY = lastY + Random.Range(-1f, 1.2f);
        randomY = Mathf.Clamp(randomY, minHeight, maxHeight);

        Vector3 spawnPos = new Vector3(lastX, randomY, 0);

        int randomType = Random.Range(0, 100);

        if (randomType < 40 && groundPrefabs.Length > 0)
        {
            SpawnPrefab(groundPrefabs, spawnPos);
        }
        else if (randomType < 70 && platformPrefabs.Length > 0)
        {
            SpawnPrefab(platformPrefabs, spawnPos);
        }
        else if (randomType < 90 && islandPrefabs.Length > 0)
        {
            SpawnPrefab(islandPrefabs, spawnPos + Vector3.up * 2f);
        }
        else
        {
            SpawnPrefab(groundPrefabs, spawnPos);
        }

        SpawnTrap(spawnPos);

        SpawnBackground();

        lastY = randomY;
    }

    void SpawnPrefab(GameObject[] list, Vector3 pos)
    {
        GameObject prefab = list[Random.Range(0, list.Length)];

        Instantiate(prefab, pos, Quaternion.identity);
    }

    void SpawnTrap(Vector3 pos)
    {
        if (trapPrefabs.Length == 0) return;

        int chance = Random.Range(0, 100);

        if (chance < 20)
        {
            GameObject trap = trapPrefabs[Random.Range(0, trapPrefabs.Length)];

            Instantiate(trap, pos + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }
    }

    void SpawnBackground()
    {
        if (backgroundPrefabs.Length == 0) return;

        if (lastX - bgLastX < 15f) return;

        bgLastX = lastX;

        GameObject bg = backgroundPrefabs[Random.Range(0, backgroundPrefabs.Length)];

        Vector3 pos = new Vector3(lastX, 0, 10);

        Instantiate(bg, pos, Quaternion.identity);
    }
}