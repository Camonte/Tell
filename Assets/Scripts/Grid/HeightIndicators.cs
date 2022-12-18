using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small script for moving the height indicators when the layout/scroll/scale changes.
/// Height indicators are two small arrows next to the side pannels that show the y position of where the current word defined in <see cref="StateManager"/> will align itself to.
/// </summary>
public class HeightIndicators : MonoBehaviour
{
    [SerializeField] GameObject left;
    [SerializeField] GameObject right;

    private void Start()
    {
        UpdatePositions();
    }

    public void OnScroll(Vector2 delta)
    {
        left.transform.position += (Vector3)delta;
        right.transform.position += (Vector3)delta;
    }

    public void UpdatePositions()
    {
        var leftPos = new Vector2(SidePanel.leftPanelX + 0.4f, GridManager.Instance.scrollOffset);
        var rightPos = new Vector2(SidePanel.rightPanelX - 0.4f, GridManager.Instance.scrollOffset);
        left.transform.position = leftPos;
        right.transform.position = rightPos;
    }
}
