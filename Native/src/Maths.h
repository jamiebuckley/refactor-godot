//
// Created by jamie on 14/07/19.
//

#ifndef REFACTOR_NATIVE_MATHS_H
#define REFACTOR_NATIVE_MATHS_H

#include <core/Vector3.hpp>
#include <core/Vector2.hpp>
#include <gen/Camera.hpp>

namespace godot {
    class Maths {
    public:
        struct RayInfo {
            Vector3 ray_from;
            Vector3 ray_to;
        };

        static RayInfo get_camera_rays(Vector2 mouse_position, Camera* camera) {
            auto ray_from = camera->project_ray_origin(mouse_position);
            auto ray_to = ray_from + camera->project_ray_normal(mouse_position) * 200;
            return RayInfo { ray_from, ray_to };
        }
    };
}

#endif //REFACTOR_NATIVE_MATHS_H
