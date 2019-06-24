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
    void* godot_entity;

    GridEntity(std::string id, bool is_blocking, godot::Vector3 orientation, EntityType entity_type, void* godot_entity) {
      this->id = id;
      this->is_blocking = is_blocking;
      this->entity_type = entity_type;
      this->orientation = orientation;
      this->grid_tile = nullptr;
      this->godot_entity = godot_entity;
    }
  };

  struct GridTile {
    int x;
    int z;
    std::vector<GridEntity*> entities;

    GridTile(int x, int z) {
      this->x = x;
      this->z = z;
    }
  };


  class Grid {
    
    public:
      Grid(int size);
      ~Grid();

      std::string add_entity(int x, int z, godot::Vector3 orientation, EntityType entity_type, void* godot_entity);
      bool delete_entity(std::string name);
      godot::Vector3 get_entity_coordinates(std::string entity_id);
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