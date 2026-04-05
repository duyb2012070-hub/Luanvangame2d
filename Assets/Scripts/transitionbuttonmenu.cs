using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class transitionbuttonmenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region KHAI BÁO BIẾN
    [Header("--- Cấu hình Scale ---")]
    public float scaleMultiplier = 1.1f;
    public float speed = 12f;

    [Header("--- Cấu hình Âm thanh ---")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Tooltip("Chỉnh lên 1.0 là to nhất theo file gốc")]
    [Range(0f, 1.5f)] public float volume = 1.0f;

    private Vector3 normalScale;
    private Vector3 hoverScale;
    private bool isHover;
    private AudioSource audioSource;
    #endregion

    void Start()
    {
        normalScale = transform.localScale;
        hoverScale = normalScale * scaleMultiplier;

        // Thiết lập AudioSource nội bộ (dùng cho Hover vì Hover không làm mất nút)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D hoàn toàn
        audioSource.ignoreListenerPause = true;

        // Đăng ký sự kiện Click cho Button
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(PlayClickSound);
        }
    }

    void Update()
    {
        float targetSpeed = Time.deltaTime * speed;
        transform.localScale = Vector3.Lerp(transform.localScale, isHover ? hoverScale : normalScale, targetSpeed);
    }

    // HÀM PHÁT ÂM THANH CLICK "BẤT TỬ"
    void PlayClickSound()
    {
        if (clickSound != null)
        {
            // TẠO MỘT OBJECT TẠM THỜI ĐỂ PHÁT TIẾNG
            // Cách này giúp âm thanh không bị ngắt khi nút ẩn đi hoặc chuyển Scene
            GameObject tempAudio = new GameObject("TempClickSound");
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();

            // Cấu hình âm thanh to rõ nhất (2D)
            tempSource.clip = clickSound;
            tempSource.volume = volume;
            tempSource.spatialBlend = 0f; // Ép về 2D
            tempSource.playOnAwake = false;
            tempSource.ignoreListenerPause = true;

            tempSource.Play();

            // Tự động xóa Object này sau khi clip chạy xong
            Destroy(tempAudio, clickSound.length + 0.1f);

            Debug.Log("--- Đã phát Click bất tử trên: " + gameObject.name + " ---");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
        if (hoverSound != null && audioSource != null)
            audioSource.PlayOneShot(hoverSound, volume * 0.8f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }
}