using Godot;

public partial class ElementPair {
    public Element FirstElement {get; set;}
    public Element SecondElement {get; set;}

    public ElementPair(Element firstElement, Element secondElement) {
        FirstElement = firstElement;
        SecondElement = secondElement;
    }
}