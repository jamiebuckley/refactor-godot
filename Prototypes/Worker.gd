extends Spatial

var destination: Vector3

func _ready():
	pass

func _process(delta):
	if destination:
		var difference = destination - translation
		translation += difference * 0.1;