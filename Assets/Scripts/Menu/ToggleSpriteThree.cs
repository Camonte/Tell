using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Simple UI toggle which changes between two sprites based on it's boolean value.
/// If present, can also toggle a label (TMPro).
/// </summary>
[RequireComponent(typeof(Button))]
public class ToggleSpriteThree : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;
    [SerializeField] private Sprite sprite3;
    [SerializeField] private bool customPivot;

    [Header("Text")]
    [SerializeField] private TMPro.TextMeshProUGUI targetText;
    [SerializeField] private string text1;
    [SerializeField] private string text2;
    [SerializeField] private string text3;

    [Space(5)]
    [SerializeField] private int currentSprite;

    public int CurrentSprite { get { return currentSprite; } set { currentSprite = value; UpdateValue(); } }
    public UnityEvent<bool> onValueChanged;
    public UnityEvent<bool> onValueChangedInverse;
    private Button button; // to set initial value and skip onValueChanged notification
    public void Initialize(int value) 
    { 
        currentSprite = value; 
        UpdateValue(); 
    }

    private void Start()
    {
        button = GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(OnClick);
        Initialize(Config.memoryDifficulty);
    }

    public void OnClick()
    {
        if (currentSprite == 1)
        {
            currentSprite = 2;
            Config.memoryDifficulty = 2;
        }
        else if (currentSprite == 2)
        {
            currentSprite = 3;
            Config.memoryDifficulty = 3;
        }
        else
        {
            currentSprite = 1;
            Config.memoryDifficulty = 1;
        }
        UpdateValue();
    }

    public void SetValue(int value)
    {
        currentSprite = value; UpdateValue();
    }

    private void UpdateValue()
    {
        if (targetImage != null)
        {
            if (currentSprite == 1)
                targetImage.sprite = sprite1;
            else if (currentSprite == 2)
                targetImage.sprite = sprite2;
            else
                targetImage.sprite = sprite3;
            if (customPivot) targetImage.rectTransform.pivot = targetImage.sprite.pivot / targetImage.sprite.rect.size;
        }
        if (targetText != null)
        {
            if (currentSprite == 1)
                targetText.text = text1;
            else if (currentSprite == 2)
                targetText.text = text2;
            else
                targetText.text = text3;
        }
    }
}