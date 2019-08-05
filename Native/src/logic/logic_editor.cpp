//
// Created by jamie on 30/07/19.
//
#include <gen/CollisionShape2D.hpp>
#include <gen/RectangleShape2D.hpp>
#include <algorithm>
#include "logic_editor.h"

using namespace Refactor;

void LogicEditor::_register_methods() {
  register_method("init", &LogicEditor::_init);
  register_method("_ready", &LogicEditor::_ready);
  register_method("_process", &LogicEditor::_process);
  register_method("_unhandled_input", &LogicEditor::_unhandled_input);
  register_method("on_logic_piece_input_event", &LogicEditor::on_logic_piece_input_event);
}

void LogicEditor::_init() {
  godot::Godot::print("LogicEditor::init");
}

void LogicEditor::_ready() {
  logic_node_creator = std::make_unique<LogicNodeCreator>(this);
  logic_node_creator->setup();

  godot::Godot::print("LogicEditor::ready");

  auto logic_node_types = LogicNodeTypes::getInstance();
  auto all_types = logic_node_types->toVector();

  auto logic_toolbox = get_tree()->get_root()->find_node("LogicToolbox", true, false);
  if (logic_toolbox == nullptr) {
    godot::Godot::print("failed to find logic toolbox");
    return;
  }

  auto tool_box_items = logic_node_creator->create_toolbox();
  for (auto toolbox_item : tool_box_items) {
    logic_toolbox->add_child(toolbox_item);
  }

  auto root_node = std::make_shared<LogicRootNode>(EntityType::TILE);
  root_nodes.emplace_back(root_node);

  this->redraw_tree();
}

void LogicEditor::_process(float delta) {
  if (dragged_node != nullptr) {
    auto mouse_position = get_viewport()->get_mouse_position();
    mouse_position.x -= 75;
    mouse_position.y -= 50;
    dragged_node->set_position(mouse_position);

    GhostNode *snapped_node = nullptr;
    for (auto &ghost_node : ghost_nodes) {

      auto distance_from = (mouse_position - ghost_node->get_position()).length();
      if (distance_from < 50) {
        snapped_node = ghost_node;
      }
    }
    if (snapped_node != nullptr) {
      dragged_node->set_position(snapped_node->get_position());
      snapped_ghost_node = snapped_node;
    } else {
      snapped_ghost_node = nullptr;
    }
  }
}

void LogicEditor::_unhandled_input(const godot::InputEvent *event) {
  if (event->get_class() == "InputEventMouseButton") {
    auto mouse_event = reinterpret_cast<const godot::InputEventMouseButton *>(event);
    if (mouse_event->get_button_index() == 1) {
      if (mouse_event->is_pressed()) {
        is_mouse_pressed = true;
      } else {
        if (is_mouse_pressed) {
          handle_drag_release();
        }
        is_mouse_pressed = false;
      }
    }
  }
}

void LogicEditor::handle_drag_release() {
  if (dragged_node == nullptr) {
    return;
  }

  if (snapped_ghost_node != nullptr) {
    if (snapped_ghost_node->is_owned_by_root_node()) {
      // dropping it on a root node
      int root_index = snapped_ghost_node->get_root_index();
      auto added_logic_node = std::make_shared<LogicNode>(LogicNode(dragged_node->get_logic_node_type()));
      root_nodes[root_index]->put_root(added_logic_node);
      root_nodes[root_index]->set_ghost_node(nullptr);
    } else {
      // dropping it on branch/leaf
      auto owning_node = snapped_ghost_node->get_owning_node();
      auto index = snapped_ghost_node->get_input_index();

      // create a new node
      auto added_logic_node = std::make_shared<LogicNode>(LogicNode(dragged_node->get_logic_node_type()));
      added_logic_node->get_output()->parent = owning_node;
      owning_node->get_inputs()[index]->ghost = nullptr;
      owning_node->get_inputs()[index]->node = added_logic_node;
    }

    ghost_nodes.erase(std::remove(ghost_nodes.begin(),
                                  ghost_nodes.end(),
                                  snapped_ghost_node),
                      ghost_nodes.end());

    remove_child(snapped_ghost_node);
    remove_child(dragged_node);

    dragged_node = nullptr;
    redraw_tree();
  }

  // are we snapping to a ghost node?
  // what is the parent node?
  // add the dragged logic node type to the parent node
  // remove the dragged node
  // remove the ghost node
  // redraw the tree
  dragged_node = nullptr;
}

