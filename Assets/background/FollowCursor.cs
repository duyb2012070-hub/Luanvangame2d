using UnityEngine;

public class FollowCursor : MonoBehaviour
{
    // Hai ô này chính là hai ô bạn vừa kéo thả ở Bước 1
    public RectTransform cursorRect;
    public Canvas parentCanvas;

    void Start()
    {
        // Ẩn con trỏ chuột mặc định của máy tính đi
        Cursor.visible = false;
    }

    void Update()
    {
        // Biến vị trí chuột thành tọa độ trong Canvas
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out movePos);

        // Cập nhật vị trí cho hình ảnh con trỏ
        cursorRect.anchoredPosition = movePos;
    }
}