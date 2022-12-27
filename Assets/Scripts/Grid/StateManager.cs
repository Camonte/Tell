using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the state/progression of the game (current sentence, current word).
/// Also handles correction (rejects/removes wrong answers, plays animation when user is correct)
/// </summary>
public class StateManager : Singleton<StateManager>
{
    [SerializeField] Button checkButton;
    [SerializeField] Image wellDoneImage;
    [SerializeField] Image wrongImage;
    [SerializeField] ToggleSpriteSwap correctionToggle;
    [SerializeField] Button loadSentenceButton; // used for DEBUG

    [SerializeField] Button previousWordButton;
    [SerializeField] Button nextWordButton;

    //[SerializeField] Button hintButton;

    List<Sentence> sentences = new List<Sentence>();
    public Sentence currentSentence;
    int currentWordIndex;

    public override void Awake()
    {
        if(!Config.freeWriting)
        {
            checkButton.onClick.AddListener(Check);
            checkButton.gameObject.SetActive(false);
        }

        wellDoneImage.gameObject.SetActive(false);

        if (Config.testMode)
        {
            correctionToggle.onValueChanged.AddListener(SetCorrectionMode);
            correctionToggle.Initialize(Config.progressiveCorrection);
        }

        previousWordButton.gameObject.SetActive(false);
        previousWordButton.onClick.AddListener(PreviousWord);
        nextWordButton.gameObject.SetActive(false);
        nextWordButton.onClick.AddListener(NextWord);

        //hintButton.onClick.AddListener(GiveHint);
        //hintButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Set the input type as defined in <see cref="Config.InputType"/>.
    /// Called from javascript.
    /// </summary>
    public void SetInputType(string type)
    {
        if (type == "WordList") Config.inputType = Config.InputType.WordList;
        else if (type == "Sentence") Config.inputType = Config.InputType.Sentence;
        else Debug.Log("Unknown correction type : " + type);
    }

    private void NextWord()
    {
        int i = sentences.IndexOf(currentSentence);
        i++;
        previousWordButton.gameObject.SetActive(true);
        nextWordButton.GetComponent<ShiftXAnimation>().active = i < sentences.Count - 1;

        currentSentence = sentences[i];

        GridManager.Instance.GenerateFor(currentSentence);
        currentWordIndex = 0;
    }

    private void PreviousWord()
    {
        int i = sentences.IndexOf(currentSentence);
        i--;
        previousWordButton.GetComponent<ShiftXAnimation>().active = i > 0;
        nextWordButton.gameObject.SetActive(true);

        currentSentence = sentences[i];
        
        GridManager.Instance.GenerateFor(currentSentence);
        currentWordIndex = 0;
    }

    private void SetCorrectionMode(bool progressive)
    {
        // Dynamic
        if (progressive)
        {
            checkButton.gameObject.SetActive(false);
        }
        // Manual
        else
        {
            GridManager.Instance.BlockGrid(false);
        }
        Config.progressiveCorrection = progressive;
        ReloadSentence();
    }

    public void SetCharacterCounts(bool show)
    {
        Config.characterCounts = show;
        ReloadSentence();
    }

    public void SetColoring(bool coloring)
    {
        Config.coloring = coloring;
        GridManager.Instance.ChangeColoringMode(coloring);
        ReloadSentence();
    }

    // public void GiveHint()
    // {
    //     if (currentSentence == null) return;
    //     int index = GridManager.Instance.FirstNullIndex();
    //     if (index == -1 || index >= currentSentence.Length) return;
    //     Word answer = currentSentence.words[index];
    //     GridManager.Instance.Hint(index, answer);
    // }

    public void Reload()
    {
        ReloadSentence();
    }

    /// <summary>
    /// Sets the next input (text). Depending on the <see cref="Config.InputType"/> the sentence will be separated into multiple <see cref="Sentence"/> objects.
    /// Called from javascript.
    /// </summary>
    public void NextSentence(string sentence)
    {
        sentences.Clear();

        switch (Config.inputType)
        {
            case Config.InputType.WordList:
                sentences = Sentence.ParseWordList(sentence);
                currentSentence = sentences[0];
                break;

            case Config.InputType.Sentence:
            case Config.InputType.AnnotatedSentence:
                currentSentence = Sentence.Parse(sentence);
                break;
        }

        previousWordButton.gameObject.SetActive(false);
        nextWordButton.gameObject.SetActive(sentences.Count > 1);

        if (currentSentence != null)
        {
            Debug.Log(currentSentence);

            loadSentenceButton.gameObject.SetActive(false);

            ShapeManager.Instance.Clear();
            GridManager.Instance.GenerateFor(currentSentence);
            currentWordIndex = 0;
        }
        //hintButton.gameObject.SetActive(currentSentence != null);
    }

    public void ReloadSentence()
    {
        ShapeManager.Instance.Clear();

        if (currentSentence != null)
        {
            GridManager.Instance.GenerateFor(currentSentence);
            currentWordIndex = 0;
            //hintButton.gameObject.SetActive(true);
        }
    }

    public void OnGridChange(List<Draggable[]> draggables, int wordChanged, bool scroll)
    {
        bool full = draggables.Count > 0 && draggables.All(w => w.All(d => d != null));

        if(Config.testMode)
        {
            if(Config.progressiveCorrection)
            {
                if (full)
                    OnAllCorrect();
                else
                {
                    currentWordIndex = GridManager.Instance.FirstNullIndex(wordChanged);
                    if (scroll) GridManager.Instance.ScrollToWord(currentWordIndex);
                }   
            }
            else
            {
                currentWordIndex = GridManager.Instance.FirstNullIndex(wordChanged);
                if (scroll) GridManager.Instance.ScrollToWord(currentWordIndex);
                checkButton.gameObject.SetActive(full);
            }
        }
        else if (Config.progressiveCorrection) 
        {
            if (full)
                OnAllCorrect();
            else
            {
                currentWordIndex = GridManager.Instance.FirstNullIndex(wordChanged);
                if (scroll) GridManager.Instance.ScrollToWord(currentWordIndex);
            }
        }
        else
        {
            if (scroll) GridManager.Instance.ScrollToWord(GridManager.Instance.FirstNullIndex(wordChanged));
            checkButton.gameObject.SetActive(full);
        }

        //hintButton.gameObject.SetActive(!full);
    }

    public List<Element[]> Solution()
    {
        var solution = new List<Element[]>();
        for (int i = 0; i < currentSentence.Length; i++)
            solution.Add(Solution(i));
        return solution;
    }

    public Element[] Solution(int wordIndex)
    {
        return Config.gameType switch
        {
            Config.GameType.WordToShape => currentSentence.words[wordIndex].phonemes,
            Config.GameType.ShapeToWord => currentSentence.words[wordIndex].graphemes,
            _ => null,
        };
    }

    public bool DropOk(Element e, int wordIndex, int elementIndex)
    {
        if (Config.progressiveCorrection)
        {
            return currentWordIndex == wordIndex && Solution(wordIndex)[elementIndex] == e;
        }
        return true;
    }

    public bool TapOk(Element e)
    {
        return Solution(currentWordIndex)[GridManager.Instance.FirstNullElement()] == e;
    }

    private void OnAllCorrect()
    {
        SoundManager.Instance.Success();
        wellDoneImage.GetComponent<WellDoneAnimation>().Animate();
    }

    public void OnWrong()
    {
       SoundManager.Instance.Failure();
       wrongImage.GetComponent<WellDoneAnimation>().Animate();
    }

    public void Check()
    {
        // get grid state from grid manager
        List<Draggable[]> fullState = GridManager.Instance.GetState();

        var allCorrect = true;

        for(int i = 0; i < fullState.Count; i++)
        {
            var state = fullState[i];
            var answer = state.Select(d => d.element).ToArray();

            var solution = Solution(i);

            // compare if they are correct
            for (int j = 0; j < state.Length; j++)
            {
                var correct = answer[j] == solution[j];
                allCorrect = allCorrect && correct;

                if (!correct)
                {
                    GridManager.Instance.Ghostify(i, j);
                }
            }
        }

        // update UI (buttons)
        OnGridChange(fullState, GridManager.Instance.FirstNullIndex(), true);

        // confetti if all correct
        if (allCorrect)
            OnAllCorrect();
    }

    public void DisplayAnswer(Grapheme e){
        GridManager.Instance.DisplayAnswer(currentWordIndex, e);
        OnGridChange(GridManager.Instance.GetState(), GridManager.Instance.FirstNullIndex(), true);
    }

    public void DisplayAnswer(Phoneme e){
        GridManager.Instance.DisplayAnswer(currentWordIndex, e);
        OnGridChange(GridManager.Instance.GetState(), GridManager.Instance.FirstNullIndex(), true);
    }

    /// <summary>
    /// Set the selected phonemes from the external window
    /// Called from javascript.
    /// </summary>
    public void UpdatePhonemes(string selectedPhonemes)
    {
        string[] phonemes = selectedPhonemes.Split('-');
        HashSet<string> set = new HashSet<string>(phonemes);
        if (GridManager.Instance.grid is GridNoShapes)
        {
            ((GridNoShapes)GridManager.Instance.grid).selectedPhonemes = CombinationPhonemes(set);
        }
    }

    private HashSet<string> CombinationPhonemes(HashSet<string> phonemes)
    {
        HashSet<string> ret = new HashSet<string>(phonemes);
        foreach (var item1 in phonemes) 
        {
            foreach (var item2 in phonemes) 
            {
                ret.Add(item1 + " " + item2);
            }
        }
        return ret;
    }
}