void LogicEditor::on_logic_piece_input_event(godot::Node *node, godot::InputEvent *input_event, int shape_idx,
                                             godot::Node *other) {
  if (input_event->get_class() == "InputEventMouseButton") {
    auto mouse_event = reinterpret_cast<const godot::InputEventMouseButton *>(input_event);
    if (mouse_event->is_pressed() && mouse_event->get_button_index() == 1) {
      auto original_node = cast_to<ToolboxNode>(other);

      if (original_node->is_in_toolbox()) {
        // dragging from toolbox
        auto toolbox_node = cast_to<ToolboxNode>(other);
        dragged_node = logic_node_creator->create_node(toolbox_node->get_logic_node_type());
        dragged_node->set_in_toolbox(false);
        dragged_node->set_logic_node_type(original_node->get_logic_node_type());
        dragged_node->set_position(mouse_event->get_position());
        add_child(dragged_node);
      } else {
        // dragging from screen
        dragged_node = original_node;
        auto logic_node = dragged_node->get_logic_node();
        auto output = logic_node->get_output();
        if (output != nullptr && !output->is_root) {
          //iterate over tree and remove all ghost nodes
          output->parent->get_inputs()[output->parent_output_index]->node = nullptr;
          redraw_tree();
        }
        // find the parent
        // set the corresponding input node to null
        // tell the parent that the node has been removed
      }
    }
  }
}


void LogicEditor::redraw_tree() {
  godot::Godot::print("LogicEditor::redraw_tree");
  for (int i = 0; i < root_nodes.size(); i++) {
    auto root = root_nodes[i];
    if (root->get_graphical_node() == nullptr) {
      auto root_graphical = logic_node_creator->create_root_node(root->get_type());
      root->set_graphical_node(root_graphical);
      add_child(root_graphical);
    }

    if (root->get_tree() == nullptr && root->get_ghost_node() == nullptr) {
      auto ghost = logic_node_creator->create_ghost_node();
      ghost->set_owned_by_root_node(true);
      ghost->set_root_index(i);
      ghost->set_position(root->get_graphical_node()->get_position() + godot::Vector2(110, 0));
      ghost_nodes.emplace_back(ghost);
      add_child(ghost);
      root->set_ghost_node(ghost);
    } else if (root->get_tree() != nullptr) {
      if (root->get_ghost_node() != nullptr) {
        remove_child(root->get_ghost_node());
        root->set_ghost_node(nullptr);
      }

      draw_branch(root->get_graphical_node()->get_position() + godot::Vector2(110, 0), root->get_tree());
    }

  }
}


void LogicEditor::draw_branch(godot::Vector2 offset, std::shared_ptr<LogicNode> tree_node) {
  godot::Godot::print("LogicEditor::redraw_branch");
  if (tree_node->get_graphical_node() == nullptr) {
    godot::Godot::print("LogicEditor::draw_branch::adding_graphical_node");
    auto graphical_node = logic_node_creator->create_node(tree_node->get_type());
    graphical_node->set_logic_node(tree_node);
    graphical_node->set_in_toolbox(false);
    graphical_node->set_position(offset);
    tree_node->set_graphical_node(graphical_node);
    add_child(graphical_node);
  }

  auto handle_node_input = [&](std::shared_ptr<LogicNodeInput> input) {
    if (input->enabled) {
      if (input->node == nullptr) {
        if (input->ghost == nullptr) {
          auto ghost = logic_node_creator->create_ghost_node();
          ghost->set_position(
              tree_node->get_graphical_node()->get_position() + godot::Vector2(110, input->index == 0 ? 0 : 90));
          ghost->set_owning_node(tree_node);
          ghost->set_input_index(input->index);
          ghost_nodes.emplace_back(ghost);
          add_child(ghost);
        }
      } else if (input->node != nullptr) {
        draw_branch(tree_node->get_graphical_node()->get_position() + godot::Vector2(110, input->index == 0 ? 0 : 90),
                    input->node);
      }
    }
  };
  handle_node_input(tree_node->get_inputs()[0]);
  handle_node_input(tree_node->get_inputs()[1]);
}