extends Control

var current_option_label: Label

export var build_modal_button_path: NodePath;

export var build_modal_path: NodePath;
var build_modal: WindowDialog

export var logic_modal_path: NodePath;

export var option_label_path: NodePath;
var option_label: Label;


func _ready():
	option_label = get_node(option_label_path);
	build_modal = get_node(build_modal_path)
	for button in get_tree().get_nodes_in_group("BuildOptionButton"):
		button.connect("pressed", self, "_on_build_option_button_press", [button])
	
	get_node(build_modal_button_path).connect("pressed", self, "on_build_modal_button_pressed")
	#show_logic_modal()

func _on_build_option_button_press(button):
	var button_name = button.text
	option_label.text = button_name
	build_modal.hide()
	
func on_build_modal_button_pressed():
	build_modal.popup()

func show_logic_modal():
	get_node(logic_modal_path).popup()
