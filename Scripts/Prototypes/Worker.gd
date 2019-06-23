extends Spatial

const Grid = preload("res://Scripts/Grid/Grid.gd")
const Maths = preload("res://Scripts/Utils/Maths.gd")

var prev_coordinates: Vector3
var destination: Vector3
var grid: Grid
var id: String

func _ready():
	$Worker/AnimationPlayer.get_animation("Running").set_loop(true)
	$Worker/AnimationPlayer.play("Running")

func _process(delta):
	if destination:
		var difference_grid = (destination - prev_coordinates).normalized()
		var rot = Maths.get_rotation_from_vector(difference_grid)
		var degrees = rot * 57.2958
		set_rotation_degrees(Vector3(0, degrees, 0))
		var wcoords = grid.get_world_coords(destination.x, destination.z)
		var difference = Vector3(wcoords.x, 0, wcoords.z) - translation
		translation += difference * 0.1