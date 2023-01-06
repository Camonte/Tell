using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// A <see cref="Grid"/> used when the game mode type is <see cref="Config.GameType.ShapeToWord"/> <br/>
/// Composed of:
/// <list type="bullet">
/// <item>A hidden text for the current sentence in order to position the other grid parts correctly.</item>
/// <item>For each word, a <see cref="DropZoneSnap"/> to show the colored shapes (input).</item>
/// <item>For each word, a <see cref="DropZoneSnapGrapheme"/> for the user to drop their answer (graphemes).</item>
/// </list>
/// </summary>
public class GridWithShapesGrapheme : Grid
{

    [SerializeField] TextMeshPro tmp;

    [SerializeField] GameObject shapeGridPrefab;
    [SerializeField] GameObject textGridPrefab;

    List<DropZoneSnap> shapeGrids = new List<DropZoneSnap>();
    List<DropZoneSnapGrapheme> grids = new List<DropZoneSnapGrapheme>();

    public override void GenerateFor(Sentence s)
    {
        currentSentence = s;
        tmp.fontSize = ScaleManager.Instance.fontSize;
        UpdateTextAreaWidth();
        UpdateGrids(true);
        ScrollToWord(0);
    }

    public override void Clear()
    {
        tmp.text = "";
        foreach (var grid in grids)
        {
            grid.GetComponent<DropZoneSnapGrapheme>().Clear();
            Destroy(grid.gameObject);
        }
        foreach (var grid in shapeGrids)
        {
            grid.GetComponent<DropZoneSnap>().Clear();
            Destroy(grid.gameObject);
        }
        // TODO: ongridchange ?
        grids.Clear();
        shapeGrids.Clear();
    }

    public override void Hint(int wordIndex, Word answer)
    {
        grids[wordIndex].FillNext(answer);
    }

    public override List<Draggable[]> GetState()
    {
        var state = new List<Draggable[]>();
        foreach (var grid in grids)
        {
            state.Add(grid.draggables);
        }
        return state;
    }

    public override int FirstNullIndex(int fromIndex = 0)
    {
        for (int i = fromIndex; i < grids.Count && i >= 0; i++)
        {
            if (grids[i].draggables.Any(d => d == null))
                return i;
        }
        return -1;
    }

    public override int FirstNullElement() 
    { 
        for (int i = 0; i < grids.Count && i >= 0; i++)
        {
            for (int j = 0 ; j < grids[i].draggables.Length ; j++)
            {
                if (grids[i].draggables[j] == null)
                {
                    return j;
                }
            }
        }
        return -1; 
    }

    public override int LastFilledIndex()
    {
        return grids.FindLastIndex(g => g.draggables.Any(d => d != null && d.name != "empty"));
    }

    public override void Scroll(Vector2 delta)
    {
        StopAllCoroutines();
        var pos = transform.position;

        pos.y += delta.y;
        transform.position = pos;
    }

    public override void ScrollToWord(int wordIndex, float duration = 0.2f)
    {
        if (currentSentence == null) return;
        
        int firstLetter;
        if (currentSentence.Length == 0)
            firstLetter = 0;
        else
            (firstLetter, _) = currentSentence.FirstAndLastFor(wordIndex);
        var lineNum = tmp.textInfo.characterInfo[firstLetter].lineNumber;
        var line = tmp.textInfo.lineInfo[lineNum];
        var y = line.ascender;

        y = -y + GridManager.Instance.scrollOffset;

        StopAllCoroutines();
        if (duration == 0)
            transform.position = new Vector2(0, y);
        else
            StartCoroutine(ScrollToYCoroutine(gameObject, y, duration));
    }

    public override void OnUpdateScale(float scale)
    {
        UpdateTextAreaWidth();
        UpdateGrids(false);
        ScrollToWord(currentScrollWord, 0);
    }

    public override void OnUpdateLayout()
    {
        UpdateTextAreaWidth();
        UpdateGrids(false, false);
        ScrollToWord(currentScrollWord, 0);
    }

    public override void OnUpdateFontSize(int fontSize)
    {
        tmp.fontSize = fontSize;
        UpdateGrids(false);
        ScrollToWord(currentScrollWord, 0);
    }

