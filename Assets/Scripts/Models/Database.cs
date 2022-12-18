using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

/// <summary>
/// Code for managing the database of words: mainly loading and parsing the data from a text file.
/// The parsed words are stored in a dictionnary (string -> <see cref="Word"/>).
/// </summary>
public class Database : Singleton<Database>
{

    [SerializeField] TextAsset dictPath;
    [SerializeField] bool isLoaded;

    bool isFetched;

    Dictionary<string, Word> dict = new Dictionary<string, Word>();

    private void Start()
    {
        // in production don't load the whole dictionnary but load words lazily when the text is changed
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            isLoaded = true;
        }
        else // in editor specify a dictionnary (csv file) with annotated words in the Database game object 
        {
            LoadWords();
        }
    }

    private void Update()
    {
        if (!isLoaded && isFetched)
        {
            LoadWords();
        }
    }

    /// <summary>
    /// Searches for a word in the database and returns the corresponding <see cref="Word"/> if it is present.
    /// If the word is not found, the word is split on the ' and - characters, and the parts are searched for separately.
    /// If all parts are found in the database, they are combined into a single <see cref="Word"/> object. <br/>
    /// Example: "rendez-vous" doesn't exist, but "rendez" and "vous" both do. <br/>
    /// If a part is not found, `null` is returned.
    /// </summary>
    public Word GetWord(string word)
    {
        if (!isLoaded)
        {
            LoadWords();
            Debug.Log("Can't get word, dictionnary not loaded yet.");
        }

        if (word.All(c => char.IsPunctuation(c))) return null;

        // clean up the word when querying: lower case, remove punctuation
        var cleaned = Regex.Replace(word.ToLower(), @"[.,?!:""\n\r]", "");

        // try to get full word
        if (dict.TryGetValue(cleaned, out Word fullWord))
        {
            var newWord = fullWord.withText(word);
            return newWord;
        }

        Debug.Log($"Word {word} not in db, trying to split");

        // check for words with the '
        var apostrophe = cleaned.IndexOf('\'');

        // otherwise, try to split on ' or - and combine the different parts together
        var wordParts = cleaned.Split('\'', '-', '–', '’');
        List<Word> partialWords = new List<Word>();
        foreach (var part in wordParts)
        {
            Word partialWord;
            if (part.Length == 0) continue;
            if (apostrophe > -1)
            {
                if(dict.TryGetValue(part + "\'", out partialWord))
                {
                    partialWords.Add(partialWord);
                    apostrophe = -1;
                    continue;
                }
            }

            if (!dict.TryGetValue(part, out partialWord))
            {
                Debug.Log($"Word {part} is not in database.");
                return null;
            }
            
            partialWords.Add(partialWord);
        }

        return Word.Combine(word, partialWords.ToArray());
    }

    public void UpdateDict(string words)
    {
        var n = 0;
        string[] lines = words.Split('\n');
        foreach (string line in lines)
        {
            Word w = ParseLine(line);
            if (w != null)
            {
                dict[w.word] = w;
                n++;
            }
        }
        Debug.Log($"Updated dict with {n} words.");
    }

    private void LoadWords()
    {
        if (isLoaded)
        {
            Debug.Log("Already loaded");
            return;
        }
        if (dictPath == null)
        {
            Debug.Log("No dictionary specified.");
            return;
        }

        Debug.Log("Loading words from dictionary...");

        string text = dictPath.text;

        string[] lines = text.Split('\n');
        foreach(string line in lines)
        {
            Word w = ParseLine(line);
            if (w != null)
            {
                dict[w.word] = w;
            }
        }
        isLoaded = true;

        Debug.Log($"Done loading {dict.Count} words.");
    }

    /// <summary>
    /// Unused: fetch a file (dictionnary) from an url
    /// </summary>
    private IEnumerator GetFileAsync(string url)
    {
        using UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string fileContents = request.downloadHandler.text;
            dictPath = new TextAsset(fileContents);
            Debug.Log($"Dict path fetched from {url}");

            isFetched = true; // LoadWords();
        }
        else
        {
            Debug.Log($"Error loading file with path {url}");
        }
    }

    /// <summary>
    /// Parses a line of text and converts it to a <see cref="Word"/>.
    /// The expected format is: word,phonemes,graphemes where the phonemes and graphemes are separated by | <br/>
    /// Example: pomme,p|O|m,p|o|mme <br/>
    /// The number of graphemes must match the number of phonemes, and all phonemes must exist otherwise `null` is returned.
    /// </summary>
    public Word ParseLine(string line)
    {
        string[] parts = line.Split(',');
        if (parts.Length < 3) return null;
        if (parts.Length == 4) { parts[0] += ","; parts[1] = parts[2]; parts[2] = parts[3]; }

        string word = parts[0].Trim();
        string[] phonemesStr = parts[1].Trim().Split('|');
        string[] graphemesStr = parts[2].Trim().Split('|');
        if (phonemesStr.Length != graphemesStr.Length)
        {
            Debug.Log($"Word incorrectly annotated: {line}");
            return null;
        }
        Phoneme[] phonemes = Array.ConvertAll(phonemesStr, p => Phoneme.For(p));
        if (Array.IndexOf(phonemes, null) != -1)
        {
            Debug.Log($"Word incorrectly annotated: {line}");
            return null;
        }
        Grapheme[] graphemes = Enumerable.Range(0, phonemes.Length).Select(i => new Grapheme(graphemesStr[i], Phoneme.Normalize(phonemes[i].id))).ToArray();
        return new Word(word, phonemes, graphemes);
    }
}
