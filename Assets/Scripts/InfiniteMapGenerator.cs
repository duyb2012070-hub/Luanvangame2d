using UnityEngine;

/// <summary>
/// Lớp quản lý sinh bản đồ vô tận (Infinite Procedural Generation).
/// Tự động tạo địa hình, phông nền và vật thể dựa trên vị trí của người chơi.
/// </summary>
public class InfiniteMapGenerator : MonoBehaviour
{
    #region KHAI BÁO BIẾN (VARIABLES)
    [Header("--- Tham chiếu đối tượng ---")]
    public Transform player;
    public GameObject leftBlockPrefab;

    [Header("--- Danh sách Prefabs Địa hình ---")]
    public GameObject[] groundPrefabs;
    public GameObject[] platformPrefabs;
    public GameObject[] islandPrefabs;

    [Header("--- Danh sách Prefabs Vật cản & Vật phẩm ---")]
    public GameObject[] trapPrefabs;
    public GameObject[] enemyPrefabs;
    public GameObject coinPrefab;
    public GameObject heartPrefab;
    public GameObject checkpointPrefab;

    [Header("--- Danh sách Prefabs Phông nền ---")]
    public GameObject[] backgroundPrefabs;

    [Header("--- Cấu hình Map ---")]
    public float backgroundWidth = 30f;
    public int startTiles = 12;
    public float spawnDistance = 35f;

    // Các biến điều khiển logic nội bộ
    private float minGap, maxGap;
    private float lastX, lastY;
    private float lastCheckpointX, bgLastX;
    private int trapChance, enemyChance, islandChance, coinChance;
    private int trapCooldown, enemyCooldown;

    private bool mapStarted = false;
    private bool isLoadedFromSave = false;
    #endregion

    #region KHỞI TẠO (INITIALIZATION)
    /// <summary>
    /// Nhận mốc X xa nhất từ hệ thống lưu trữ để tiếp tục sinh map từ điểm đó.
    /// </summary>
    public void SetLastX(float x)
    {
        this.lastX = x;
        Debug.Log("🏁 Generator tiếp tục sinh từ mốc X: " + lastX);
    }

    /// <summary>
    /// Hàm khởi tạo chính cho bản đồ.
    /// </summary>
    public void InitializeMap(bool loadingFromSave)
    {
        this.isLoadedFromSave = loadingFromSave;
        ApplyDifficulty();

        // 1. XỬ LÝ LEFT BLOCK: Luôn chốt ở vị trí bắt đầu cố định để giới hạn biên trái
        SpawnLeftBlockFixed();

        // --- TOÁN HỌC BACKGROUND TUYỆT ĐỐI ---
        int currentTileIndex = Mathf.FloorToInt(player.position.x / backgroundWidth);
        bgLastX = (currentTileIndex - 2) * backgroundWidth;

        if (!isLoadedFromSave)
        {
            // CHẾ ĐỘ CHƠI MỚI: Bắt đầu từ tọa độ của người chơi
            lastX = player.position.x;
            lastY = player.position.y - 2f;
            SpawnStartGround();

            // Sinh phông nền và các ô đất ban đầu
            for (int i = 0; i < 10; i++) SpawnBackground();
            for (int i = 0; i < startTiles; i++) GenerateTile();
        }
        else
        {
            // CHẾ ĐỘ LOAD SAVE: Phục hồi trạng thái phông nền từ vị trí lưu
            lastY = player.position.y - 2f;
            while (bgLastX < player.position.x + spawnDistance)
            {
                SpawnBackground();
            }
        }

        lastCheckpointX = player.position.x;
        mapStarted = true;
    }
    #endregion

    #region VÒNG LẶP CẬP NHẬT (UPDATE)
    void Update()
    {
        if (!mapStarted || player == null) return;

        // Sinh thêm địa hình (Tile) khi người chơi tiến tới gần giới hạn cuối
        if (player.position.x + spawnDistance > lastX)
        {
            GenerateTile();
        }

        // Sinh thêm phông nền (Background) theo lưới tọa độ
        if (player.position.x + spawnDistance > bgLastX)
        {
            SpawnBackground();
        }
    }
    #endregion