    private void UpdateTextAreaWidth()
    {
        var rectTransform = tmp.rectTransform;
        var rectSize = rectTransform.sizeDelta;

        var halfWidth = Mathf.Min(Mathf.Abs(SidePanel.rightPanelX), Mathf.Abs(SidePanel.leftPanelX));

        rectSize.x = halfWidth * 1.8f; // 2 * half width - some padding
        rectTransform.sizeDelta = rectSize;
    }

    public override void Clear(int wordIndex, int elementIndex, bool ghost = false)
    {
        grids?[wordIndex]?.Clear(elementIndex, ghost);
    }

    private float[] ComputeGraphemeCenters(int wordIndex, int filter = 0)
    {
        tmp.ForceMeshUpdate();

        var info = tmp.textInfo;

        var w = currentSentence.words[wordIndex];
        var (first, last) = currentSentence.FirstAndLastFor(wordIndex);
        var charIndex = first;

        var centers = new float[w.Length];

        for (int i = 0; i < w.Length; i++)
        {
            string grapheme = w.graphemes[i].id;
            bool skip = ((int)w.phonemes[i].voco & filter) > 0;

            var min = Mathf.Infinity;

            for (int j = 0; j < grapheme.Length; j++)
            {
                // find next matching character (skips punctuation except for ' in some cases)
                char c = info.characterInfo[charIndex].character;
                while (!string.Equals(c.ToString(), grapheme[j].ToString(), StringComparison.OrdinalIgnoreCase) && charIndex <= last)
                {
                    c = info.characterInfo[++charIndex].character;
                }

                // filter based on phoneme type
                if (skip)
                {
                    charIndex++;
                    continue;
                }

                min = Mathf.Min(min, info.characterInfo[charIndex].bottomLeft.x);

                charIndex++;
            }
            centers[i] = (min + info.characterInfo[charIndex-1].bottomRight.x) / 2;
        }
        return centers;
    }

    private void UpdateGrids(bool reset, bool recomputewidths = true)
    {
        if (currentSentence == null) return;

        if (recomputewidths)
        {
            RegenerateText(currentSentence.sentence, false);
            var (scale, widths) = ComputeMinScale();
            var height = ComputeDropZoneHeight(scale);
            var text = ComputeWidthsAndGetUpdatedText(height, scale, widths);
            RegenerateText(text, true);
            RepositionGrids(height, scale, widths, reset);
        }
        else
        {
            if (grids.Count == 0) return;
            var scale = shapeGrids[0].scale;
            var height = ComputeDropZoneHeight(scale);
            var widths = shapeGrids.Select(g => g.transform.localScale.x).ToArray();
            RepositionGrids(height, scale, widths, reset);
        }
    }

    private void RepositionGrids(float height, float scale, float[] widths, bool reset)
    {
        var characterInfo = tmp.textInfo.characterInfo;
        var lineInfo = tmp.textInfo.lineInfo;

        for (int i = 0; i < currentSentence.Length; i++)
        {
            var word = currentSentence.words[i];
            var (firstChar, lastChar) = currentSentence.FirstAndLastFor(i);

            // get x position (center of word)
            var firstX = characterInfo[firstChar].bottomLeft.x;
            var lastX = characterInfo[lastChar].topRight.x;
            var x = (firstX + lastX) / 2;

            // center text grids on line
            var lineNum = characterInfo[firstChar].lineNumber;
            var line = lineInfo[lineNum];
            var y = (line.lineExtents.max.y + line.lineExtents.min.y) / 2;

            // add or update grid element for dropping graphemes
            DropZoneSnapGrapheme grid;
            if (reset)
                grid = Instantiate(textGridPrefab, transform).GetComponent<DropZoneSnapGrapheme>();
            else
                grid = grids[i];

            grid.gameObject.SetActive(true);
            grid.transform.SetParent(tmp.transform);
            grid.transform.localPosition = new Vector2(x, y);
            grid.transform.localScale = tmp.GetPreferredValues(word.word);
            if (reset)
            {
                var centers = ComputeGraphemeCenters(i);
                grid.Init(i, word.Length, centers);
                grids.Add(grid);
            }
            else
            {
                grid.UpdateWidth(tmp.fontSize);
            }

            // add or update grid elements prefilled with shapes and with deactivated colliders
            DropZoneSnap shapeGrid;
            if (reset)
            {
                shapeGrid = Instantiate(shapeGridPrefab, transform).GetComponent<DropZoneSnap>();
                shapeGrid.GetComponent<Collider2D>().enabled = false;
            }
            else
                shapeGrid = shapeGrids[i];

            // position it above the (hidden) text
            var width = widths[i];
            var shapesY = line.ascender + height / 2;
            shapeGrid.gameObject.SetActive(true);
            shapeGrid.transform.SetParent(tmp.transform);
            shapeGrid.transform.localPosition = new Vector2(x, shapesY);
            shapeGrid.transform.localScale = new Vector2(width, height);
            if (reset)
            {
                shapeGrid.Init(i, scale, word.Length, Config.characterCounts);
                shapeGrids.Add(shapeGrid);
                shapeGrid.Fill(word);
                shapeGrid.Block(true);
            }
            else
            {
                shapeGrid.UpdateWidth(scale);
            }
        }
    }

