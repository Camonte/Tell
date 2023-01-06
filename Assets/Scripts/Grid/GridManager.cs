using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the <see cref="Grid"/>
/// Creates the appropriate <see cref="Grid"/> type depending on the type of game, and serves as an interface between it and the <see cref="StateManager"/>.
/// Also handles the vertical position of the grid (scrolling).
/// </summary>
public class GridManager : Singleton<GridManager>
{
    [Header("Grids")]
    [SerializeField] Grid gridWithShapes;
    [SerializeField] Grid gridNoShapes;
    [SerializeField] Grid gridWithShapesGrapheme;
    [Space(5)]
    public Grid grid;

    [Space(10)]
    [Header("Scrolling")]
    [SerializeField] GameObject scrollObject;
    [SerializeField] HeightIndicators heightIndicators;

    [Space(10)]
    [Header("Free writing mode")]
    /// <summary>
    /// Used in the free writing mode to switch between then phonemes and graphemes panels
    /// </summary>
    [SerializeField] Button switchGridsButton;
    /// <summary>
    /// Used in the free writing mode to clear all the phonemes and graphemes currently on the grid
    /// </summary>
    [SerializeField] Button clearGridButton;

    [Space(10)]
    [Header("Panels")]
    [SerializeField] SidePanel consonnantsPanel;
    [SerializeField] SidePanel vowelsPanel;
    [SerializeField] Slider scaleSlider;

    private Sentence currentSentence;

    internal float scrollOffset;
    private float screenHeight;

    public override void Awake()
    {
        // If we are in the free writing mode, we configure the buttons to switch the panels and to clear the grid
        if (Config.freeWriting)
        {
            switchGridsButton.onClick.AddListener(changePanels);
            switchGridsButton.gameObject.SetActive(true);
            
            // To clear the grid, we use the ChangeColoringMode methods which instantiates a new grid
            clearGridButton.onClick.AddListener(ChangeColoringMode);
            clearGridButton.gameObject.SetActive(true);
        }
        screenHeight = Camera.main.orthographicSize * 2;
    }

    private void Start()
    {
        if (FindObjectOfType<Tutorial>() != null) return;

        // If we are in free writing mode, we are in a WordToShape situation, so we instantiate a GridWithShapes grid
        if(Config.freeWriting){
            Config.gameType = Config.GameType.WordToShape;
            grid = Instantiate(gridWithShapes.gameObject).GetComponent<GridWithShapes>();
        }

        if (Config.gameType == Config.GameType.WordToShape)
        {
            grid = Config.coloring switch
            {
                false => Instantiate(gridWithShapes.gameObject).GetComponent<GridWithShapes>(),
                true => Instantiate(gridNoShapes.gameObject).GetComponent<GridNoShapes>(),
            };
        }
        else if (Config.gameType == Config.GameType.ShapeToWord)
        {
            grid = Instantiate(gridWithShapesGrapheme.gameObject).GetComponent<GridWithShapesGrapheme>();
        }
    }

    public void GenerateFor(Sentence s)
    {
        Clear();

        currentSentence = s;
        grid?.GenerateFor(s);
        grid?.ChangeFilter(Config.filter);
    }

    public void OnGridChange(int id, bool scroll)
    {
        StateManager.Instance.OnGridChange(GetState(), id, scroll);
    }

    internal void Clear()
    {
        grid?.Clear();
    }

    internal void ChangeFilter(int filter)
    {
        grid?.ChangeFilter(filter);
    }

    internal void ChangeColoringMode(bool coloring)
    {
        Clear();

        Destroy(grid.gameObject);

        grid = coloring switch
        {
            false => Instantiate(gridWithShapes.gameObject).GetComponent<GridWithShapes>(),
            true => Instantiate(gridNoShapes.gameObject).GetComponent<GridNoShapes>(),
        };
    }

    internal void ChangeColoringMode()
    {
        ChangeColoringMode(true);
    }

