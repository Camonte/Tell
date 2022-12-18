using UnityEngine;

/// <summary>
/// Animation for the well done checkmark.
/// Scales up with a small spin and then scales down to disappear.
/// </summary>
public class WellDoneAnimation : MonoBehaviour
{
    [SerializeField] GameObject targetObject;
    [SerializeField] float scale = 128f;

    private Vector3 initialScale;

    private void Awake()
    {
        if (targetObject == null) targetObject = gameObject;
        initialScale = Vector3.one;
    }

    public void Animate()
    {
        targetObject.SetActive(true);
        iTween.ScaleTo(targetObject, iTween.Hash("scale", initialScale * scale, "time", 0.25f, "easeType", iTween.EaseType.easeInSine, "oncompletetarget", gameObject, "onComplete", "BackToOriginal"));
        iTween.RotateBy(targetObject, iTween.Hash("z", -1, "time", 0.25f, "easeType", iTween.EaseType.easeInOutSine));
    }

    private void BackToOriginal()
    {
        iTween.ScaleTo(targetObject, iTween.Hash("scale", initialScale, "easeType", iTween.EaseType.easeOutSine, "time", 0.25f, "delay", 0.5f, "oncompletetarget", gameObject, "onComplete", "CleanUp"));
    }

    private void CleanUp() => targetObject.SetActive(false);
}
