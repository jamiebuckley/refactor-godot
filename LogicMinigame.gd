extends Viewport

# Declare member variables here. Examples:
# var a = 2
# var b = "text"

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
func _input(event):
	unhandled_input(event)

func _on_LogicDialog_modal_closed():
	print("disabling input")
	set_disable_input(true)


func _on_LogicDialog_about_to_show():
	print("enabling input")
	set_disable_input(false)


func _on_LogicDialog_visibility_changed():
	pass # Replace with function body.
