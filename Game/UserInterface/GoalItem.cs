using Godot;

namespace Refactor1.Game
{
    public class GoalItem : Control
    {
        [Export] public NodePath TitlePath { get; set; }
        
        [Export] public NodePath DescriptionPath { get; set; }

        private Label _title;

        private RichTextLabel _description;

        public override void _Ready()
        {
            GD.Print("here");
            GD.Print(TitlePath.ToString());
            _title = GetNode(TitlePath) as Label;
            _description = GetNode(DescriptionPath) as RichTextLabel;
        }

        public void SetTitle(string title)
        {
            _title.SetText(title);
        }

        public void SetDescription(string description)
        {
            _description.SetBbcode(description);
        }

        public void SetCompleted(bool completed)
        {
            
        }
    }
}