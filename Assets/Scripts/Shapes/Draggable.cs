using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Represents an element that can be dragged by the finger/pen.
/// Draggables can interact with <see cref="DropZone"/>s.
/// This is the main script attached to all <see cref="Phoneme"/>s and <see cref="Grapheme"/>s. The attached element can be accessed via the `element` attribute.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(LeanSelectableByFinger))]
[RequireComponent(typeof(LeanDragTranslate))]
[RequireComponent(typeof(Rigidbody2D))]
public class Draggable : MonoBehaviour
{
    private LeanFinger currentFinger;

    Dictionary<DropZone, float> dropZones = new Dictionary<DropZone, float>();
    public DropZone currentDropZone;

    public SortingGroup sortingGroup;

    private Vector2 lastPosition;
    private string rejectedReason;

    const float moveThreshold = 5;
    const float longHoverThreshold = 0.5f;
    private float stationaryTime = 0f;
    private bool isStationary = false;

    private bool isDragged = false;
    public DropZone currentHover;

    public Element element;

    static int z = 1;

    private void Start()
    {
        lastPosition = transform.position;
        currentDropZone = null;
        InitSortingGroup();
    }

    void Awake()
    {
        if (TryGetComponent<LeanFingerTap>(out var tap))
        {
            tap.OnFinger.AddListener(OnTap);
        }
    }

    public virtual void OnTap(LeanFinger finger)
    {
        Bounce();
        SoundManager.Instance.Play(element.id);
    }

