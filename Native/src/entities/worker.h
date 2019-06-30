//
// Created by jamie on 30/06/19.
//

#ifndef REFACTOR_NATIVE_WORKER_H
#define REFACTOR_NATIVE_WORKER_H

using namespace Refactor;

class Worker : public GridEntity {

public:
    Worker(std::string id, godot::Vector3 orientation, godot::Spatial* godot_entity)
            : GridEntity(std::move(id), true, orientation, EntityType::WORKER, godot_entity) {};
    ~Worker() override = default;;
};

#endif //REFACTOR_NATIVE_WORKER_H
