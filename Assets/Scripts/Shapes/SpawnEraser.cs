using UnityEngine;
using Lean.Touch;

/// <summary>
/// Script attached to a Phoneme Generator, that spawns a new Phoneme of the same type when touched.
/// See also <see cref="DropZoneMerge"/>.
/// </summary>
public class SpawnEraser : DropZoneMerge
{
    private LeanSelectByFinger touchManager;

    private void Awake()
    {
        priority = 2;
        touchManager = FindObjectOfType<LeanSelectByFinger>();
    }

    public void SpawnObject(LeanFinger finger)
    {
        var obj = ShapeManager.Instance.CreateEraser(transform.position);
        var selectable = obj.GetComponent<LeanSelectableByFinger>();

        touchManager.Select(selectable, finger);
    }

    // no drop on generators
    public override bool CanDrop(Draggable draggable) => false;

    public override void OnDrop(Draggable draggable) { }
}
