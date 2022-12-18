using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small 360 degrees spin animation.
/// </summary>
public class SpinAnimation : MonoBehaviour
{
    [SerializeField] GameObject targetObject;
    [SerializeField] bool right;

    private void Awake()
    {
        if(TryGetComponent<Button>(out var button))
        {
            button.onClick.AddListener(Animate);
        }
        if (targetObject == null) targetObject = gameObject;
    }

    public void Animate()
    {
        iTween.RotateBy(targetObject, iTween.Hash("z", right ? -1 : 1, "time", 0.5f, "easeType", iTween.EaseType.easeOutBack));
    }
}
