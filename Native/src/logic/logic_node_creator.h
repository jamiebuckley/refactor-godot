//
// Created by jamie on 04/08/19.
//

#ifndef REFACTOR_NATIVE_LOGIC_NODE_CREATOR_H
#define REFACTOR_NATIVE_LOGIC_NODE_CREATOR_H

#include <Godot.hpp>
#include <Node2D.hpp>
#include <TextureRect.hpp>
#include <AtlasTexture.hpp>
#include <ResourceLoader.hpp>
#include <Label.hpp>
#include <Area2D.hpp>
#include <RectangleShape2D.hpp>
#include <vector>
#include <map>

#include "logic_node.h"

namespace Refactor {
    class GhostNode : public godot::Node2D {
        GODOT_CLASS(GhostNode, godot::Node2D)

    public:
        static void _register_methods() {
          register_method("init", &GhostNode::_init);
          register_method("_ready", &GhostNode::_ready);
          register_method("_process", &GhostNode::_process);
        }

        void _init() {}
        void _ready() {}
        void _process(float delta) {}

        bool is_owned_by_root_node() const {
          return owned_by_root_node;
        }

        void set_owned_by_root_node(bool owned_by_root_node) {
          GhostNode::owned_by_root_node = owned_by_root_node;
        }

        std::shared_ptr<LogicNode> get_owning_node() const {
          return owning_node;
        }

        void set_owning_node(std::shared_ptr<LogicNode> owning_node) {
          GhostNode::owning_node = owning_node;
        }

        int get_input_index() const {
          return input_index;
        }

        void set_input_index(int input_index) {
          GhostNode::input_index = input_index;
        }

        int get_root_index() const {
          return root_index;
        }

        void set_root_index(int root_index) {
          GhostNode::root_index = root_index;
        }

    private:
        bool owned_by_root_node;
        int root_index;
        std::shared_ptr<LogicNode> owning_node;
        int input_index;
    };

    class ToolboxNode: public godot::Node2D  {
        GODOT_CLASS(ToolboxNode, godot::Node2D)

    public:
        static void _register_methods() {
          register_method("init", &GhostNode::_init);
          register_method("_ready", &GhostNode::_ready);
          register_method("_process", &GhostNode::_process);
        }

        void _init() {}
        void _ready() {}
        void _process(float delta) {}

        const LogicNodeType* get_logic_node_type() { return logic_node_type; }
        void set_logic_node_type(const LogicNodeType* _logic_node_type) { this->logic_node_type = _logic_node_type; }

        bool is_in_toolbox() const { return in_toolbox; }
        void set_in_toolbox(bool in_toolbox) { ToolboxNode::in_toolbox = in_toolbox; }

        std::shared_ptr<LogicNode> get_logic_node() const {return logic_node;}
        void set_logic_node(std::shared_ptr<LogicNode> logic_node) {ToolboxNode::logic_node = logic_node; }

    private:
        const LogicNodeType* logic_node_type;
        std::shared_ptr<LogicNode> logic_node;
        bool in_toolbox = true;
    };

    class LogicNodeCreator {
    public:
        LogicNodeCreator(godot::Object* event_listener) { this->event_listener = event_listener; }
        godot::Node2D *create_root_node(EntityType entity_type);
        ToolboxNode *create_node(const LogicNodeType *logic_node_type);
        GhostNode *create_ghost_node();
        void create_atlas();
        void setup();
        std::vector<godot::Control*> create_toolbox();

    private:
        std::map<LogicNodeConnection, godot::AtlasTexture *> logic_in_atlas_map;
        std::map<LogicNodeConnection, godot::AtlasTexture *> logic_out_atlas_map;
        godot::AtlasTexture *main_body_atlas;
        godot::AtlasTexture *ghost_atlas;
        godot::AtlasTexture *entry_point_atlas;
        godot::Object *event_listener;
    };
}


#endif //REFACTOR_NATIVE_LOGIC_NODE_CREATOR_H
