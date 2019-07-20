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
        static std::vector<std::weak_ptr<GridEntity>> query_type(Grid* grid, EntityType entity_type) {
          std::vector<std::weak_ptr<GridEntity>> result;
          for(int x = 0; x < grid->getSize(); x++) {
            for(int z = 0; z < grid->getSize(); z++) {
              auto tile = grid->getInternalGrid()[x * grid->getSize() + z];
              auto entity_or_end = std::find_if(tile->entities.begin(), tile->entities.end(), [&](std::shared_ptr<GridEntity> entity) { return entity->getEntityType() == entity_type; });
              if (entity_or_end != tile->entities.end()) {
                auto grid_entity = *entity_or_end;
                result.push_back(grid_entity);
              }
            }
          }
          return result;
        }
    };
}

#endif //REFACTOR_NATIVE_GRID_QUERIES_H
