using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Element generated and used by <see cref="Grid"/>s, typically one per Word.
/// Hides and disables dropped <see cref="Draggable"/> Phonemes.
/// </summary>
public class DropZoneSnapHide : DropZone
{
    public Vector2[] centers;
    public Draggable[] draggables;
    private GridNoShapes grid;
    int id;
    private Draggable hiddenDraggable;

    private void Awake()
    {
        priority = 2;
    }

    public void Init(GridNoShapes grid, int id, float[] centers)
    {
        this.grid = grid;
        this.id = id;
        var n = centers.Length;
        draggables = new Draggable[n];
        OnStateChange(false);
        var width = transform.localScale.x;
        this.centers = centers.Select(x => new Vector2((x - transform.position.x) / width, 0)).ToArray();
    }

    public void UpdateCenters(float[] centers)
    {
        var width = transform.localScale.x;
        this.centers = centers.Select(x => new Vector2((x - transform.position.x) / width, 0)).ToArray();
    }

    public override bool CanHover(Draggable draggable) => !Config.testMode;
    public override void Hover(Draggable draggable)
    {
        Vector2 pos = transform.InverseTransformPoint(draggable.transform.position);
        var index = BestIndex(pos);
        if (index == -1 || !StateManager.Instance.DropOk(draggable.element, id, index))
        {
            grid.ResetColors();
        }
        else
        {
            grid.ResetColors();
            grid.ColorGrapheme(id, index, ((Phoneme)draggable.element).colors, true);
        }
    }

    public override void HoverExit(Draggable draggable)
    {
        grid.ResetColors();
    }

    internal void Fill(Phoneme[] phonemes, int filter, HashSet<string> selectedPhonemes)
    {
        Clear();

        for (int i = 0; i < phonemes.Length; i++)
        {
            var ph = phonemes[i];
            if (selectedPhonemes.Contains(ph.id) && (filter & (int)ph.voco) == 0) continue;
            if (draggables[i] != null) continue;
            ;
            var d = ShapeManager.Instance.CreatePhoneme(Phoneme.empty, centers[i]).GetComponent<Draggable>();
            d.transform.SetParent(transform);
            draggables[i] = d;
            d.Hide();
            d.enabled = false;
        }
    }

    public void Clear(int i, bool ghost = false)
    {
        draggables[i]?.Destroy();
        draggables[i] = null;

        grid.ColorGrapheme(id, i, new Color[] { Color.white });
    }

    internal void FillNext(Word answer)
    {
        for (int i = 0; i < draggables.Length; i++)
        {
            if (draggables[i] == null)
            {
                Phoneme ph = answer.phonemes[i];
                var d = ShapeManager.Instance.CreatePhoneme(ph, centers[i]).GetComponent<Draggable>();

                d.transform.SetParent(transform);
                draggables[i] = d;
                d.Hide();
                d.Block();
                var colors = ph.colors;
                grid.ColorGrapheme(id, i, colors);
                grid.Splash(colors, transform.TransformPoint(centers[i]));
                OnStateChange(true);
                break;
            }
        }
    }

    public void Block(bool block)
    {
        foreach (var d in draggables) if (d != null) d.enabled = !block;
    }

    public override void OnDrop(Draggable draggable)
    {
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

        draggable.transform.SetParent(transform);
        draggables[index] = draggable;
        draggable.Hide();
        draggable.Block();
        var colors = ((Phoneme)draggable.element).colors;
        grid.ColorGrapheme(id, index, colors);
        grid.Splash(colors, transform.TransformPoint(centers[index]));
        OnStateChange(true);
    }

    public void Clear()
    {
        for (int i = 0; i < draggables.Length; i++)
        {
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
    }

    private int BestIndex(Vector2 pos)
    {
        if (Config.progressiveCorrection) return new List<Draggable>(draggables).FindIndex(d => d == null);

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
