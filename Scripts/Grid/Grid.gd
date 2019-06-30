const TILE_SIZE = 1.0
var size = 0

#var workers = []

func _init(size):
	self.size = size

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


static func get_tile_position(position):
	var xToTile = floor(position.x / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2)
	var zToTile = floor(position.z / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2)
	return Vector3(xToTile, 0, zToTile)