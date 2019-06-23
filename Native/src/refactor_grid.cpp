#include "include/refactor_grid.h"
#include <Godot.hpp>
#include <string>

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

std::string Grid::add_entity(int x, int z, EntityType entity_type, godot::Vector3 orientation) {
  auto id_string = std::to_string(this->last_number);
  auto padded_id_string = std::string().append(24 - id_string.length(), '0').append(id_string);
  this->last_number++;

  auto blocking = entity_type == EntityType::WORKER 
    || entity_type == EntityType::BLOCK
    || entity_type == EntityType::ENTRANCE
    || entity_type == EntityType::EXIT;

  GridEntity* gridEntity = new GridEntity(padded_id_string, blocking, orientation);

  GridTile* tile = internal_grid[x * size + z];
  tile->entity = gridEntity;
  gridEntity->grid_tile = tile;

  return padded_id_string;
}

bool Grid::delete_entity(std::string& id) {
  return true;
}

void Grid::step() {

}

bool Grid::is_blocked(int x, int z) {
  auto grid_tile = this->internal_grid[x * size + z];
  if (grid_tile->entity == NULL) {
    return false;
  }

  return grid_tile->entity->is_blocking;
}