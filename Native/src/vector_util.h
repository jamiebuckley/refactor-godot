//
// Created by jamie on 30/06/19.
//

#ifndef REFACTOR_NATIVE_VECTOR_UTIL_H
#define REFACTOR_NATIVE_VECTOR_UTIL_H

#include <core/Vector3.hpp>

namespace Refactor {
    struct Point3 {
    public:
        int x;
        int y;
        int z;

        Point3(int x, int y, int z) {
          this->x = x;
          this->y = y;
          this->z = z;
        }

        static Point3 from_vector3(godot::Vector3 vector3) {
          return {static_cast<int>(vector3.x), static_cast<int>(vector3.y), static_cast<int>(vector3.z)};
        }

        static Point3 add(Point3 a, Point3 b) {
          return {a.x + b.x, a.y + b.y, a.z + b.z};
        }
    };
}

#endif //REFACTOR_NATIVE_VECTOR_UTIL_H
