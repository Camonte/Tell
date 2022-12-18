using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Element generated and used by <see cref="Grid"/>s, typically one per Word.
/// Snaps dropped <see cref="Draggable"/> Graphemes on a line. The elements are placed using an invisible text mesh.
/// </summary>
public class DropZoneSnapGrapheme : DropZone
{
    public Vector2[] centers;
    public Draggable[] draggables;
    private GameObject[] ghosts;
    int id;
    [SerializeField] TMPro.TextMeshPro tmp;
    private int hiddenIndex;

    private void Awake()
    {
        hiddenIndex = -2;
        priority = 2;
    }

    public void Init(int id, int n, float[] centersX)
    {
        this.id = id;
        draggables = new Draggable[n];
        ghosts = new GameObject[n];
        OnStateChange(false);
        var width = transform.localScale.x;
        centers = centersX.Select(x => new Vector2((x - transform.position.x) / width, 0)).ToArray();

        tmp.text = "";
        tmp.transform.SetParent(null);
        tmp.transform.localScale = Vector2.one;
        tmp.rectTransform.sizeDelta = new Vector2(width, 1);
        tmp.transform.SetParent(transform);
        tmp.transform.localPosition = Vector2.zero;
    }

    public void UpdateWidth(float fontSize)
    {
        int n = centers.Length;
        var width = transform.localScale.x;
        for (int i = 0; i < n; i++)
        {
            var d = draggables[i];
            if (d == null) continue;
            d.transform.SetParent(null);
            d.transform.localScale = Vector2.one;
            ShapeManager.Instance.UpdateGraphemeSize(d.gameObject, fontSize);
            d.transform.SetParent(transform);
            d.transform.localPosition = centers[i];
        }

        tmp.transform.SetParent(null);
        tmp.transform.localScale = Vector2.one;
        tmp.fontSize = fontSize;
        tmp.rectTransform.sizeDelta = new Vector2(width, 1);
        tmp.transform.SetParent(transform);
        tmp.transform.localPosition = Vector2.zero;
    }

    internal void FillNext(Word answer)
    {
        for (int i = 0; i < draggables.Length; i++)
        {
            if (draggables[i] == null)
            {
                Grapheme ph = answer.graphemes[i];
                var d = ShapeManager.Instance.CreateGrapheme(ph, centers[i]).GetComponent<Draggable>();
                draggables[i] = d;
                RecomputeCenters(StringParts());
                d.transform.SetParent(transform);
                d.transform.localPosition = centers[i];
                d.enabled = false;
                d.GetComponent<DropZoneMergeGrapheme>().enabled = false;
                OnStateChange(true);
                break;
            }
        }
    }

    public override bool CanHover(Draggable draggable) => !Config.testMode;

    public override bool CanHover(DraggableEraser draggable) => true;

    private GameObject previewGhost;

    public override void Hover(Draggable draggable)
    {
        Vector2 pos = transform.InverseTransformPoint(draggable.transform.position);
        var index = BestIndex(pos);
        if (previewGhost == null)
        {
            previewGhost = ShapeManager.Instance.CreateGhost(draggable.gameObject);
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

            draggables[index] = draggable;
            RecomputeCenters(StringParts(), true);
            draggables[index] = null;
            previewGhost.transform.localPosition = centers[index];
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

    public override void HoverExit(DraggableEraser draggable)
    {
        if (hiddenIndex != -2)
        {
            draggables[hiddenIndex].Show();
            hiddenIndex = -2;
        }
    }

    public override void Hover(DraggableEraser draggable)
    {
        Vector2 pos = transform.InverseTransformPoint(draggable.transform.position);
        var index = BestFilledIndex(pos);
        if (index != -1 && draggables[index] != null)
        {
            draggables[index].Hide();
            hiddenIndex = index;
        }
    }

    public void Clear(int i, bool ghost = false)
    {
        if (draggables.All(d => d == null)) return;
        Clear();
        ShapeManager.Instance.Cross(transform.position);

        /*
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
            ghostObj.transform.SetParent(transform);
            ghostObj.transform.localPosition = centers[i];
            ghosts[i] = ghostObj;
        }

        Destroy(d.gameObject);
        draggables[i] = null;
        */
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

        draggables[index] = draggable;

        // recompute centers
        RecomputeCenters(StringParts());

        // remove ghost
        Destroy(ghosts[index]);
        ghosts[index] = null;

        draggable.transform.SetParent(transform);
        iTween.MoveTo(draggable.gameObject, iTween.Hash("position", (Vector3)centers[index], "islocal", true, "speed", 1f));
        //draggable.transform.localPosition = centers[index];
        //if (Config.progressiveCorrection) // LOCK regardless of correction mode
        draggable.enabled = false;

        draggable.GetComponent<DropZoneMergeGrapheme>().enabled = false;

        OnStateChange(true);
    }

    public override void OnDrop(DraggableEraser draggable)
    {
        // get local coordinates
        Vector2 pos = transform.InverseTransformPoint(draggable.transform.position);

        var index = BestFilledIndex(pos);

        // nothing to erase
        if (index == -1)
        {
            draggable.OnReject(this, "full");
            return;
        }
        //if (!StateManager.Instance.DropOk(draggable.element, id, index))
        //{
        //    draggable.OnReject(this, "wrong");
        //    return;
        //}
        draggables[index].Destroy();
        draggables[index] = null;
        draggable.Destroy();
        OnStateChange(true);
    }

    private string[] StringParts()
    {
        return draggables.Where(d => d != null).Select(d => d.element.id).ToArray();
    }

    private void RecomputeCenters(string[] graphemes, bool temporary = false)
    {
        var oldText = tmp.text;
        tmp.text = string.Join("", graphemes);
        tmp.ForceMeshUpdate();
        var info = tmp.textInfo.characterInfo;

        var start = 0;
        for(int i = 0; i < graphemes.Length; i++)
        {
            var gr = graphemes[i];
            var x = (info[start].bottomLeft.x + info[start+gr.Length-1].bottomRight.x) / 2;
            start += gr.Length;
            var pos = transform.InverseTransformPoint(new Vector2(x, 0) + (Vector2)transform.position);
            centers[i] = pos;   
        }
        if (temporary)
        {
            tmp.text = oldText;
            tmp.ForceMeshUpdate();
        }
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

        draggable.GetComponent<DropZoneMergeGrapheme>().enabled = true;
        draggables[i] = null;
        RecomputeCenters(StringParts());
        OnStateChange(false);
        draggable.transform.SetParent(null);
        draggable.transform.localScale = Vector2.one;
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
