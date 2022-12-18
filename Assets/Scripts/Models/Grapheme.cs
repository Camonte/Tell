using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a grapheme.
/// Also has a reference to it's corresponding <see cref="Phoneme"/>.
/// </summary>
public class Grapheme : Element
{
    /// <summary>
    /// List of existing graphemes for each phoneme, taken from the "tableau des graphèmes" 
    /// </summary>
    static Dictionary<string, List<string>> graphemesForPhoneme = new Dictionary<string, List<string>>()
    {
        // first one in list is archigraphème
        { "m", new List<string>() {"m", "ms", "me", "mes", "mme", "mmes", "m'", "mm", "ment", "mment"} },
        { "f", new List<string>() {"f", "ph", "fe", "fes", "fent", "ff", "ffe", "ffes", "ffent", "phent", "phe", "phes", "v"} },
        { "v", new List<string>() {"v", "ve", "ves", "f", "w", "vent"} },
        { "p", new List<string>() {"p", "pe", "pes", "ps", "b", "pent", "pp", "ppe", "ppes", "ppent"} },
        { "b", new List<string>() {"b", "be", "bes", "bent", "bb", "bbe"} },
        { "n", new List<string>() {"n", "n'", "ne", "nes", "nent", "nne", "nnes", "nnent", "nn", "mn", "mne", "mnes", "mnent"} },
        { "t", new List<string>() {"t", "t'", "tu", "tre", "te", "tes", "tent", "d", "de", "tte", "ttes", "ttent", "tt", "th", "the", "thes", "thent", "c", "pt", "pte", "ptes", "ptent"} },
        { "d", new List<string>() {"d", "d'", "de", "des", "dent", "dd", "dh", "g", "z"} },
        { "s", new List<string>() {"s", "s'", "se", "ses", "sent", "sce", "sces", "scent", "sc", "ss", "sse", "sses", "ssent", "t", "th", "sth", "x", "z", "ls", "ce", "ces", "cent", "c", "c'", "cc", "ç", "sç"} },
        { "z", new List<string>() {"z", "ze", "zes", "zent", "zz", "s", "se", "ses", "sent", "x"} },
        { "S", new List<string>() {"ch", "che", "ches", "chent", "s", "c", "sc", "sh", "chs", "sch", "je", "she", "shes", "shent", "sche", "sches", "schent"} },
        { "Z", new List<string>() {"j", "je", "j'", "g", "gg", "ge", "ges", "gent"} },
        { "l", new List<string>() {"l", "le", "les", "lent", "ll", "lle", "lles", "llent", "ls", "l'", "lp", "gl"} },
        { "j", new List<string>() {"ll", "lle", "lles", "llent", "ille", "illes", "illent", "ill", "lli", "il", "illi", "j", "i", "ye", "yes", "yent", "hy", "y", "ï", "ïe", "hi"} },
        { "R", new List<string>() {"r", "rs", "rt", "remt", "rds", "rd", "rr", "rre", "rres", "rrent", "rc", "re", "rf", "rg", "rps", "rh", "rrh", "rrhe"} },
        { "k", new List<string>() {"k", "x", "g", "cc", "cch", "ccu", "cqu", "cque", "c", "ch", "chs", "cques", "cquent", "ck", "cke", "ckes", "ckent", "cu", "qu", "qu'", "que", "q", "ques", "quent", "ke", "kk"} },
        { "g", new List<string>() {"g", "gu", "gue", "gues", "guent", "gh", "ghe", "gg", "c"} },
        
        { "y", new List<string>() {"u", "ul", "ud", "eû", "ux", "eût", "eu", "eus", "eut", "eue", "eues", "üe", "û", "ue", "ues", "uent", "û", "uh", "us", "ut", "ût", "hu", "hue", "hues", "huent", "uth", "hut", "üt"} },
        { "e", new List<string>() {"é", "e", "ê", "ers", "er", "et", "ez", "hè", "es", "ée", "és", "ë", "hai", "he", "ées", "éent", "oe", "ei", "ai", "aie", "eds", "ed", "ee", "a", "ae", "aî", "ay"} },
        { "i", new List<string>() {"i", "u", "ea", "its", "ïe", "e", "ee", "iz", "ix", "id", "ïs", "ït", "œ", "î", "is", "it", "ît", "il", "ï", "ie", "ies", "ient", "ils", "hi", "hie", "hî", "hît", "his", "hit", "y", "ys", "ye", "hy", "yg" } },
        { "2", new List<string>() {"eu", "œu", "œud", "ö", "eû", "œux", "eur", "eus", "eux", "œh", "eut", "eue", "heu", "euh", "œufs" } },
        { "9", new List<string>() {"e", "u", "ue", "i", "on", "o", "he", "œ", "ö", "hu", "heu", "eu", "œu", "a", "ai" } },
        { "E", new List<string>() {"è", "é", "ë", "ê", "e", "hê", "ès", "oe", "eh", "es", "est", "ef", "hè", "he", "et", "ets", "ect", "ey", "ei", "êt", "êts", "elle", "haie", "hai", "ea", "aie", "aies", "aient", "a", "aix", "ai", "ais", "ait", "aît", "ä", "aid", "hais", "hait", "ay", "aye", "aî"} },
        { "a", new List<string>() {"a", "aa", "ac", "ha", "ah", "acs", "az", "ars", "as", "at", "ap", "hâ", "ât", "act", "e", "à", "â", "ach", "ae"} },
        { "e~", new List<string>() {"in", "yn", "în", "înt", "înts", "ingt", "ing", "inq", "inct", "ins", "int", "eing", "ein", "en", "ens", "ent", "hin", "ïn", "im", "ainc", "aincs", "aing", "hen", "inc", "ain", "eins", "eint", "eints", "aim", "eun", "hein", "ains", "aints", "ym", "um", "un", "aint", "u", "hum", "unt"} },
        { "a~", new List<string>() {"an", "am", "han", "ang", "aans", "ham", "ans", "ant", "en", "em", "emp", "aon", "anc", "amp", "and", "emps", "ends", "end", "empt", "ens", "ent", "e", "eng", "hen"} },
        { "o~", new List<string>() {"on", "onc", "ond", "onds", "un", "ons", "ont", "hon", "hong", "om", "omp", "omps", "o", "ompt", "omb"} },
        { "o", new List<string>() {"au", "a", "oa", "ao", "oc", "oh", "ho", "hô", "hot", "aw", "ow", "ô", "o", "ot", "ôt", "haw", "aulx", "os", "op", "ault", "aul", "aud", "aut", "aux", "ha", "hau", "eau", "haut", "heau", "eaux", "ach", "oth"} },
        { "O", new List<string>() {"o", "hô", "ho", "ü", "u", "hau", "al", "oi", "au", "a", "ot", "oa", "oo"} },
        { "u", new List<string>() {"ou", "ouls", "aou", "aoul", "houe", "hou", "houx", "août", "aoû", "oû", "oud", "ouds", "u", "oug", "où", "oue", "oues", "ue", "w", "ouc", "oux", "oût", "oup", "ouent", "o", "oo", "hoo", "ow", "ous", "out", "oûl", "ouh", "ew"} },

        { "n j", new List<string>() {"gn", "gne", "gnes", "gnent", "gni"} },
        { "g z", new List<string>() {"x"} },
        { "k s", new List<string>() {"x", "xe", "xes", "xent"} },

        { "u a", new List<string>() {"oi", "o", "oî", "oid", "oient", "oigt", "oie", "oies", "hoi", "oix", "ois", "oit", "oît", "oê", "oy", "oig", "oe"} },
        { "u e~", new List<string>() {"oin", "oins", "oint", "oing"} },

        { "", new List<string>() {"-e", "-s", "-x", "-es", "-t", "-ent"} },
    };

