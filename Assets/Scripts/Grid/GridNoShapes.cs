using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine;
using System;

/// <summary>
/// A <see cref="Grid"/> used when the game mode type is <see cref="Config.GameType.WordToShape"/> and <see cref="Config.coloring"/> is turned on <br/>
/// Composed of:
/// <list type="bullet">
/// <item>A text for the current sentence (input), also used to position the other grid parts correctly. To handle two-colored texts, a secondary text is overlayed on top of the first one and they top/bottom is masked out.</item>
/// <item>For each word, a <see cref="DropZoneSnapHide"/> for the user to drop their answer (phonemes).</item>
/// </list>
/// </summary>
public class GridNoShapes : Grid
{
    [SerializeField] TextMeshPro tmp;
    [SerializeField] TextMeshPro tmp2;

    [SerializeField] GameObject maskPrefab;
    [SerializeField] GameObject gridPrefab;

    [SerializeField] Splash splash;

    public List<DropZoneSnapHide> grids = new List<DropZoneSnapHide>();
    List<GameObject> masks = new List<GameObject>();

    public HashSet<string> selectedPhonemes;

    private Color32[] primaryColors;
    private Color32[] secondaryColors;

    public override void GenerateFor(Sentence s)
    {
        currentSentence = s;

        tmp.fontSize = ScaleManager.Instance.fontSize;
        tmp2.fontSize = ScaleManager.Instance.fontSize;

        tmp.text = s.sentence;
        tmp2.text = s.sentence;

        UpdateTextAreaWidth();
        UpdateGrids(true, true);
        ScrollToWord(0);
    }

    public override void ChangeFilter(int filter)
    {
        if (currentSentence == null) return;

        // fill in the grids
        for (int i = 0; i < currentSentence.Length; i++)
        {
            grids[i].Fill(currentSentence.words[i].phonemes, filter, selectedPhonemes);
        }
        GridManager.Instance.OnGridChange(0, false);

        // update the letter colors
        tmp2.gameObject.SetActive(filter != Config.Filter.Both);
        ColorLetters(true, filter);
    }

    public override void Clear()
    {
        tmp.text = "";
        foreach (var grid in grids)
        {
            grid.GetComponent<DropZoneSnapHide>().Clear();
            Destroy(grid.gameObject);
        }
        // TODO: ongridchange ?
        grids.Clear();

        foreach (var mask in masks)
        {
            Destroy(mask);
        }
        masks.Clear();
    }

    public override void Hint(int wordIndex, Word answer)
    {
        grids[wordIndex].FillNext(answer);
    }

    public override void BlockGrid(bool block)
    {
        foreach (var grid in grids) grid.Block(block);
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

        //var minScroll = -tmp.textInfo.lineInfo[0].descender;
        //var maxScroll = -tmp.textInfo.lineInfo[tmp.textInfo.lineCount - 1].descender;
        //pos.y = Mathf.Clamp(pos.y + delta.y, minScroll, maxScroll);

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
            (firstLetter, _) = currentSentence.FirstAndLastFor(wordIndex, false);
        var lineNum = tmp.textInfo.characterInfo[firstLetter].lineNumber;
        var line = tmp.textInfo.lineInfo[lineNum];
        var y = line.baseline;

        y = -y + GridManager.Instance.scrollOffset;

        StopAllCoroutines();
        if (duration == 0)
            transform.position = new Vector2(0, y);
        else
            StartCoroutine(ScrollToYCoroutine(gameObject, y, duration));
    }

    public override void OnUpdateScale(float scale)
    {
        OnUpdateLayout();
    }

    public override void OnUpdateLayout()
    {
        UpdateTextAreaWidth();
        UpdateGrids(false, false);
        ScrollToWord(currentScrollWord, 0);
    }

    private void UpdateTextAreaWidth()
    {
        var rectTransform = tmp.rectTransform;
        var rectSize = rectTransform.sizeDelta;

        var halfWidth = Mathf.Min(Mathf.Abs(SidePanel.rightPanelX), Mathf.Abs(SidePanel.leftPanelX));

        rectSize.x = halfWidth * 1.8f; // 2 * half width - some padding
        rectTransform.sizeDelta = rectSize;
        tmp2.rectTransform.sizeDelta = rectSize;
    }

    public override void OnUpdateFontSize(int fontSize)
    {
        tmp.fontSize = fontSize;
        tmp2.fontSize = fontSize;
        UpdateGrids(false, true);

        ScrollToWord(currentScrollWord, 0);
    }

