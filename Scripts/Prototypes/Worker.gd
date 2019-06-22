extends Spatial

const Grid = preload("res://Scripts/Grid/Grid.gd")

var destination: Vector3
var grid: Grid

func _ready():
	$Worker/AnimationPlayer.play("Running")

func _process(delta):
	if destination:
		var wcoords = grid.get_world_coords(destination.x, destination.z)
		var difference = Vector3(wcoords.x, 0, wcoords.z) - translation
		translation += difference * 0.1