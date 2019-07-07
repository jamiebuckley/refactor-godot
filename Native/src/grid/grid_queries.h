//
// Created by jamie on 30/06/19.
//
#include <vector>
#include <entities/grid_entity.h>
#include <common.h>
#include <algorithm>
#include "grid.h"

#ifndef REFACTOR_NATIVE_GRID_QUERIES_H
#define REFACTOR_NATIVE_GRID_QUERIES_H

namespace Refactor {
    class GridQueries {
    public:
        static std::vector<GridEntity *> query_type(Grid* grid, EntityType entity_type) {
          std::vector<GridEntity*> result;
          for(int x = 0; x < grid->getSize(); x++) {
            for(int z = 0; z < grid->getSize(); z++) {
              auto tile = grid->getInternalGrid()[x * grid->getSize() + z];
              auto entity_or_end = std::find_if(tile->entities.begin(), tile->entities.end(), [&](GridEntity* entity) { return entity->getEntityType() == entity_type; });
              if (entity_or_end != tile->entities.end()) {
                result.push_back(*entity_or_end);
              }
            }
          }
          return result;
        }
    };
}

#endif //REFACTOR_NATIVE_GRID_QUERIES_H
