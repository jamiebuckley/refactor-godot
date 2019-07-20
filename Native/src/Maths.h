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

        static float get_rotation_from_vector(Vector3 vector) {
          // north
          if (vector.x == 0 && vector.z == 1) {
            return 0.0f;
          }
          else if(vector.x == 1 && vector.z == 0) {
            return Math_PI / 2.0f;
          }
          else if (vector.x == 0 && vector.z == -1) {
            return Math_PI;
          }
          else if (vector.x == -1 && vector.z == 0) {
            return - Math_PI / 2;
          }
          return 0;
        }

        static Vector3 get_edge_orientation(int x, int z, int min_val, int max_val) {
          Vector3 result;
          if(x == min_val) {
            result.x = 1;
          }
          if (x == max_val) {
            result.x = -1;
          }
          if (z == min_val) {
            result.z = 1;
          }
          if (z == max_val) {
            result.z = -1;
          }
          return result;
        }
    };
}

#endif //REFACTOR_NATIVE_MATHS_H
