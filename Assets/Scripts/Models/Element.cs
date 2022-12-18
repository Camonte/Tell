/// <summary>
/// Abstract class that represents a linguistic element (either a Phoneme or a Grapheme)
/// </summary>
public class Element
{
    public string id;

    public static bool operator ==(Element e1, Element e2)
    {
        if (e1.GetType() == typeof(Phoneme))
            return (Phoneme)e1 == (Phoneme)e2;
        else if (e1.GetType() == typeof(Grapheme))
            return (Grapheme)e1 == (Grapheme)e2;
        else
            return e1 == e2;
    }

    public static bool operator !=(Element e1, Element e2)
    {
        return !(e1 == e2);
    }

    public override bool Equals(object obj)
    {
        return obj is Element && this == (Element)obj;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public bool isEmpty()
    {
        return (GetType() == typeof(Phoneme)) && ((Phoneme)this) == Phoneme.empty;
    }
}