    /// <summary>
    /// Disables the collider and hides this object.
    /// </summary>
    public void Block()
    {
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    public virtual void OnDisable()
    {
        GetComponent<LeanDragTranslate>().enabled = false;
        GetComponent<DropZoneMerge>().enabled = false;
        if (currentHover != null)
            currentHover.HoverExit(this);
    }

    public virtual void OnEnable()
    {
        GetComponent<DropZoneMerge>().enabled = true;
        GetComponent<LeanDragTranslate>().enabled = true;
    }

    public void InitSortingGroup()
    {
        if (sortingGroup == null)
        {
            var sg = GetComponent<SortingGroup>();
            var initialZ = z - 1;
            sg.sortingOrder = initialZ;
            sortingGroup = sg;
        }
    }

    /// <summary>
    /// Sets the sorting order for rendering. A negative value resets it to default.
    /// </summary>
    /// <param name="o"></param>
    public void SetSortingOrder(int o)
    {
        sortingGroup.sortingOrder = o > 0 ? o : z;
    }

    /// <summary>
    /// Called when drag is initiated.
    /// </summary>
    public void OnDragStart(LeanFinger finger)
    {
        if (!enabled || currentFinger != null) return;

        rejectedReason = "";

        // bring to front
        z++;
        if (sortingGroup != null) sortingGroup.sortingOrder = z;

        // pick up from a drop zone ?
        OnPickup();

        // update last position
        lastPosition = transform.position;

        // update finger
        currentFinger = finger;
        isDragged = true;

        Bounce();
    }

    public DropZone OnPickup()
    {
        DropZone oldDropZone = currentDropZone;
        if (currentDropZone != null)
        {
            currentDropZone.OnPickup(this);
            currentDropZone = null;
        }
        return oldDropZone;
    }

    public void Destroy()
    {
        OnPickup();
        Destroy(gameObject);
    }

    public bool longHoverExecuted = false;

    /// <summary>
    /// Called every time the objects position changes when dragged.
    /// </summary>
    public void OnDrag(Vector2 delta)
    {
        var wasStationary = isStationary;
        isStationary = delta.magnitude <= moveThreshold;

        if (isStationary)
        {
            // newly stationary
            if (!wasStationary)
            {
                stationaryTime = 0;

                // show the loader indicator
                if (!longHoverExecuted && currentHover != null && currentHover.CanLongHover(this))
                    ShapeManager.Instance.Loader(currentFinger.GetWorldPosition(0) + new Vector3(0,0.5f), longHoverThreshold);
            }

            // add hover time
            stationaryTime += Time.deltaTime;

            // generate long hover event
            if (stationaryTime > longHoverThreshold && !longHoverExecuted)
            {
                OnLongHover();
                //isStationary = false;
            }
        }
        else
        {
            longHoverExecuted = false;
        }
    }

    /// <summary>
    /// Called when a long hover is registered.
    /// </summary>
    public void OnLongHover()
    {
        if (currentHover != null && currentHover.CanLongHover(this))
        {
            currentHover.LongHover(this);
            longHoverExecuted = true;
        }
    }

    /// <summary>
    /// Called when the current hover changes.
    /// </summary>
    public void OnChangeHover()
    {
        isStationary = false;
        ShapeManager.Instance.KillLoader();
    }

    /// <summary>
    /// Called when a drag is finished.
    /// </summary>
    public void OnDragEnd(LeanFinger finger)
    {
        if (Config.testMode)
        {
            Destroy();
        }
        else
        {
            if (currentFinger == null || finger != currentFinger) return;
            currentFinger = null;
            isDragged = false;

            if (sortingGroup != null) sortingGroup.sortingOrder = z;

            // clear hovering data
            isStationary = false;

            // get closest dropzone on which we can drop
            var dropped = false;
            if (dropZones.Count > 0)
            {
                var sortedDropZones = dropZones.OrderBy(e => -e.Key.priority).ThenBy(e => e.Value).SkipWhile(e => e.Key == null || !e.Key.CanDrop(this));
                if (sortedDropZones.Count() > 0)
                {
                    // stop animation
                    bouncing = false;

                    DropZone closestZone = sortedDropZones.First().Key;
                    closestZone.OnDrop(this);
                    currentDropZone = closestZone;
                    dropped = true;
                }
            }

            // remove object if dropped outside of main platform
            if (!dropped)
            {
                if (SidePanel.Contains(transform.position)) Destroy();
                else GridManager.Instance.AttachToGrid(transform);
            }
        }
    }

    /// <summary>
    /// Called when rejected by a dropzone. The reason indicates why it was rejected.
    /// </summary>
    public void OnReject(DropZone dropzone, string reason)
    {
        rejectedReason = reason;
        currentDropZone = null;

        DestroyAnim();
    }

    public void DestroyAnim()
    {
        iTween.RotateBy(gameObject, iTween.Hash("amount", Vector3.back, "time", 0.5f, "easetype", iTween.EaseType.linear));
        iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.zero, "time", 0.5f, "easetype", iTween.EaseType.easeInBack, "oncomplete", "OnAnimEnded"));
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enabled) return;

        // add to potential drop zones
        DropZone d = collision.GetComponent<DropZone>();
        if (d != null)
        {
            dropZones.Add(d, collision.bounds.SqrDistance(transform.position));

            // new hoverable zone detected
            if (isDragged && d.CanHover(this))
            {
                var sortedDropZones = dropZones.OrderBy(e => -e.Key.priority).ThenBy(e => e.Value).SkipWhile(e => e.Key == null || !e.Key.CanHover(this));
                if (sortedDropZones.Count() > 0)
                {
                    var closest = sortedDropZones.First().Key;
                    
                    // skip if no change
                    if (currentHover != closest)
                    {
                        // exit previous zone
                        if (currentHover != null)
                            currentHover.HoverExit(this);

                        // enter closer one
                        closest.HoverEnter(this);
                        currentHover = closest;
                        OnChangeHover();
                    }                
                }
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (!enabled) return;

        // update distance
        DropZone d = collision.GetComponent<DropZone>();
        if (d != null && dropZones.ContainsKey(d))
        {
            dropZones[d] = collision.bounds.SqrDistance(transform.position);

            // new hoverable zone detected
            if (isDragged && d.CanHover(this))
            {
                var sortedDropZones = dropZones.OrderBy(e => -e.Key.priority).ThenBy(e => e.Value).SkipWhile(e => e.Key == null || !e.Key.CanHover(this));
                if (sortedDropZones.Count() > 0)
                {
                    var closest = sortedDropZones.First().Key;

                    // skip if no change
                    if (currentHover != closest)
                    {
                        // exit previous zone
                        if (currentHover != null)
                            currentHover.HoverExit(this);

                        // enter closer one
                        closest.HoverEnter(this);
                        currentHover = closest;
                        OnChangeHover();
                    } else
                    {
                        currentHover.Hover(this);
                    }
                }
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!enabled) return;

        // remove from potential dropzones
        DropZone d = collision.GetComponent<DropZone>();
        if (d != null)
        {
            dropZones.Remove(d);
            d.HoverExit(this);
            if (d == currentHover)
            {
                currentHover = null;
                OnChangeHover();
                ShapeManager.Instance.KillLoader();
            }
        }
    }

    /// <summary>
    /// Hides the object
    /// </summary>
    public void Hide()
    {
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
    }

    /// <summary>
    /// Shows the object
    /// </summary>
    public void Show()
    {
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
    }

    public void GoToPosAnim(Vector3 endPos, float duration)
    {
        iTween.MoveTo(gameObject,
            iTween.Hash("position", endPos, "time", duration, "easetype", iTween.EaseType.easeOutBack,
            "oncomplete", "OnAnimEnded"));
    }

    public void OnAnimEnded()
    {
        var destroy = rejectedReason != "" || SidePanel.Contains(transform.position);
        if (rejectedReason == "wrong")
            ShapeManager.Instance.Cross(transform.position);
        
        if (destroy)
            Destroy();
    }



    public Vector2 startScale;
    public bool bouncing;
    /// <summary>
    /// Perform a bouncing animation. TODO: change this to use iTween.
    /// </summary>
    public void Bounce()
    {
        // not running
        if (!bouncing)
        {
            startScale = transform.localScale;
        }
        // running
        else
        {
            StopCoroutine("Bounce");
            bouncing = false;
            transform.localScale = startScale;
        }
        StartCoroutine(Bounce(0.2f, 1, 0.2f));
    }

    public IEnumerator Bounce(float amount, int repetitions, float duration)
    {
        bouncing = true;
        float elapsedTime = 0;
        Vector2 startScale = transform.localScale;
        while (elapsedTime < duration && bouncing)
        {
            float scaledTime = Mathf.Repeat(elapsedTime * repetitions, duration);
            float delta = Mathf.Lerp(-amount, amount, scaledTime / duration);
            delta = 1 + (amount - Mathf.Abs(delta));
            elapsedTime += Time.deltaTime;
            transform.localScale = startScale * delta;
            yield return null;
        }
        if (bouncing) // stopped externally, probably the local scale was changed by something else so don't reset it
            transform.localScale = startScale;
        bouncing = false;
    }
}
