using System;
using Godot;

namespace Refactor1.Game.Logic
{

    public class GraphicalLogicNode : Node2D
    {
        public delegate void PressedHandler(GraphicalLogicNode graphicalLogicNode);
        public event PressedHandler OnPressed;
        
        private TextureRect textureRect;

        private Color _modulateColor;
        private Color _modulateColorHover;
        private bool wasMouseDown = false;

        public LogicNode LogicNode;

        
        public GhostIndexInformation GhostInformation { get; set; }

        public override void _Ready()
        {
            textureRect = GetTextureRect();
            _modulateColor = textureRect.Modulate;
            _modulateColorHover = _modulateColor.Lightened(0.3f);
            textureRect.Connect("gui_input", this, "OnGuiInput");
        }

        public void SetColor(Color color)
        {
            GetTextureRect().SetModulate(color);
            _modulateColor = textureRect.Modulate;
            _modulateColorHover = _modulateColor.Lightened(0.3f);
        }

        public TextureRect GetTextureRect()
        {
            return GetNode("texture") as TextureRect;
        }

        public void SetText(String text)
        {
            var label = GetNode("label") as Label;
            label.SetText(text);
        }

        public override void _Input(InputEvent @event)
        {

        }

        public void OnGuiInput(object @event)
        {
            if (@event is InputEventMouseButton iemb && textureRect.GetRect().HasPoint(GetLocalMousePosition()))
            {
                if (!iemb.Pressed && iemb.ButtonIndex == (int) ButtonList.Left)
                {
                    OnPressed.Invoke(this);
                }
            }
            
            if (@event is InputEventMouseMotion imm)
            {
                if (textureRect.GetRect().HasPoint(GetLocalMousePosition()))
                {
                    textureRect.SetModulate(_modulateColorHover);
                }
            }
        }

        public override void _Process(float delta)
        {
            if (!textureRect.GetRect().HasPoint(GetLocalMousePosition()))
            {
                textureRect.SetModulate(_modulateColor);
            }
        }
    }
}