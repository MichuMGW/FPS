using Godot;

public enum Element {
    None,
    Fire,
    Water,
    Nature,
    Air,
    Magma, //Fire + Nature
    Storm, //Fire + Air 
    Dark, //Fire + Water 
    Poison, //Water + Nature
    Ice, //Water + Air
    Earth //Nature + Air
}