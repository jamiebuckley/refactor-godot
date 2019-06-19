extends Spatial

var TILE_SIZE = 1

var pickerScene = preload("res://Picker.tscn")
var picker: Spatial;

# Called when the node enters the scene tree for the first time.
func _ready():
	picker = pickerScene.instance()
	add_child(picker)

func _process(delta):
	var mousePos = get_viewport().get_mouse_position()
	var ray_from = $Camera.project_ray_origin(mousePos)
	var ray_to = ray_from + $Camera.project_ray_normal(mousePos) * 200
	
	var space_state = get_world().direct_space_state
	var result = space_state.intersect_ray(ray_from, ray_to, [], 0x7FFFFFFF, true, true)
	if result:
		var position = result.position
		var xToTile = floor(position.x / TILE_SIZE) * TILE_SIZE  + (picker.scale.x / 2)
		var zToTile = floor(position.z / TILE_SIZE) * TILE_SIZE  + (picker.scale.z / 2)
		picker.visible = true
		picker.translation = Vector3(xToTile, position.y, zToTile)
	else:
		picker.visible = false
		
	
	