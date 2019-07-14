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
	grid = Grid.new(20)
	add_child(native_grid)

func _process(delta):
	_handle_time(delta)

func _unhandled_input(event):
	if event is InputEventMouseButton && event.pressed:
		_handle_mouse_click(event)
	if event is InputEventKey && event.scancode == KEY_ESCAPE:
		if buildOption != null:
			buildOption = null
			$UI.current_option_label.text = ""
		
func _handle_mouse_click(event: InputEventMouseButton):
	var result = _get_world_mouse_position()
	if result:
		var grid_coords = grid.get_grid_coords(picker.translation.x, picker.translation.z)
		_handle_grid_coords_click(grid_coords)

func _handle_grid_coords_click(grid_coords):
	print(str(grid_coords.x) + " " + str(grid_coords.z))
	if buildOption != null:
		if !native_grid.is_blocked(grid_coords.x, grid_coords.z):
			if buildOption == BuildOptionType.ENTRANCE || buildOption == BuildOptionType.EXIT:
				_handle_build_exit_entrance(grid_coords, buildOption == BuildOptionType.EXIT)
			else:
				var instance = _get_build_option_instance()
				instance.set_meta("build_option", buildOption)
				instance.translation = picker.translation
				add_child(instance)

#
# Build Entrances and Exits
#
func _handle_build_exit_entrance(grid_coords, is_exit):
	var minAxis = 0;
	var maxAxis = 19;
	var validX = (grid_coords.x == minAxis || grid_coords.x == maxAxis) && (grid_coords.z > minAxis && grid_coords.z < maxAxis)
	var validZ = (grid_coords.z == minAxis || grid_coords.z == maxAxis) && (grid_coords.x > minAxis && grid_coords.x < maxAxis)
	if !validX && !validZ:
		return

	var instance = _get_build_option_instance()
	instance.set_meta("build_option", buildOption)
	instance.translation = picker.translation
	instance.orientation = Maths.get_edge_orientation(grid_coords.x, grid_coords.z, minAxis, maxAxis)
	native_grid.add_entity(grid_coords.x, grid_coords.z, instance.orientation, "Entrance", instance)
	instance.rotate(Vector3(0, 1, 0), Maths.get_rotation_from_vector(instance.orientation))
	add_child(instance)

func _get_build_option_instance():
	if buildOption == BuildOptionType.ENTRANCE:
		return EntranceScene.instance()
	elif buildOption == BuildOptionType.EXIT:
		return ExitScene.instance()
	elif buildOption == BuildOptionType.DIRECTIONAL_TILE:
		return DirectionalTileScene.instance() 
	else:
		print("Nothing here")


func _on_build_option_button_press(button):
	buildOption = BuildOption.get_build_option_from_button_name(button.name)

func _get_world_mouse_position():
	var rays = Maths.get_camera_rays(get_viewport().get_mouse_position(), $Camera)
	var space_state = get_world().direct_space_state
	var result = space_state.intersect_ray(rays.ray_from, rays.ray_to, [], 0x7FFFFFFF, true, true)
	return result
	
func _handle_time(delta):
	pulseTimer += delta
	if pulseTimer > 1.0:
		# pulse
		native_grid.step()
		pulseTimer = fmod(pulseTimer, 1.0)

# Comes from native
func add_worker(x, z, orientation):
	print("adding worker")
	var workerWorldCoords = grid.get_world_coords(x, z)

	var worker = WorkerScene.instance()
	worker.translation.x = workerWorldCoords.x
	worker.translation.z = workerWorldCoords.z
	add_child(worker)

	worker.destination = Vector3(x, 0, z)
	worker.grid = grid
	return worker

# Comes from game
func add_entrance(worldX, worldZ, x, z, orientation):
	var entrance = EntranceScene.instance()
	entrance.translation.x = worldX
	entrance.translation.z = worldZ
	add_child(entrance)
	return entrance