using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Merging of grapheme objects. Basically the same as <see cref="DropZoneMerge"/>.
/// </summary>
public class DropZoneMergeGrapheme : DropZoneMerge
{
    public Grapheme grapheme;
    public Grapheme oldGrapheme;

    public DropZoneExpand container;

    private void Awake()
    {
        priority = 1;
    }

    private void Start()
    {
        _draggable = GetComponent<Draggable>();
        if (_draggable != null)
        {
            grapheme = (Grapheme)_draggable.element;
        }
        oldGrapheme = new Grapheme(grapheme.id, grapheme.phoneme);
        isGenerator = _draggable == null;
    }

    public override bool CanDrop(Draggable draggable) => enabled && CanHover(draggable);

    public override bool CanLongHover(Draggable draggable) => enabled && oldGrapheme != grapheme;

    public override bool CanHover(Draggable draggable) => (enabled && (Grapheme.CanMerge(oldGrapheme, draggable.element) || Grapheme.CanMerge(draggable.element, oldGrapheme)));

    public override void OnDrop(Draggable draggable)
    {
        if (oldGrapheme != grapheme)
        {
            oldGrapheme = grapheme;
            draggable.Destroy();
            _draggable.Bounce();
        }
    }

    public override void LongHover(Draggable draggable)
    {
        if (oldGrapheme != grapheme)
        {
            ShapeManager.Instance.UpdateGrapheme(draggable.gameObject, grapheme);
            draggable.GetComponent<DropZoneMergeGrapheme>().oldGrapheme = grapheme;

            if (isGenerator)
            {
                // reset the generator to old grapheme
                ShapeManager.Instance.UpdateGrapheme(gameObject, oldGrapheme);
                grapheme = oldGrapheme;
                container?.Collapse();
            }

            _draggable?.Destroy();

            draggable.Show();
            draggable.Bounce();
        }
    }


    public override void Hover(Draggable draggable)
    {
        if (draggable != currentHover) return;

        var thisIsRight = transform.position.x > draggable.transform.position.x;
        var isCloseEnough = Mathf.Abs(transform.position.y - draggable.transform.position.y) < 0.3f;
        var newState = isCloseEnough ? (thisIsRight ? State.Left : State.Right) : State.None;

        if (state != newState)
        {
            draggable.OnChangeHover();
            state = newState;
            var gr = state switch
            {
                State.Right => Grapheme.Merge(oldGrapheme, draggable.element),
                State.Left => Grapheme.Merge(draggable.element, oldGrapheme),
                _ => null,
            };

            var changed = grapheme != gr;

            if (gr == null)
            {
                gr = oldGrapheme;
                draggable.Show();
            }
            else
            {
                draggable.Hide();
            }

            if (changed)
            {
                ShapeManager.Instance.UpdateGrapheme(gameObject, gr);
                grapheme = gr;

                _draggable?.Bounce();
                draggable.Bounce();
            }
        }
    }

    public override void HoverEnter(Draggable draggable)
    {
        if (currentHover == null)
        {
            currentHover = draggable;
            Hover(draggable);
        }
    }

    public override void HoverExit(Draggable draggable)
    {
        if (currentHover == draggable)
        {
            draggable.Show();
            currentHover = null;
            state = State.None;

            if (oldGrapheme != grapheme)
            {
                ShapeManager.Instance.UpdateGrapheme(gameObject, oldGrapheme);
                grapheme = oldGrapheme;
                _draggable?.Bounce();
                draggable.Bounce();
            }
        }
    }
}
