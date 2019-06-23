#ifndef REFACTOR_GRID_H
#define REFACTOR_GRID_H
#include <vector>
#include <map>
#include <string>
#include <Godot.hpp>
#include <Vector3.hpp>

namespace Refactor {

  enum EntityType {
    WORKER,
    ENTRANCE,
    EXIT,
    TILE,
    BLOCK
  };

  /* Forward declaration for GridTile */
  struct GridTile;

  struct GridEntity {
    std::string id;
    bool is_blocking;
    EntityType entity_type;
    godot::Vector3 orientation;
    GridTile* grid_tile;

    GridEntity(std::string id, bool is_blocking, godot::Vector3 orientation, EntityType entity_type) {
      this->id = id;
      this->is_blocking = is_blocking;
      this->entity_type = entity_type;
      this->orientation = orientation;
      this->grid_tile = nullptr;
    }
  };

  struct GridTile {
    int x;
    int z;
    GridEntity* entity;

    GridTile(int x, int z) {
      this->x = x;
      this->z = z;
      this->entity = nullptr;
    }
  };


  class Grid {
    
    public:
      Grid(int size);
      ~Grid();

      std::string add_entity(int x, int z, EntityType entity_type, godot::Vector3 orientation);
      bool delete_entity(std::string name);
      godot::Vector3 get_entity_coordinates(std::string entity_id);
      bool is_blocked(int x, int z);
      void step();

    private:
      int size;
      int last_number = 0;
      std::vector<GridTile*> internal_grid;
      std::map<std::string, GridEntity*> entity_map;
  };
}

#endif