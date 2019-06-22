const TILE_SIZE = 1.0

static func get_camera_rays(mouse_position, camera: Camera):
    var ray_from = camera.project_ray_origin(mouse_position)
    var ray_to = ray_from + camera.project_ray_normal(mouse_position) * 200
    return {
        ray_from = ray_from,
        ray_to = ray_to
    }

static func get_tile_position(position):
    var xToTile = floor(position.x / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2)
    var zToTile = floor(position.z / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2)
    return Vector3(xToTile, 0, zToTile)