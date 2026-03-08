using UnityEngine;

public class InfiniteMapGenerator : MonoBehaviour
{
    // ================= PLAYER =================
    [Header("Player")]
    public Transform player; // tham chiếu tới player để biết vị trí player

    // ================= GROUND =================
    [Header("Ground")]
    public GameObject[] groundPrefabs; // danh sách prefab mặt đất

    // ================= PLATFORM =================
    [Header("Platform")]
    public GameObject[] platformPrefabs; // danh sách prefab platform

    // ================= ISLAND =================
    [Header("Island")]
    public GameObject[] islandPrefabs; // các đảo nổi

    // ================= TRAP =================
    [Header("Trap")]
    public GameObject[] trapPrefabs; // các bẫy

    // ================= COIN =================
    [Header("Coin")]
    public GameObject coinPrefab; // prefab coin

    // ================= BACKGROUND =================
    [Header("Background")]
    public GameObject[] backgroundPrefabs; // background lặp lại

    // ================= CHECKPOINT =================
    [Header("Checkpoint")]
    public GameObject checkpointPrefab; // prefab checkpoint
    public float checkpointDistance = 60f; // khoảng cách giữa các checkpoint

    // ================= MAP SETTINGS =================
    [Header("Map Settings")]
    public float tileWidth = 4f; // chiều rộng mỗi tile
    public int startTiles = 12; // số tile spawn lúc đầu
    public float spawnDistance = 20f; // khoảng cách player để sinh map mới

    // ================= BACKGROUND SETTINGS =================
    [Header("Background Settings")]
    public float backgroundWidth = 30f; // chiều rộng background

    // ================= COIN SETTINGS =================
    [Header("Coin Settings")]
    public float coinStartDistance = 15f; // khoảng cách bắt đầu spawn coin

    // ================= HEIGHT SETTINGS =================
    [Header("Height Settings")]
    public float minHeight = -1f; // chiều cao tối thiểu của map
    public float maxHeight = 2f;  // chiều cao tối đa của map

    // ================= LEFT BLOCK =================
    [Header("Left Block Settings")]
    public GameObject leftBlockPrefab; // prefab vật cản phía sau player
    public float leftBlockDistance = 3f; // khoảng cách vật cản so với player

    // ================= BIẾN LƯU TRẠNG THÁI =================
    float lastX; // vị trí X của tile cuối
    float lastY; // vị trí Y của tile cuối
    float bgLastX; // vị trí background cuối
    float lastCheckpointX; // vị trí checkpoint cuối

    int seed; // seed dùng để random map

    // ================= ĐỘ KHÓ =================
    int trapChance = 20; // % spawn trap
    float heightVariation = 0.7f; // độ thay đổi chiều cao map
    int islandChance = 20; // % spawn island
    int platformChance = 30; // % spawn platform

