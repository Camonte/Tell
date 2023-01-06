using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Facilitates the generation and updating of Phoneme and Grapheme draggable objects and generators.
/// </summary>
public class ShapeManager : Singleton<ShapeManager>
{
    [Header("Phoneme prefabs")]
    [SerializeField] GameObject phonemeDraggablePrefab;
    [SerializeField] GameObject phonemeGeneratorPrefab;
    [SerializeField] GameObject phonemeEmptyPrefab;

    [Header("Grapheme prefabs")]
    [SerializeField] GameObject graphemeDraggablePrefab;
    [SerializeField] GameObject graphemeGeneratorPrefab;
    [SerializeField] GameObject expandableGeneratorPrefab;

    [Header("Other objects")]
    [SerializeField] GameObject crossObj;
    [SerializeField] Image loader;
    private Queue<GameObject> crosses = new Queue<GameObject>();

    public override void Awake() { }

    /// <summary>
    /// Creates a draggable Phoneme at a given position.
    /// </summary>
    public GameObject CreatePhoneme(Phoneme ph, Vector2 pos)
    {
        return CreatePhoneme(ph, pos, ScaleManager.Instance.GetScale());
    }

    /// <summary>
    /// Creates a draggable Phoneme at a given position with a given scale.
    /// </summary>
    public GameObject CreatePhoneme(Phoneme ph, Vector2 pos, float scale)
    {
        GameObject parent = Instantiate(phonemeDraggablePrefab);
        CreateGameObject(ph, parent, pos, new Vector2(scale, scale));
        return parent;
    }

    /// <summary>
    /// Creates an (optionnaly expandable) Phoneme generator at a given position, scale, and with the given parent.
    /// </summary>
    public GameObject CreatePhonemeGenerator(string ph, Vector2 pos, float scale, Transform parent, bool expandable = false)
    {
        var phoneme = Phoneme.For(ph);

        GameObject obj;
        if (expandable)
        {
            obj = Instantiate(expandableGeneratorPrefab);
            obj.GetComponentInChildren<DropZoneExpand>().id = ph;
        }
        else
        {
            obj = Instantiate(phonemeGeneratorPrefab);
            CreateGameObject(phoneme, obj, pos, new Vector2(scale, scale));
            obj.GetComponent<DropZoneMerge>().phoneme = phoneme;
        }

        obj.transform.SetParent(parent);
        obj.transform.localPosition = pos;
        return obj;
    }

    /// <summary>
    /// Creates an empty Phoneme object at a given position.
    /// </summary>
    public GameObject CreateEmpty(Vector2 pos)
    {
        var obj = Instantiate(phonemeEmptyPrefab).GetComponent<Draggable>();
        obj.element = Phoneme.empty;
        obj.enabled = false;
        obj.name = "empty";
        obj.transform.localScale = ScaleManager.Instance.GetScaleVector();
        obj.transform.position = pos;
        return obj.gameObject;
    }

    /// <summary>
    /// Updates a Grapheme object with a new Grapheme Element.
    /// </summary>
    internal void UpdateGrapheme(GameObject parent, Grapheme gr)
    {
        if (gr == null) return;
        CreateGraphemeObject(gr, parent, parent.transform.position, parent.GetComponentInChildren<TMPro.TextMeshPro>().fontSize);
    }

    /// <summary>
    /// Sets the opacity of an object.
    /// </summary>
    public void SetOpacity(GameObject obj, float opacity)
    {
        foreach (Transform child in obj.transform)
        {
            if (child.TryGetComponent(out SpriteRenderer renderer))
            {
                var color = renderer.color;
                color.a = opacity;
                renderer.color = color;
            }
            else
            {
                foreach (var tmp in child.GetComponentsInChildren<TMPro.TextMeshPro>())
                {
                    var color = tmp.color;
                    color.a = opacity;
                    tmp.color = color;
                }
            }
        }
    }