    #region LOGIC SINH PHÔNG NỀN (BACKGROUND)
    void SpawnBackground()
    {
        if (backgroundPrefabs.Length == 0) return;

        float spawnX = bgLastX + backgroundWidth;

        // Cơ chế Deterministic: Chọn ảnh dựa trên Grid Index để đảm bảo tính đồng nhất
        int gridIndex = Mathf.RoundToInt(spawnX / backgroundWidth);
        int prefabIndex = Mathf.Abs(gridIndex) % backgroundPrefabs.Length;

        GameObject bgPrefab = backgroundPrefabs[prefabIndex];

        // Sinh 3 tầng dọc (Trên - Giữa - Dưới) để lấp đầy camera
        GameObject mid = Instantiate(bgPrefab, new Vector3(spawnX, 0, 10), Quaternion.identity);
        float h = mid.GetComponent<SpriteRenderer>().bounds.size.y;

        Instantiate(bgPrefab, new Vector3(spawnX, h, 10), Quaternion.identity);
        Instantiate(bgPrefab, new Vector3(spawnX, -h, 10), Quaternion.identity);

        bgLastX = spawnX;
    }

    void SpawnLeftBlockFixed()
    {
        if (leftBlockPrefab == null || GameObject.Find("LeftBlockFixed")) return;

        Vector3 fixedPos = new Vector3(-6f, 0f, 0f);
        GameObject block = Instantiate(leftBlockPrefab, fixedPos, Quaternion.identity);
        block.name = "LeftBlockFixed";
        block.transform.localScale = new Vector3(2f, 100f, 1f);
    }
    #endregion