    private void ColorLetters(bool forceRecompute, int filter)
    {
        if (primaryColors == null || secondaryColors == null || forceRecompute)
        {
            tmp.ForceMeshUpdate();
            tmp2.ForceMeshUpdate();

            var info = tmp.textInfo;

            primaryColors = tmp.textInfo.meshInfo[0].colors32.ToArray();
            secondaryColors = tmp2.textInfo.meshInfo[0].colors32.ToArray();

            for (int wordIndex = 0; wordIndex < currentSentence.Length; wordIndex++)
            {
                var w = currentSentence.words[wordIndex];
                var (first, last) = currentSentence.FirstAndLastFor(wordIndex, false);
                var charIndex = first;

                for (int i = 0; i < w.Length; i++)
                {
                    string grapheme = w.graphemes[i].id;
                    bool skip = (!selectedPhonemes.Contains(w.phonemes[i].id) || ((int)w.phonemes[i].voco & filter) > 0);

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

                        int vertexIndex = info.characterInfo[charIndex].vertexIndex;

                        Color color = Color.white;
                        primaryColors[vertexIndex + 0] = color;
                        primaryColors[vertexIndex + 1] = color;
                        primaryColors[vertexIndex + 2] = color;
                        primaryColors[vertexIndex + 3] = color;

                        secondaryColors[vertexIndex + 0] = color;
                        secondaryColors[vertexIndex + 1] = color;
                        secondaryColors[vertexIndex + 2] = color;
                        secondaryColors[vertexIndex + 3] = color;

                        charIndex++;
                    }
                }
            }
        }

        primaryColors.CopyTo(tmp.textInfo.meshInfo[0].colors32, 0);
        secondaryColors.CopyTo(tmp2.textInfo.meshInfo[0].colors32, 0);
        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        tmp2.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private Color32[] tempPrimary;
    private Color32[] tempSecondary;

