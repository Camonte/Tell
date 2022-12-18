using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Represents a word, composed of <see cref="Phoneme"/>s and <see cref="Grapheme"/>s.
/// </summary>
public class Word
{
    public string word { get; set; }
    public Phoneme[] phonemes { get; private set; }
    public Grapheme[] graphemes { get; private set; }

    public int Length { get; private set; }

    public Word(string word, Phoneme[] phonemes, Grapheme[] graphemes)
    {
        this.word = word;
        this.phonemes = phonemes;
        this.graphemes = graphemes;
        this.Length = phonemes == null ? 0 : phonemes.Length;
    }

    /// <summary>
    /// Changes the text content of a word, but keeps it's <see cref="Phoneme"/>s and <see cref="Grapheme"/>s the same.
    /// </summary>
    public Word withText(string text)
    {
        return new Word(text, this.phonemes.ToArray(), this.graphemes.ToArray()); // create a copy !
    }

    /// <summary>
    /// Combines a list of words into a single one, with the given `word` as it's text content.
    /// </summary>
    public static Word Combine(string word, params Word[] partialWords)
    {
        IEnumerable<Phoneme> phonemes = Enumerable.Empty<Phoneme>();
        IEnumerable<Grapheme> graphemes = Enumerable.Empty<Grapheme>();

        foreach (var w in partialWords)
        {
            phonemes = phonemes.Concat(w.phonemes);
            graphemes = graphemes.Concat(w.graphemes);
        }

        return new Word(word, phonemes.ToArray(), graphemes.ToArray());
    }

    public override string ToString()
    {
        var ph = string.Join("|", Array.ConvertAll(phonemes, p => p.id));
        var gr = string.Join("|", Array.ConvertAll(graphemes, g => g.id));
        return $"{word}, {ph}, {gr}";
    }
}
