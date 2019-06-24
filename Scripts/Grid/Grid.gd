const TILE_SIZE = 1.0
const BuildOptionType = preload("res://Scripts/BuildOption.gd").BuildOptionType

var size = 0
var main

var workers = []
var native_grid;

func _init(size, main):
	self.size = size
	self.main = main
	native_grid = load("res://Native/refactor_grid_spatial.gdns").new(self)

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

func get_blocked(x, z):
	return native_grid.is_blocked(x, z)

func _add_worker(x, z, orientation):
	var workerWorldCoords = get_world_coords(x, z)
	var worker = main.add_worker(workerWorldCoords.x, workerWorldCoords.z, x, z, orientation)
	worker.destination = Vector3(x, 0, z)
	worker.prev_coordinates = worker.destination
	worker.grid = self
	workers.push_back(worker)
	return worker

func add_entity(x, z, orientation, type, entity):
	native_grid.add_entity(x, z, orientation, type, entity);


static func get_tile_position(position):
	var xToTile = floor(position.x / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2)
	var zToTile = floor(position.z / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2)
	return Vector3(xToTile, 0, zToTile)