    /// <summary>
    /// Creates a "ghost" version of an object (immovable, with small opacity).
    /// </summary>
    public GameObject CreateGhost(GameObject obj)
    {
        var ghost = new GameObject("ghost");
        foreach(Transform child in obj.transform)
        {
            var childClone = Instantiate(child.gameObject, ghost.transform);
            if (childClone.TryGetComponent(out SpriteRenderer renderer))
            {
                var color = renderer.color;
                color.a = 0.3f;
                renderer.color = color;
            }
            else
            {
                foreach(var tmp in childClone.GetComponentsInChildren<TMPro.TextMeshPro>())
                {
                    var color = tmp.color;
                    color.a = 0.3f;
                    tmp.color = color;
                }
            }
            
        }
        ghost.AddComponent<SortingGroup>();
        return ghost;
    }

    /// <summary>
    /// Updates the font size of a Grapheme object.
    /// </summary>
    public GameObject UpdateGraphemeSize(GameObject parent, float fontSize)
    {
        var tmps = parent.GetComponentsInChildren<TMPro.TextMeshPro>(true);
        var mask = parent.GetComponentInChildren<SpriteMask>(true);

        foreach (var tmp in tmps)
        {
            tmp.fontSize = fontSize;
        }

        var mainTmp = tmps[0];
        mainTmp.ForceMeshUpdate();

        // set size of collider
        var size = mainTmp.bounds.size;
        size = Vector2.Max(size * colliderScaleFactor, minColliderSize);

        var collider = parent.GetComponent<BoxCollider2D>();
        collider.size = size;
        collider.offset = mainTmp.bounds.center;

        // setup mask
        var info = mainTmp.textInfo.characterInfo[0];
        var maskHeight = (info.ascender - info.descender) * 0.4f; // some ratio of total height
        var maskY = info.descender + maskHeight / 2; // descender + half of mask height
        mask.transform.localPosition = new Vector2(mainTmp.bounds.center.x, maskY);
        mask.transform.localScale = new Vector2(size.x, maskHeight);

        return parent;
    }

    /// <summary>
    /// Creates a Draggable Grapheme object at a given position with a given font size.
    /// </summary>
    public GameObject CreateGrapheme(Grapheme gr, Vector2 pos, float fontSize)
    {
        GameObject parent = Instantiate(graphemeDraggablePrefab);
        CreateGraphemeObject(gr, parent, pos, fontSize);
        return parent;
    }

    /// <summary>
    /// Creates a Draggable Grapheme object at a given position with default font size.
    /// </summary>
    public GameObject CreateGrapheme(Grapheme gr, Vector2 pos)
    {
        return CreateGrapheme(gr, pos, ScaleManager.Instance.fontSize);
    }

    /// <summary>
    /// Creates a Grapheme Generator with a given position, font size, parent. Optionnally, a refernce to the expand phoneme it is part of.
    /// </summary>
    public GameObject CreateGraphemeGenerator(string gr, string ph, Vector3 pos, float fontSize, Transform parent = null, DropZoneExpand dropZone = null)
    {
        GameObject obj = Instantiate(graphemeGeneratorPrefab);
        var grapheme = new Grapheme(gr, ph);
        var generatorBehviour = obj.GetComponent<DropZoneMergeGrapheme>();
        generatorBehviour.grapheme = grapheme;
        generatorBehviour.container = dropZone;
        CreateGraphemeObject(grapheme, obj, pos, fontSize);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = pos;
        return obj;
    }

    /// <summary>
    /// Deletes all draggable elements.
    /// </summary>
    public void Clear()
    {
        foreach(var obj in FindObjectsOfType<Draggable>())
        {
            /*if (obj.CompareTag("Phoneme"))*/ Destroy(obj.gameObject);
        }
    }

