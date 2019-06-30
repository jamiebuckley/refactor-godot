#include "grid.h"
#include "vector_util.h"
#include <Godot.hpp>
#include <string>
#include <stack>
#include <algorithm>
#include <entities/entrance.h>
#include <entities/worker.h>
#include <sstream>

using namespace Refactor;

Grid::Grid(int size, GodotInterface* godot_interface) {
  this->size = size;
  this->internal_grid.reserve(size * size);
  this->godot_interface = godot_interface;

  for(int x = 0; x < size; x++) {
    for(int z = 0; z < size; z++) {
      this->internal_grid[x * size + z] = new GridTile(x, z);
    }
  }
}

Grid::~Grid() {
  for(auto p: this->internal_grid) {
    delete p;
  }
  for(const auto& p: this->entity_map) {
    delete p.second;
  }
}

std::string Grid::add_entity(int x, int z, godot::Vector3 orientation, EntityType entity_type, godot::Spatial* variant) {
  if(is_blocked(x, z)) {
    return "error_is_blocked";
  }
  auto id_string = std::to_string(this->last_number);
  auto padded_id_string = std::string().append(24 - id_string.length(), '0').append(id_string);
  this->last_number++;

  auto blocking = entity_type == EntityType::WORKER 
    || entity_type == EntityType::BLOCK
    || entity_type == EntityType::ENTRANCE
    || entity_type == EntityType::EXIT;

  GridEntity* grid_entity;
  if (entity_type == EntityType::ENTRANCE) {
    grid_entity = new Entrance(padded_id_string, orientation, variant);
  }
  else if (entity_type == EntityType::WORKER) {
    grid_entity = new Worker(padded_id_string, orientation, variant);
  }
  else {
    grid_entity = new GridEntity(padded_id_string, blocking, orientation, entity_type, variant);
  }
  this->entity_map.insert(std::pair<std::string, GridEntity*>(padded_id_string, grid_entity));

  GridTile* tile = internal_grid[x * size + z];
  tile->entities.push_back(grid_entity);
  grid_entity->setGridTile(tile);

  return padded_id_string;
}

bool Grid::delete_entity(const std::string& id) {
  auto entity = entity_map[id];
  auto grid_tile = entity->getGridTile();

  auto ets = grid_tile->entities;
  ets.erase(std::remove(ets.begin(), ets.end(), entity));
  entity_map.erase(id);
  delete entity;
  return true;
}

/**
 * Test if the grid tile at x,z contains a blocking entity
 */
bool Grid::is_blocked(int x, int z) {
  if (x < 0 || x >= size || z < 0 || z >= size) {
    return true;
  }
  auto grid_tile = this->internal_grid[x * size + z];
  return std::any_of(grid_tile->entities.begin(), grid_tile->entities.end(), [](GridEntity* entity){
    return entity->isBlocking();
  });
}

godot::Vector3 Grid::get_entity_coordinates(const std::string& entity_id) {
  auto entity = this->entity_map[entity_id];
  auto grid_tile = entity->getGridTile();
  return {static_cast<real_t>(grid_tile->x), 0, static_cast<real_t>(grid_tile->z)};
}

void Grid::step() {
  step_workers();
  step_entrances();
}

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

bool isWorker(GridEntity* entity) {
  return entity->getEntityType() == EntityType::WORKER;
}

void clear_grid(std::vector<GridTile*>& grid, std::vector<GridTileTemp*>& temp_grid, std::vector<TempWorker*>& temp_workers, int size) {
  for(auto x = 0; x < size; x++) {
    for(auto z = 0; z < size; z++) {
      auto grid_tile = grid[x * size + z];
      auto grid_tile_temp = new GridTileTemp {
        grid_tile
      };
      temp_grid.push_back(grid_tile_temp);


      // Remove all the workers
      std::vector<GridEntity*> workers;
      std::vector<GridEntity*> non_workers;
      
      int n = std::count_if(grid_tile->entities.begin(), grid_tile->entities.end(), isWorker);
      workers.resize(n);
      non_workers.resize(grid_tile->entities.size() - n);
      std::partition_copy(grid_tile->entities.begin(), grid_tile->entities.end(), workers.begin(), non_workers.begin(), isWorker);
      
      grid_tile->entities = non_workers;

      // Push the workers to a temp workers list
      std::for_each(workers.begin(), workers.end(), [&](GridEntity* entity) {
          temp_workers.push_back(new TempWorker {
            entity,
            nullptr,
            grid_tile_temp
          });
      });

    }
  }
}