    internal void ResetColors()
    {
        if (tempPrimary != null && tempSecondary != null)
        {
            tempPrimary.CopyTo(primaryColors, 0);
            tempSecondary.CopyTo(secondaryColors, 0);
            tempPrimary = null;
            tempSecondary = null;

            tmp.ForceMeshUpdate();
            tmp2.ForceMeshUpdate();

            primaryColors.CopyTo(tmp.textInfo.meshInfo[0].colors32, 0);
            secondaryColors.CopyTo(tmp2.textInfo.meshInfo[0].colors32, 0);
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            tmp2.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }

    internal void Splash(Color[] colors, Vector2 pos)
    {
        splash.WithColors(colors, pos);
    }

    internal void ColorGrapheme(int wordIndex, int graphemeIndex, Color[] colors, bool temporary = false)
    {
        tmp.ForceMeshUpdate();
        tmp2.ForceMeshUpdate();

        if (temporary)
        {
            tempPrimary = primaryColors.ToArray();
            tempSecondary = secondaryColors.ToArray();
        }

        var info = tmp.textInfo;

        var w = currentSentence.words[wordIndex];
        var (first, last) = currentSentence.FirstAndLastFor(wordIndex, false);
        var charIndex = first;

        for (int i = 0; i < w.Length; i++)
        {
            string grapheme = w.graphemes[i].id;

            for (int j = 0; j < grapheme.Length; j++)
            {
                // find next matching character (skips punctuation except for ' in some cases)
                char c = info.characterInfo[charIndex].character;
                while (!string.Equals(c.ToString(), grapheme[j].ToString(), StringComparison.OrdinalIgnoreCase) && charIndex <= last)
                {
                    c = info.characterInfo[++charIndex].character;
                }

                // only color the wanted grapheme
                if (i != graphemeIndex)
                {
                    charIndex++;
                    continue;
                }

                int vertexIndex = info.characterInfo[charIndex].vertexIndex;

                Color color = colors[0];
                primaryColors[vertexIndex + 0] = color;
                primaryColors[vertexIndex + 1] = color;
                primaryColors[vertexIndex + 2] = color;
                primaryColors[vertexIndex + 3] = color;

                if (colors.Length > 1) color = colors[1];
                secondaryColors[vertexIndex + 0] = color;
                secondaryColors[vertexIndex + 1] = color;
                secondaryColors[vertexIndex + 2] = color;
                secondaryColors[vertexIndex + 3] = color;

                charIndex++;
            }
        }

        primaryColors.CopyTo(tmp.textInfo.meshInfo[0].colors32, 0);
        secondaryColors.CopyTo(tmp2.textInfo.meshInfo[0].colors32, 0);
        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        tmp2.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private float[] ComputeGraphemeCenters(int wordIndex)
    {
        tmp.ForceMeshUpdate();

        var info = tmp.textInfo;

        var w = currentSentence.words[wordIndex];
        var (first, last) = currentSentence.FirstAndLastFor(wordIndex, false);
        var charIndex = first;

        var centers = new float[w.Length];

        for (int i = 0; i < w.Length; i++)
        {
            string grapheme = w.graphemes[i].id;

            var min = Mathf.Infinity;

            for (int j = 0; j < grapheme.Length; j++)
            {
                // find next matching character (skips punctuation except for ' in some cases)
                char c = info.characterInfo[charIndex].character;
                while (!string.Equals(c.ToString(), grapheme[j].ToString(), StringComparison.OrdinalIgnoreCase) && charIndex <= last)
                {
                    c = info.characterInfo[++charIndex].character;
                }

                min = Mathf.Min(min, info.characterInfo[charIndex].bottomLeft.x);

                charIndex++;
            }
            centers[i] = (min + info.characterInfo[charIndex - 1].bottomRight.x) / 2;
        }
        return centers;
    }

    private void UpdateGrids(bool reset, bool recomputewidths = true)
    {
        if (currentSentence == null) return;

        if (recomputewidths)
        {
            tmp.ForceMeshUpdate(); tmp2.ForceMeshUpdate();

            // get widths of words to scale dropzones
            var widths = ComputeWidths();

            // update positions of those grids
            RepositionGrids(widths, reset);
        }
        else
        {
            if (grids.Count == 0) return;
            var widths = grids.Select(g => g.transform.localScale.x).ToArray();

            // update positions of grids
            RepositionGrids(widths, reset);
        }

        tmp.ForceMeshUpdate(); tmp2.ForceMeshUpdate();
        ColorLetters(reset, Config.filter);
        UpdateMasks(tmp);
    }

    public void UpdateGrids(int i, int j, Phoneme e)
    {
        GameObject temp = ShapeManager.Instance.CreatePhoneme(e, new Vector2(0, 0));
        grids[i].draggables[j] = temp.GetComponent<Draggable>();
        temp.SetActive(false);
    }

    private void RepositionGrids(float[] widths, bool reset)
    {
        var characterInfo = tmp.textInfo.characterInfo;
        var lineInfo = tmp.textInfo.lineInfo;

        var height = lineInfo[0].ascender - lineInfo[0].descender;

        for (int i = 0; i < currentSentence.Length; i++)
        {
            var word = currentSentence.words[i];
            var (firstChar, lastChar) = currentSentence.FirstAndLastFor(i, false);

            // get x position (center of word)
            var firstX = characterInfo[firstChar].bottomLeft.x;
            var lastX = characterInfo[lastChar].topRight.x;
            var x = (firstX + lastX) / 2;

            // get descender (y position of line)
            var lineNum = characterInfo[firstChar].lineNumber;
            var line = lineInfo[lineNum];
            var y = (line.ascender + line.descender) / 2;

            // add or update grid element
            DropZoneSnapHide grid;
            if (reset)
                grid = Instantiate(gridPrefab, transform).GetComponent<DropZoneSnapHide>();
            else
                grid = grids[i];

            var width = widths[i];

            grid.transform.SetParent(tmp.transform);
            grid.transform.localPosition = new Vector2(x, y);
            grid.transform.localScale = new Vector2(width, height);
            var centers = ComputeGraphemeCenters(i);
            
            if (reset)
            {
                grid.Init(this, i, centers);
                grids.Add(grid);
            }
            else
            {
                grid.UpdateCenters(centers);
            }
        }
    }

    private float[] ComputeWidths()
    {
        var widths = new float[currentSentence.Length];
        var firstAndLast = currentSentence.FirstAndLast;
        var characterInfo = tmp.textInfo.characterInfo;

        for (int i = 0; i < currentSentence.Length; i++)
        {
            var (firstChar, lastChar) = firstAndLast[i];

            var firstX = characterInfo[firstChar].bottomLeft.x;
            var lastX = characterInfo[lastChar].topRight.x;
            var width = lastX - firstX;
            widths[i] = width;
        }

        return widths;
    }

    public override void Clear(int wordIndex, int elementIndex, bool ghost = false)
    {
        grids?[wordIndex]?.Clear(elementIndex, ghost);
    }

    public override bool UndoLastMove()
    {
        var wordIndex = LastFilledIndex();
        if (wordIndex == -1) return false;
        for (int graphemeIndex = grids[wordIndex].draggables.Length - 1; graphemeIndex >= 0; graphemeIndex--)
        {
            var d = grids[wordIndex].draggables[graphemeIndex];
            if (d != null && d.name != "empty")
            {
                Clear(wordIndex, graphemeIndex);
                GridManager.Instance.OnGridChange(wordIndex, true);
                break;
            }
        }
        return LastFilledIndex() != -1;
    }

    private void UpdateMasks(TextMeshPro tmp)
    {
        var lines = tmp.textInfo.lineInfo;
        while (masks.Count < tmp.textInfo.lineCount)
            masks.Add(Instantiate(maskPrefab, tmp.transform));

        for (int i = 0; i < tmp.textInfo.lineCount; i++)
        {
            var height = (lines[i].ascender - lines[i].descender) * 0.4f; // some ratio of total height
            var y = lines[i].descender + height / 2; // descender + half of mask height
            var mask = masks[i];
            mask.transform.localScale = new Vector2(30, height);
            mask.transform.localPosition = new Vector2(0, y);
        }

        for (int i = tmp.textInfo.lineCount; i > masks.Count; i--)
        {
            Destroy(masks[i]);
            masks.RemoveAt(i);
        }
    }
}