    private void RegenerateText(string text, bool enableWordWrapping)
    {
        tmp.enableWordWrapping = enableWordWrapping;
        tmp.text = text;
        tmp.ForceMeshUpdate();
    }

    private string ComputeWidthsAndGetUpdatedText(float height, float scale, float[] widths)
    {
        // we add hidden characters (indicated in the example below by [?]) so that TMPro takes into account the extra horizontal spaces we add around words when figuring out the wrapping of the text
        // note that this adds 2 extra characters per word, which needs to be taken into account when getting a character's mesh (for coloring, or position) via it's index
        // we also add a <nobr> tag around each word so that they aren't split (we can't break up dropzones over multiple lines)
        //
        // example: [?]   hello   [?]
        var hiddenCharStr = "<size=0>\u2060<size=1e>";

        var firstAndLast = currentSentence.FirstAndLast;

        var text = "";

        // add blank space to account for short words with lots of shapes
        var nAdded = text.Length;
        text += currentSentence.sentence;
        for (int i = 0; i < currentSentence.Length; i++)
        {
            var width = widths[i];
            var n = currentSentence.words[i].Length;
            var minWidth = n * scale + (n - 1) * padding; // padding only between shapes
            var strBefore = "";
            var strAfter = "";

            strBefore += "<nobr>" + hiddenCharStr;

            if (width < minWidth)
            {
                // extra width in pixels
                var space = (minWidth - width) * 10f;
                var spaceStr = $"<space={space / 2}>";
                strBefore += spaceStr;
                strAfter += spaceStr;
            }

            strAfter += hiddenCharStr + "</nobr>";

            if (strBefore.Length > 0)
            {
                var (firstChar, lastChar) = firstAndLast[i];
                var beforeIdx = firstChar + nAdded;
                text = text.Insert(beforeIdx, strBefore);
                nAdded += strBefore.Length;

                var afterIdx = lastChar + 1 + nAdded;
                text = text.Insert(afterIdx, strAfter);
                nAdded += strAfter.Length;
            }

            widths[i] = minWidth;
        }

        // make spaces (and `-`) larger for clear separation between words
        text = text.Replace("-", " ").Replace(" ", "<size=30> <size=1e>");

        // define lineheight = fontsize + height of drop zone + 20 pixels padding
        text = text.Insert(0, $"<line-height={tmp.fontSize + 10 * height + 20}>");

        return text;
    }

    private float ComputeDropZoneHeight(float scale)
    {
        return scale + 2 * padding;
    }

    private (float, float[]) ComputeMinScale()
    {
        var characterInfo = tmp.textInfo.characterInfo;
        var firstAndLast = currentSentence.FirstAndLast;

        // get smallest scale
        var widths = new float[currentSentence.Length];
        var scale = ScaleManager.Instance.GetScale();
        for (int i = 0; i < currentSentence.Length; i++)
        {
            var word = currentSentence.words[i];
            var (firstChar, lastChar) = firstAndLast[i];

            // get width of word
            var firstX = characterInfo[firstChar].bottomLeft.x;
            var lastX = characterInfo[lastChar].topRight.x;
            var width = lastX - firstX;
            scale = Mathf.Min(scale, width / word.Length);
            widths[i] = width;
        }
        return (Mathf.Max(ScaleManager.minScale, scale), widths);
    }

    public void PlaceGrapheme(Grapheme e, int i, int j){
        var d = ShapeManager.Instance.CreateGrapheme(e, grids[i].transform.position).GetComponent<Draggable>();
        d.transform.localScale = Vector2.one;
        d.transform.SetParent(grids[i].transform);
        d.transform.localPosition = grids[i].centers[j];
        grids[i].draggables[j] = d;
        d.enabled = false;
    }
}
