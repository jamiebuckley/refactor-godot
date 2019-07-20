#include "grid.h"
#include "grid_stepper.h"
#include "vector_util.h"
#include <Godot.hpp>
#include <string>
#include <stack>
#include <algorithm>
#include <entities/entrance.h>
#include <entities/worker.h>
#include <sstream>
#include <unordered_map>
#include <functional>

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

GridEntity * Grid::add_entity(int x, int z, godot::Vector3 orientation, EntityType entity_type, godot::Spatial* variant) {
  if(is_blocked(x, z)) {
    throw std::logic_error("Tried to add an entity to a blocked tile, call is_blocked first");
  }

  GridEntity* grid_entity;
  auto shared_this = shared_from_this();
  auto weak_grid = std::weak_ptr<Grid>(shared_this);
  if (entity_type == EntityType::ENTRANCE) {
    grid_entity = new Entrance("", weak_grid, orientation, variant);
  }
  else if (entity_type == EntityType::WORKER) {
    grid_entity = new Worker("", weak_grid, orientation, variant);
  }
  else {
    grid_entity = new GridEntity("", weak_grid, false, orientation, entity_type, variant);
  }
  this->entity_map.insert(std::pair<std::string, GridEntity*>("", grid_entity));

  GridTile* tile = internal_grid[x * size + z];
  tile->entities.push_back(grid_entity);
  grid_entity->setGridTile(tile);

  return grid_entity;
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
  if (is_in_bounds(x, z)) {
    auto grid_tile = this->internal_grid[x * size + z];
    return std::any_of(grid_tile->entities.begin(), grid_tile->entities.end(), [](GridEntity* entity){
        return entity->isBlocking();
    });
  }
  return true;
}

bool Grid::is_in_bounds(int x, int z) {
  return !(x < 0 || x >= size || z < 0 || z >= size);
}

godot::Vector3 Grid::get_entity_coordinates(const std::string& entity_id) {
  auto entity = this->entity_map[entity_id];
  auto grid_tile = entity->getGridTile();
  return {static_cast<real_t>(grid_tile->x), 0, static_cast<real_t>(grid_tile->z)};
}

void Grid::step() {
  GridStepper gridStepper(this);
  gridStepper.step();
}

int Grid::getSize() const {
  return size;
}

std::vector<GridTile *> &Grid::getInternalGrid() {
  return internal_grid;
}

GodotInterface* Grid::getGodotInterface() {
  return godot_interface;
}
