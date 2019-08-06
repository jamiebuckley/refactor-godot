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
#include "logic_node_creator.h"

namespace Refactor {

    class LogicEditor : public godot::Node2D {
    GODOT_CLASS(LogicEditor, godot::Node2D)

    public:
        static void _register_methods();

        void _init();

        void _ready();

        void _unhandled_input(const godot::InputEvent *event);

        void _process(float delta);

        void on_logic_piece_input_event(godot::Node *node, godot::InputEvent *input_event, int shape_idx,
                                        godot::Node *other);

        void handle_drag_release();

    private:
        std::vector<std::shared_ptr<LogicNode>> root_nodes;
        void redraw_tree();
        void draw_branch(godot::Vector2 offset, std::shared_ptr<LogicNode> tree_node);

        ToolboxNode *dragged_node;
        GhostNode *snapped_ghost_node;
        std::vector<GhostNode *> ghost_nodes;

        bool is_mouse_pressed;

        std::unique_ptr<LogicNodeCreator> logic_node_creator;
    };
}


#endif //REFACTOR_NATIVE_LOGIC_EDITOR_H
