//
// Created by jamie on 24/06/19.
//

#ifndef REFACTOR_NATIVE_GRID_TILE_H
#define REFACTOR_NATIVE_GRID_TILE_H

#include "entities/grid_entity.h"
#include "vector_util.h"
#include <vector>

namespace Refactor {
    class GridEntity;

    struct GridTile {
        int x;
        int z;
        std::vector<GridEntity *> entities;

        GridTile(int x, int z) {
          this->x = x;
          this->z = z;
        }

        Point3 getPosition() {
          return Point3(this->x, 0, this->z);
        }
    };
}


#endif //REFACTOR_NATIVE_GRID_TILE_H
