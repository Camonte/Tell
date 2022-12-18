using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class representing a grid. The grid is located in the center pannel and is composed of the input (text or shapes) as well as dropzones where the user can put their answer.
/// </summary>
public class Grid : MonoBehaviour
{
    /// <summary>
    /// Padding between shapes inside the grid.
    /// </summary>
    public const float padding = 0.1f;

    protected Sentence currentSentence;
    protected int currentScrollWord = 0;

    public virtual void Init() { }

    /// <summary>
    /// Generates the grid given a <see cref="Sentence"/>.
    /// </summary>
    public virtual void GenerateFor(Sentence s) { }

    /// <summary>
    /// Clears the current grid.
    /// </summary>
    public virtual void Clear() { }

    /// <summary>
    /// Should be called when the filter defined in <see cref="Config.Filter"/> is changed.
    /// </summary>
    public virtual void ChangeFilter(int filter) { }

    /// <summary>
    /// Clears a particular element inside a word of the current sentence. Can be removed or "ghosted".
    /// </summary>
    public virtual void Clear(int wordIndex, int elementIndex, bool ghost = false) { }

    /// <summary>
    /// Blocks/unblocks the elements inside the grid, notably the <see cref="Draggable"/>s become movable/immovable.
    /// </summary>
    public virtual void BlockGrid(bool block) { }

    /// <summary>
    /// Returns the current state of the grid.
    /// For each word of the current sentence, an array of <see cref="Draggable"/>s indicates that word's state (filled in elements). Null values indicate empty slots.
    /// </summary>
    public virtual List<Draggable[]> GetState() { return null; }

    /// <summary>
    /// Returns the index of the first word containing an empty element (null).
    /// </summary>
    public virtual int FirstNullIndex(int fromIndex = 0) { return -1; }

    /// <summary>
    /// Returns the index of the first empty element (null).
    /// </summary>
    public virtual int FirstNullElement() { return -1; }

    /// <summary>
    /// Return the index of the last word that is completely filled.
    /// </summary>
    public virtual int LastFilledIndex() { return -1; }

    /// <summary>
    /// Changes the coloring mode. Should be called when <see cref="Config.coloring"/> is changed.
    /// </summary>
    public virtual void ChangeColoringMode(bool coloring) { }

    /// <summary>
    /// Scrolls to a given word in the current sentence.
    /// </summary>
    public virtual void ScrollToWord(int wordIndex, float duration = 0.2f) { }

    /// <summary>
    /// Should be called when the scroll object in the <see cref="GridManager"/> is updated via <see cref="Lean.Touch.LeanDragTranslate"/>.
    /// </summary>
    /// <param name="delta"></param>
    public virtual void Scroll(Vector2 delta) { }

    /// <summary>
    /// How to behave when the font size changes in <see cref="ScaleManager"/>.
    /// </summary>
    public virtual void OnUpdateFontSize(int fontSize) { }

    /// <summary>
    /// How to behave when the layout changes in <see cref="ScaleManager"/>.
    /// </summary>
    public virtual void OnUpdateLayout() { }

    /// <summary>
    /// How to behave when the scale changes in <see cref="ScaleManager"/>.
    /// </summary>
    /// <param name="scale"></param>
    public virtual void OnUpdateScale(float scale) { }

    /// <summary>
    /// Runs a scrolling animation that scrolls a given object to a given y position (world coordinates).
    /// </summary>
    protected IEnumerator ScrollToYCoroutine(GameObject g, float y, float duration)
    {
        var endPos = new Vector2(0, y);
        var startPos = g.transform.position;
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            g.transform.position = Vector2.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        g.transform.position = endPos;
    }

    public virtual void Hint(int wordIndex, Word answer) { }

    /// <summary>
    /// UNUSED
    /// </summary>
    public virtual bool UndoLastMove() { return false; }

    private void Awake()
    {
        Init();
    }
}
