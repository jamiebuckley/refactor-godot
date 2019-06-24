//
// Created by jamie on 24/06/19.
//

#ifndef REFACTOR_NATIVE_GRID_TILE_H
#define REFACTOR_NATIVE_GRID_TILE_H

#include "GridEntity.h"
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
    };
}


#endif //REFACTOR_NATIVE_GRID_TILE_H
