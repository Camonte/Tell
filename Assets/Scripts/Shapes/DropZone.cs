using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class to represent a zone where we can drop <see cref="Draggable"/> objects.
/// </summary>
public class DropZone : MonoBehaviour
{
    public int priority = 0;

    /// <summary>
    /// Can the draggable be dropped on this zone ?
    /// </summary>
    public virtual bool CanDrop(Draggable draggable)
    {
        return true;
    }

    /// <summary>
    /// Called when draggable is dropped on this drop zone. Only called if CanDrop is true.
    /// </summary>
    public virtual void OnDrop(Draggable draggable) { }

    /// <summary>
    /// Called when draggable is picked up from this drop zone.
    /// </summary>
    public virtual void OnPickup(Draggable draggable) { }

    /// <summary>
    /// Called when draggable hovers this drop zone for a long time without moving. Only called if CanLongHover is true.
    /// </summary>
    public virtual void LongHover(Draggable draggable) { }

    /// <summary>
    /// Can the draggable have a long hover interaction with this zone ?
    /// </summary>
    public virtual bool CanLongHover(Draggable draggable)
    {
        return false;
    }

    /// <summary>
    /// Can the draggable hover this zone ?
    /// </summary>
    public virtual bool CanHover(Draggable draggable)
    {
        return false;
    }

    /// <summary>
    /// Called when draggable is hovering this zone. Only called if CanHover is true.
    /// </summary>
    public virtual void Hover(Draggable draggable) { }

    /// <summary>
    /// Called when draggable stops hovering this zone. Only called if CanHover is true.
    /// </summary>
    public virtual void HoverExit(Draggable draggable) { }

    /// <summary>
    /// Called when draggable starts hovering this zone. Only called if CanHover is true.
    /// </summary>
    public virtual void HoverEnter(Draggable draggable) { }
}
