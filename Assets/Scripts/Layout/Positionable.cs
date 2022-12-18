using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper script that positions, anchors, or stretches a gameobject based on it's collider (similar to <see cref="RectTransform"/>).
/// Vertical alignment options include bottom, middle and top. Horizontal alignment options include left, middle and right.
/// All alignment options are relative to the screen (not to parent).
/// The object is put in a parent "anchor" object, such that scaling doesn't f*ck things up (i.e. if the object was aligned to the left, scaling won't make it overflow over the left screen limit).
/// The collider can also be stretched to fit the whole screen, either vertically or horizontally.
/// </summary>
public class Positionable : MonoBehaviour
{

    public enum VAlign { Bottom, Middle, Top };
    public enum HAlign { Left, Middle, Right };

    [SerializeField] bool updateOnStart = true;
    [SerializeField] VAlign v;
    [SerializeField] HAlign h;
    [SerializeField] bool responsive;
    [SerializeField] public Collider2D c;
    [SerializeField] bool fillVertically;
    [SerializeField] bool fillHorizontally;

    private Transform anchor;

    public void Awake()
    {
        if (!enabled) return;

        if (c == null)
        {
            c = GetComponentInParent<Collider2D>();
            if (c == null)
            {
                Debug.Log("No collider (bounds) found on " + gameObject.name);
                Destroy(this);
            }
        }

        anchor = new GameObject(transform.name + " anchor").transform;
        anchor.SetParent(transform.parent);
        transform.SetParent(anchor);
    }

    public Bounds Bounds()
    {
        return c.bounds;
    }

    private void Start()
    {
        // initial align
        if (updateOnStart)
        {
            Align();
            Stretch();
        }
    }

    private void Update()
    {
        // update constantly if responsive is true
        if (responsive)
        {
            Align();
            Stretch();
        }
    }

    public void Align()
    {
        transform.localPosition = OffsetToAnchor(h, v);
        anchor.position = WorldPosition(h, v);
    }

    public void Stretch()
    {
        Vector2 maxScale = WorldPosition(HAlign.Right, VAlign.Top) - WorldPosition(HAlign.Left, VAlign.Bottom);
        Vector2 scale = transform.localScale;
        Vector2 lossyScale = transform.lossyScale;
        if (fillVertically) scale.y = maxScale.y * (scale.y / lossyScale.y);
        if (fillHorizontally) scale.x = maxScale.x * (scale.x / lossyScale.x);
        transform.localScale = scale;
    }

    private Vector2 OffsetToAnchor(HAlign h, VAlign v)
    {
        Bounds bounds = c.bounds;
        return new Vector2((1 - (int)h) * bounds.extents.x, (1 - (int)v) * bounds.extents.y);
    }

    private Vector2 WorldPosition(HAlign h, VAlign v)
    {
        Vector2 viewportPos = new Vector2((int)h / 2f, (int)v / 2f);
        Vector2 pos = Camera.main.ViewportToWorldPoint(viewportPos);
        return pos;
    }

    private Vector2 PositionAlignedTo(HAlign h, VAlign v)
    {
        Vector2 pos = WorldPosition(h, v);
        Bounds bounds = c.bounds;
        pos.x += (1 - (int)h) * bounds.extents.x;
        pos.y += (1 - (int)v) * bounds.extents.y;
        return pos;
    }

    
}