    /// <summary>
    /// Updates a Phoneme object with a new Phoneme Element.
    /// </summary>
    public void UpdatePhoneme(GameObject parent, Phoneme ph, bool destroyChildren = true)
    {
        if (ph == null) return;

        if (destroyChildren)
        {
            foreach (Transform child in parent.transform)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        CreateGameObject(ph, parent, parent.transform.position, parent.transform.localScale);
    }

    private Vector2 minColliderSize = new Vector2(1f,1f);
    private const float colliderScaleFactor = 1.2f;

    private GameObject CreateGraphemeObject(Grapheme gr, GameObject parent, Vector2 pos, float fontSize)
    {
        Draggable d = parent.GetComponent<Draggable>();
        var tmps = parent.GetComponentsInChildren<TMPro.TextMeshPro>(true);
        var mask = parent.GetComponentInChildren<SpriteMask>(true);

        if (d != null)
        {
            d.element = gr;
        }

        // get colors
        var colors = Phoneme.For(gr.phoneme).colors;

        for (int i = 0; i < colors.Length; i++)
        {
            var tmp = tmps[i];
            tmp.gameObject.SetActive(true);
            tmp.text = gr.id;
            tmp.fontSize = fontSize;
            //tmp.autoSizeTextContainer = true;
            tmp.color = colors[i];
        }
        // deactivate second text and mask if only one color
        var isDouble = colors.Length == 2;
        mask.gameObject.SetActive(isDouble);
        tmps[1].gameObject.SetActive(isDouble);

        var mainTmp = tmps[0];
        mainTmp.ForceMeshUpdate();

        // set size of collider
        var size = mainTmp.bounds.size;
        size = Vector2.Max(size * colliderScaleFactor, minColliderSize);

        var collider = parent.GetComponent<BoxCollider2D>();
        collider.size = size;
        collider.offset = mainTmp.bounds.center;

        // setup mask
        var info = mainTmp.textInfo.characterInfo[0];
        var maskHeight = (info.ascender - info.descender) * 0.4f; // some ratio of total height
        var maskY = info.descender + maskHeight / 2; // descender + half of mask height
        mask.transform.localPosition = new Vector2(mainTmp.bounds.center.x, maskY);
        mask.transform.localScale = new Vector2(size.x, maskHeight);

        parent.name = gr.id;
        parent.transform.position = pos;
        return parent;
    }

    private GameObject CreateGameObject(Phoneme ph, GameObject parent, Vector3 pos, Vector2 scale)
    {
        Draggable d = parent.GetComponent<Draggable>();
        
        for (int i = 0; i < ph.Length; i++)
        {
            GameObject child = new GameObject($"Shape Part {i+1}");
            var renderer = child.AddComponent<SpriteRenderer>();
            var sprite = Resources.Load<Sprite>($"Shapes/borders/{ph.shapes[i]}");
            renderer.color = ph.colors[i];
            renderer.sprite = sprite;

            child.transform.SetParent(parent.transform);
            child.transform.localScale = Vector2.one;
            if (ph.isDouble)
            {
                child.transform.localPosition = new Vector2(0, (i == 0 ? 1 : -1) * 0.25f);      
            } else
            {
                child.transform.localPosition = new Vector2();
            }
        }
        if (d != null) d.element = ph;
        parent.name = ph.id;
        parent.transform.position = pos;
        parent.transform.localScale = scale;
        return parent;
    }

    /// <summary>
    /// Starts the long hover loader at a given position (world coordinates) with a given duration to complete.
    /// </summary>
    public void Loader(Vector2 pos, float duration)
    {
        StopAllCoroutines();
        var go = loader.transform.parent.gameObject;
        go.SetActive(true);
        go.transform.position = pos;
        StartCoroutine(LoaderAnim(duration));
    }

    /// <summary>
    /// Stops the long hover loader.
    /// </summary>
    public void KillLoader()
    {
        StopAllCoroutines();
        loader.transform.parent.gameObject.SetActive(false);
    }

    private IEnumerator LoaderAnim(float duration)
    {
        var elapsed = 0f;
        while (elapsed < duration)
        {
            loader.fillAmount = Mathf.Lerp(0, 1, elapsed/duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        loader.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Show a cross at a given position (world coordinates). The cross expands and disappears after a short amount of time.
    /// </summary>
    public void Cross(Vector2 pos)
    {
        var cross = crosses.Count > 0 ? crosses.Dequeue() : Instantiate(crossObj, transform);
        cross.SetActive(true);
        cross.transform.position = pos;
        iTween.ScaleFrom(cross, iTween.Hash("scale", Vector3.zero, "time", 0.5f, "easetype", iTween.EaseType.easeOutBack, "oncompletetarget", gameObject, "oncomplete", "CleanupCross", "oncompleteparams", cross));
    }

    private void CleanupCross(GameObject cross)
    {
        cross.SetActive(false);
        crosses.Enqueue(cross);
    }
}
