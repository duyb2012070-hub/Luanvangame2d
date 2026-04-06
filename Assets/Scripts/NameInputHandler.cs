using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SQLite;

public class NameInputHandler : MonoBehaviour
{
    [Header("--- UI References ---")]
    public TMP_InputField nameInput;
    public Button playButton;
    public Button continueButton;
    public TextMeshProUGUI errorText;
    public GameObject difficultyPanel;

    [Header("--- Settings ---")]
    [SerializeField] private int minLength = 3;
    [SerializeField] private int maxLength = 15;

    void Start()
    {
        // 1. Luôn để trống ô nhập khi mở game
        if (nameInput != null) nameInput.text = "";

        // 2. Xóa dữ liệu tạm của phiên trước
        PlayerPrefs.SetString("playerName", "");
        PlayerPrefs.Save();

        // Đăng ký sự kiện thay đổi text
        if (nameInput != null)
        {
            nameInput.onValueChanged.AddListener(delegate { UpdateButtonsState(); });
        }

        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        string input = nameInput.text.Trim();

        // Kiểm tra logic độ dài
        bool isTooShort = input.Length > 0 && input.Length < minLength;
        bool isTooLong = input.Length > maxLength;
        bool isLengthValid = input.Length >= minLength && input.Length <= maxLength;

        // Kiểm tra tồn tại trong Database
        bool isExists = isLengthValid && CheckNameExistsInDB(input);

        // Lưu tạm vào PlayerPrefs để các script khác sử dụng
        if (isLengthValid)
        {
            PlayerPrefs.SetString("playerName", input);
        }

        // Bật/Tắt tương tác của nút bấm
        if (playButton != null) playButton.interactable = isLengthValid;
        if (continueButton != null) continueButton.interactable = isExists;

        // --- HỆ THỐNG THÔNG BÁO MÀU SẮC ---
        if (string.IsNullOrEmpty(input))
        {
            SetErrorMessage("");
        }
        else if (isTooShort)
        {
            // Đỏ: Quá ngắn
            SetErrorMessage($"<color=red>Tên quá ngắn (Tối thiểu {minLength} ký tự)</color>");
        }
        else if (isTooLong)
        {
            // Đỏ: Quá dài
            SetErrorMessage($"<color=red>Tên quá dài (Tối đa {maxLength} ký tự)</color>");
        }
        else if (isExists)
        {
            // Xanh: Tìm thấy dữ liệu cũ
            SetErrorMessage("<color=green>Tìm thấy bản lưu! Có thể Continue.</color>");
        }
        else
        {
            // Xanh: Tên mới hợp lệ
            SetErrorMessage("<color=green>Tên hợp lệ! Có thể Play mới.</color>");
        }
    }

    private void SetErrorMessage(string message)
    {
        if (errorText != null) errorText.text = message;
    }

    // Gắn vào sự kiện OnClick của nút PLAY
    public void OnPlayButtonClick()
    {
        string input = nameInput.text.Trim();
        if (input.Length >= minLength && input.Length <= maxLength)
        {
            PlayerPrefs.SetString("playerName", input);
            PlayerPrefs.SetInt("IsLoadingSave", 0); // Đánh dấu chơi mới
            PlayerPrefs.Save();
            if (difficultyPanel != null) difficultyPanel.SetActive(true);
        }
    }

    private bool CheckNameExistsInDB(string nameToCheck)
    {
        if (DatabaseManager.db == null) return false;
        try
        {
            var result = DatabaseManager.db.Table<SaveGameData>()
                .Where(v => v.playerName.ToLower() == nameToCheck.ToLower()).FirstOrDefault();
            return result != null;
        }
        catch { return false; }
    }
}