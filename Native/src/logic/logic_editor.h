//
// Created by jamie on 30/07/19.
//

#ifndef REFACTOR_NATIVE_LOGIC_EDITOR_H
#define REFACTOR_NATIVE_LOGIC_EDITOR_H

#include <Godot.hpp>
#include <Node2D.hpp>
#include "logic_root_node.h"

namespace Refactor {
class LogicEditor: public godot::Node2D {
        GODOT_CLASS(LogicEditor, godot::Node2D)

        public:
            static void _register_methods();
            void _init();
            void _ready();
            void _process(float delta);
    };
}


#endif //REFACTOR_NATIVE_LOGIC_EDITOR_H