    void Start()
    {
        // lấy seed từ GameManager
        seed = GameManager.instance.mapSeed;

        // khởi tạo random theo seed
        Random.InitState(seed);

        // thiết lập độ khó
        SetDifficulty();

        // vị trí bắt đầu map
        lastX = player.position.x;
        lastY = player.position.y - 2f;

        // background bắt đầu phía sau player
        bgLastX = player.position.x - backgroundWidth;

        // checkpoint đầu
        lastCheckpointX = player.position.x;

        // spawn vật cản bên trái player
        SpawnLeftBlock();

        // spawn mặt đất ban đầu
        SpawnStartGround();

        // sinh map ban đầu
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

    // ================= THIẾT LẬP ĐỘ KHÓ =================
    void SetDifficulty()
    {
        int diff = GameManager.instance.difficulty;

        // EASY
        if (diff == 0)
        {
            trapChance = 5;
            heightVariation = 0.3f;
            islandChance = 10;
            platformChance = 20;
        }

        // NORMAL
        if (diff == 1)
        {
            trapChance = 20;
            heightVariation = 0.7f;
            islandChance = 20;
            platformChance = 30;
        }

        // HARD
        if (diff == 2)
        {
            trapChance = 40;
            heightVariation = 1.2f;
            islandChance = 35;
            platformChance = 35;
        }
    }

    void Update()
    {
        // nếu player tiến gần tile cuối → sinh thêm map
        if (player.position.x + spawnDistance > lastX)
        {
            GenerateTile();
        }

        // spawn thêm background
        SpawnBackground();
    }

    // ================= SPAWN CỘT CHẶN BÊN TRÁI =================
    void SpawnLeftBlock()
    {
        if (leftBlockPrefab == null) return;

        // đặt block ngay phía sau tile đầu tiên
        float x = player.position.x - tileWidth;

        // đặt block thấp xuống để nó đứng từ dưới đất lên
        float y = player.position.y - 4f;

        Vector3 pos = new Vector3(x, y, 0);

        GameObject block = Instantiate(leftBlockPrefab, pos, Quaternion.identity);

        // làm block cao để player không nhảy qua
        block.transform.localScale = new Vector3(2f, 20f, 1f);
    }

    // ================= SPAWN MẶT ĐẤT BAN ĐẦU =================
    void SpawnStartGround()
    {
        Vector3 pos = new Vector3(player.position.x, player.position.y - 2f, 0);

        GameObject ground = groundPrefabs[Random.Range(0, groundPrefabs.Length)];

        Instantiate(ground, pos, Quaternion.identity);

        lastX = pos.x;
        lastY = pos.y;
    }

    // ================= SINH TILE MỚI =================
    void GenerateTile()
    {
        // tăng vị trí X
        lastX += tileWidth;

        // random chiều cao
        float randomY = lastY + Random.Range(-heightVariation, heightVariation);

        // giới hạn chiều cao
        randomY = Mathf.Clamp(randomY, minHeight, maxHeight);

        Vector3 spawnPos = new Vector3(lastX, randomY, 0);

        int randomType = Random.Range(0, 100);

        GameObject tile = null;
        bool isIsland = false;
        bool isGround = false;

        // spawn ground
        if (randomType < 50 && groundPrefabs.Length > 0)
        {
            tile = SpawnPrefab(groundPrefabs, spawnPos);
            isGround = true;
        }
        // spawn platform
        else if (randomType < 50 + platformChance && platformPrefabs.Length > 0)
        {
            tile = SpawnPrefab(platformPrefabs, spawnPos);
        }
        // spawn island
        else if (randomType < 50 + platformChance + islandChance && islandPrefabs.Length > 0)
        {
            tile = SpawnPrefab(islandPrefabs, spawnPos + Vector3.up * 2f);
            isIsland = true;
        }
        // fallback ground
        else
        {
            tile = SpawnPrefab(groundPrefabs, spawnPos);
            isGround = true;
        }

        // spawn coin
        if (tile != null && lastX > coinStartDistance)
        {
            int chance = Random.Range(0, 100);

            if (isIsland)
            {
                if (chance < 15)
                    SpawnCoinGroup(tile);
            }
            else
            {
                if (chance < 40)
                    SpawnCoinGroup(tile);
            }
        }

        bool trapSpawned = false;

        // spawn trap
        if (!isIsland)
        {
            trapSpawned = SpawnTrap(spawnPos);
        }

        // spawn checkpoint
        SpawnCheckpoint(tile, isGround, trapSpawned);

        lastY = randomY;
    }

    // ================= SPAWN PREFAB RANDOM =================
    GameObject SpawnPrefab(GameObject[] list, Vector3 pos)
    {
        if (list.Length == 0) return null;

        GameObject prefab = list[Random.Range(0, list.Length)];

        return Instantiate(prefab, pos, Quaternion.identity);
    }

    // ================= SPAWN TRAP =================
    bool SpawnTrap(Vector3 pos)
    {
        if (trapPrefabs.Length == 0) return false;

        if (Random.Range(0, 100) < trapChance)
        {
            GameObject trap = trapPrefabs[Random.Range(0, trapPrefabs.Length)];

            Vector3 trapPos = pos + new Vector3(0, 0.5f, 0);

            Instantiate(trap, trapPos, Quaternion.identity);

            return true;
        }

        return false;
    }

    // ================= SPAWN CHECKPOINT =================
    void SpawnCheckpoint(GameObject tile, bool isGround, bool trapSpawned)
    {
        if (checkpointPrefab == null) return;

        if (!isGround) return;
        if (trapSpawned) return;

        if (lastX - lastCheckpointX < checkpointDistance) return;

        Vector3 pos = tile.transform.position + new Vector3(0, 1.5f, 0);

        Instantiate(checkpointPrefab, pos, Quaternion.identity);

        lastCheckpointX = lastX;
    }

    // ================= SPAWN COIN GROUP =================
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

    // ================= COIN LINE =================
    void SpawnLineCoins(Vector3 basePos, float height)
    {
        int count = Random.Range(3, 5);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(basePos.x - 1 + i, height, 0);
            Instantiate(coinPrefab, pos, Quaternion.identity);
        }
    }

    // ================= COIN ARC =================
    void SpawnArcCoins(Vector3 basePos, float height)
    {
        int count = 5;

        for (int i = 0; i < count; i++)
        {
            float x = basePos.x - 2 + i;
            float y = height + Mathf.Sin(i * Mathf.PI / (count - 1));

            Instantiate(coinPrefab, new Vector3(x, y, 0), Quaternion.identity);
        }
    }

    // ================= COIN TRIANGLE =================
    void SpawnTriangleCoins(Vector3 basePos, float height)
    {
        Instantiate(coinPrefab, new Vector3(basePos.x, height + 1f, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(basePos.x - 0.8f, height, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(basePos.x + 0.8f, height, 0), Quaternion.identity);
    }

    // ================= COIN SQUARE =================
    void SpawnSquareCoins(Vector3 basePos, float height)
    {
        Instantiate(coinPrefab, new Vector3(basePos.x - 0.8f, height, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(basePos.x + 0.8f, height, 0), Quaternion.identity);

        Instantiate(coinPrefab, new Vector3(basePos.x - 0.8f, height + 0.8f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(basePos.x + 0.8f, height + 0.8f, 0), Quaternion.identity);
    }

    // ================= SPAWN BACKGROUND =================
    void SpawnBackground()
    {
        if (backgroundPrefabs.Length == 0) return;

        if (player.position.x + spawnDistance > bgLastX)
        {
            GameObject bg = backgroundPrefabs[Random.Range(0, backgroundPrefabs.Length)];

            float spawnX = bgLastX + backgroundWidth;

            // spawn background chính
            GameObject mainBG = Instantiate(bg, new Vector3(spawnX, 0, 10), Quaternion.identity);

            // lấy chiều cao sprite
            float bgHeight = mainBG.GetComponent<SpriteRenderer>().bounds.size.y;

            // spawn background ngay dưới (khít)
            Vector3 bottomPos = new Vector3(spawnX, -bgHeight, 10);

            Instantiate(bg, bottomPos, Quaternion.identity);

            bgLastX += backgroundWidth;
        }
    }
}