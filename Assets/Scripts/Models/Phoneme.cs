using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a phoneme.
/// Keeps track of it's colors, shapes, type (vowel/consonnant).
/// </summary>
public class Phoneme : Element
{

    private static Dictionary<string, Phoneme> allPhonemes = new Dictionary<string, Phoneme>()
    {
        {"2",new Phoneme("2","#00B760","v")},
        {"2 j",new Phoneme("2 j","#00B760|#E8112D","v")},

        {"9",new Phoneme("9","#CEEA82","v")},
        {"9 j",new Phoneme("9 j","#CEEA82|#E8112D","v")},

        {"a",new Phoneme("a","#930FA5","v")},
        {"a j",new Phoneme("a j","#930FA5|#E8112D","v")},
        {"a S",new Phoneme("a S","#930FA5|#CE898C","v")},
        {"a u",new Phoneme("a u","#930FA5|#4CCED1","v")},

        {"a~",new Phoneme("a~","#EDC4DD","v")},
        {"a~ n",new Phoneme("a~ n","#EDC4DD|#2B4C3F","v")},

        {"b",new Phoneme("b","#84003C","c")},

        {"d",new Phoneme("d","#B06F00","c")},
        {"d v",new Phoneme("d v","#B06F00|#AE9A00","v")},
        {"d Z",new Phoneme("d Z","#E29100|#894FBF","c")},

        {"e",new Phoneme("e","#ED6E00","v")},

        {"E",new Phoneme("E","#F43FA5","v")},
        {"E f",new Phoneme("E f","#F43FA5|#787A7B","v")},
        {"E j",new Phoneme("E j","#F43FA5|#E8112D","v")},

        {"e j",new Phoneme("e j","#ED6E00|#E8112D","v")},

        {"E l",new Phoneme("E l","#F43FA5|#A3C1AD","v")},
        {"E n",new Phoneme("E n","#F43FA5|#2B4C3F","v")},
        {"E R",new Phoneme("E R","#930FA5|#D3BFB7","v")},
        {"E s",new Phoneme("E s","#F43FA5|#FCBB6F","v")},

        {"e~",new Phoneme("e~","#F9BF9E","v")},
        {"e~ n",new Phoneme("e~ n","#F9BF9E|#2B4C3F","c")},

        {"f",new Phoneme("f","#96938E","c")},

        {"g",new Phoneme("g","#9B0070","c")},
        {"g z",new Phoneme("g z","#9B0070|#FC9BB2","c")},

        {"y 2",new Phoneme("y 2","#F7D917|#00B760","v")},
        {"y 9",new Phoneme("y 9","#F7D917|#CEEA82","v")},
        {"y a",new Phoneme("y a","#F7D917|#930FA5","v")},
        {"y a~",new Phoneme("y a~","#F7D917|#E7C1E3","v")},
        {"y e",new Phoneme("y e","#F7D917|#ED6E00","v")},
        {"y E",new Phoneme("y E","#F7D917|#F43FA5","v")},
        {"y e~",new Phoneme("y e~","#F7D917|#F9BF9E","v")},
        {"y i",new Phoneme("y i","#F7D917|#E8112D","v")},
        {"y j",new Phoneme("y j","#F7D917|#E8112D","v")},
        {"y O",new Phoneme("y O","#F7D917|#5B77CC","v")},
        {"y R",new Phoneme("y R","#F7D917|#D3BFB7","c")},

        {"i",new Phoneme("i","#E8112D","v")},
        {"i j",new Phoneme("i j","#E8112D|#E7102C","v")},

        {"j",new Phoneme("j","#E8112D","v")},

        {"n j",new Phoneme("n j","#2B4C3F|#E8112D","c")},

        {"j 2",new Phoneme("j 2","#E8112D|#00B760","v")},
        {"j 9",new Phoneme("j 9","#E8112D|#CEEA82","v")},
        {"j a",new Phoneme("j a","#E8112D|#930FA5","v")},
        {"j a~",new Phoneme("j a~","#E8112D|#E7C1E3","v")},
        {"j E",new Phoneme("j E","#E8112D|#F43FA5","v")},
        {"j e",new Phoneme("j e","#E8112D|#ED6E00","v")},
        {"j e~",new Phoneme("j e~","#E8112D|#F9BF9E","v")},
        {"j i",new Phoneme("j i","#E7102C|#E8112D","v")},
        {"j O",new Phoneme("j O","#E8112D|#5B77CC","v")},
        {"j o",new Phoneme("j o","#E8112D|#0051BA","v")},
        {"j o~",new Phoneme("j o~","#E8112D|#C4D8E2","v")},
        {"j u",new Phoneme("j u","#E8112D|#4CCED1","v")},
        {"j y",new Phoneme("j y","#E8112D|#F7D917","v")},

        {"k",new Phoneme("k","#3A4972","c")},
        {"k s",new Phoneme("k s","#3A4972|#FCBB6F","c")},
        {"k u",new Phoneme("k u","#3A4972|#4CCED1","c")},

        {"l",new Phoneme("l","#A3C1AD","c")},

        {"m",new Phoneme("m","#A53F0F","c")},

        {"n",new Phoneme("n","#2B4C3F","c")},
        {"n g",new Phoneme("n g","#2B4C3F|#602144","c")},

        {"O",new Phoneme("O","#5B77CC","v")},

        {"o",new Phoneme("o","#0051BA","v")},

        {"O j",new Phoneme("O j","#5B77CC|#E8112D","v")},
        {"O u",new Phoneme("O u","#5B77CC|#4CCED1","v")},

        {"o~",new Phoneme("o~","#C4D8E2","v")},

        {"p",new Phoneme("p","#3A7728","c")},

        {"R",new Phoneme("R","#D3BFB7","c")},

        {"s",new Phoneme("s","#FCBB6F","c")},

        {"S",new Phoneme("S","#CE898C","c")},

        {"t",new Phoneme("t","#007770","c")},
        {"t S",new Phoneme("t S","#007770|#CE898C","c")},

        {"u",new Phoneme("u","#4CCED1","v")},
        {"u j",new Phoneme("u j","#4CCED1|#E8112D","v")},

        {"v",new Phoneme("v","#AA930A","c")},

        {"u 9",new Phoneme("u 9","#4CCED1|#CEEA82","v")},
        {"u a",new Phoneme("u a","#4CCED1|#930FA5","v")},
        {"u a~",new Phoneme("u a~","#4CCED1|#E7C1E3","v")},
        {"u E",new Phoneme("u E","#4CCED1|#F43FA5","v")},
        {"u e",new Phoneme("u e","#4CCED1|#ED6E00","v")},
        {"u e~",new Phoneme("u e~","#4CCED1|#F9BF9E","v")},
        {"u i",new Phoneme("u i","#4CCED1|#E8112D","v")},
        {"u O",new Phoneme("u O","#4CCED1|#5B77CC","v")},
        {"u o",new Phoneme("u o","#4CCED1|#0051BA","v")},
        {"u o~",new Phoneme("u o~","#4CCED1|#C4D8E2","v")},

        //{"u u",new Phoneme("u u","#4CCED1|#4CCED1","v")},
        {"y",new Phoneme("y","#F7D917","v")},

        {"z",new Phoneme("z","#FC9BB2","c")},

        {"Z",new Phoneme("Z","#894FBF","c")},

        {"z d",new Phoneme("z d","#FC9BB2|#B06F00","c")},

        // j == i
        {"2 i",new Phoneme("2 i","#00B760|#E8112D","v")},

        {"9 i",new Phoneme("9 i","#CEEA82|#E8112D","v")},

        {"a i",new Phoneme("a i","#930FA5|#E8112D","v")},

        {"E i",new Phoneme("E i","#F43FA5|#E8112D","v")},

        {"e i",new Phoneme("e i","#ED6E00|#E8112D","v")},
        //{"i i",new Phoneme("i i","#E8112D|#E7102C","v")},

        {"n i",new Phoneme("n i","#2B4C3F|#E8112D","c")},

        {"i 2",new Phoneme("i 2","#E8112D|#00B760","v")},
        {"i 9",new Phoneme("i 9","#E8112D|#CEEA82","v")},
        {"i a",new Phoneme("i a","#E8112D|#930FA5","v")},
        {"i a~",new Phoneme("i a~","#E8112D|#E7C1E3","v")},
        {"i E",new Phoneme("i E","#E8112D|#F43FA5","v")},
        {"i e",new Phoneme("i e","#E8112D|#ED6E00","v")},
        {"i e~",new Phoneme("i e~","#E8112D|#F9BF9E","v")},
        {"i O",new Phoneme("i O","#E8112D|#5B77CC","v")},
        {"i o",new Phoneme("i o","#E8112D|#0051BA","v")},
        {"i o~",new Phoneme("i o~","#E8112D|#C4D8E2","v")},
        {"i u",new Phoneme("i u","#E8112D|#4CCED1","v")},
        {"i y",new Phoneme("i y","#E8112D|#F7D917","v")},

        {"O i",new Phoneme("O i","#5B77CC|#E8112D","v")},

        {"",new Phoneme("","#FFFFFF","undef")},
        //{"-",new Phoneme("-","#000000","undef")},
        
    };

