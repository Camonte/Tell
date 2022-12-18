using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Expanding Phoneme containing grapheme generators for a particular Phoneme: expands when tapped or when a Grapheme long hovers over it.
/// really messy tbh.
/// </summary>
public class DropZoneExpand : DropZone
{
    [SerializeField] GameObject generatorsWrapper;
    [SerializeField] GameObject background;

    [SerializeField] float maxScale = 8;

    [SerializeField] bool loaded; // DEBUG, don't set to false

    // phoneme for which we're expanding
    public string id;
    private Phoneme.Type type;
    private bool expanded = false;
    private List<GameObject> generators = new List<GameObject>();
    private SpriteRenderer[] renderers;
    private SortingGroup parentGroup;
    private TMPro.TextMeshPro archigrapheme;
    private Color[] colors;

    private Vector3 offset;
    private Camera _camera;

    private const float fontSizeArchi = 14;
    private const float fontSize = 6;
    private const float padding = 0.1f;
    private const int sortingOrder = 9999;

    public static DropZoneExpand currentlyExpandedZone;

    private static Dictionary<string, string> alternateBackgrounds = new Dictionary<string, string>()
    {
        {"u a", "a"}, {"k s", "k"}, {"g z", "g"}, {"n j", "n"}
    };

    private void Start()
    {
        _camera = Camera.main;
        transform.parent.name += $" ({id})";

        // replace the box collider by a circle collider for more accurate collisions on vowels
        var ph = Phoneme.For(id);
        if (ph.voco != Phoneme.Type.Consonant)
        {
            Destroy(GetComponent<BoxCollider2D>());
            gameObject.AddComponent<CircleCollider2D>();
        }
        type = ph.voco;

        ShapeManager.Instance.UpdatePhoneme(background, ph, false); // make it the shape & color of phoneme
        renderers = background.GetComponentsInChildren<SpriteRenderer>();
        // store colors
        colors = renderers.Select(r => r.color).ToArray();
        archigrapheme = GetComponentInChildren<TMPro.TextMeshPro>();
        archigrapheme.gameObject.SetActive(false); // disable archigrapheme (not used anymore)

        // align generators wrapper
        generatorsWrapper.transform.position = transform.position;
        generatorsWrapper.SetActive(false);

        parentGroup = transform.parent.GetComponent<SortingGroup>();
        generatorsWrapper.GetComponent<SortingGroup>().sortingOrder = 2; // always draw generators on top of background

        if (loaded)
        {
            var (scale, data) = Grapheme.dataForPhoneme[id];
            maxScale = scale;
            foreach (var (gr, position, isArchi) in data)
            {
                var obj = ShapeManager.Instance.CreateGraphemeGenerator(gr, id, position, isArchi ? fontSizeArchi : fontSize, generatorsWrapper.transform, this);
                generators.Add(obj);
                //if (isArchi && archigrapheme.text == "") archigrapheme.text = gr;
            }
            return;
        }

        // DEBUG
        var list = Grapheme.ListFor(id);
        for (int i = 0; i < list.Count; i++)
        {
            var pos = new Vector2(i % 4 - 1.5f, -i / 4);
            var generator = ShapeManager.Instance.CreateGraphemeGenerator(list[i], id, pos, fontSize, generatorsWrapper.transform, this);
            generators.Add(generator);
        }
    }

    // DEBUG
    public void Save()
    {
        var s = $"{{\"{id}\", ({maxScale}f, new List<(string, Vector2, bool)>() {{";
        foreach (var g in generators)
        {
            var tmp = g.GetComponentInChildren<TMPro.TextMeshPro>();
            s += $"(\"{g.name}\", new Vector2({g.transform.localPosition.x:0.#}f, {g.transform.localPosition.y:0.#}f), {(tmp.fontSize > fontSize ? "true" : "false")}), ";
        }
        s += "}) },\n";

        Debug.Log(s);
    }

    public void Expand()
    {
        if (!expanded)
        {
            if (currentlyExpandedZone != null)
            {
                currentlyExpandedZone.Collapse();
            }
            StopAllCoroutines();
            RecomputeOffset();
            StartCoroutine(ExpandCoroutine());
            expanded = true;
        }
    }

