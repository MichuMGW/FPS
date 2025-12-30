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
        return element switch
        {
            Element.Fire => new Color(1, 0.5f, 0),
            Element.Water => new Color(0, 0.5f, 1),
            Element.Nature => new Color(0, 1, 0),
            Element.Air => new Color(0.8f, 0.8f, 0.8f),
            Element.Magma => new Color(1, 0.3f, 0),
            Element.Storm => new Color(0, 0.7f, 1),
            Element.Dark => new Color(0.6f, 0, 0.6f),
            Element.Poison => new Color(0, 0.6f, 0),
            Element.Ice => new Color(0.5f, 0.8f, 1),
            Element.Earth => new Color(0.6f, 0.4f, 0.2f),
            _ => Colors.White,
        };
    }
}