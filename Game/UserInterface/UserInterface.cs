using Godot;
using Godot.Collections;
using Refactor1.Game.Common;
using Refactor1.Game.Events;
using Refactor1.Game.Logic;

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

        public Control BuildModal { get; private set; }

        public Control LogicModal { get; private set; }

        public Label OptionLabel { get; private set; }

        private VBoxContainer _goalBox;
        
        public override void _Ready()
        {
            OptionLabel = GetNode(OptionLabelPath) as Label;
            BuildModal = GetNode(BuildModalPath) as Control  ;
            LogicModal = GetNode(LogicModalPath) as Control;
            _goalBox = GetNode(GoalBoxPath) as VBoxContainer;

            GameEvents.Instance.SubscribeTo(typeof(GoalCompletedEvent), evt =>
            {
                GoalCompletedEvent goalCompletedEvent = (GoalCompletedEvent) evt;
                _goalBox.RemoveChild(goalCompletedEvent.Goal.GoalItem.GetParent());
            });
            
            foreach (Button button in GetTree().GetNodesInGroup("BuildOptionButton"))
            {
                button.Connect("pressed", this, "OnBuildOptionButtonPress", new Array {button});
            }

            GetNode(BuildModalButtonPath).Connect("pressed", this, "OnBuildModalButtonPressed");
        }


        public void OnBuildOptionButtonPress(Button button)
        {
            OptionLabel.Text = button.Text;
            BuildModal.Hide();
        }

        public void OnBuildModalButtonPressed()
        {
            BuildModal.Show();
        }

        public void ShowLogicModal()
        {
            LogicModal.Visible = true;
        }
        
        public void SetLogicModalCoordinates(Point2D gridCoords)
        {
            var logicEditor = (LogicEditor) LogicModal.FindNode("LogicEditorArea");
            logicEditor.Coordinates = gridCoords;
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
            OptionLabel.Text = text;
        }
    }
}