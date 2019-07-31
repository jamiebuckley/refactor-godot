//
// Created by jamie on 30/07/19.
//

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
    }
  }
}
