//
// Created by jamie on 13/07/19.
//

#ifndef REFACTOR_NATIVE_GRID_WORKER_STEPPER_H
#define REFACTOR_NATIVE_GRID_WORKER_STEPPER_H


#include <memory>
#include "godot_entities/godot_worker.h"

namespace Refactor {

    bool isWorker(GridEntity* entity) {
      return entity->getEntityType() == EntityType::WORKER;
    }

    class GridWorkerStepper {

        struct TempWorker;

        struct GridTileTemp {
            GridTile *grid_tile;
            std::vector<std::shared_ptr<TempWorker>> next_entities;
        };

        struct TempWorker {
            GridEntity *real_entity;
            GridTileTemp *new_grid_tile;
            GridTileTemp *old_grid_tile;
        };

    public:
        static void clear_grid(std::vector<GridTile *> &grid, int grid_size,
                               std::vector<std::shared_ptr<TempWorker>> &temp_workers,
                               std::vector<std::shared_ptr<GridTileTemp>> &temp_grid) {

          for (auto x = 0; x < grid_size; x++) {
            for (auto z = 0; z < grid_size; z++) {
              GridTile *grid_tile = grid[x * grid_size + z];
              auto grid_tile_temp = std::make_shared<GridTileTemp>();
              grid_tile_temp->grid_tile = grid_tile;
              temp_grid.push_back(grid_tile_temp);

              // Remove all the workers
              std::vector<GridEntity *> workers;
              std::vector<GridEntity *> non_workers;

              int number_of_workers = std::count_if(grid_tile->entities.begin(), grid_tile->entities.end(), isWorker);
              workers.resize(number_of_workers);
              non_workers.resize(grid_tile->entities.size() - number_of_workers);
              std::partition_copy(grid_tile->entities.begin(), grid_tile->entities.end(), workers.begin(),
                                  non_workers.begin(), isWorker);
              grid_tile->entities = non_workers;

              // Push the workers to a temp workers list
              std::for_each(workers.begin(), workers.end(), [&](GridEntity *real_entity) {
                  temp_workers.push_back(std::make_shared<TempWorker>(TempWorker{
                          real_entity,
                          nullptr,
                          grid_tile_temp.get()
                  }));
              });
            }
          }
        }


        static void step_workers(Grid *grid) {
          auto temp_workers = std::vector<std::shared_ptr<TempWorker>>();
          auto temp_grid = std::vector<std::shared_ptr<GridTileTemp>>();

          // clear all workers from the grid
          clear_grid(grid->getInternalGrid(), grid->getSize(), temp_workers, temp_grid);

          for (const auto &temp_worker : temp_workers) {
            auto orientation_pt3 = Point3::from_vector3(temp_worker->real_entity->getOrientation());
            auto worker_pt3 = temp_worker->old_grid_tile->grid_tile->getPosition();
            auto new_position = Point3::add(orientation_pt3, worker_pt3);

            // if is off the map or can't move
            bool is_out_of_bounds = !grid->is_in_bounds(new_position.x, new_position.z);
            bool is_blocked = !is_out_of_bounds && grid->is_blocked(new_position.x, new_position.z);

            // stays on same tile
            if (is_out_of_bounds || is_blocked) {
              // this throws errors, not sure why
              // worker.new_grid_tile = worker.old_grid_tile;
              temp_worker->new_grid_tile = temp_worker->old_grid_tile;
              auto next_entities = temp_worker->new_grid_tile->next_entities;
              next_entities.push_back(temp_worker);
              temp_worker->new_grid_tile->next_entities.push_back(temp_worker);
            } else {
              auto next_tile = temp_grid[new_position.x * grid->getSize() + new_position.z];
              temp_worker->new_grid_tile = next_tile.get();
              next_tile->next_entities.push_back(temp_worker);
            }
          }

          // check if any temp grid tiles are invalid and add them to stack
          std::stack<GridTileTemp *> invalid_stack;
          for (const auto &grid_tile : temp_grid) {
            if (grid_tile->next_entities.size() > 1) {
              invalid_stack.push(grid_tile.get());
            }
          }

          while (!invalid_stack.empty()) {
            auto tile = invalid_stack.top();
            invalid_stack.pop();

            // if there are more than 1 entities on a tile
            // make one a winner, and move the losers back to their old tile
            std::vector<std::shared_ptr<TempWorker>> cut;
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

            // move the losers back to their old tile
            for (const auto &temp_worker : cut) {
              temp_worker->new_grid_tile = temp_worker->old_grid_tile;
              temp_worker->old_grid_tile->next_entities.push_back(temp_worker);

              // if that caused multiple workers on the old tile, mark that tile as invalid
              if (temp_worker->old_grid_tile->next_entities.size() > 1) {
                invalid_stack.push(temp_worker->old_grid_tile);
              }
            }
          }

          // commit
          for (const auto &temp_worker : temp_workers) {
            auto grid_tile = temp_worker->new_grid_tile->grid_tile;
            grid_tile->entities.push_back(temp_worker->real_entity);
            temp_worker->real_entity->setGridTile(grid_tile);

            auto godot_entity = temp_worker->real_entity->getGodotEntity();
            auto godot_worker = static_cast<godot::GodotWorker *>(godot_entity);
            godot_worker->set_destination(godot::Vector3(grid_tile->x, 0, grid_tile->z));
          }
        }
    };
}

#endif //REFACTOR_NATIVE_GRID_WORKER_STEPPER_H
