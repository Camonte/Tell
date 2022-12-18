using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a sentence, composed of multiple <see cref="Word"/>s.
/// </summary>
public class Sentence
{

    public string sentence;
    public Word[] words { get; private set; }
    public List<Phoneme[]> phonemes { get; private set; }
    public List<Grapheme[]> graphemes { get; private set; }

    public int Length { get; private set; }

    /// <summary>
    /// An array of the indices of the first and last characters for each <see cref="Word"/> of the sentence.
    /// </summary>
    public (int,int)[] FirstAndLast { get; private set; }

    /// <summary>
    /// The indices of the first and last characters for the `i`-th <see cref="Word"/> of the sentence.
    /// By default, the indices returned include the extra characters added by the <see cref="Grid"/>.
    /// </summary>
    public (int,int) FirstAndLastFor(int i, bool extraChars = true)
    {
        if (!extraChars) return FirstAndLast[i];

        var skip = (2 * i + 1);
        var (f,l) = FirstAndLast[i];
        return (f + skip, l + skip);
    }

    public Sentence(string sentence, Word[] words)
    {
        // get know words
        var knownWords = words.Where(w => w != null).ToArray();

        this.words = knownWords;
        this.sentence = sentence;
        this.Length = knownWords.Length;

        // get graphemes and phonemes and indices of first and last characters of words
        (this.phonemes, this.graphemes) = PhonemesAndGraphemes(knownWords);
        this.FirstAndLast = ComputeFirstAndLast(this.sentence, words);
    }
    public Sentence(params Word[] words) : this(string.Join(" ", Array.ConvertAll(words, w => w.word)), words) { } 

    public override string ToString()
    {
        return sentence;
    }

    private (List<Phoneme[]>, List<Grapheme[]>) PhonemesAndGraphemes(Word[] words)
    {
        var phonemes = new List<Phoneme[]>();
        var graphemes = new List<Grapheme[]>();
        foreach (var word in words)
        {
            phonemes.Add(word.phonemes);
            graphemes.Add(word.graphemes);
        }
        return (phonemes, graphemes);
    }

    private (int,int)[] ComputeFirstAndLast(string sentence, Word[] words)
    {

        var firstAndLast = new List<(int, int)>();
        int i = 0;
        foreach (var word in words)
        {
            if (word == null)
            {
                i = sentence.IndexOfAny(new char[] { ' ', '\n' }, i);
                if (i == -1) break;
            }
            else
            {
                i = sentence.IndexOf(word.word, i);
                firstAndLast.Add((i, i + word.word.Trim().Length - 1));
                i += word.word.Length;
            }
            while (i < sentence.Length && Char.IsWhiteSpace(sentence[i])) i++;
        }

        return firstAndLast.ToArray();
    }

    /// <summary>
    /// DEBUG ONLY Parses an annotated file (includes the text as well as every word annotated manually).
    /// </summary>
    public static Sentence ParseAnnotatedFile(string text)
    {
        var sep = "<--->";
        var end = text.IndexOf(sep);

        // upto separator is sentence
        var sentence = Regex.Unescape(text.Substring(0, end)).Replace("\r\n", "\n").Replace("\r", "\n");

        // rest is encoded word per line
        var rest = Regex.Unescape(text.Substring(end + sep.Length)).Replace("\r\n", "\n").Replace("\r", "\n");
        var lines = rest.Split('\n');
        var words = lines.Where(l => l.Length > 0).Select(l => Database.Instance.ParseLine(l)).ToArray();

        return new Sentence(sentence, words);
    }

    /// <summary>
    /// Parses a string into a <see cref="Sentence"/>.
    /// </summary>
    public static Sentence Parse(string sentence)
    {
        //
        if (sentence.Contains("<--->")) return ParseAnnotatedFile(sentence);

        // normalize line endings
        var s = Regex.Unescape(sentence).Replace("<br>", "\n");
        s = s.Replace("\r\n", "\n").Replace("\r", "\n"); //.Replace("\n", Environment.NewLine);
        Word[] words = s.Split(' ', '\n', '-').Where(w => w.Length > 0).Select(w => Database.Instance.GetWord(w)).ToArray();
        
        return new Sentence(s, words);
    }

    /// <summary>
    /// Parses a string into a list of <see cref="Sentence"/>s. Newlines indicate where to split.
    /// </summary>
    internal static List<Sentence> ParseWordList(string text)
    {
        List<Sentence> sentences = new List<Sentence>();

        foreach(var line in Regex.Unescape(text).Split('\n'))
        {
            if (line.Length == 0) continue;
            sentences.Add(Parse(line));
        }

        return sentences;
    }
}
