//
// Created by jamie on 30/07/19.
//

#ifndef REFACTOR_NATIVE_LOGIC_EDITOR_H
#define REFACTOR_NATIVE_LOGIC_EDITOR_H

#include <Godot.hpp>
#include <SceneTree.hpp>
#include <Viewport.hpp>
#include <Node2D.hpp>
#include <Texture.hpp>
#include <AtlasTexture.hpp>
#include <Label.hpp>
#include <PackedScene.hpp>
#include <HBoxContainer.hpp>
#include <ResourceLoader.hpp>
#include <Spatial.hpp>
#include <TextureRect.hpp>
#include <Area2D.hpp>
#include <CollisionShape2D.hpp>
#include <CollisionObject2D.hpp>
#include <InputEventMouseButton.hpp>
#include <Shape2D.hpp>
#include <InputEvent.hpp>

#include <vector>
#include <memory>
#include <map>

#include "logic_node.h"

namespace Refactor {
class LogicEditor: public godot::Node2D {
        GODOT_CLASS(LogicEditor, godot::Node2D)

        public:
            static void _register_methods();
            void _init();
            void _ready();
            void _unhandled_input(const godot::InputEvent* event);
            void _process(float delta);
            void on_logic_piece_input_event(godot::Node* node, godot::InputEvent* input_event, int shape_idx, godot::Node* other);

        private:
          std::vector<std::shared_ptr<LogicRootNode>> root_nodes;
          void redraw_tree();

          godot::Node2D* create_root_node(EntityType entity_type);
          godot::Node2D* create_node(const LogicNodeType* logic_node_type);

          std::map<Refactor::EntityType, godot::Ref<godot::PackedScene>> root_node_type_to_scene_map;
          std::map<int, godot::PackedScene> node_type__to_scene_map;

          std::map<LogicNodeConnection, godot::AtlasTexture*> logic_in_atlas_map;
          std::map<LogicNodeConnection, godot::AtlasTexture*> logic_out_atlas_map;
          godot::AtlasTexture* main_body_atlas;
          godot::Node2D* dragged_node;
          bool is_mouse_pressed;

    void create_atlas();
};
}


#endif //REFACTOR_NATIVE_LOGIC_EDITOR_H
