using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 normalScale;
    private Vector3 hoverScale;

    public float scaleMultiplier = 1.1f;
    public float speed = 10f;

    private bool isHover;

    void Start()
    {
        normalScale = transform.localScale;
        hoverScale = normalScale * scaleMultiplier;
    }

    void Update()
    {
        if (isHover)
            transform.localScale = Vector3.Lerp(transform.localScale, hoverScale, Time.deltaTime * speed);
        else
            transform.localScale = Vector3.Lerp(transform.localScale, normalScale, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
    }
}