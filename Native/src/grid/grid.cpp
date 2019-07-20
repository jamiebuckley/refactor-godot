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
  godot_interface->print("Starting");
  this->size = size;
  this->internal_grid = std::vector<std::shared_ptr<GridTile>>(size * size);
  //this->internal_grid.reserve(size * size);
  this->godot_interface = godot_interface;

  for(int x = 0; x < size; x++) {
    for(int z = 0; z < size; z++) {
      this->internal_grid[x * size + z] = std::make_shared<GridTile>(x, z);
    }
  }
  godot_interface->print("Done");
}

Grid::~Grid() {

}

std::shared_ptr<GridEntity> Grid::add_entity(int x, int z, godot::Vector3 orientation, EntityType entity_type, godot::Spatial* variant) {
  if(!can_place_entity_type(x, z, entity_type)) {
    throw std::logic_error("Tried to add an entity to a blocked tile, call is_blocked first");
  }

  auto id_string = std::to_string(this->last_number);
  auto padded_id_string = std::string().append(24 - id_string.length(), '0').append(id_string);
  this->last_number++;

  std::shared_ptr<GridEntity> grid_entity;
  auto shared_this = shared_from_this();
  auto weak_grid = std::weak_ptr<Grid>(shared_this);

  if (entity_type == EntityType::ENTRANCE) {
    grid_entity = std::make_shared<Entrance>(id_string, weak_grid, orientation, variant);
  }
  else if (entity_type == EntityType::WORKER) {
    grid_entity = std::make_shared<Worker>(id_string, weak_grid, orientation, variant);
  }
  else {
    grid_entity = std::make_shared<GridEntity>(id_string, weak_grid, false, orientation, entity_type, variant);
  }
  this->entity_map.insert(std::pair<std::string, std::shared_ptr<GridEntity>>(id_string, grid_entity));

  auto tile = internal_grid[x * size + z];
  tile->entities.push_back(grid_entity);
  grid_entity->setGridTile(std::weak_ptr(tile));

  return grid_entity;
}

bool Grid::delete_entity(const std::string& id) {
  auto entity = entity_map[id];
  auto grid_tile = entity->getGridTile().lock();

  auto ets = grid_tile->entities;
  ets.erase(std::remove(ets.begin(), ets.end(), entity));
  entity_map.erase(id);
  return true;
}

/**
 * Test if the grid tile at x,z contains a blocking entity
 */
bool Grid::is_blocked(int x, int z) {
  if (is_in_bounds(x, z)) {
    auto grid_tile = this->internal_grid[x * size + z];
    return std::any_of(grid_tile->entities.begin(), grid_tile->entities.end(), [](std::shared_ptr<GridEntity> entity){
        return entity->isBlocking();
    });
  }
  return true;
}

/**
 * Test if you can place a particular entity on a tile
 * @param x The x position of the tile
 * @param z The z position of the tile
 * @param entityType The entity type to place
 * @return True if can be placed else false
 */
bool Grid::can_place_entity_type(int x, int z, EntityType entity_type) {
  if (is_in_bounds(x, z)) {
    auto grid_tile = this->internal_grid[x * size + z];
    if(entity_type == EntityType::ENTRANCE || entity_type == EntityType::EXIT) {
      return !std::any_of(grid_tile->entities.begin(), grid_tile->entities.end(), [](std::shared_ptr<GridEntity> entity){
          return entity->isBlocking();
      });
    }
    else if (entity_type == EntityType::TILE) {
      return !std::any_of(grid_tile->entities.begin(), grid_tile->entities.end(), [](std::shared_ptr<GridEntity> entity){
          return entity->getEntityType() == EntityType::TILE;
      });
    }
    else if (entity_type == EntityType::WORKER) {
      return !std::any_of(grid_tile->entities.begin(), grid_tile->entities.end(), [](std::shared_ptr<GridEntity> entity){
          return entity->getEntityType() != ENTRANCE && entity->isBlocking();
      });
    }
  }
  return false;
}

bool Grid::is_in_bounds(int x, int z) {
  return !(x < 0 || x >= size || z < 0 || z >= size);
}

godot::Vector3 Grid::get_entity_coordinates(const std::string& entity_id) {
  auto entity = this->entity_map[entity_id];
  auto grid_tile = entity->getGridTile().lock();
  return {static_cast<real_t>(grid_tile->x), 0, static_cast<real_t>(grid_tile->z)};
}

std::optional<std::shared_ptr<GridTile>> Grid::get_grid_tile(int x, int z) {
  if (!is_in_bounds(x, z)) {
    return {};
  }

  auto grid_tile = internal_grid[x * size + z];
  return grid_tile;
}

void Grid::step() {
  GridStepper gridStepper(this);
  gridStepper.step();
}

int Grid::getSize() const {
  return size;
}

std::vector<std::shared_ptr<GridTile>> &Grid::getInternalGrid() {
  return internal_grid;
}

GodotInterface* Grid::getGodotInterface() {
  return godot_interface;
}
