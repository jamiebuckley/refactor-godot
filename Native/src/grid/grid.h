#ifndef REFACTOR_GRID_H
#define REFACTOR_GRID_H
#include <vector>
#include <map>
#include <string>
#include <memory>
#include <functional>
#include <optional>
#include <Godot.hpp>
#include <Vector3.hpp>
#include "common.h"
#include "grid_tile.h"
#include "godot_interface.h"

namespace Refactor {

  class Grid: public std::enable_shared_from_this<Grid> {
    
    public:
      Grid(int size, GodotInterface* godot_interface);
      ~Grid();

      std::shared_ptr<GridEntity> add_entity(int x, int z, godot::Vector3 orientation, EntityType entity_type, godot::Spatial* godot_entity);
      bool delete_entity(const std::string& name);
      godot::Vector3 get_entity_coordinates(const std::string& entity_id);
      std::optional<std::shared_ptr<GridTile>> get_grid_tile(int x, int z);
      bool is_blocked(int x, int z);
      bool can_place_entity_type(int x, int z, EntityType entity_type);
      bool is_in_bounds(int x, int z);
      void step();

      int getSize() const;
      std::vector<std::shared_ptr<GridTile>> &getInternalGrid();
      GodotInterface* getGodotInterface();

  private:
      int size;
      int last_number = 0;
      std::vector<std::shared_ptr<GridTile>> internal_grid;
      std::map<std::string, std::shared_ptr<GridEntity>> entity_map;
      GodotInterface* godot_interface;

  };
}

#endif