    public static Phoneme empty = new Phoneme("empty", "#FFFFFF", "undef");

    public enum Type { Undef = 0, Vowel = 1, Consonant = 2};

    public Color[] colors { get; private set; }
    public string[] shapes { get; private set; }
    public Type voco { get; private set; }

    public bool isDouble { get; private set; }

    public int Length { get; private set; }

    private Phoneme(string id, string colors, string voco)
    {
        this.id = id;
        this.colors = Array.ConvertAll(colors.Split('|'), c => HexToRGB(c));
        this.shapes = ShapesForPhoneme(id);
        this.voco = voco == "v" ? Type.Vowel : voco == "c" ? Type.Consonant : Type.Undef;
        this.isDouble = this.colors.Length == 2;
        this.Length = this.colors.Length;
    }

    /// <summary>
    /// All phonemes which aren't double.
    /// </summary>
    public static List<Phoneme> All()
    {
        return allPhonemes.Values.Where(p => p.voco != Type.Undef && !p.isDouble).ToList();
    }

    /// <summary>
    /// All phonemes of a certain type (doesn't include double phonemes).
    /// </summary>
    public static List<Phoneme> All(Type type)
    {
        return allPhonemes.Values.Where(p => p.voco == type && !p.isDouble).ToList();
    }

    /// <summary>
    /// All phonemes, filtered by a filter as defined in <see cref="Config.Filter"/>
    /// </summary>
    public static List<Phoneme> All(int filter)
    {
        return All().Where(p => ((int)p.voco & filter) == 0).ToList();
    }

