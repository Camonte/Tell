using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

/// <summary>
/// Script attached to a Grapheme Generator, that spawns a new Grapheme of the same type when touched.
/// See also <see cref="DropZoneMergeGrapheme"/>.
/// </summary>
public class SpawnGrapheme : DropZoneMergeGrapheme
{
    private LeanSelectByFinger touchManager;

    private void Awake()
    {
        priority = 2;
        touchManager = FindObjectOfType<LeanSelectByFinger>();
    }

    public void SpawnObject(LeanFinger finger)
    {
        if (!Config.testMode)
        {
            var obj = ShapeManager.Instance.CreateGrapheme(grapheme, transform.position);
            var selectable = obj.GetComponent<LeanSelectableByFinger>();
            touchManager.Select(selectable, finger);
        }
        container?.Collapse();
        if(Config.testMode && StateManager.Instance.currentSentence != null)
        {
            if(Config.progressiveCorrection & !StateManager.Instance.TapOk(grapheme))
            {
                StateManager.Instance.OnWrong();
            } else {
                StateManager.Instance.DisplayAnswer((Grapheme)grapheme);
            }
        }
    }

    // no drop on generators
    public override bool CanDrop(Draggable draggable) => false;

    public override void OnDrop(Draggable draggable) => draggable.Destroy();
}
