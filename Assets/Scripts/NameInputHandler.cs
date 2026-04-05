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

        // 2. Xóa dữ liệu tạm của phiên trước để tránh nhận nhầm tên cũ
        PlayerPrefs.SetString("playerName", "");
        PlayerPrefs.Save();

        nameInput.onValueChanged.AddListener(delegate { UpdateButtonsState(); });
        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        string input = nameInput.text.Trim();
        bool isLengthValid = input.Length >= minLength && input.Length <= maxLength;
        bool isExists = isLengthValid && CheckNameExistsInDB(input);

        // --- ĐÂY LÀ ĐOẠN QUAN TRỌNG NHẤT ---
        // Cập nhật PlayerPrefs NGAY LẬP TỨC khi gõ tên hợp lệ
        if (isLengthValid)
        {
            PlayerPrefs.SetString("playerName", input);
            // Không cần gọi PlayerPrefs.Save() ở đây để tránh giảm hiệu năng, 
            // bộ nhớ RAM sẽ giữ giá trị này cho nút Continue dùng.
        }

        // Bật/Tắt nút bấm theo trạng thái
        if (playButton != null) playButton.interactable = isLengthValid;
        if (continueButton != null) continueButton.interactable = isExists;

        // Hiển thị thông báo (Giữ nguyên logic của bạn)
        if (string.IsNullOrEmpty(input)) SetErrorMessage("");
        else if (input.Length < minLength) SetErrorMessage("Tên quá ngắn...");
        else if (isExists) SetErrorMessage("<color=green>Tìm thấy bản lưu! Có thể Continue.</color>");
        else SetErrorMessage("Tên hợp lệ! Có thể Play mới.");
    }

    private void SetErrorMessage(string message) { if (errorText != null) errorText.text = message; }

    // Gắn vào nút PLAY
    public void OnPlayButtonClick()
    {
        if (nameInput.text.Length >= minLength)
        {
            PlayerPrefs.SetString("playerName", nameInput.text.Trim());
            PlayerPrefs.SetInt("IsLoadingSave", 0); // Đánh dấu là CHƠI MỚI
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