    #region LOGIC SINH ĐỊA HÌNH (TILES)
    void GenerateTile()
    {
        // Tính toán khoảng trống và tọa độ tiếp theo
        float gap = Random.Range(minGap, maxGap);
        lastX += gap;

        // Giới hạn độ lệch chiều cao giữa các ô (Clamp)
        float height = Mathf.Clamp(lastY + Random.Range(-1.5f, 1.5f), -1f, 3f);
        Vector3 spawnPos = new Vector3(lastX, height, 0);

        int type = Random.Range(0, 100);
        GameObject tile = null;
        bool isGround = false;

        // Quyết định loại địa hình dựa trên xác suất cấu hình
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

        // Tính toán đỉnh cao nhất của Collider để đặt vật phẩm/bẫy lên trên
        float topY = tile.GetComponent<Collider2D>().bounds.max.y;

        // Quy trình sinh vật phẩm: Ưu tiên bẫy -> Kẻ địch -> Tiền
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

    void SpawnStartGround()
    {
        if (groundPrefabs.Length == 0) return;
        Vector3 pos = new Vector3(player.position.x, player.position.y - 2f, 0);
        // Luôn sử dụng Prefab đầu tiên cho điểm xuất phát
        Instantiate(groundPrefabs[0], pos, Quaternion.identity);
        lastX = pos.x;
        lastY = pos.y;
    }

    GameObject SpawnPrefab(GameObject[] list, Vector3 pos)
    {
        if (list.Length == 0) return null;
        return Instantiate(list[Random.Range(0, list.Length)], pos, Quaternion.identity);
    }
    #endregion

    #region LOGIC VẬT CẢN & VẬT PHẨM (OBSTACLES & ITEMS)
    bool SpawnTrap(GameObject tile, float topY)
    {
        if (trapPrefabs.Length == 0 || trapCooldown > 0 || Random.Range(0, 100) > trapChance)
        {
            if (trapCooldown > 0) trapCooldown--;
            return false;
        }
        float x = Random.Range(tile.GetComponent<Collider2D>().bounds.min.x + 1f, tile.GetComponent<Collider2D>().bounds.max.x - 1f);
        Instantiate(trapPrefabs[Random.Range(0, trapPrefabs.Length)], new Vector3(x, topY + Random.Range(5f, 7f), 0), Quaternion.identity);
        trapCooldown = 2; // Khoảng chờ giữa 2 lần sinh bẫy
        return true;
    }

    void SpawnEnemy(GameObject tile, float topY)
    {
        if (enemyPrefabs.Length == 0 || enemyCooldown > 0 || Random.Range(0, 100) > enemyChance)
        {
            if (enemyCooldown > 0) enemyCooldown--;
            return;
        }
        float x = Random.Range(tile.GetComponent<Collider2D>().bounds.min.x + 1f, tile.GetComponent<Collider2D>().bounds.max.x - 1f);
        Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], new Vector3(x, topY + Random.Range(5f, 7f), 0), Quaternion.identity);
        enemyCooldown = 2;
    }

    void SpawnCoinPattern(GameObject tile, float topY)
    {
        if (coinPrefab == null || Random.Range(0, 100) > coinChance) return;
        float cx = tile.GetComponent<Collider2D>().bounds.center.x;
        float by = topY + 3.8f;
        int p = Random.Range(0, 4);

        if (p == 0) SpawnCoinLine(cx, by);
        else if (p == 1) SpawnCoinArc(cx, by);
        else if (p == 2) SpawnCoinTriangle(cx, by);
        else SpawnCoinDiamond(cx, by);
    }

    void SpawnHeart(GameObject tile, float topY)
    {
        if (heartPrefab == null || Random.Range(0, 100) > 20) return;
        float x = Random.Range(tile.GetComponent<Collider2D>().bounds.min.x + 1f, tile.GetComponent<Collider2D>().bounds.max.x - 1f);
        Instantiate(heartPrefab, new Vector3(x, topY + 3f, 0), Quaternion.identity);
    }

    void SpawnCheckpoint(GameObject tile, bool isGround, bool trapSpawned, float topY)
    {
        if (checkpointPrefab == null || !isGround || trapSpawned || lastX - lastCheckpointX < 60f) return;
        Instantiate(checkpointPrefab, new Vector3(tile.transform.position.x, topY + 1.2f, 0), Quaternion.identity);
        lastCheckpointX = lastX;
    }
    #endregion

    #region CÁC MẪU SINH TIỀN (COIN PATTERNS)
    void SpawnCoinLine(float x, float y) { for (int i = -3; i <= 3; i++) Instantiate(coinPrefab, new Vector3(x + i * 0.8f, y, 0), Quaternion.identity); }
    void SpawnCoinArc(float x, float y) { for (int i = -3; i <= 3; i++) Instantiate(coinPrefab, new Vector3(x + i * 0.8f, y + Mathf.Abs(i) * 0.6f, 0), Quaternion.identity); }
    void SpawnCoinTriangle(float x, float y)
    {
        Instantiate(coinPrefab, new Vector3(x, y + 2f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x - 0.8f, y + 1f, 0), Quaternion.identity); Instantiate(coinPrefab, new Vector3(x + 0.8f, y + 1f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x - 1.6f, y, 0), Quaternion.identity); Instantiate(coinPrefab, new Vector3(x, y, 0), Quaternion.identity); Instantiate(coinPrefab, new Vector3(x + 1.6f, y, 0), Quaternion.identity);
    }
    void SpawnCoinDiamond(float x, float y)
    {
        Instantiate(coinPrefab, new Vector3(x, y + 2f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x - 0.8f, y + 1f, 0), Quaternion.identity); Instantiate(coinPrefab, new Vector3(x + 0.8f, y + 1f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x, y, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x - 0.8f, y - 1f, 0), Quaternion.identity); Instantiate(coinPrefab, new Vector3(x + 0.8f, y - 1f, 0), Quaternion.identity);
        Instantiate(coinPrefab, new Vector3(x, y - 2f, 0), Quaternion.identity);
    }
    #endregion

    #region ĐỘ KHÓ (DIFFICULTY)
    /// <summary>
    /// Áp dụng các thông số tỷ lệ dựa trên lựa chọn độ khó của người chơi.
    /// </summary>
    void ApplyDifficulty()
    {
        int d = PlayerPrefs.GetInt("difficulty", 0);
        if (d == 0) // EASY
        {
            minGap = 2.5f; maxGap = 4f; trapChance = 8; enemyChance = 15; islandChance = 10; coinChance = 85;
        }
        else if (d == 1) // NORMAL
        {
            minGap = 3.5f; maxGap = 5.5f; trapChance = 25; enemyChance = 30; islandChance = 20; coinChance = 30;
        }
        else // HARD
        {
            minGap = 4.5f; maxGap = 6f; trapChance = 65; enemyChance = 55; islandChance = 40; coinChance = 20;
        }
    }
    #endregion
}