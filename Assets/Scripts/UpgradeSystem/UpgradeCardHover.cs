using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Refs")]
    public Image backgroundImage;
    public TextMeshProUGUI label;

    [Header("Colors")]
    public Color normalColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color hoverColor  = new Color(0.30f, 0.30f, 0.30f, 0.95f);

    [Header("Scale")]
    public float hoverScale = 1.06f;

    private Vector3 originalScale;

    public void Init(Image bg, TextMeshProUGUI txt)
    {
        backgroundImage = bg;
        label = txt;
    }

    void Awake()
    {
        originalScale = transform.localScale;

        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundImage != null)
            backgroundImage.color = hoverColor;

        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (backgroundImage != null)
            backgroundImage.color = normalColor;

        transform.localScale = originalScale;
    }
}
