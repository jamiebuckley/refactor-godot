extends MarginContainer

var current_option_label: Label

var build_modal: HBoxContainer


func _ready():
	current_option_label = get_node("TopBar/TopBarHBar/CurrentOptionLabel")
	build_modal = get_node("TopBar/MarginContainer/BuildModal")
	for button in get_tree().get_nodes_in_group("BuildOptionButton"):
		button.connect("pressed", self, "_on_build_option_button_press", [button])

func _on_build_option_button_press(button):
	var button_name = button.text
	current_option_label.text = button_name
	build_modal.visible = false
