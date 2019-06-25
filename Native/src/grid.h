#ifndef REFACTOR_GRID_H
#define REFACTOR_GRID_H
#include <vector>
#include <map>
#include <string>
#include <Godot.hpp>
#include <Vector3.hpp>
#include "common.h"
#include "grid_tile.h"

namespace Refactor {

  class Grid {
    
    public:
      Grid(int size);
      ~Grid();

      std::string add_entity(int x, int z, godot::Vector3 orientation, EntityType entity_type, void* godot_entity);
      bool delete_entity(const std::string& name);
      godot::Vector3 get_entity_coordinates(const std::string& entity_id);
      bool is_blocked(int x, int z);
      void step();

    private:
      int size;
      int last_number = 0;
      std::vector<GridTile*> internal_grid;
      std::map<std::string, GridEntity*> entity_map;
      void step_workers();
      void step_entrances();
      std::vector<GridEntity*> query_type(EntityType);
  };
}

#endif