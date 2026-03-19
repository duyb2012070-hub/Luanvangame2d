using UnityEngine;
using UnityEngine.UI;
using TMPro; // Phải có cái này để điều khiển Text
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static string SceneToLoad;
    public Slider loadingBar;
    public TextMeshProUGUI percentText; 
    public TextMeshProUGUI tipText;    

    // Danh sách các câu mẹo
    public string[] tips = {
        "Mẹo: Hãy thu thập trái tim để heal 1 máu!",
        "Mẹo: Đừng chạm vào quái vật slame.",
        "Bạn có biết: JUMFORCE đang chờ bạn khám phá!",
        "Mẹo: cẩn thận té vực chết luôn nhé ."
    };

    void Start()
    {
        // Chọn ngẫu nhiên 1 câu mẹo khi vừa vào scene
        if (tipText != null && tips.Length > 0)
        {
            tipText.text = tips[Random.Range(0, tips.Length)];
        }

        StartCoroutine(LoadAsync());
    }

    IEnumerator LoadAsync()
    {
        string target = string.IsNullOrEmpty(SceneToLoad) ? "game" : SceneToLoad;
        AsyncOperation op = SceneManager.LoadSceneAsync(target);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            if (loadingBar != null)
            {
                // Thanh bar chạy mượt dần dần
                loadingBar.value = Mathf.MoveTowards(loadingBar.value, progress, Time.deltaTime * 0.5f);

                // Cập nhật con số %
                if (percentText != null)
                    percentText.text = (loadingBar.value * 100f).ToString("F0") + "%";
            }

            if (loadingBar.value >= 0.99f && op.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.8f);
                op.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}