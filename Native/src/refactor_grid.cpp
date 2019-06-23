#include "include/refactor_grid.h"
#include <Godot.hpp>
#include <string>
#include <stack>
#include <algorithm>

using namespace Refactor;

Grid::Grid(int size) {
  this->size = size;
  this->internal_grid.reserve(size * size);
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
  for(auto p: this->entity_map) {
    delete p.second;
  }
}

std::string Grid::add_entity(int x, int z, EntityType entity_type, godot::Vector3 orientation) {
  if(is_blocked(x, z)) {
    return nullptr;
  }
  auto id_string = std::to_string(this->last_number);
  auto padded_id_string = std::string().append(24 - id_string.length(), '0').append(id_string);
  this->last_number++;

  auto blocking = entity_type == EntityType::WORKER 
    || entity_type == EntityType::BLOCK
    || entity_type == EntityType::ENTRANCE
    || entity_type == EntityType::EXIT;

  GridEntity* gridEntity = new GridEntity(padded_id_string, blocking, orientation, entity_type);
  this->entity_map.insert(std::pair<std::string, GridEntity*>(padded_id_string, gridEntity));

  GridTile* tile = internal_grid[x * size + z];
  tile->entity = gridEntity;
  gridEntity->grid_tile = tile;

  return padded_id_string;
}

bool Grid::delete_entity(std::string id) {
  auto entity = entity_map[id];
  auto grid_tile = entity->grid_tile;
  grid_tile->entity = nullptr;
  entity_map.erase(id);
  delete entity;
  return true;
}

/**
 * Test if the grid tile at x,z contains a blocking entity
 */
bool Grid::is_blocked(int x, int z) {
  auto grid_tile = this->internal_grid[x * size + z];
  if (grid_tile->entity == nullptr) {
    return false;
  }

  return grid_tile->entity->is_blocking;
}

godot::Vector3 Grid::get_entity_coordinates(std::string entity_id) {
  auto entity = this->entity_map[entity_id];
  auto grid_tile = entity->grid_tile;
  return godot::Vector3(grid_tile->x, 0, grid_tile->z);
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

void clear_grid(std::vector<GridTile*>& grid, std::vector<GridTileTemp*>& temp_grid, std::vector<TempWorker*>& temp_workers, int size) {
  for(auto x = 0; x < size; x++) {
    for(auto z = 0; z < size; z++) {
      auto grid_tile = grid[x * size + z];
      auto grid_tile_temp = new GridTileTemp {
        grid_tile
      };
      temp_grid.push_back(grid_tile_temp);

      if (grid_tile->entity != nullptr && grid_tile->entity->entity_type == EntityType::WORKER) {
        temp_workers.push_back(new TempWorker {
          grid_tile->entity,
          nullptr,
          grid_tile_temp
        });
        grid_tile->entity = nullptr;
      }
    }
  }
}

void Grid::step() {
  std::vector<GridTileTemp*> temp_grid;
  std::vector<TempWorker*> temp_workers;

  // Clear all workers from the grid
  clear_grid(this->internal_grid, temp_grid, temp_workers, size);

  for(TempWorker* worker : temp_workers) {
    int newX = worker->old_grid_tile->grid_tile->x + worker->entity->orientation.x;
    int newZ = worker->old_grid_tile->grid_tile->z + worker->entity->orientation.z;

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
    std::vector<TempWorker*> cut;
    std::copy_if (tile->next_entities.begin(), tile->next_entities.end(), std::back_inserter(cut), [](TempWorker* tw){return tw->new_grid_tile != tw->old_grid_tile; } );

    tile->next_entities.erase(tile->next_entities.begin() + 1, tile->next_entities.end());
    for(auto temp_worker : cut) {
      temp_worker->new_grid_tile = temp_worker->old_grid_tile;
      temp_worker->old_grid_tile->next_entities.push_back(temp_worker);
      if(temp_worker->old_grid_tile->next_entities.size() > 1) 
        invalid_stack.push(temp_worker->old_grid_tile);
    }
    invalid_stack.pop();
  }

  // commit
  for(auto temp_worker : temp_workers) {
    temp_worker->entity->grid_tile = temp_worker->new_grid_tile->grid_tile;
    temp_worker->entity->grid_tile->entity = temp_worker->entity;
  }

  // clean up
  for(auto temp_worker : temp_workers) {
    delete temp_worker;
  }

  for(auto grid_tile : temp_grid) {
    delete grid_tile;
  }
}