//
// Created by jamie on 30/06/19.
//
#include "grid/grid.h"

#ifndef REFACTOR_NATIVE_WORKER_H
#define REFACTOR_NATIVE_WORKER_H

using namespace Refactor;

namespace Refactor {
    class Worker : public GridEntity {

    public:
        Worker(std::string id, const std::weak_ptr<Refactor::Grid> &grid, godot::Vector3 orientation,
               godot::Spatial *godot_entity)
                : GridEntity(std::move(id), grid, true, orientation, EntityType::WORKER, godot_entity) {};

        ~Worker() override = default;;

        const godot::Vector3 &getDestination() const {
          return destination;
        }

        void setDestination(const godot::Vector3 &destination) {
          Worker::destination = destination;
        }

    private:
        godot::Vector3 destination;
    };
}
#endif //REFACTOR_NATIVE_WORKER_H
