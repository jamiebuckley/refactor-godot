const TILE_SIZE = 1.0
const BuildOptionType = preload("res://Scripts/BuildOption.gd").BuildOptionType

class GridTile:
	var is_blocked = false
	var instance

var size = 0
var main
var grid = []
var workers = []

var native_grid;

func _init(size, main):
	self.size = size
	self.main = main
	native_grid = load("res://Native/refactor_grid_spatial.gdns").new()
	grid.resize(size * size)
	for x in range(0, size):
		for z in range(0, size):
			grid[x * size + z] = GridTile.new()

func get_blocked(x, z):
	if x < 0 || x >= size || z < 0 || z >= size:
		return false
	return grid[x * size + z].is_blocked

func set_blocked(x, z, instance):
	if x < 0 || x >= size || z < 0 || z >= size:
		return
	
	var index = x * size + z
	grid[index].is_blocked = true
	grid[index].instance = instance

func get_grid_coords(worldX, worldZ):
	var offsetWorldX = worldX + (size * TILE_SIZE) / 2;
	var offsetWorldZ = worldZ + (size * TILE_SIZE) / 2;
	return {
		x = floor(offsetWorldX / TILE_SIZE),
		z = floor(offsetWorldZ / TILE_SIZE)
	}

func get_world_coords(gridX, gridZ):
	return {
		x = (gridX * TILE_SIZE) - (size * TILE_SIZE) / 2 + (TILE_SIZE / 2),
		z = (gridZ * TILE_SIZE) - (size * TILE_SIZE) / 2 + (TILE_SIZE / 2)
	}

func handle_pulse():
	native_grid.step()
	for worker in workers:
		worker.prev_coordinates = worker.destination
		worker.destination = native_grid.get_entity_coordinates(worker.id)
	
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
		if instance.count == 0:
			return
		if !native_grid.is_blocked(x, z):
			var workerWorldCoords = get_world_coords(x, z)
			var worker = main.add_worker(workerWorldCoords.x, workerWorldCoords.z, x, z, instance.orientation)
			if worker != null:
				worker.id = native_grid.add_entity(x, z, "Worker", instance.orientation)
				worker.destination = Vector3(x, 0, z)
				worker.prev_coordinates = worker.destination
				worker.grid = self
				instance.count -= 1
				workers.push_back(worker)

static func get_tile_position(position):
	var xToTile = floor(position.x / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2)
	var zToTile = floor(position.z / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2)
	return Vector3(xToTile, 0, zToTile)