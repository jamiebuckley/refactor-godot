//
// Created by jamie on 30/07/19.
//

#include <gen/ResourceLoader.hpp>
#include <gen/Spatial.hpp>
#include <gen/TextureRect.hpp>
#include <gen/Label.hpp>
#include "logic_editor.h"

using namespace Refactor;

void LogicEditor::_register_methods() {
  register_method("init", &LogicEditor::_init);
  register_method("_ready", &LogicEditor::_ready);
  register_method("_process", &LogicEditor::_process);
}

void LogicEditor::_init() {
  godot::Godot::print("LogicEditor::init");
}

void LogicEditor::_ready() {
  godot::Godot::print("LogicEditor::ready");

  auto resource_loader = godot::ResourceLoader::get_singleton();

  auto logic_entrance = resource_loader->load("res://Prototypes/Logic/LogicEntrance.tscn");
  root_node_type_to_scene_map.insert(std::pair<EntityType, godot::Ref<godot::PackedScene>> (EntityType::TILE, logic_entrance));

  auto logic_node_types = LogicNodeTypes::getInstance();
  auto root = std::make_shared<LogicRootNode>(LogicRootNode(EntityType::TILE));

  auto node_toggle_if = std::make_shared<LogicNode>(logic_node_types->TOGGLE_IF);
  node_toggle_if->set_root_output(root);
  root->put_root(node_toggle_if);

  auto node_worker_has = std::make_shared<LogicNode>(logic_node_types->WORKER_HAS);
  node_worker_has->set_output(node_toggle_if);
  node_toggle_if->set_input_1(node_worker_has);

  auto node_coal = std::make_shared<LogicNode>(logic_node_types->INVENTORY_ITEM);
  node_coal->set_output(node_worker_has);
  node_worker_has->set_input_1(node_coal);

  auto node_equals = std::make_shared<LogicNode>(logic_node_types->NUMERICAL_EQUALS);
  node_coal->set_output(node_worker_has);
  node_worker_has->set_input_2(node_equals);

  auto node_one = std::make_shared<LogicNode>(logic_node_types->NUMBER);
  node_one->set_output(node_equals);

  root_nodes.emplace_back(root);
  this->redraw_tree();
}

void LogicEditor::_process(float delta) {

}

void LogicEditor::redraw_tree() {
  auto logic_node_types = LogicNodeTypes::getInstance();

  godot::Godot::print("LogicEditor::redraw_tree");
  for(int i = 0; i < root_nodes.size(); i++) {
    auto root = root_nodes[i];
    if (root_node_type_to_scene_map.find(root->get_type()) == root_node_type_to_scene_map.end()) {
      std::string message = "Could not find matching scene for root logic node " + entity_type_names.at(root->get_type());
      godot::Godot::print(message.c_str());
      continue;
    }

    auto root_node_scene = create_root_node(root->get_type());
    root_node_scene->set_position(godot::Vector2(200.0f, 100.0f));
    add_child(root_node_scene);
  }
}

godot::Node2D *LogicEditor::create_root_node(EntityType entity_type) {
    auto resource_loader = godot::ResourceLoader::get_singleton();
    auto root_node = godot::Node2D::_new();
    auto texture_rect = godot::TextureRect::_new();
    texture_rect->set_texture(resource_loader->load("res://Assets/Textures/logic_input_nameless.png"));
    root_node->add_child(texture_rect);

    auto label = godot::Label::_new();
    label->set_text("Tile");
    label->set("custom_fonts/font", resource_loader->load("res://Assets/Fonts/Montserrat.tres"));
    label->set_position(godot::Vector2(50, 50));
    root_node->add_child(label);

    root_node->set_scale(godot::Vector2(0.2f, 0.2f));
    return root_node;
}

godot::Node2D *LogicEditor::create_node(LogicNodeType logic_node_type) {
    return nullptr;
}
