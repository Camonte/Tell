using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Element generated and used by <see cref="Grid"/>s, typically one per Word.
/// Snaps dropped <see cref="Draggable"/> Phonemes on a line.
/// </summary>
public class DropZoneSnap : DropZone
{
    public float scale { get; private set; }
    public Vector2[] centers;
    public Draggable[] draggables;
    private GameObject[] ghosts;
    int id;

    [SerializeField] GameObject graphemeCountPrefab;
    private TMPro.TextMeshPro[] counts;

    private GameObject previewGhost;

    private void Awake()
    {
        priority = 2;
    }

    public void Init(int id, float scale, int n, bool withCounts = false)
    {
        this.id = id;
        this.scale = scale;
        draggables = new Draggable[n];
        ghosts = new GameObject[n];
        OnStateChange(false);
        var width = transform.localScale.x;
        centers = Enumerable.Range(0, n).Select(i => new Vector2(((i+0.5f) * scale + i * Grid.padding) / width - 0.5f, 0)).ToArray();
        if (withCounts)
        {
            counts = new TMPro.TextMeshPro[n];
            for(int i = 0; i < centers.Length; i++)
            {
                var tmp = Instantiate(graphemeCountPrefab);
                tmp.SetActive(true);
                tmp.transform.SetParent(transform);
                tmp.transform.localPosition = centers[i] + Vector2.up * 0.5f;
                counts[i] = tmp.GetComponent<TMPro.TextMeshPro>();
            }
        }
    }

    internal void FillNext(Word answer)
    {
        for (int i = 0; i < draggables.Length; i++)
        {
            if (draggables[i] == null)
            {
                Phoneme ph = answer.phonemes[i];
                var d = ShapeManager.Instance.CreatePhoneme(ph, centers[i]).GetComponent<Draggable>();
                d.transform.localScale = new Vector2(scale, scale);
                d.transform.SetParent(transform);
                d.transform.localPosition = centers[i];
                draggables[i] = d;
                if (Config.progressiveCorrection)
                    d.enabled = false;

                OnStateChange(true);
                break;
            }
        }
    }

    public void UpdateWidth(float scale)
    {
        int n = centers.Length;
        this.scale = scale;
        var width = transform.localScale.x;
        centers = Enumerable.Range(0, n).Select(i => new Vector2(((i + 0.5f) * scale + i * Grid.padding) / width - 0.5f, 0)).ToArray();
        for(int i = 0; i < n; i++)
        {
            if (counts != null)
            {
                counts[i].transform.localPosition = centers[i] + Vector2.up * 0.5f;
            }
            var d = draggables[i];
            if (d == null) continue;
            d.transform.localPosition = d.name == "empty" ? centers[i] + Vector2.down * (scale/2) : centers[i];
            d.transform.SetParent(null);
            d.transform.localScale = new Vector2(scale, scale);
            d.transform.SetParent(transform);
        }
    }

    public override bool CanHover(Draggable draggable) => !Config.testMode;

