extends Control

var dragging = false
var offset

export var nodePath: NodePath

func _ready():
	pass

func _input(event):
	if event is InputEventMouseButton:
		if event.get_button_index() == 1:
			dragging = !dragging
			offset = get_local_mouse_position()

func _process(delta):
	if dragging:
		var controlNode = get_node(nodePath)
		controlNode.set_position(get_global_mouse_position() - offset)