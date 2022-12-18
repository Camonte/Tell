using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys a draggable on drop.
/// </summary>
public class DropZoneDestroy : DropZone
{
    public override void OnDrop(Draggable draggable)
    {
        Destroy(draggable.gameObject);
    }

    public override void OnDrop(DraggableEraser draggable)
    {
        Destroy(draggable.gameObject);
    }
}