    public void BlockGrid(bool block)
    {
        grid?.BlockGrid(block);
    }

    public List<Draggable[]> GetState()
    {
        return grid.GetState();
    }

    public int FirstNullIndex(int fromIndex = 0)
    {
        return grid.FirstNullIndex(fromIndex);
    }

    public int FirstNullElement()
    {
        return grid.FirstNullElement();
    }

    public int LastFilledIndex()
    {
        return grid.LastFilledIndex();
    }

    public void OnUpdateScale(float scale)
    {
        UpdateScrollObject();
        grid?.OnUpdateScale(scale);
    }

    internal bool UndoLastMove()
    {
        if (currentSentence == null) return false;

        return grid != null && grid.UndoLastMove();
    }

    public void OnUpdateLayout()
    {
        UpdateScrollObject();
        grid?.OnUpdateLayout();
    }

    public void OnUpdateFontSize(int fontSize)
    {
        grid?.OnUpdateFontSize(fontSize);
    }

    public void Scroll(Vector2 delta)
    {
        var old = scrollOffset;
        var max = screenHeight / 2 - 2f;
        scrollOffset = Mathf.Clamp(scrollOffset + delta.y, -max, max);

        var change = new Vector2(0, scrollOffset - old); // force x to zero
        heightIndicators.OnScroll(change);

        if (currentSentence == null) return;
        grid?.Scroll(change);
    }

    internal void Hint(int wordIndex, Word answer)
    {
        grid?.Hint(wordIndex, answer);
    }

    public void ScrollToWord(int wordIndex, float duration = 0.2f)
    {
        if (currentSentence == null) return;

        if (wordIndex < 0 || wordIndex > currentSentence.Length - 1)
            return;

        grid?.ScrollToWord(wordIndex, duration);
    }

    private void UpdateScrollObject()
    {
        var pos = scrollObject.transform.position;
        pos.x = (SidePanel.rightPanelX + SidePanel.leftPanelX) / 2;
        var w = Mathf.Abs(SidePanel.rightPanelX) + Mathf.Abs(SidePanel.leftPanelX); // maybe add a tiny padding ?
        scrollObject.transform.localScale = new Vector2(w, screenHeight);
        scrollObject.transform.position = pos;
    }

    internal void Ghostify(int wordIndex, int elementIndex)
    {
        grid?.Clear(wordIndex, elementIndex, true);
    }

    internal void AttachToGrid(Transform transform)
    {
        if (grid != null) transform.SetParent(grid.transform);
    }

    public void DisplayAnswer(int wordIndex, Phoneme e){
        if(Config.coloring){
            ((GridNoShapes)grid).ColorGrapheme(wordIndex, FirstNullElement(), e.colors);
            ((GridNoShapes)grid).Splash(((Phoneme)e).colors, ((GridNoShapes)grid).grids[wordIndex].transform.TransformPoint(((GridNoShapes)grid).grids[wordIndex].centers[FirstNullElement()]));
            ((GridNoShapes)grid).UpdateGrids(wordIndex, FirstNullElement(), e);
        } else {
            ((GridWithShapes)grid).PlacePhoneme(e, wordIndex, FirstNullElement());
        }
    }

    public void DisplayAnswer(int wordIndex, Grapheme e){
        ((GridWithShapesGrapheme)grid).PlaceGrapheme(e, wordIndex, FirstNullElement());
    }

    /// <summary>
    /// Used to switch between the graphèmes and phonèmes panels in the free writing mode. The panels are cleared and then reloaded
    /// </summary>
    public void changePanels()
    {
        consonnantsPanel.ClearDict();
        vowelsPanel.ClearDict();
        Config.gameType = Config.gameType == Config.GameType.WordToShape ? Config.GameType.ShapeToWord : Config.GameType.WordToShape;
        consonnantsPanel.ReloadPanel();
        vowelsPanel.ReloadPanel();
    }
}