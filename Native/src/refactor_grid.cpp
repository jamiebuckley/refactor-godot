#include "include/refactor_grid.h"

using namespace Refactor;

Grid::Grid(int size) {
  this->size = size;
  this->internal_grid = std::vector<GridTile>(size * size);
}

std::string Grid::add_entity(int x, int z, EntityType entity_type) {
  auto id_string = std::to_string(this->last_number);
  auto padded_id_string = std::string().append(24 - id_string.length(), '0').append(id_string);
  this->last_number++;
  return padded_id_string;
}

bool Grid::delete_entity(std::string& id) {
  return true;
}

void Grid::step() {

}