    /// <summary>
    /// Data used for generating the <see cref="DropZoneExpand"/>s <br/>
    /// Format: phoneme -> ( maxScale, list of (grapheme, localPos, isArchigrapheme) )
    /// </summary>
    public static Dictionary<string, (float, List<(string, Vector2, bool)>)> dataForPhoneme = new Dictionary<string, (float, List<(string, Vector2, bool)>)>()
    {
        {"m", (5.5f, new List<(string, Vector2, bool)>() {("m", new Vector2(-1.7f, 0.6f), true), ("ms", new Vector2(-0.3f, 1f), false), ("me", new Vector2(0.7f, 1f), false), ("mes", new Vector2(1.9f, 1f), false), ("mme", new Vector2(0f, 0.1f), false), ("mmes", new Vector2(1.6f, 0.1f), false), ("m'", new Vector2(-2.1f, -1f), false), ("mm", new Vector2(-1.2f, -1f), false), ("ment", new Vector2(0f, -1f), false), ("mment", new Vector2(1.6f, -1f), false), }) },
        {"f", (6f, new List<(string, Vector2, bool)>() {("f", new Vector2(-1.8f, 1.1f), true), ("ph", new Vector2(-1.8f, -0.9f), true), ("fe", new Vector2(-0.4f, 1.6f), false), ("fes", new Vector2(0.7f, 1.6f), false), ("fent", new Vector2(1.9f, 1.6f), false), ("ff", new Vector2(-0.3f, 0.5f), false), ("ffe", new Vector2(0.8f, 0.5f), false), ("ffes", new Vector2(1.9f, 0.5f), false), ("ffent", new Vector2(-0.2f, -0.6f), false), ("phent", new Vector2(1.6f, -0.6f), false), ("phe", new Vector2(-0.3000001f, -1.6f), false), ("phes", new Vector2(0.9f, -1.6f), false), ("v", new Vector2(2f, -1.6f), false), }) },
        {"v", (4f, new List<(string, Vector2, bool)>() {("v", new Vector2(-0.8f, 0.5f), true), ("ve", new Vector2(0.8f, 1f), false), ("ves", new Vector2(0.8f, 0f), false), ("f", new Vector2(-1.2f, -1.1f), false), ("w", new Vector2(-0.4f, -1.1f), false), ("vent", new Vector2(0.8f, -1.1f), false), }) },
        {"p", (5.5f, new List<(string, Vector2, bool)>() {("p", new Vector2(-1.5f, 0.8f), true), ("pe", new Vector2(0f, 1.2f), false), ("pes", new Vector2(1.2f, 1.2f), false), ("ps", new Vector2(0f, 0.1f), false), ("b", new Vector2(0.8f, 0.1f), false), ("pent", new Vector2(1.8f, 0.1f), false), ("pp", new Vector2(-2.1f, -0.8f), false), ("ppe", new Vector2(-1f, -0.8f), false), ("ppes", new Vector2(0.3f, -0.8f), false), ("ppent", new Vector2(1.7f, -0.8f), false), }) },
        {"b", (4.5f, new List<(string, Vector2, bool)>() {("b", new Vector2(-1.3f, 0.4f), true), ("be", new Vector2(0f, 0.7f), false), ("bes", new Vector2(1.1f, 0.7f), false), ("bent", new Vector2(0.2f, -0.1f), false), ("bb", new Vector2(0f, -0.9f), false), ("bbe", new Vector2(1.1f, -0.9f), false), }) },
        {"n", (6f, new List<(string, Vector2, bool)>() {("n", new Vector2(-2f, 1.2f), true), ("n'", new Vector2(-1f, 1f), false), ("ne", new Vector2(-0.2f, 1f), false), ("nes", new Vector2(0.8f, 1f), false), ("nent", new Vector2(2f, 1f), false), ("nne", new Vector2(-0.9f, 0f), false), ("nnes", new Vector2(0.5f, 0f), false), ("nnent", new Vector2(2f, 0f), false), ("nn", new Vector2(-2.2f, 0f), false), ("mn", new Vector2(-2.2f, -1f), false), ("mne", new Vector2(-1.1f, -1f), false), ("mnes", new Vector2(0.3f, -1f), false), ("mnent", new Vector2(1.9f, -1f), false), }) },
        {"t", (6f, new List<(string, Vector2, bool)>() {("t", new Vector2(-1.7f, 1.6f), true), ("t'", new Vector2(-0.3f, 2.1f), false), ("tu", new Vector2(0.8f, 2.1f), false), ("tre", new Vector2(2f, 2.1f), false), ("te", new Vector2(-0.2f, 1.1f), false), ("tes", new Vector2(0.8f, 1.1f), false), ("tent", new Vector2(2f, 1.1f), false), ("d", new Vector2(-2.3f, 0.1f), false), ("de", new Vector2(-1.5f, 0.1f), false), ("tte", new Vector2(-0.5f, 0.1f), false), ("ttes", new Vector2(0.5f, 0.1f), false), ("ttent", new Vector2(1.9f, 0.1f), false), ("tt", new Vector2(-2.3f, -1f), false), ("th", new Vector2(-1.5f, -1f), false), ("the", new Vector2(-0.5f, -1f), false), ("thes", new Vector2(0.5f, -1f), false), ("thent", new Vector2(1.9f, -1f), false), ("c", new Vector2(-2.3f, -2f), false), ("pt", new Vector2(-1.5f, -2f), false), ("pte", new Vector2(-0.5f, -2f), false), ("ptes", new Vector2(0.5f, -2f), false), ("ptent", new Vector2(1.9f, -2f), false), }) },
        {"s", (7f, new List<(string, Vector2, bool)>() {("s", new Vector2(-2.1f, 2.3f), true), ("s'", new Vector2(-0.7f, 2.6f), false), ("se", new Vector2(0.1f, 2.6f), false), ("ses", new Vector2(1.2f, 2.6f), false), ("sent", new Vector2(2.4f, 2.6f), false), ("sce", new Vector2(-0.5f, 1.6f), false), ("sces", new Vector2(0.8f, 1.6f), false), ("scent", new Vector2(2.3f, 1.6f), false), ("sc", new Vector2(-2.6f, 0.6f), false), ("ss", new Vector2(-1.6f, 0.6f), false), ("sse", new Vector2(-0.6f, 0.6f), false), ("sses", new Vector2(0.7f, 0.6f), false), ("ssent", new Vector2(2.3f, 0.6f), false), ("t", new Vector2(-2.6f, -0.5f), false), ("th", new Vector2(-1.6f, -0.5f), false), ("sth", new Vector2(-0.6f, -0.5f), false), ("x", new Vector2(0.5f, -0.5f), false), ("z", new Vector2(1.5f, -0.5f), false), ("ls", new Vector2(2.5f, -0.5f), false), ("ce", new Vector2(-0.6f, -1.5f), false), ("ces", new Vector2(0.6f, -1.5f), false), ("cent", new Vector2(2f, -1.5f), false), ("c", new Vector2(-2.1f, -1.8f), true), ("c'", new Vector2(-0.5f, -2.5f), false), ("cc", new Vector2(0.5f, -2.5f), false), ("ç", new Vector2(1.5f, -2.5f), false), ("sç", new Vector2(2.5f, -2.5f), false), }) },
        {"z", (4.5f, new List<(string, Vector2, bool)>() {("z", new Vector2(-1.4f, 1.1f), true), ("ze", new Vector2(0f, 1.5f), false), ("zes", new Vector2(1.2f, 1.5f), false), ("zent", new Vector2(0.2f, 0.5f), false), ("zz", new Vector2(1.2f, 0.5f), false), ("s", new Vector2(-1.4f, -0.9f), true), ("se", new Vector2(0f, -0.5f), false), ("ses", new Vector2(1.2f, -0.5f), false), ("sent", new Vector2(0.2f, -1.5f), false), ("x", new Vector2(1.2f, -1.5f), false), }) },
        {"d", (4.5f, new List<(string, Vector2, bool)>() {("d", new Vector2(-1.3f, -0.5f), true), ("d'", new Vector2(-1.7f, 1f), false), ("de", new Vector2(-0.9f, 1f), false), ("des", new Vector2(0.1f, 1f), false), ("dent", new Vector2(1.4f, 1f), false), ("dd", new Vector2(0.3f, -0.1f), false), ("dh", new Vector2(1.3f, -0.1f), false), ("g", new Vector2(0.3f, -1f), false), ("z", new Vector2(1.3f, -1f), false), }) },
        {"l", (5f, new List<(string, Vector2, bool)>() {("l", new Vector2(-1.3f, 1f), true), ("le", new Vector2(0.3f, 1.5f), false), ("les", new Vector2(1.6f, 1.5f), false), ("lent", new Vector2(0.5f, 0.5f), false), ("ll", new Vector2(1.5f, 0.5f), false), ("lle", new Vector2(-1.5f, -0.6f), false), ("lles", new Vector2(-0.1f, -0.6f), false), ("llent", new Vector2(1.3f, -0.6f), false), ("ls", new Vector2(-1.5f, -1.6f), false), ("l'", new Vector2(-0.5f, -1.6f), false), ("lp", new Vector2(0.6f, -1.6f), false), ("gl", new Vector2(1.5f, -1.6f), false), }) },
        {"j", (7f, new List<(string, Vector2, bool)>() {("ll", new Vector2(-2.1f, 2f), true), ("lle", new Vector2(-1f, 2.5f), false), ("lles", new Vector2(0.4f, 2.5f), false), ("llent", new Vector2(2f, 2.5f), false), ("ille", new Vector2(-1f, 1.5f), false), ("illes", new Vector2(0.4f, 1.5f), false), ("illent", new Vector2(2f, 1.5f), false), ("ill", new Vector2(-2.1f, 0.5f), false), ("lli", new Vector2(-1f, 0.5f), false), ("il", new Vector2(0.2f, 0.5f), false), ("illi", new Vector2(1.3f, 0.5f), false), ("j", new Vector2(2.3f, 0.5f), false), ("i", new Vector2(-2.1f, -1f), true), ("ye", new Vector2(-0.3f, -0.6f), false), ("yes", new Vector2(0.8f, -0.6f), false), ("yent", new Vector2(2.1f, -0.6f), false), ("hy", new Vector2(0.4f, -2.7f), false), ("y", new Vector2(1.7f, -1.9f), true), ("ï", new Vector2(-2.1f, -2.7f), false), ("ïe", new Vector2(-0.9f, -2.7f), false), ("hi", new Vector2(0f, -1.7f), false), }) },
        {"R", (6.5f, new List<(string, Vector2, bool)>() {("r", new Vector2(-1.6f, 1.6f), true), ("rs", new Vector2(0f, 2.1f), false), ("rt", new Vector2(1f, 2.1f), false), ("remt", new Vector2(2.1f, 2.1f), false), ("rds", new Vector2(0.3f, 1f), false), ("rd", new Vector2(1.9f, 1f), false), ("rr", new Vector2(-2.1f, 0f), false), ("rre", new Vector2(-0.8f, 0f), false), ("rres", new Vector2(0.6f, 0f), false), ("rrent", new Vector2(2f, 0f), false), ("rc", new Vector2(-2.1f, -1f), false), ("re", new Vector2(-0.8f, -1f), false), ("rf", new Vector2(0.5f, -1f), false), ("rg", new Vector2(2f, -1f), false), ("rps", new Vector2(-2.1f, -2f), false), ("rh", new Vector2(-0.8f, -2f), false), ("rrh", new Vector2(0.5f, -2f), false), ("rrhe", new Vector2(2f, -2f), false), }) },
        {"S", (7f, new List<(string, Vector2, bool)>() {("ch", new Vector2(-2.4f, 1.5f), true), ("che", new Vector2(-0.9f, 2.1f), false), ("ches", new Vector2(0.6f, 2.1f), false), ("chent", new Vector2(2.2f, 2.1f), false), ("s", new Vector2(-0.9f, 1.1f), false), ("c", new Vector2(0.6f, 1.1f), false), ("sc", new Vector2(2.2f, 1.1f), false), ("sh", new Vector2(-2.4f, -1f), true), ("chs", new Vector2(-0.9f, 0f), false), ("sch", new Vector2(0.6f, 0f), false), ("je", new Vector2(2.2f, 0f), false), ("she", new Vector2(-0.9f, -1.1f), false), ("shes", new Vector2(0.6f, -1.1f), false), ("shent", new Vector2(2.2f, -1.1f), false), ("sche", new Vector2(-1f, -2.2f), false), ("sches", new Vector2(0.6f, -2.2f), false), ("schent", new Vector2(2.2f, -2.2f), false), }) },
        {"Z", (4.5f, new List<(string, Vector2, bool)>() {("j", new Vector2(-1f, 0.5f), true), ("je", new Vector2(0.5f, 1f), false), ("j'", new Vector2(1.6f, 1f), false), ("g", new Vector2(0.5f, 0.1f), false), ("gg", new Vector2(1.6f, 0.1f), false), ("ge", new Vector2(-1.5f, -0.9f), false), ("ges", new Vector2(-0.3f, -0.9f), false), ("gent", new Vector2(1.3f, -0.9f), false), }) },
        {"k", (8f, new List<(string, Vector2, bool)>() {("k", new Vector2(2f, -1.6f), true), ("x", new Vector2(-3.3f, 2.1f), false), ("g", new Vector2(-2.6f, 2.1f), false), ("cc", new Vector2(-1.8f, 2.1f), false), ("cch", new Vector2(-0.7f, 2.1f), false), ("ccu", new Vector2(0.5f, 2.1f), false), ("cqu", new Vector2(1.7f, 2.1f), false), ("cque", new Vector2(3f, 2.1f), false), ("c", new Vector2(-3f, 0.6f), true), ("ch", new Vector2(-1.9f, 0.9f), false), ("chs", new Vector2(-0.6f, 0.9f), false), ("cques", new Vector2(0.9f, 0.9f), false), ("cquent", new Vector2(2.8f, 0.9f), false), ("ck", new Vector2(-1.9f, -0.1f), false), ("cke", new Vector2(-0.8f, -0.1f), false), ("ckes", new Vector2(0.4f, -0.1f), false), ("ckent", new Vector2(2f, -0.1f), false), ("cu", new Vector2(3.3f, -0.1f), false), ("qu", new Vector2(-3.1f, -1.5f), true), ("qu'", new Vector2(-1.8f, -1.1f), false), ("que", new Vector2(-0.4f, -1.1f), false), ("q", new Vector2(0.9f, -1.1f), false), ("ques", new Vector2(-1.6f, -2.1f), false), ("quent", new Vector2(0.4f, -2.1f), false), ("ke", new Vector2(3.3f, -1.1f), false), ("kk", new Vector2(3.3f, -2.1f), false), }) },
        {"g", (5.5f, new List<(string, Vector2, bool)>() {("g", new Vector2(-1.6f, -0.3f), true), ("gu", new Vector2(-2.1f, 1f), false), ("gue", new Vector2(-1f, 1f), false), ("gues", new Vector2(0.2f, 1f), false), ("guent", new Vector2(1.7f, 1f), false), ("gh", new Vector2(-0.2f, 0f), false), ("ghe", new Vector2(1f, 0f), false), ("gg", new Vector2(-0.2f, -1.1f), false), ("c", new Vector2(0.7f, -1.1f), false), }) },

        {"y", (7.5f, new List<(string, Vector2, bool)>() {("u", new Vector2(-0.7f, 0f), true), ("ul", new Vector2(-2.1f, 2.4f), false), ("ud", new Vector2(-1f, 2.4f), false), ("eû", new Vector2(0f, 2.4f), false), ("ux", new Vector2(1f, 2.4f), false), ("eût", new Vector2(2.1f, 2.4f), false), ("eu", new Vector2(-2.1f, 1.3f), false), ("eus", new Vector2(-1f, 1.3f), false), ("eut", new Vector2(0f, 1.3f), false), ("eue", new Vector2(1.1f, 1.3f), false), ("eues", new Vector2(2.5f, 1.3f), false), ("üe", new Vector2(-3.1f, 0.3f), false), ("û", new Vector2(-2f, 0.3f), false), ("ue", new Vector2(0.6f, 0.3f), false), ("ues", new Vector2(1.7f, 0.3f), false), ("uent", new Vector2(2.8f, 0.3f), false), ("û", new Vector2(-3f, -0.7f), false), ("uh", new Vector2(-2f, -0.7f), false), ("us", new Vector2(0.6f, -0.7f), false), ("ut", new Vector2(1.5f, -0.7f), false), ("ût", new Vector2(2.6f, -0.7f), false), ("hu", new Vector2(-2f, -1.7f), false), ("hue", new Vector2(-0.8f, -1.7f), false), ("hues", new Vector2(0.6f, -1.7f), false), ("huent", new Vector2(2.2f, -1.7f), false), ("uth", new Vector2(-1.1f, -2.8f), false), ("hut", new Vector2(-0.1f, -2.8f), false), ("üt", new Vector2(0.9f, -2.8f), false), }) },
        {"e", (7.5f, new List<(string, Vector2, bool)>() {("é", new Vector2(0f, 0.6f), true), ("e", new Vector2(0f, 3f), false), ("ê", new Vector2(-2.2f, 2.1f), false), ("ers", new Vector2(-1.1f, 2.1f), false), ("er", new Vector2(0f, 2.1f), false), ("et", new Vector2(1f, 2.1f), false), ("ez", new Vector2(2.1f, 2.1f), false), ("hè", new Vector2(-2.7f, 1.1f), false), ("es", new Vector2(-1.6f, 1.1f), false), ("ée", new Vector2(1.4f, 1.1f), false), ("és", new Vector2(2.4f, 1.1f), false), ("ë", new Vector2(0f, -3f), false), ("hai", new Vector2(-2.8f, 0.1f), false), ("he", new Vector2(-1.6f, 0.1f), false), ("ées", new Vector2(1.5f, 0.1f), false), ("éent", new Vector2(2.8f, 0.1f), false), ("oe", new Vector2(-2.6f, -0.9f), false), ("ei", new Vector2(-1.6f, -0.9f), false), ("ai", new Vector2(-0.7f, -0.9f), false), ("aie", new Vector2(0.5f, -0.9f), false), ("eds", new Vector2(1.6f, -0.9f), false), ("ed", new Vector2(2.7f, -0.9f), false), ("ee", new Vector2(-2.1f, -2f), false), ("a", new Vector2(-1f, -2f), false), ("ae", new Vector2(0.1f, -2f), false), ("aî", new Vector2(1.1f, -2f), false), ("ay", new Vector2(2.1f, -2f), false), }) },
        {"i", (7.5f, new List<(string, Vector2, bool)>() {("i", new Vector2(-0.4f, 0.2f), true), ("u", new Vector2(-2.1f, 2.8f), false), ("ea", new Vector2(-1f, 2.9f), false), ("its", new Vector2(0f, 2.9f), false), ("ïe", new Vector2(1f, 2.9f), false), ("e", new Vector2(2f, 2.9f), false), ("ee", new Vector2(-2.5f, 1.8f), false), ("iz", new Vector2(-1.5f, 1.8f), false), ("ix", new Vector2(-0.4f, 1.8f), false), ("id", new Vector2(0.5f, 1.8f), false), ("ïs", new Vector2(1.6f, 1.8f), false), ("ït", new Vector2(2.6f, 1.8f), false), ("œ", new Vector2(-2.8f, 0.7f), false), ("î", new Vector2(-1.7f, 0.7f), false), ("is", new Vector2(0.8f, 0.7f), false), ("it", new Vector2(1.8f, 0.7f), false), ("ît", new Vector2(2.9f, 0.7f), false), ("il", new Vector2(-2.7f, -0.3f), false), ("ï", new Vector2(-1.7f, -0.3f), false), ("ie", new Vector2(0.8f, -0.2f), false), ("ies", new Vector2(1.9f, -0.2f), false), ("ient", new Vector2(3.1f, -0.2f), false), ("ils", new Vector2(-3f, -1.2f), false), ("hi", new Vector2(-2f, -1.2f), false), ("hie", new Vector2(-1f, -1.2f), false), ("hî", new Vector2(0f, -1.2f), false), ("hît", new Vector2(1f, -1.2f), false), ("his", new Vector2(2.1f, -1.2f), false), ("hit", new Vector2(3.1f, -1.2f), false), ("y", new Vector2(-0.4f, -2.3f), true), ("ys", new Vector2(-1.7f, -2.3f), false), ("ye", new Vector2(0.8f, -2.3f), false), ("hy", new Vector2(1.8f, -2.3f), false), ("yg", new Vector2(0.8f, -3.2f), false), }) },
        {"2", (6f, new List<(string, Vector2, bool)>() {("eu", new Vector2(-0.6f, -0.3f), true), ("œu", new Vector2(-1f, 2.1f), false), ("œud", new Vector2(0.7f, 2.1f), false), ("ö", new Vector2(-2f, 1.1f), false), ("eû", new Vector2(-1f, 1.1f), false), ("œux", new Vector2(0.5f, 1.1f), false), ("eur", new Vector2(-2.1f, 0f), false), ("eus", new Vector2(1.9f, 1.1f), false), ("eux", new Vector2(2.2f, 0f), false), ("œh", new Vector2(-2f, -1.1f), false), ("eut", new Vector2(0.9f, 0f), false), ("eue", new Vector2(-1f, -2f), false), ("heu", new Vector2(0.9f, -1.1f), false), ("euh", new Vector2(2.1f, -1.1f), false), ("œufs", new Vector2(0.5f, -2f), false), }) },
        {"9", (6f, new List<(string, Vector2, bool)>() {("e", new Vector2(-0.2f, 0.7f), true), ("u", new Vector2(-0.7f, 2.1f), false), ("ue", new Vector2(0.4f, 2.1f), false), ("i", new Vector2(-1.7f, 1.1f), false), ("on", new Vector2(1.2f, 1.2f), false), ("o", new Vector2(2.2f, 1.2f), false), ("he", new Vector2(-1.9f, 0.1f), false), ("œ", new Vector2(1.2f, 0.1f), false), ("ö", new Vector2(2.2f, 0.1f), false), ("hu", new Vector2(-1.7f, -1f), false), ("heu", new Vector2(-0.6f, -1f), false), ("eu", new Vector2(0.5f, -1f), false), ("œu", new Vector2(1.7f, -1f), false), ("a", new Vector2(-0.6f, -2f), false), ("ai", new Vector2(0.5f, -2f), false), }) },
        {"E", (9f, new List<(string, Vector2, bool)>() {("è", new Vector2(0.1f, 1.2f), true), ("é", new Vector2(-1.4f, 3.7f), false), ("ë", new Vector2(-0.4f, 3.7f), false), ("ê", new Vector2(0.6f, 3.7f), false), ("e", new Vector2(1.6f, 3.7f), false), ("hê", new Vector2(-2.5f, 2.6f), false), ("ès", new Vector2(-1.5f, 2.6f), false), ("oe", new Vector2(-0.4f, 2.6f), false), ("eh", new Vector2(0.6f, 2.6f), false), ("es", new Vector2(1.6f, 2.6f), false), ("est", new Vector2(2.7f, 2.6f), false), ("ef", new Vector2(-3.5f, 1.6f), false), ("hè", new Vector2(-2.5f, 1.6f), false), ("he", new Vector2(-1.5f, 1.6f), false), ("et", new Vector2(1.6f, 1.6f), false), ("ets", new Vector2(2.7f, 1.6f), false), ("ect", new Vector2(-3.6f, 0.6f), false), ("ey", new Vector2(-2.5f, 0.6f), false), ("ei", new Vector2(-1.5f, 0.6f), false), ("êt", new Vector2(1.6f, 0.6f), false), ("êts", new Vector2(2.7f, 0.6f), false), ("elle", new Vector2(-3.7f, -0.5f), false), ("haie", new Vector2(-2.5f, -0.5f), false), ("hai", new Vector2(-1.4f, -0.5f), false), ("ea", new Vector2(-0.4f, -0.5f), false), ("aie", new Vector2(0.7f, -0.5f), false), ("aies", new Vector2(2f, -0.5f), false), ("aient", new Vector2(3.5f, -0.5f), false), ("a", new Vector2(-2.5f, -1.5f), false), ("aix", new Vector2(-1.5f, -1.5f), false), ("ai", new Vector2(0f, -1.9f), true), ("ais", new Vector2(1.4f, -1.5f), false), ("ait", new Vector2(2.5f, -1.5f), false), ("aît", new Vector2(3.5f, -1.5f), false), ("ä", new Vector2(-2.5f, -2.5f), false), ("aid", new Vector2(-1.5f, -2.5f), false), ("hais", new Vector2(1.5f, -2.5f), false), ("hait", new Vector2(2.8f, -2.5f), false), ("ay", new Vector2(-1.1f, -3.5f), false), ("aye", new Vector2(0.1f, -3.5f), false), ("aî", new Vector2(1.2f, -3.5f), false), }) },
        {"a", (7f, new List<(string, Vector2, bool)>() {("a", new Vector2(0f, 0.1f), true), ("aa", new Vector2(0f, 2.7f), false), ("ac", new Vector2(-1.6f, 1.7f), false), ("ha", new Vector2(-0.5f, 1.7f), false), ("ah", new Vector2(0.4f, 1.7f), false), ("acs", new Vector2(1.5f, 1.7f), false), ("az", new Vector2(-2.7f, 0.6f), false), ("ars", new Vector2(-1.6f, 0.6f), false), ("as", new Vector2(1.5f, 0.7f), false), ("at", new Vector2(2.5f, 0.7f), false), ("ap", new Vector2(-2.7f, -0.4f), false), ("hâ", new Vector2(-1.6f, -0.4f), false), ("ât", new Vector2(1.5f, -0.4f), false), ("act", new Vector2(2.6f, -0.4f), false), ("e", new Vector2(-1.6f, -1.4f), false), ("à", new Vector2(-0.5f, -1.4f), false), ("â", new Vector2(0.5f, -1.4f), false), ("ach", new Vector2(1.5f, -1.4f), false), ("ae", new Vector2(0f, -2.4f), false), }) },
        {"e~", (9.5f, new List<(string, Vector2, bool)>() {("in", new Vector2(0.2f, 1f), true), ("yn", new Vector2(-1.7f, 3.5f), false), ("în", new Vector2(-0.7f, 3.5f), false), ("înt", new Vector2(0.4f, 3.5f), false), ("înts", new Vector2(1.5f, 3.5f), false), ("ingt", new Vector2(-2.7f, 2.5f), false), ("ing", new Vector2(-1.5f, 2.5f), false), ("inq", new Vector2(-0.4f, 2.5f), false), ("inct", new Vector2(0.8f, 2.5f), false), ("ins", new Vector2(1.9f, 2.5f), false), ("int", new Vector2(2.9f, 2.5f), false), ("eing", new Vector2(-3.6f, 1.5f), false), ("ein", new Vector2(-2.3f, 1.5f), false), ("en", new Vector2(-1.3f, 1.5f), false), ("ens", new Vector2(1.8f, 1.5f), false), ("ent", new Vector2(2.9f, 1.5f), false), ("hin", new Vector2(-3.6f, 0.4f), false), ("ïn", new Vector2(-2.4f, 0.4f), false), ("im", new Vector2(-1.3f, 0.4f), false), ("ainc", new Vector2(1.9f, 0.4f), false), ("aincs", new Vector2(3.5f, 0.4f), false), ("aing", new Vector2(-3.7f, -0.6f), false), ("hen", new Vector2(-2.4f, -0.6f), false), ("inc", new Vector2(-1.3f, -0.6f), false), ("ain", new Vector2(-0.3f, -0.6f), false), ("eins", new Vector2(1f, -0.6f), false), ("eint", new Vector2(2.3f, -0.6f), false), ("eints", new Vector2(3.6f, -0.6f), false), ("aim", new Vector2(-3.6f, -1.6f), false), ("eun", new Vector2(-2.5f, -1.6f), false), ("hein", new Vector2(-1.3f, -1.6f), false), ("ains", new Vector2(2f, -1.6f), false), ("aints", new Vector2(3.4f, -1.6f), false), ("ym", new Vector2(-2.4f, -2.6f), false), ("um", new Vector2(-1.3f, -2.6f), false), ("un", new Vector2(0.3f, -2.2f), true), ("aint", new Vector2(2f, -2.7f), false), ("u", new Vector2(-1.4f, -3.5f), false), ("hum", new Vector2(-0.2f, -3.6f), false), ("unt", new Vector2(1.2f, -3.6f), false), }) },
        {"a~", (8f, new List<(string, Vector2, bool)>() {("an", new Vector2(-0.5f, 1.8f), true), ("am", new Vector2(-1.4f, 3.1f), false), ("han", new Vector2(-2.3f, 2.1f), false), ("ang", new Vector2(1.1f, 2.2f), false), ("aans", new Vector2(2.4f, 2.2f), false), ("ham", new Vector2(-2.4f, 1.1f), false), ("ans", new Vector2(1.1f, 1.1f), false), ("ant", new Vector2(2.4f, 1.1f), false), ("en", new Vector2(-0.5f, -1.3f), true), ("em", new Vector2(-3.3f, 0.1f), false), ("emp", new Vector2(-2.1f, 0.1f), false), ("aon", new Vector2(-0.7f, 0.1f), false), ("anc", new Vector2(0.6f, 0.1f), false), ("amp", new Vector2(1.9f, 0.1f), false), ("and", new Vector2(3.2f, 0.1f), false), ("emps", new Vector2(-2.3f, -0.9f), false), ("ends", new Vector2(1.3f, -0.9f), false), ("end", new Vector2(2.8f, -0.9f), false), ("empt", new Vector2(-2.3f, -2f), false), ("ens", new Vector2(1.2f, -2f), false), ("ent", new Vector2(2.5f, -2f), false), ("e", new Vector2(-1.4f, -3f), false), ("eng", new Vector2(-0.2f, -3f), false), ("hen", new Vector2(1.2f, -3f), false), }) },
        {"o~", (6.5f, new List<(string, Vector2, bool)>() {("on", new Vector2(-0.5f, 0.7f), true), ("onc", new Vector2(-1.7f, 2f), false), ("ond", new Vector2(-0.2f, 2f), false), ("onds", new Vector2(1.3f, 2f), false), ("un", new Vector2(-2.1f, 1f), false), ("ons", new Vector2(1.1f, 1f), false), ("ont", new Vector2(2.3f, 1f), false), ("hon", new Vector2(-2.2f, 0f), false), ("hong", new Vector2(1.3f, 0f), false), ("om", new Vector2(-2.1f, -1f), false), ("omp", new Vector2(-0.7f, -1f), false), ("omps", new Vector2(1f, -1f), false), ("o", new Vector2(2.2f, -1f), false), ("ompt", new Vector2(-1f, -2.1f), false), ("omb", new Vector2(1f, -2.1f), false), }) },
        {"o", (9f, new List<(string, Vector2, bool)>() {("au", new Vector2(0.3f, -0.8f), true), ("a", new Vector2(-1.2f, 3.7f), false), ("oa", new Vector2(-0.1f, 3.7f), false), ("ao", new Vector2(1.2f, 3.7f), false), ("oc", new Vector2(-2.3f, 2.7f), false), ("oh", new Vector2(-1.2f, 2.7f), false), ("ho", new Vector2(-0.1f, 2.7f), false), ("hô", new Vector2(1f, 2.7f), false), ("hot", new Vector2(2.2f, 2.7f), false), ("aw", new Vector2(-3.1f, 1.7f), false), ("ow", new Vector2(-2.1f, 1.7f), false), ("ô", new Vector2(-1f, 1.7f), false), ("o", new Vector2(0.4f, 1.1f), true), ("ot", new Vector2(1.9f, 1.6f), false), ("ôt", new Vector2(2.9f, 1.6f), false), ("haw", new Vector2(-3.4f, 0.6f), false), ("aulx", new Vector2(-1.6f, 0.6f), false), ("os", new Vector2(1.9f, 0.6f), false), ("op", new Vector2(3.1f, 0.6f), false), ("ault", new Vector2(-3.2f, -0.4f), false), ("aul", new Vector2(-1.6f, -0.4f), false), ("aud", new Vector2(2.1f, -1.5f), false), ("aut", new Vector2(2f, -0.4f), false), ("aux", new Vector2(3.3f, -0.4f), false), ("ha", new Vector2(-3.1f, -1.4f), false), ("hau", new Vector2(-1.6f, -1.4f), false), ("eau", new Vector2(3.3f, -1.5f), false), ("haut", new Vector2(-1.6f, -2.5f), false), ("heau", new Vector2(0.3f, -2.5f), false), ("eaux", new Vector2(2.2f, -2.5f), false), ("ach", new Vector2(-1f, -3.5f), false), ("oth", new Vector2(1f, -3.5f), false), }) },
        {"O", (6f, new List<(string, Vector2, bool)>() {("o", new Vector2(-0.7f, 0.2f), true), ("hô", new Vector2(-1f, 1.5f), false), ("ho", new Vector2(0f, 1.5f), false), ("ü", new Vector2(1.1f, 1.5f), false), ("u", new Vector2(-2f, 0.5f), false), ("hau", new Vector2(0.9f, 0.5f), false), ("al", new Vector2(2f, 0.5f), false), ("oi", new Vector2(-2f, -0.5f), false), ("au", new Vector2(0.8f, -0.5f), false), ("a", new Vector2(2f, -0.5f), false), ("ot", new Vector2(-1f, -1.5f), false), ("oa", new Vector2(0f, -1.5f), false), ("oo", new Vector2(1.1f, -1.5f), false), }) },
        {"u", (8.5f, new List<(string, Vector2, bool)>() {("ou", new Vector2(-0.5f, 0.7f), true), ("ouls", new Vector2(-1.5f, 3f), false), ("aou", new Vector2(0.1f, 3f), false), ("aoul", new Vector2(1.5f, 3f), false), ("houe", new Vector2(-2.4f, 2.1f), false), ("hou", new Vector2(-1f, 2.1f), false), ("houx", new Vector2(0.5f, 2.1f), false), ("août", new Vector2(2f, 2.1f), false), ("aoû", new Vector2(-3.2f, 1.1f), false), ("oû", new Vector2(-2f, 1.1f), false), ("oud", new Vector2(1.1f, 1.1f), false), ("ouds", new Vector2(2.4f, 1.1f), false), ("u", new Vector2(3.5f, 1.1f), false), ("oug", new Vector2(-3.3f, 0f), false), ("où", new Vector2(-2f, 0f), false), ("oue", new Vector2(1.1f, 0f), false), ("oues", new Vector2(2.4f, 0f), false), ("ue", new Vector2(3.6f, 0f), false), ("w", new Vector2(-3.4f, -1f), false), ("ouc", new Vector2(-2.4f, -1f), false), ("oux", new Vector2(-1.1f, -1f), false), ("oût", new Vector2(0.1f, -1f), false), ("oup", new Vector2(1.4f, -1f), false), ("ouent", new Vector2(2.9f, -1f), false), ("o", new Vector2(-2.9f, -2f), false), ("oo", new Vector2(-2.1f, -2f), false), ("hoo", new Vector2(-0.9f, -2f), false), ("ow", new Vector2(0.2f, -2f), false), ("ous", new Vector2(1.3f, -2f), false), ("out", new Vector2(2.6f, -2f), false), ("oûl", new Vector2(-1.6f, -3f), false), ("ouh", new Vector2(0f, -3f), false), ("ew", new Vector2(1.6f, -3f), false), }) },

        {"g z", (1.5f, new List<(string, Vector2, bool)>() {("x", new Vector2(0f, 0.2f), true), }) },
        {"k s", (2.5f, new List<(string, Vector2, bool)>() {("x", new Vector2(-0.6f, 0.7f), true), ("xe", new Vector2(0.6f, 0.6f), false), ("xes", new Vector2(-0.6f, -0.5f), false), ("xent", new Vector2(0.6f, -0.5f), false), }) },
        {"n j", (3f, new List<(string, Vector2, bool)>() {("gn", new Vector2(-0.7f, 1.1f), true), ("gne", new Vector2(0.8f, 1f), false), ("gnes", new Vector2(-0.7f, -0.1f), false), ("gnent", new Vector2(0f, -1f), false), ("gni", new Vector2(0.8f, -0.1f), false), }) },


        {"", (4f, new List<(string, Vector2, bool)>() {("-e", new Vector2(-1.2f, 0.6f), false), ("-s", new Vector2(0f, 0.6f), false), ("-x", new Vector2(1.2f, 0.6f), false), ("-es", new Vector2(-1.2f, -0.5f), false), ("-t", new Vector2(0f, -0.5f), false), ("-ent", new Vector2(1.2f, -0.5f), false), }) },
        {"u a", (5f, new List<(string, Vector2, bool)>() {("oi", new Vector2(-0.9f, 2f), false), ("o", new Vector2(0f, 2f), false), ("oî", new Vector2(0.8f, 2f), false), ("oid", new Vector2(-1.5f, 1.1f), false), ("oient", new Vector2(0f, 1.1f), false), ("oigt", new Vector2(1.5f, 1.1f), false), ("oie", new Vector2(-1.9f, 0.1f), false), ("oies", new Vector2(-0.6f, 0.1f), false), ("hoi", new Vector2(0.7f, 0.1f), false), ("oix", new Vector2(1.9f, 0.1f), false), ("ois", new Vector2(-1.7f, -0.9f), false), ("oit", new Vector2(-0.4f, -0.9f), false), ("oît", new Vector2(0.7f, -0.9f), false), ("oê", new Vector2(1.8f, -0.9f), false), ("oy", new Vector2(-1f, -1.8f), false), ("oig", new Vector2(0f, -1.8f), false), ("oe", new Vector2(1f, -1.8f), false), }) },

    };

