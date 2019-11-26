extends Control

var dragging = false
var offset

export var nodePath: NodePath

func _ready():
	pass

func _input(event):
	if event is InputEventMouseButton:
		if event.get_button_index() == 1 && get_rect().has_point(get_local_mouse_position()) && event.is_pressed():
			offset = get_local_mouse_position()
			dragging = true
		else:
			dragging = false

func _process(delta):
	if dragging:
		var controlNode = get_node(nodePath)
		controlNode.set_position(get_global_mouse_position() - offset)
