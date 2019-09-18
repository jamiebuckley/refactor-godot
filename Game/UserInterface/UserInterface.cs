using Godot;
using Godot.Collections;

namespace Refactor1.Game
{
    public class UserInterface : Control
    {
        [Export]
        public NodePath OptionLabelPath { get; set; }
        
        [Export]
        public NodePath BuildModalPath { get; set; }
        
        [Export]
        public NodePath BuildModalButtonPath { get; set; }
        
        [Export]
        public NodePath LogicModalPath { get; set; }
        
        [Export]
        public NodePath GoalBoxPath { get; set; }

        private WindowDialog _buildModal;

        private WindowDialog _logicModal;

        private Label _optionLabel;

        private VBoxContainer _goalBox;
        
        public override void _Ready()
        {
            _optionLabel = GetNode(OptionLabelPath) as Label;
            _buildModal = GetNode(BuildModalPath) as WindowDialog;
            _logicModal = GetNode(LogicModalPath) as WindowDialog;
            _goalBox = GetNode(GoalBoxPath) as VBoxContainer;
            
            foreach (Button button in GetTree().GetNodesInGroup("BuildOptionButton"))
            {
                button.Connect("pressed", this, "OnBuildOptionButtonPress", new Array {button});
            }

            GetNode(BuildModalButtonPath).Connect("pressed", this, "OnBuildModalButtonPressed");
        }


        public void OnBuildOptionButtonPress(Button button)
        {
            _optionLabel.Text = button.Text;
            _buildModal.Hide();
        }

        public void OnBuildModalButtonPressed()
        {
            _buildModal.Popup_();
        }

        public void ShowLogicModal()
        {
            _logicModal.Popup_();
        }

        public GoalItem AddGoal(string title, string description)
        {
            var goalItemScene = ResourceLoader.Load<PackedScene>("res://UI/GoalItem.tscn");
            var goal = goalItemScene.Instance() as GoalItem;
            var hBoxContainer = new HBoxContainer();
            hBoxContainer.SetAlignment(BoxContainer.AlignMode.End);

            hBoxContainer.AddChild(goal);

            _goalBox.AddChild(hBoxContainer);
            
            
            goal.SetTitle(title);
            goal.SetDescription(description);

            return goal;
        }

        public void RemoveGoal(GoalItem goal)
        {
            
        }

        public void SetOptionLabelText(string text)
        {
            _optionLabel.Text = text;
        }
    }
}