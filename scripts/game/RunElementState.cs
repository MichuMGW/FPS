using Godot;

public partial class RunElementState : Node
{
    public Element First { get; private set; } = Element.None;
    public Element Second { get; private set; } = Element.None;

    public bool HasFirst => First != Element.None;
    public bool HasSecond => Second != Element.None;

    public void Reset()
    {
        First = Element.None;
        Second = Element.None;
    }

    public void SetFirst(Element e) => First = e;
    public void SetSecond(Element e)
    {
        if (e == First)
        {
            Second = Element.None;
            return;
        }

        Second = e;
    }

    public Element GetCombined()
    {
        if (!HasFirst) return Element.None;
        if (!HasSecond) return First;
        return ElementCombiner.Combine(First, Second);
    }
}