    public void Collapse()
    {
        if (expanded)
        {
            if (currentlyExpandedZone == this)
            {
                currentlyExpandedZone = null;
            }
            StopAllCoroutines();
            StartCoroutine(CollapseCoroutine());
            expanded = false;
        }
    }

    public void OnUpdateLayout()
    {
        RecomputeOffset();
        //generatorsWrapper.transform.localPosition = offset;
    }

    private void RecomputeOffset()
    {
        // clamp position of expanded zone to the screen borders and don't go over the height of the highest Phoneme in the side pannels
        var maxHeight = _camera.orthographicSize;
        var maxWidth = _camera.aspect * maxHeight;
        var maxTop = type == Phoneme.Type.Consonant ? SidePanel.leftMaxY : SidePanel.rightMaxY;
        maxTop = Mathf.Min(maxTop, maxHeight);
        var halfMaxScale = (maxScale * ScaleManager.Instance.GetScale()) / 2 + padding;

        var pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, -maxWidth + halfMaxScale, maxWidth - halfMaxScale);
        pos.y = Mathf.Clamp(pos.y, -maxHeight + halfMaxScale, maxTop - halfMaxScale);

        offset = transform.InverseTransformPoint(pos);

        generatorsWrapper.transform.position = pos;
    }

    public override bool CanDrop(Draggable draggable) => false;

    public override bool CanHover(Draggable draggable) => true;

    public override bool CanLongHover(Draggable draggable)
    {
        var ph = ((Grapheme)draggable.element).phoneme;
        return !expanded && (Phoneme.For(id, ph) != null || Phoneme.For(ph, id) != null);
    }

    public override void LongHover(Draggable draggable)
    {
        Expand();
    }

    public override void HoverEnter(Draggable draggable)
    {
        draggable.SetSortingOrder(sortingOrder + 1);
    }

    public override void HoverExit(Draggable draggable) { }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Draggable d))
        {
            if (expanded) d.SetSortingOrder(-1);
            Collapse();
        }
    }

    private Color white = new Color(1f, 1f, 1f, 0.8f);

    private IEnumerator ExpandCoroutine(float duration = 0.2f)
    {
        // dim the color and change the shape if needed
        archigrapheme.gameObject.SetActive(false);
        if (alternateBackgrounds.ContainsKey(id)) { ShapeManager.Instance.UpdatePhoneme(background, Phoneme.For(alternateBackgrounds[id])); renderers = background.GetComponentsInChildren<SpriteRenderer>(); }
        foreach (var r in renderers) r.color = white;
        parentGroup.sortingOrder = sortingOrder;

        float elapsedTime = 0;
        var startScale = new Vector2(1, 1);
        var targetScale = new Vector2(maxScale, maxScale);
        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(Vector3.zero, offset, elapsedTime / duration);
            transform.localScale = Vector2.Lerp(startScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = offset;
        transform.localScale = targetScale;
        currentlyExpandedZone = this;
        generatorsWrapper.SetActive(true);
    }

    private IEnumerator CollapseCoroutine(float duration = 0.2f)
    {
        generatorsWrapper.SetActive(false);

        float elapsedTime = 0;
        var startScale = new Vector2(maxScale, maxScale);
        var targetScale = new Vector2(1, 1);
        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(offset, Vector3.zero, elapsedTime / duration);
            transform.localScale = Vector2.Lerp(startScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = Vector3.zero;
        transform.localScale = targetScale;

        // full opacity and restore the original shape
        if (alternateBackgrounds.ContainsKey(id)) { ShapeManager.Instance.UpdatePhoneme(background, Phoneme.For(id)); renderers = background.GetComponentsInChildren<SpriteRenderer>(); }
        for (int i = 0; i < renderers.Length; i++) { var r = renderers[i]; r.color = colors[i]; }
        archigrapheme.gameObject.SetActive(true);
        parentGroup.sortingOrder = 0;
    }
}
