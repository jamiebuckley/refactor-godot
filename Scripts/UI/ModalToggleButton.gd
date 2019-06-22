extends Button

export(NodePath) var modal_path

var modal: Node

# Called when the node enters the scene tree for the first time.
func _ready():
    connect("pressed", self, "_on_pressed")
    modal = get_node(modal_path)


func _on_pressed():
    modal.visible = !modal.visible