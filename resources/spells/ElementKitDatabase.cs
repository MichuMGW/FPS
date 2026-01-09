using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class ElementKitDatabase : Resource
{
    [Export] public Godot.Collections.Array<ElementKitDefinition> Kits { get; set; }

    private Dictionary<Element, ElementKitDefinition> _map;

    public override string ToString() => $"ElementKitDatabase(Kits={Kits?.Count ?? 0})";

    public ElementKitDefinition GetKit(Element element)
    {
        EnsureMap();
        return _map.TryGetValue(element, out var kit) ? kit : null;
    }

    private void EnsureMap()
    {
        if (_map != null) return;

        _map = new Dictionary<Element, ElementKitDefinition>();
        if (Kits == null) return;

        foreach (var kit in Kits)
        {
            if (kit == null) continue;
            _map[kit.Element] = kit;
        }
    }
}
