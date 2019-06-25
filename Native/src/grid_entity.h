//
// Created by jamie on 24/06/19.
//

#ifndef REFACTOR_NATIVE_GRID_ENTITY_H
#define REFACTOR_NATIVE_GRID_ENTITY_H


#include <string>
#include <core/Vector3.hpp>
#include "common.h"
#include "grid_tile.h"

namespace Refactor {

    class GridTile;

    class GridEntity {
    public:
        std::string id;
        bool is_blocking;
        EntityType entity_type;
        godot::Vector3 orientation;
        GridTile *grid_tile;
        void *godot_entity;

        GridEntity(std::string id, bool is_blocking, godot::Vector3 orientation, EntityType entity_type,
                   void *godot_entity) {
            this->id = id;
            this->is_blocking = is_blocking;
            this->entity_type = entity_type;
            this->orientation = orientation;
            this->grid_tile = nullptr;
            this->godot_entity = godot_entity;
        }
    };
}


#endif //REFACTOR_NATIVE_GRID_ENTITY_H
