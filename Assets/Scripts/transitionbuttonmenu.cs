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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;

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
        // KIỂM TRA ĐIỀU KIỆN SFX TRƯỚC KHI PHÁT
        if (clickSound != null && PlayerPrefs.GetInt("SfxOn", 1) == 1)
        {
            GameObject tempAudio = new GameObject("TempClickSound");
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();

            tempSource.clip = clickSound;
            tempSource.volume = volume;
            tempSource.spatialBlend = 0f;
            tempSource.playOnAwake = false;
            tempSource.ignoreListenerPause = true;

            tempSource.Play();

            Destroy(tempAudio, clickSound.length + 0.1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;

        // KIỂM TRA ĐIỀU KIỆN SFX TRƯỚC KHI PHÁT HOVER
        if (hoverSound != null && audioSource != null && PlayerPrefs.GetInt("SfxOn", 1) == 1)
        {
            audioSource.PlayOneShot(hoverSound, volume * 0.8f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }
}