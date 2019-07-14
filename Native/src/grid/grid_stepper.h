//
// Created by jamie on 30/06/19.
//

#ifndef REFACTOR_NATIVE_GRID_STEPPER_H
#define REFACTOR_NATIVE_GRID_STEPPER_H

#include <grid_tile.h>
#include <algorithm>
#include <stack>
#include "grid.h"
#include "grid_queries.h"
#include "grid_worker_stepper.h"
#include <sstream>
#include <vector_util.h>

namespace Refactor {
    class GridStepper {
    public:
        explicit GridStepper(Grid* grid) {
          this->grid = grid;
        }

        void step_entrances() {
          std::vector < GridEntity * > entrances = GridQueries::query_type(grid, EntityType::ENTRANCE);
          std::for_each(entrances.begin(), entrances.end(), [&](GridEntity *entrance) {
              auto entrance_orientation = entrance->getOrientation();
              auto entrance_grid_tile = entrance->getGridTile();

              if (grid->is_blocked(entrance_grid_tile->x, entrance_grid_tile->z)) {
                std::ostringstream message;
                message << "Blocked entrance creating worker " << entrance_grid_tile->x << " " << entrance_grid_tile->z
                        << std::endl;
                grid->getGodotInterface()->print(message.str().c_str());
              } else {
                grid->getGodotInterface()->create_worker(entrance_grid_tile->x, entrance_grid_tile->z, entrance_orientation);
              }
          });
        }

        void step() {
          GridWorkerStepper::step_workers(this->grid);
          step_entrances();
        }

    private:
        Grid *grid;
    };
}

#endif //REFACTOR_NATIVE_GRID_STEPPER_H
