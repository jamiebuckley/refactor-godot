#include <utility>

//
// Created by jamie on 24/06/19.
//

#ifndef REFACTOR_NATIVE_ENTRANCE_H
#define REFACTOR_NATIVE_ENTRANCE_H

#include <entities/grid_entity.h>

namespace Refactor {
    class Entrance : public GridEntity {

    public:
        Entrance(std::string id, std::weak_ptr<Grid> grid, godot::Vector3 orientation, godot::Spatial *godot_entity)
                : GridEntity(std::move(id), grid, false, orientation, EntityType::ENTRANCE, godot_entity) {};

        ~Entrance() override = default;;
    };
}

#endif //REFACTOR_NATIVE_ENTRANCE_H
