using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Simple UI toggle which changes between two sprites based on it's boolean value.
/// If present, can also toggle a label (TMPro).
/// </summary>
[RequireComponent(typeof(Button))]
public class ToggleSpriteSwap : MonoBehaviour {
    [Header("Image")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private bool customPivot;

    [Header("Text")]
    [SerializeField] private TMPro.TextMeshProUGUI targetText;
    [SerializeField] private string onText;
    [SerializeField] private string offText;

    [Space(5)]
    [SerializeField] private bool isOn;

    public bool IsOn { get { return isOn; } set { isOn = value; UpdateValue(); } }
    public UnityEvent<bool> onValueChanged;
    public UnityEvent<bool> onValueChangedInverse;
    private Button button; // to set initial value and skip onValueChanged notification
    public void Initialize(bool value) { isOn = value; UpdateValue(false); }
    private void Start()
    {
        button = GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(OnClick);
        Initialize(IsOn);
    }

    public void OnClick()
    {
        isOn = !isOn;
        UpdateValue();
    }

    public void SetValue(bool value)
    {
        isOn = value; UpdateValue();
    }

    private void UpdateValue(bool notifySubscribers = true) {
        if(notifySubscribers && onValueChanged != null)
            onValueChanged.Invoke(isOn);
        if (notifySubscribers && onValueChangedInverse != null)
            onValueChangedInverse.Invoke(!isOn);
        if (targetImage != null)
        {
            targetImage.sprite = isOn ? onSprite : offSprite;
            if (customPivot) targetImage.rectTransform.pivot = targetImage.sprite.pivot / targetImage.sprite.rect.size;
        }
            
        if (targetText != null)
            targetText.text = isOn ? onText : offText;
    }
}