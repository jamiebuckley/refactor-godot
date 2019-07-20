//
// Created by jamie on 24/06/19.
//

#ifndef REFACTOR_NATIVE_GRID_ENTITY_H
#define REFACTOR_NATIVE_GRID_ENTITY_H


#include <string>
#include <memory>
#include <core/Vector3.hpp>
#include <gen/Spatial.hpp>
#include <grid/grid.h>
#include "common.h"
#include "grid_tile.h"

namespace Refactor {
    class GridTile;
    class Grid;

    class GridEntity {
    private:
        std::string id;
        std::weak_ptr<Grid> grid;
        bool is_blocking;
        EntityType entity_type;
        godot::Vector3 orientation;
        GridTile *grid_tile;
        godot::Spatial *godot_entity;

    public:
        GridEntity(std::string id, const std::weak_ptr<Grid> grid, bool is_blocking, godot::Vector3 orientation,
                   EntityType entity_type, godot::Spatial *godot_entity) {
          this->id = id;
          this->grid = grid;
          this->is_blocking = is_blocking;
          this->entity_type = entity_type;
          this->orientation = orientation;
          this->grid_tile = nullptr;
          this->godot_entity = godot_entity;
        }

        virtual ~GridEntity() = default;

        const std::string &getId() const {
          return id;
        }

        bool isBlocking() const {
          return is_blocking;
        }

        void setIsBlocking(bool isBlocking) {
          is_blocking = isBlocking;
        }

        EntityType getEntityType() const {
          return entity_type;
        }

        void setEntityType(EntityType entityType) {
          entity_type = entityType;
        }

        const godot::Vector3 &getOrientation() const {
          return orientation;
        }

        void setOrientation(const godot::Vector3 &orientation) {
          GridEntity::orientation = orientation;
        }

        GridTile *getGridTile() const {
          return grid_tile;
        }

        void setGridTile(GridTile *gridTile) {
          grid_tile = gridTile;
        }

        void *getGodotEntity() const {
          return godot_entity;
        }

        void setGodotEntity(godot::Spatial *godotEntity) {
          godot_entity = godotEntity;
        }
    };
}


#endif //REFACTOR_NATIVE_GRID_ENTITY_H
