using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCounterView : MonoBehaviour
{
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Image iconImage;
    [SerializeField] private bool showLabel = true;
    [SerializeField] private string amountFormat = "{0}";

    public ResourceType ResourceType => resourceType;

    public void Configure(ResourceType type, Sprite icon = null, string displayName = null)
    {
        resourceType = type;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        SetLabel(string.IsNullOrWhiteSpace(displayName) ? type.ToString() : displayName);
    }

    public void SetAmount(int amount)
    {
        if (amountText != null)
            amountText.text = string.Format(amountFormat, amount);
    }

    private void Awake()
    {
        if (labelText == null || amountText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

            if (labelText == null && texts.Length > 0)
                labelText = texts[0];

            if (amountText == null && texts.Length > 1)
                amountText = texts[1];
            else if (amountText == null && texts.Length > 0)
                amountText = texts[0];
        }

        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>(true);
    }

    private void OnValidate()
    {
        SetLabel(resourceType.ToString());
    }

    private void SetLabel(string text)
    {
        if (labelText == null)
            return;

        labelText.gameObject.SetActive(showLabel);
        labelText.text = text;
    }
}
