//
// Created by jamie on 20/07/19.
//

#ifndef REFACTOR_NATIVE_GODOTWORKER_H
#define REFACTOR_NATIVE_GODOTWORKER_H

#include <Godot.hpp>
#include <gen/Spatial.hpp>
#include <memory>
#include <game.h>

namespace godot {
    class GodotWorker : public Spatial {
      GODOT_CLASS(GodotWorker, Spatial)

    public:
        static void _register_methods();
        void _init();
        void _ready();
        void _process(float delta);
        void set_destination(Vector3 new_destination);
        void set_game(Game* new_game);

    private:
        Vector3 destination;
        Vector3 previous_coordinates;
        Game* game;
    };
}

#endif //REFACTOR_NATIVE_GODOTWORKER_H