void Grid::step_workers() {
std::vector<GridTileTemp*> temp_grid;
  std::vector<TempWorker*> temp_workers;

  // Clear all workers from the grid
  clear_grid(this->internal_grid, temp_grid, temp_workers, size);

  for(TempWorker* worker : temp_workers) {
    int newX = worker->old_grid_tile->grid_tile->x + static_cast<int>(floor(worker->entity->getOrientation().x));
    int newZ = worker->old_grid_tile->grid_tile->z + static_cast<int>(floor(worker->entity->getOrientation().z));

    // if is off the map or can't move
    bool is_out_of_bounds = newX < 0 || newX >= size || newZ < 0 || newZ >= size;
    bool is_blocked = !is_out_of_bounds && this->is_blocked(newX, newZ);

    // stays on same tile
    if(is_out_of_bounds || is_blocked) {
      // this throws errors, not sure why
      //worker.new_grid_tile = worker.old_grid_tile;
      worker->new_grid_tile = worker->old_grid_tile;
      auto next_entities = worker->new_grid_tile->next_entities;
      next_entities.push_back(worker);

      worker->new_grid_tile->next_entities.push_back(worker);
    } else {
      auto next_tile = temp_grid[newX * size + newZ];
      worker->new_grid_tile = next_tile;
      next_tile->next_entities.push_back(worker);
    }
  }

  // check if any temp grid tiles are invalid and add them to stack
  std::stack<GridTileTemp*> invalid_stack;
  for(GridTileTemp* grid_tile : temp_grid) {
    if(grid_tile->next_entities.size() > 1) {
      invalid_stack.push(grid_tile);
    }
  }

  while(!invalid_stack.empty()) {
    GridTileTemp* tile = invalid_stack.top();
    invalid_stack.pop();

    std::vector<TempWorker*> cut;
    auto iterator = tile->next_entities.begin();
    while(iterator != tile->next_entities.end() && tile->next_entities.size() > 1) {
      auto next_entity = *iterator;
      if(next_entity->new_grid_tile != next_entity->old_grid_tile) {
        iterator = tile->next_entities.erase(iterator);
        cut.push_back(next_entity);
      } else {
        ++iterator;
      }
    }

    for(auto temp_worker : cut) {
      temp_worker->new_grid_tile = temp_worker->old_grid_tile;
      temp_worker->old_grid_tile->next_entities.push_back(temp_worker);
      if(temp_worker->old_grid_tile->next_entities.size() > 1) 
        invalid_stack.push(temp_worker->old_grid_tile);
    }
  }

  // commit
  for(auto temp_worker : temp_workers) {
    auto grid_tile = temp_worker->new_grid_tile->grid_tile;
    grid_tile->entities.push_back(temp_worker->entity);
    temp_worker->entity->setGridTile(grid_tile);
    auto godot_worker = static_cast<godot::Spatial *>(temp_worker->entity->getGodotEntity());
    godot_interface->call(godot_worker, "set_destination", godot::Vector3(grid_tile->x, 0, grid_tile->z));
  }

  // clean up
  for(auto temp_worker : temp_workers) {
    delete temp_worker;
  }

  for(auto grid_tile : temp_grid) {
    delete grid_tile;
  }
}

void Grid::step_entrances() {
  std::vector<GridEntity*> entrances = query_type(EntityType::ENTRANCE);
  std::for_each(entrances.begin(), entrances.end(), [&](GridEntity* entrance) {
    auto entrance_orientation = entrance->getOrientation();
    auto entrance_grid_tile = entrance->getGridTile();

    if(is_blocked(entrance_grid_tile->x, entrance_grid_tile->z)) {
      std::ostringstream message;
      message << "Blocked entrance creating worker " << entrance_grid_tile->x << " " << entrance_grid_tile->z << std::endl;
      godot_interface->print(message.str().c_str());
    } else {
      godot_interface->create_worker(entrance_grid_tile->x, entrance_grid_tile->z, entrance_orientation);
      // create worker
    }
  });
}

std::vector<GridEntity *> Grid::query_type(EntityType entity_type) {
    std::vector<GridEntity*> result;
    for(int x = 0; x < size; x++) {
        for(int z = 0; z < size; z++) {
            auto tile = internal_grid[x * size + z];
            auto entity_or_end = std::find_if(tile->entities.begin(), tile->entities.end(), [&](GridEntity* entity) { return entity->getEntityType() == entity_type; });
            if (entity_or_end != tile->entities.end()) {
                result.push_back(*entity_or_end);
            }
        }
    }
    return result;
}
