const Maths = preload("res://Scripts/Utils/Maths.gd")
const BuildOptionType = preload("res://Scripts/BuildOption.gd").BuildOptionType

class GridTile:
	var is_blocked = false
	var instance

var size = 0
var main
var grid = []

func _init(size, main):
	self.size = size
	self.main = main
	grid.resize(size * size)
	for x in range(0, size):
		for z in range(0, size):
			grid[x * size + z] = GridTile.new()

func get_blocked(x, z):
	return grid[x * size + z].is_blocked

func set_blocked(x, z, instance):
	var index = x * size + z
	grid[index].is_blocked = true
	grid[index].instance = instance

func get_grid_coords(worldX, worldZ):
	var offsetWorldX = worldX + (size * Maths.TILE_SIZE) / 2;
	var offsetWorldZ = worldZ + (size * Maths.TILE_SIZE) / 2;
	return {
		x = floor(offsetWorldX / Maths.TILE_SIZE),
		z = floor(offsetWorldZ / Maths.TILE_SIZE)
	}

func get_world_coords(gridX, gridZ):
	return {
		x = (gridX * Maths.TILE_SIZE) - (size * Maths.TILE_SIZE) / 2 + (Maths.TILE_SIZE / 2),
		z = (gridZ * Maths.TILE_SIZE) - (size * Maths.TILE_SIZE) / 2 + (Maths.TILE_SIZE / 2)
	}

func handle_pulse():
	for x in range(0, size):
		for z in range(0, size):
			handle_tile_pulse(x, z)

func handle_tile_pulse(x, z):
	var grid_tile = grid[x * size + z]
	if !grid_tile.instance:
		return
	
	var instance = grid_tile.instance
	if !instance.has_meta("build_option"):
		return

	if instance.get_meta("build_option") == BuildOptionType.ENTRANCE:
		var workerWorldCoords = get_world_coords(x + 1, z)
		main.add_worker(workerWorldCoords.x, workerWorldCoords.z)