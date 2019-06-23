#ifndef REFACTOR_GRID_H
#define REFACTOR_GRID_H
#include <vector>
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
    godot::Vector3 orientation;
    GridTile* grid_tile;

    GridEntity(std::string id, bool is_blocking, godot::Vector3 orientation) {
      this->id = id;
      this->is_blocking = is_blocking;
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

      std::string add_entity(int x, int z, EntityType entity_type);
      bool delete_entity(std::string& name);
      bool is_blocked(int x, int z);
      void step();

    private:
      int size;
      int last_number = 0;
      std::vector<GridTile> internal_grid;
  };
}

#endif