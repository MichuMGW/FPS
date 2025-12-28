using Godot;
using System;

public partial class ItemStackWidget : Control
    {
        private readonly TextureRect _icon;
        private readonly Label _count;

        public ItemStackWidget()
        {
            // rozmiar ikonki (zmień jak chcesz)
            CustomMinimumSize = new Vector2(48, 48);

            _icon = new TextureRect
            {
                ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                AnchorRight = 1,
                AnchorBottom = 1,
                Modulate = new Color(1, 1, 1, 0.65f) // półprzezroczyste
            };

            _count = new Label
            {
                Text = "x1",
                AnchorLeft = 0,
                AnchorTop = 0,
                AnchorRight = 1,
                AnchorBottom = 1,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            // lekkie odsunięcie od rogu
            _count.OffsetRight = -4;
            _count.OffsetBottom = -2;

            AddChild(_icon);
            AddChild(_count);

            MouseFilter = MouseFilterEnum.Stop; // tooltip działa, klik nie przechodzi dalej
        }

        public void SetData(Texture2D icon, int count)
        {
            _icon.Texture = icon;
            _count.Text = $"x{count}";
        }
    }