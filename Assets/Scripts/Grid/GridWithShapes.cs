using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// A <see cref="Grid"/> used when the game mode type is <see cref="Config.GameType.WordToShape"/> and <see cref="Config.coloring"/> is turned off <br/>
/// Composed of:
/// <list type="bullet">
/// <item>A text for the current sentence (input), also used to position the other grid parts correctly.</item>
/// <item>For each word, a <see cref="DropZoneSnap"/> for the user to drop their answer (phonemes).</item>
/// </list>
/// </summary>
public class GridWithShapes : Grid
{
    [SerializeField] TextMeshPro tmp;

    [SerializeField] GameObject gridPrefab;

    public List<DropZoneSnap> grids = new List<DropZoneSnap>();

    public HashSet<string> selectedPhonemes;

    public override void Init()
    {
        base.Init();
    }

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
            grid.GetComponent<DropZoneSnap>().Clear();
            Destroy(grid.gameObject);
        }
        // TODO: ongridchange ?
        grids.Clear();
    }
    public override void BlockGrid(bool block)
    {
        foreach (var grid in grids) grid.Block(block);
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
            (firstLetter, _) = currentSentence.FirstAndLastFor(wordIndex);
        var lineNum = tmp.textInfo.characterInfo[firstLetter].lineNumber;
        var line = tmp.textInfo.lineInfo[lineNum];
        var y = line.descender;

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

    private void UpdateTextAreaWidth()
    {
        var rectTransform = tmp.rectTransform;
        var rectSize = rectTransform.sizeDelta;

        var halfWidth = Mathf.Min(Mathf.Abs(SidePanel.rightPanelX), Mathf.Abs(SidePanel.leftPanelX));

        rectSize.x = halfWidth * 1.8f; // 2 * half width - some padding
        rectTransform.sizeDelta = rectSize;
    }

    public override void OnUpdateFontSize(int fontSize)
    {
        tmp.fontSize = fontSize;
        UpdateGrids(false);

        ScrollToWord(currentScrollWord, 0);
    }

    public override void Clear(int wordIndex, int elementIndex, bool ghost = false)
    {
        grids?[wordIndex]?.Clear(elementIndex, ghost);
    }

    /*
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
    */

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
            var scale = grids[0].scale;
            var height = ComputeDropZoneHeight(scale);
            var widths = grids.Select(g => g.transform.localScale.x).ToArray();
            RepositionGrids(height, scale, widths, reset);
        }

        //UpdateMasks();
    }

    private void RepositionGrids(float height, float scale, float[] widths, bool reset)
    {
        var characterInfo = tmp.textInfo.characterInfo; //.Where(c => c.pointSize > 0).ToArray();
        var lineInfo = tmp.textInfo.lineInfo;

        for (int i = 0; i < currentSentence.Length; i++)
        {
            var word = currentSentence.words[i];
            var (firstChar, lastChar) = currentSentence.FirstAndLastFor(i);

            // get x position (center of word)
            var firstX = characterInfo[firstChar].bottomLeft.x;
            var lastX = characterInfo[lastChar].topRight.x;
            var x = (firstX + lastX) / 2;

            // get descender (y position of line)
            var lineNum = characterInfo[firstChar].lineNumber;
            var line = lineInfo[lineNum];
            var y = line.descender; // line.baseline

            // add or update grid element
            DropZoneSnap grid;
            if (reset)
                grid = Instantiate(gridPrefab, transform).GetComponent<DropZoneSnap>();
            else
                grid = grids[i];

            var width = widths[i];

            grid.transform.SetParent(tmp.transform);
            grid.transform.localPosition = new Vector2(x, y - height / 2);
            grid.transform.localScale = new Vector2(width, height);
            if (reset)
            {
                grid.Init(i, scale, word.Length);
                grids.Add(grid);
            }
            else
            {
                grid.UpdateWidth(scale);
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
        var hiddenCharStr = "<size=0>\u2060<size=1e>";

        var firstAndLast = currentSentence.FirstAndLast;

        // define lineheight = fontsize + height of drop zone + 10 pixels padding
        var text = $"<line-height={tmp.fontSize + 10 * height + 10}>";

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
                // extra width in pixels (ratio is x 10)
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

    public void PlacePhoneme(Phoneme e, int i, int j){
        var d = ShapeManager.Instance.CreatePhoneme(e, grids[i].transform.position).GetComponent<Draggable>();
        d.transform.localScale = new Vector2(grids[i].scale, grids[i].scale);
        d.transform.SetParent(grids[i].transform);
        d.transform.localPosition = grids[i].centers[j];
        grids[i].draggables[j] = d;
        d.enabled = false;
        if (!Config.testMode)
        {
            d.enabled = true;
        }
    }
}
