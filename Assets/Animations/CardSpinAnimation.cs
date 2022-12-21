using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small 180 degrees spin animation.
/// </summary>
public class CardSpinAnimation : MonoBehaviour
{
    [SerializeField] GameObject targetObject;
    [SerializeField] bool right;
    GameObject memoryControl;
    int animationNo;

    private void Awake()
    {
        if (targetObject == null) targetObject = gameObject;
        memoryControl = GameObject.Find("MemoryControl");
    }

    public void Animate()
    {
        if(memoryControl.GetComponent<MemoryControl>().animationOn1)
        {
            animationNo = 2;
            memoryControl.GetComponent<MemoryControl>().animationOn2 = true;
        }
        else
        {
            animationNo = 1;
            memoryControl.GetComponent<MemoryControl>().animationOn1 = true;
        }
        iTween.RotateAdd(targetObject, iTween.Hash("y", 180, "time", 1.0f, "oncomplete", "ResetFlag"));
    }

    private void ResetFlag()
    {
        if(animationNo == 1)
            memoryControl.GetComponent<MemoryControl>().animationOn1 = false;
        else
           memoryControl.GetComponent<MemoryControl>().animationOn2 = false;
    }
}
