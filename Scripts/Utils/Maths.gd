static func get_camera_rays(mouse_position, camera: Camera):
    var ray_from = camera.project_ray_origin(mouse_position)
    var ray_to = ray_from + camera.project_ray_normal(mouse_position) * 200
    return {
        ray_from = ray_from,
        ray_to = ray_to
    }

static func get_edge_orientation(x, z, minVal, maxVal):
    var vecX = 0
    var vecZ = 0
    if x == minVal:
        vecX = 1;
    if x == maxVal:
        vecX = -1
    if z == minVal:
        vecZ = 1
    if z == maxVal:
        vecZ = -1
    return Vector3(vecX, 0, vecZ)

static func get_rotation_from_vector(vector):
    if vector.x == 0 && vector.z == 1:
        #north
        print("north")
        return 0
    elif vector.x == 1 && vector.z == 0:
        #north
        print("east")
        return PI / 2
    elif vector.x == 0 && vector.z == -1:
        #north
        print("south")
        return PI
    elif vector.x == -1 && vector.z == 0:
        #north
        print("west")
        return - PI / 2
    else:
        return 0