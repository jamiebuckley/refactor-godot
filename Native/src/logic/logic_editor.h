//
// Created by jamie on 30/07/19.
//

#ifndef REFACTOR_NATIVE_LOGIC_EDITOR_H
#define REFACTOR_NATIVE_LOGIC_EDITOR_H

#include <Godot.hpp>
#include <Node2D.hpp>

#include <vector>
#include <memory>
#include <map>
#include <gen/PackedScene.hpp>
#include "logic_node.h"

namespace Refactor {
class LogicEditor: public godot::Node2D {
        GODOT_CLASS(LogicEditor, godot::Node2D)

        public:
            static void _register_methods();
            void _init();
            void _ready();
            void _process(float delta);

        private:
          std::vector<std::shared_ptr<LogicRootNode>> root_nodes;
          void redraw_tree();

          std::map<Refactor::EntityType, godot::Ref<godot::PackedScene>> root_node_type_to_scene_map;

          std::map<int, godot::PackedScene> node_type__to_scene_map;
    };
}


#endif //REFACTOR_NATIVE_LOGIC_EDITOR_H
