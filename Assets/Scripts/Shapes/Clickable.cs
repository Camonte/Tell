using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Represents an element that can be clicked by the finger/pen.
/// This is the main script attached to all Cards of the memory game. The attached element can be accessed via the `element` attribute.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(LeanFingerTap))]
[RequireComponent(typeof(Rigidbody2D))]
public class Clickable : MonoBehaviour
{
    private LeanFinger currentFinger;
    public SortingGroup sortingGroup;
    public Element element;
    static int z = 1;

    private void Start()
    {
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
        // DO THINGS WITH CARD
    }

    /// <summary>
    /// Disables the collider and hides this object.
    /// </summary>
    public void Block()
    {
        this.enabled = false;
        // MAYBE NEEDED WHEN FOUND
    }

    public virtual void OnDisable()
    {
        GetComponent<LeanFingerTap>().enabled = false;
    }

    public virtual void OnEnable()
    {
        GetComponent<DropZoneMerge>().enabled = true;
        GetComponent<LeanFingerTap>().enabled = true;
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

    public void Destroy()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Called when rejected by a dropzone. The reason indicates why it was rejected.
    /// </summary>
    public void OnReject(string reason)
    {
        Bounce();
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
