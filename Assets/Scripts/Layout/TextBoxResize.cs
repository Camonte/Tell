using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple script to keep the text from overflowing into the side pannels. Should be used in combination with <see cref="Scalable"/>
/// by linking it's relevant functions to <see cref="UpdateSize"/>.
/// </summary>
public class TextBoxResize : MonoBehaviour
{
    private TMPro.TextMeshPro tmp;

    private void Awake()
    {
        tmp = GetComponent<TMPro.TextMeshPro>();
    }

    public void UpdateSize()
    {
        // update text area width
        var rectTransform = tmp.rectTransform;
        var rectSize = rectTransform.sizeDelta;

        var halfWidth = Mathf.Min(Mathf.Abs(SidePanel.rightPanelX), Mathf.Abs(SidePanel.leftPanelX));

        rectSize.x = halfWidth * 1.8f; // 2 * halfwidth - some padding
        rectTransform.sizeDelta = rectSize;
    }
}
