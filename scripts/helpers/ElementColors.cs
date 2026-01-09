using Godot;

public static class ElementColors
{
    public static Color GetColor(Element element)
    {
        return element switch
        {
            Element.Fire => Colors.OrangeRed,
            Element.Water => Colors.DodgerBlue,
            Element.Nature => Colors.LimeGreen,
            Element.Air => Colors.LightGray,
            Element.Magma => Colors.Orange,
            Element.Storm => Colors.Cyan,
            Element.Dark => Colors.Purple,
            Element.Poison => Colors.Green,
            Element.Ice => Colors.LightBlue,
            Element.Earth => Colors.SaddleBrown,
            _ => Colors.White,
        };
    }

    public static Color GetStatusEffectColor(Element element)
    {
        Color c = GetColor(element);
        c = c.Lightened(0.35f);
        return c;
    }
}