    /// <summary>
    /// List of graphemes given a phoneme id.
    /// </summary>
    internal static List<string> ListFor(string id)
    {
        return graphemesForPhoneme[id];
    }

    public int Length { get; private set; }
    public string phoneme { get; private set; }

    private bool terminated;

    public Grapheme(string gr, string ph, bool term = false)
    {
        id = gr;
        phoneme = ph;
        terminated = term || ph == "";
        Length = gr.Replace("-", "").Length;
    }

    /// <summary>
    /// Checks if the two elements are graphemes and can be merged based on their associated phonemes.
    /// </summary>
    public static bool CanMerge(Element e1, Element e2)
    {
        if (!(e1 is Grapheme && e2 is Grapheme)) return false;
        var g1 = e1 as Grapheme;
        var g2 = e2 as Grapheme;
        var phoneme = string.Join(" ", g1.phoneme, g2.phoneme).TrimEnd();
        return !g1.terminated && Phoneme.For(phoneme) != null;
    }

    /// <summary>
    /// Merges two grapheme elements into a single one.
    /// </summary>
    public static Grapheme Merge(Element e1, Element e2)
    {
        var g1 = e1 as Grapheme;
        var g2 = e2 as Grapheme;
        if (!CanMerge(e1, e2)) return null;
        var phoneme = string.Join(" ", g1.phoneme, g2.phoneme).TrimEnd();
        return new Grapheme((g1.id+g2.id).Replace("-",""), phoneme, g2.terminated);
    }

    public override string ToString()
    {
        return $"Grapheme: {id} ({phoneme}){(terminated ? " - terminated": "")}";
    }

    public static bool operator ==(Grapheme g1, Grapheme g2)
    {
        // null check
        if (g2 is null)
            return g1 is null;
        else if (g1 is null)
            return g2 is null;

        return g1.id == g2.id && g1.phoneme.Replace("j", "i") == g2.phoneme.Replace("j", "i");
    }

    public static bool operator !=(Grapheme g1, Grapheme g2)
    {
        return !(g1 == g2);
    }

    public override bool Equals(object obj)
    {
        return obj is Grapheme && (Grapheme)obj == this;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode() ^ phoneme.GetHashCode();
    }
}