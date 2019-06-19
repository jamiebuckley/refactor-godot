extends Spatial

var BuildOptionType = preload("res://Scripts/BuildOption.gd").BuildOptionType

var TILE_SIZE = 1
var pickerScene = preload("res://Picker.tscn")
var picker: Spatial;
var buildOption;

# Called when the node enters the scene tree for the first time.
func _ready():
	picker = pickerScene.instance()
	add_child(picker)
	for button in $UI.get_tree().get_nodes_in_group("BuildOptionButton"):
		button.connect("pressed", self, "_on_build_option_button_press", [button])

func _process(delta):
	_place_picker()

func _unhandled_input(event):
	if event is InputEventMouseButton:
		_handleMouseClick()
		
func _handleMouseClick():
	print_debug("Clicked")
	pass

func _on_build_option_button_press(button):
	if button.name == "BOptDirectionalTileButton":
		buildOption = BuildOptionType.DIRECTIONAL_TILE
	elif button.name == "BOptEntranceButton":
		buildOption = BuildOptionType.ENTRANCE
	elif button.name == "BOptExitButton":
		buildOption = BuildOptionType.EXIT
	
func _place_picker():
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
		
	
	