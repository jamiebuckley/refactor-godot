extends Spatial

const BuildOption = preload("res://Scripts/BuildOption.gd")
const BuildOptionType = BuildOption.BuildOptionType

var Maths = preload("res://Scripts/Utils/Maths.gd")
const Grid = preload("res://Scripts/Grid/Grid.gd")

var EntranceScene = preload("res://Prototypes/Entrance.tscn")
var ExitScene = preload("res://Prototypes/Exit.tscn")
var DirectionalTileScene = preload("res://Prototypes/DirectionalTile.tscn")
var WorkerScene = preload("res://Prototypes/Worker.tscn")

var picker: Spatial;
var buildOption;
var grid;

var pulseTimer = 0.0;

var native_grid;

# Called when the node enters the scene tree for the first time.
func _ready():
	native_grid = load("res://Native/refactor_grid_spatial.gdns").new()
	native_grid.set_main_entity(self)
	add_child(native_grid)

func _process(delta):
	pass

func clear_current_option_label():
	$UI.option_label.text = ""

func _get_world_mouse_position():
	var rays = Maths.get_camera_rays(get_viewport().get_mouse_position(), $Camera)
	var space_state = get_world().direct_space_state
	var result = space_state.intersect_ray(rays.ray_from, rays.ray_to, [], 0x7FFFFFFF, true, true)
	return result
