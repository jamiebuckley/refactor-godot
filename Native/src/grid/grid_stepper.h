//
// Created by jamie on 30/06/19.
//

#ifndef REFACTOR_NATIVE_GRID_STEPPER_H
#define REFACTOR_NATIVE_GRID_STEPPER_H

#include <grid_tile.h>
#include <algorithm>
#include <stack>
#include "grid.h"
#include <sstream>

bool isWorker(GridEntity* entity) {
  return entity->getEntityType() == EntityType::WORKER;
}

namespace Refactor {
    struct TempWorker;

    struct GridTileTemp {
        GridTile* grid_tile;
        std::vector<TempWorker*> next_entities;
    };

    struct TempWorker {
        GridEntity* entity;
        GridTileTemp* new_grid_tile;
        GridTileTemp* old_grid_tile;
    };


    class GridStepper {
    public:
        GridStepper(Grid* grid) {
          this->grid = grid;
        }

        void clear_grid(std::vector<GridTile *> &grid, std::vector<GridTileTemp *> &temp_grid,
                        std::vector<TempWorker *> &temp_workers, int size) {
          for (auto x = 0; x < size; x++) {
            for (auto z = 0; z < size; z++) {
              auto grid_tile = grid[x * size + z];
              auto grid_tile_temp = new GridTileTemp{
                      grid_tile
              };
              temp_grid.push_back(grid_tile_temp);


              // Remove all the workers
              std::vector < GridEntity * > workers;
              std::vector < GridEntity * > non_workers;

              int n = std::count_if(grid_tile->entities.begin(), grid_tile->entities.end(), isWorker);
              workers.resize(n);
              non_workers.resize(grid_tile->entities.size() - n);
              std::partition_copy(grid_tile->entities.begin(), grid_tile->entities.end(), workers.begin(),
                                  non_workers.begin(), isWorker);

              grid_tile->entities = non_workers;

              // Push the workers to a temp workers list
              std::for_each(workers.begin(), workers.end(), [&](GridEntity *entity) {
                  temp_workers.push_back(new TempWorker{
                          entity,
                          nullptr,
                          grid_tile_temp
                  });
              });

            }
          }
        }

        std::vector<GridEntity *> query_type(EntityType entity_type) {
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

        void step_workers() {
          std::vector < GridTileTemp * > temp_grid;
          std::vector < TempWorker * > temp_workers;

          // Clear all workers from the grid
          clear_grid(this->grid->getInternalGrid(), temp_grid, temp_workers, grid->getSize());

          for (TempWorker *worker : temp_workers) {
            int newX =
                    worker->old_grid_tile->grid_tile->x + static_cast<int>(floor(worker->entity->getOrientation().x));
            int newZ =
                    worker->old_grid_tile->grid_tile->z + static_cast<int>(floor(worker->entity->getOrientation().z));

            // if is off the map or can't move
            bool is_out_of_bounds = newX < 0 || newX >= grid->getSize() || newZ < 0 || newZ >= grid->getSize();
            bool is_blocked = !is_out_of_bounds && grid->is_blocked(newX, newZ);

            // stays on same tile
            if (is_out_of_bounds || is_blocked) {
              // this throws errors, not sure why
              //worker.new_grid_tile = worker.old_grid_tile;
              worker->new_grid_tile = worker->old_grid_tile;
              auto next_entities = worker->new_grid_tile->next_entities;
              next_entities.push_back(worker);

              worker->new_grid_tile->next_entities.push_back(worker);
            } else {
              auto next_tile = temp_grid[newX * grid->getSize() + newZ];
              worker->new_grid_tile = next_tile;
              next_tile->next_entities.push_back(worker);
            }
          }

          // check if any temp grid tiles are invalid and add them to stack
          std::stack < GridTileTemp * > invalid_stack;
          for (GridTileTemp *grid_tile : temp_grid) {
            if (grid_tile->next_entities.size() > 1) {
              invalid_stack.push(grid_tile);
            }
          }

          while (!invalid_stack.empty()) {
            GridTileTemp *tile = invalid_stack.top();
            invalid_stack.pop();

            std::vector < TempWorker * > cut;
            auto iterator = tile->next_entities.begin();
            while (iterator != tile->next_entities.end() && tile->next_entities.size() > 1) {
              auto next_entity = *iterator;
              if (next_entity->new_grid_tile != next_entity->old_grid_tile) {
                iterator = tile->next_entities.erase(iterator);
                cut.push_back(next_entity);
              } else {
                ++iterator;
              }
            }

            for (auto temp_worker : cut) {
              temp_worker->new_grid_tile = temp_worker->old_grid_tile;
              temp_worker->old_grid_tile->next_entities.push_back(temp_worker);
              if (temp_worker->old_grid_tile->next_entities.size() > 1)
                invalid_stack.push(temp_worker->old_grid_tile);
            }
          }

          // commit
          for (auto temp_worker : temp_workers) {
            auto grid_tile = temp_worker->new_grid_tile->grid_tile;
            grid_tile->entities.push_back(temp_worker->entity);
            temp_worker->entity->setGridTile(grid_tile);
            auto godot_worker = static_cast<godot::Spatial *>(temp_worker->entity->getGodotEntity());
            grid->getGodotInterface()->call(godot_worker, "set_destination", godot::Vector3(grid_tile->x, 0, grid_tile->z));
          }

          // clean up
          for (auto temp_worker : temp_workers) {
            delete temp_worker;
          }

          for (auto grid_tile : temp_grid) {
            delete grid_tile;
          }
        }

        void step_entrances() {
          std::vector < GridEntity * > entrances = query_type(EntityType::ENTRANCE);
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
                // create worker
              }
          });
        }

        void step() {
          step_workers();
          step_entrances();
        }

    private:
        Grid *grid;
    };
}

#endif //REFACTOR_NATIVE_GRID_STEPPER_H
