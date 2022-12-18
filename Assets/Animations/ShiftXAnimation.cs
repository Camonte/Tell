using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small shift animation that moves the target element on the x axis and goes back to the initial scale.
/// The target object is disabled after the animation if the `active` parameter is set to `false` by another script.
/// </summary>
public class ShiftXAnimation : MonoBehaviour
{
    private Vector3 initialPos;
    private Vector3 targetPos;

    public bool active = true;

    [SerializeField] bool right;
    [SerializeField] GameObject targetObject;

    private void Awake()
    {
        if(TryGetComponent<Button>(out var button))
        {
            button.onClick.AddListener(Animate);
        }
        if (targetObject == null) targetObject = gameObject;
        initialPos = targetObject.transform.localPosition;
        targetPos = initialPos + (right ? Vector3.right : Vector3.left) * 16;
    }

    public void Animate()
    {
        iTween.MoveTo(targetObject, iTween.Hash("position", targetPos, "time", 0.2f, "isLocal", true, "oncompletetarget", gameObject, "onComplete", "BackToOriginal"));
    }

    private void BackToOriginal()
    {
        iTween.MoveTo(targetObject, iTween.Hash("position", initialPos, "time", 0.2f, "delay", 0.1f, "isLocal", true, "oncompletetarget", gameObject, "onComplete", "CleanUp"));
    }

    private void CleanUp()
    {
        if (!active) targetObject.SetActive(false);
        active = true;
    }
}