    public override void Hover(Draggable draggable)
    {
        Vector2 pos = transform.InverseTransformPoint(draggable.transform.position);
        var index = BestIndex(pos);
        if (previewGhost == null)
        {
            previewGhost = ShapeManager.Instance.CreateGhost(draggable.gameObject);
            previewGhost.transform.localScale = new Vector2(scale, scale);
            previewGhost.transform.SetParent(transform);
        }

        if (index == -1 || !StateManager.Instance.DropOk(draggable.element, id, index))
        {
            previewGhost.gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < ghosts.Length; i++) if (ghosts[i] != null) ghosts[i].gameObject.SetActive(i != index);
            previewGhost.gameObject.SetActive(true);
            previewGhost.transform.localPosition = centers[index];
        }
    }

    internal void Fill(Word w)
    {
        Clear();

        for (int i = 0; i < w.Length; i++)
        {
            var ph = w.phonemes[i];

            var d = ShapeManager.Instance.CreatePhoneme(ph, centers[i]).GetComponent<Draggable>();
            d.transform.localScale = new Vector2(scale, scale);
            d.transform.SetParent(transform);
            d.transform.localPosition = centers[i];
            draggables[i] = d;
            // always immovable
            d.enabled = false;

            if (counts == null) continue;
            var grLength = w.graphemes[i].Length;
            counts[i].text = grLength.ToString();
        }
    }

    internal void Fill(Phoneme[] phonemes, int filter)
    {
        Clear();

        for(int i = 0; i < phonemes.Length; i++)
        {
            var ph = phonemes[i];
            if ((filter & (int)ph.voco) == 0) continue;

            if (draggables[i] != null) continue;

            var d = ShapeManager.Instance.CreateEmpty(centers[i]).GetComponent<Draggable>();
            d.transform.localScale = new Vector2(scale, scale);
            d.transform.SetParent(transform);
            d.transform.localPosition = centers[i] + Vector2.down * (scale / 2);
            draggables[i] = d;
            // always immovable
            d.enabled = false;
        }
    }

    public override void HoverExit(Draggable draggable)
    {
        if (previewGhost != null)
        {
            Destroy(previewGhost);
            previewGhost = null;
        }

        foreach (var ghost in ghosts) if (ghost != null) ghost.gameObject.SetActive(true);
    }

    public void Clear(int i, bool ghost = false)
    {
        if (ghost && ghosts[i] != null)
        {
            Destroy(ghosts[i]);
            ghosts[i] = null;
        }

        var d = draggables[i];
        if (d == null) return;
        if (ghost)
        {
            var ghostObj = ShapeManager.Instance.CreateGhost(d.gameObject);
            ghostObj.transform.localScale = new Vector2(scale, scale);
            ghostObj.transform.SetParent(transform);
            ghostObj.transform.localPosition = centers[i];
            ghosts[i] = ghostObj;
        }

        Destroy(d.gameObject);
        draggables[i] = null;
    }

    public void Block(bool block)
    {
        foreach (var d in draggables) if (d != null) d.enabled = !block;
    }

    public override void OnDrop(Draggable draggable)
    {
        // remove ghost
        if (previewGhost != null)
        {
            Destroy(previewGhost);
            previewGhost = null;
        }

        // get local coordinates
        Vector2 pos = transform.InverseTransformPoint(draggable.transform.position);

        var index = BestIndex(pos);

        // full grid or the state manager doesn't accept it e.g. progressive correction mode
        if (index == -1)
        {
            draggable.OnReject(this, "full");
            return;
        }
        if (!StateManager.Instance.DropOk(draggable.element, id, index))
        {
            draggable.OnReject(this, "wrong");
            return;
        }

        // remove ghost
        Destroy(ghosts[index]);
        ghosts[index] = null;

        draggable.transform.localScale = new Vector2(scale, scale);
        draggable.transform.SetParent(transform);
        iTween.MoveTo(draggable.gameObject, iTween.Hash("position", (Vector3)centers[index], "islocal", true, "speed", 1f));
        draggables[index] = draggable;
        if (Config.progressiveCorrection)
            draggable.enabled = false;
        OnStateChange(true);
    }

    public void Clear()
    {
        for (int i = 0; i < draggables.Length; i++) {
            draggables[i]?.Destroy();
            draggables[i] = null;
        }
    }

    private void OnStateChange(bool scroll)
    {
       GridManager.Instance.OnGridChange(id, scroll);
    }

    public override void OnPickup(Draggable draggable)
    {
        int i = Array.IndexOf(draggables, draggable);
        if (i == -1)
        {
            return;
        }
        draggables[i] = null;
        OnStateChange(false);
        draggable.transform.SetParent(null);
        draggable.transform.localScale = ScaleManager.Instance.GetScaleVector();
    }

    private int BestIndex(Vector2 pos)
    {
        int indexOfLastNonEmpty = new List<Draggable>(draggables).FindLastIndex(d => d != null);
        var best = centers.AsEnumerable()
            .Select((v, i) => (v: Vector2.Distance(pos, centers[i]), i))
            .OrderBy(x => x.v)
            .Select(x => x.i)
            .SkipWhile(i => draggables[i] != null || i > indexOfLastNonEmpty + 1);

        if (best.Count() > 0)
            return best.First();
        return -1;
    }

    private int BestFilledIndex(Vector2 pos)
    {
        var best = centers.AsEnumerable()
            .Select((v, i) => (v: Vector2.Distance(pos, centers[i]), i))
            .OrderBy(x => x.v)
            .Select(x => x.i)
            .SkipWhile(i => draggables[i] == null);

        if (best.Count() > 0)
            return best.First();
        return -1;
    }
}