    /// <summary>
    /// Normalizes the phoneme string id to be in the format used in this project.
    /// </summary>
    public static string Normalize(string phoneme)
    {
        return phoneme.Replace('H', 'y').Replace('w', 'u').Replace('@', '9').Replace("9~", "e~").Replace("J", "n j").Replace("N", "n g").Trim();
    }

    public override string ToString()
    {
        return $"Phoneme (id={id}, type={voco}, double={isDouble})";
    }

    public override bool Equals(object obj)
    {
        return obj is Phoneme && (Phoneme)obj == this;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public static bool operator ==(Phoneme p1, Phoneme p2)
    {
        // null check
        if (p2 is null)
            return p1 is null;
        else if (p1 is null)
            return p2 is null;

        if (p1.id == "empty" || p2.id == "empty") return true;

        // consider j and i as identical
        return p1.id.Replace('j', 'i') == p2.id.Replace('j', 'i');
    }

    public static bool operator !=(Phoneme p1, Phoneme p2)
    {
        return !(p1 == p2);
    }

    /// <summary>
    /// Checks if the two elements are phonemes and if they can be merged.
    /// </summary>
    public static bool CanMerge(Element e1, Element e2)
    {
        if (!(e1 is Phoneme && e2 is Phoneme)) return false;
        var p1 = e1 as Phoneme;
        var p2 = e2 as Phoneme;
        if (p1.isDouble || p2.isDouble) return false;

        return Phoneme.For(p1.id, p2.id) != null;
    }

    /// <summary>
    /// Gets phoneme for the given string(s), returns null if doesn't exist.
    /// </summary>
    public static Phoneme For(params string[] phonemes)
    {
        string phoneme = string.Join(" ", phonemes);
        if (!allPhonemes.TryGetValue(Normalize(phoneme), out Phoneme p))
        {
            //Debug.Log($"Unknown phoneme ( {phoneme} )");
        }
        return p;
    }

    /// <summary>
    /// Returns phoneme resulting of merge of the two elements. If phoneme doesn't exist, return null. 
    /// </summary>
    public static Phoneme Merge(Element e1, Element e2)
    {
        var p1 = e1 as Phoneme;
        var p2 = e2 as Phoneme;
        return Phoneme.For(p1.id, p2.id);
    }

    private static string[] ShapesForPhoneme(string ph)
    {
        string oralVowels = "yeiE29aOou";
        string nasalVowels = "e~o~a~";
        string sonoreConsonants = "vbzdZg";
        string sourdeConsonants = "fpstSk";
        string sourdeSonoreConsonants = "Rl";
        string nasalConsonants = "mn";
        string semiConsonants = "j";

        string[] parts = ph.Split(' ');
        string[] shapes = new string[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            string p = parts[i];
            if (oralVowels.Contains(p)) shapes[i] = "v_orale";
            else if (nasalVowels.Contains(p)) shapes[i] = "v_nasale";
            else if (sonoreConsonants.Contains(p)) shapes[i] = "c_sonore";
            else if (sourdeConsonants.Contains(p)) shapes[i] = "c_sourde";
            else if (nasalConsonants.Contains(p)) shapes[i] = "c_nasale";
            else if (sourdeSonoreConsonants.Contains(p)) shapes[i] = "c_sourdesonore";
            else if (semiConsonants.Contains(p)) shapes[i] = "c_semi";
            else if (p == "empty") shapes[i] = "c_sourde";
            else if (p == "") shapes[i] = "v_orale";
            else Debug.Log($"Unknown shape for phoneme: {p}");
        }

        if (parts.Length == 2)
        {
            shapes[0] += "_top";
            shapes[1] += "_bottom";
        }

        return shapes;
    }

    static private Color HexToRGB(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color rgb))
            return rgb;
        else
            return Color.black;
    }
}
