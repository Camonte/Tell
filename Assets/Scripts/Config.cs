/// <summary>
/// Globally accessible configuration used to keep track of game type / parameters accross scenes.
/// </summary>
public class Config
{
    /// <summary>
    /// ShapeToWord: shapes are shown to the user, the user needs to drag and drop graphemes
    /// WordToShape: text is shown to the user, the user needs to drag and drop colored shapes (phonemes)
    /// </summary>
    public enum GameType { ShapeToWord, WordToShape }
    //public static GameType gameType = GameType.ShapeToWord;
    public static GameType gameType = GameType.WordToShape;

    /// <summary>
    /// WordList: each line of text is considered as a separate sentence
    /// Sentence: the whole text is considered as a block (doesn't split on new lines)
    /// AnnotatedSentence: (mostly for debug) similar to sentence, but in addition to text also includes each word on separate lines, manually annotated
    /// </summary>
    public enum InputType { WordList, Sentence, AnnotatedSentence }
    public static InputType inputType = InputType.WordList;

    /// <summary>
    /// Do we only accept correct answers ?
    /// </summary>
    public static bool progressiveCorrection = true;

    /// <summary>
    /// Are we in test mode?
    /// </summary>
    public static bool testMode = false;

    /// <summary>
    /// Are we in free writing mode?
    /// </summary>
    public static bool freeWriting = false;

    /// <summary>
    /// Difficulty of the memory game
    /// </summary>
    /// 1 = easy, 2 = medium, 3 =  difficult
    public static int memoryDifficulty = 1;

    /// <summary>
    /// Type of memory game
    /// </summary>
    /// false = images, true = phonemes
    public static bool memoryMode = false;

    /// <summary>
    /// Audio: board plays a sound and the user needs to touch the correct shape <br />
    /// Visual: board highlights a shape and the user needs to pronounce the correct sound
    /// </summary>
    public enum TutorialType { Audio, Visual }
    public static TutorialType tutorialType = TutorialType.Audio;

    /// <summary>
    /// Which type of phonemes should be filtered out ?
    /// </summary>
    public static class Filter
    {
        public const int None = 0;
        public const int Vowels = 1;
        public const int Consonants = 2;
        public const int Both = 3;
    }

    /// <summary>
    /// Currently applied <see cref="Filter"/>
    /// </summary>
    public static int filter { get { return (vowelsActive ? Filter.None : Filter.Vowels) + (consonantsActive ? Filter.None : Filter.Consonants); } }

    /// <summary>
    /// UNUSED
    /// </summary>
    public static bool isBlack = true;

    /// <summary>
    /// Are vowels currently being used (not filtered out) ?
    /// </summary>
    public static bool vowelsActive = true;

    /// <summary>
    /// Are consonants currently being used (not filtered out) ?
    /// </summary>
    public static bool consonantsActive = true;

    /// <summary>
    /// Are we in coloring mode ?
    /// </summary>
    public static bool coloring = false;

    /// <summary>
    /// Do we want to display the number of characters per grapheme ?
    /// </summary>
    public static bool characterCounts = false;

    /// <summary>
    /// Default font size
    /// </summary>
    public static int defaultFontSize = 14;

    /// <summary>
    /// Default scale for the side pannels
    /// </summary>
    public static float defaultScale = 1f;

    /// <summary>
    /// Is the teacher input window open ?
    /// </summary>
    public static bool secondaryWindowOpen = false;

    /// <summary>
    /// Sets the vowels to `active`. If both vowels and consonnants are deactivated, reactivates the consonnants.
    /// </summary>
    /// <returns>If the consonnants have been reactivated</returns>
    public static bool SetVowelsActive(bool active)
    {
        vowelsActive = active;
        if (vowelsActive || consonantsActive) return false;
        consonantsActive = true;
        return true;
    }

    /// <summary>
    /// Sets the consonnants to `active`. If both vowels and consonnants are deactivated, reactivates the vowels.
    /// </summary>
    /// <returns>If the vowels have been reactivated</returns>
    public static bool SetConsonantsActive(bool active)
    {
        consonantsActive = active;
        if (vowelsActive || consonantsActive) return false;
        vowelsActive = true;
        return true;
    }
}
