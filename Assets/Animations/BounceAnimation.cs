using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small bounce animation that scales up the target element and goes back to the initial scale.
/// </summary>
public class BounceAnimation : MonoBehaviour
{
    [SerializeField] GameObject targetObject;
    [SerializeField] float scale = 1.2f;
    private Vector3 initialScale;

    private void Awake()
    {
        if(TryGetComponent<Button>(out var button))
        {
            button.onClick.AddListener(Animate);
        }
        if (targetObject == null) targetObject = gameObject;
        initialScale = targetObject.transform.localScale;
    }

    public void Animate()
    {
        //iTween.PunchScale(targetObject, new Vector3(0.3f,0.3f,0), 0.5f);
        iTween.ScaleTo(targetObject, iTween.Hash("scale", initialScale * scale, "time", 0.25f, "easeType", iTween.EaseType.easeOutBack, "oncompletetarget", gameObject, "onComplete", "BackToOriginal"));
    }

    private void BackToOriginal()
    {
        iTween.ScaleTo(targetObject, iTween.Hash("scale", initialScale, "easeType", iTween.EaseType.easeOutBack, "time", 0.25f));
    }
}
