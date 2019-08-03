//
// Created by jamie on 30/07/19.
//
#include <gen/CollisionShape2D.hpp>
#include <gen/RectangleShape2D.hpp>
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
  godot::Godot::print("LogicEditor::ready");
  this->create_atlas();

  auto logic_node_types = LogicNodeTypes::getInstance();
  auto all_types = logic_node_types->toVector();

  auto logic_toolbox = get_tree()->get_root()->find_node("LogicToolbox", true, false);
  if (logic_toolbox == nullptr) {
    godot::Godot::print("failed to find logic toolbox");
    return;
  }

  /**
   * Create a node for each type
   */
  for(int i = 1; i < all_types.size(); i++) {
    auto current_type = all_types[i];
    auto visual_node = create_node(current_type);
    visual_node->set_scale(godot::Vector2(0.2, 0.2));

    auto pickable_control = godot::Control::_new();
    pickable_control->add_child(visual_node);
    visual_node->set_position(godot::Vector2(5, 5));
    pickable_control->set_custom_minimum_size(godot::Vector2(150, 150));
    logic_toolbox->add_child(pickable_control);
  }

  auto resource_loader = godot::ResourceLoader::get_singleton();

  auto logic_entrance = resource_loader->load("res://Prototypes/Logic/LogicEntrance.tscn");
  root_node_type_to_scene_map.insert(
      std::pair<EntityType, godot::Ref<godot::PackedScene>>(EntityType::TILE, logic_entrance));


//  // constructing tree
//  auto root = std::make_shared<LogicRootNode>(LogicRootNode(EntityType::TILE));
//  auto node_toggle_if = std::make_shared<LogicNode>(logic_node_types->TOGGLE_IF);
//  node_toggle_if->set_root_output(root);
//  root->put_root(node_toggle_if);
//
//  auto node_worker_has = std::make_shared<LogicNode>(logic_node_types->WORKER_HAS);
//  node_worker_has->set_output(node_toggle_if);
//  node_toggle_if->set_input_1(node_worker_has);
//
//  auto node_coal = std::make_shared<LogicNode>(logic_node_types->INVENTORY_ITEM);
//  node_coal->set_output(node_worker_has);
//  node_worker_has->set_input_1(node_coal);
//
//  auto node_equals = std::make_shared<LogicNode>(logic_node_types->NUMERICAL_EQUALS);
//  node_coal->set_output(node_worker_has);
//  node_worker_has->set_input_2(node_equals);
//
//  auto node_one = std::make_shared<LogicNode>(logic_node_types->NUMBER);
//  node_one->set_output(node_equals);
//
//  root_nodes.emplace_back(root);
  this->redraw_tree();
}

void LogicEditor::_process(float delta) {
  if(dragged_node != nullptr) {
    auto mouse_position = get_viewport()->get_mouse_position();
    mouse_position.x -= 75;
    mouse_position.y -= 50;

    dragged_node->set_position(mouse_position);
  }
  // if the mouse button is down
  // and it wasn't down previously
  // and it is over one of the selection options
  // assign the selection option as a drag item

  // if the mouse button is up
  // and is was down previously
  // and a selection item is being dragged
  // - if it is over the main area
  // drop it on, and try to snap it to a tile
  // - if it is over the toolbox area
  // remove it

  // if the mouse button is down
  // and it was down previously
  // and an item is being dragged
  // draw the item at the current mouse position
  // or snap it to a tree element if it is close enough and of the right type

  // if the mouse button is up
  // if it is over one of the selection options
  // change the color of the selection options
}

void LogicEditor::_unhandled_input(const godot::InputEvent *event) {
  if(event->get_class() == "InputEventMouseButton") {
    auto mouse_event = reinterpret_cast<const godot::InputEventMouseButton *>(event);
    if (mouse_event->get_button_index() == 1) {
      if (mouse_event->is_pressed()) {
        is_mouse_pressed = true;
      } else {
        if (is_mouse_pressed) {
          dragged_node = nullptr;
        }
        is_mouse_pressed = false;
      }
    }
  }
}

void LogicEditor::redraw_tree() {
  auto logic_node_types = LogicNodeTypes::getInstance();

  godot::Godot::print("LogicEditor::redraw_tree");
  for (int i = 0; i < root_nodes.size(); i++) {
    auto root = root_nodes[i];
    if (root_node_type_to_scene_map.find(root->get_type()) == root_node_type_to_scene_map.end()) {
      std::string message =
          "Could not find matching scene for root logic node " + entity_type_names.at(root->get_type());
      godot::Godot::print(message.c_str());
      continue;
    }

    auto root_node_scene = create_node(logic_node_types->WORKER_TYPE);
    root_node_scene->set_position(godot::Vector2(200.0f, 100.0f));
    add_child(root_node_scene);
  }
}

godot::Node2D *LogicEditor::create_root_node(EntityType entity_type) {
  auto resource_loader = godot::ResourceLoader::get_singleton();
  auto logic_background_node = godot::Node2D::_new();

  auto main_block_texture_rect = godot::TextureRect::_new();
  main_block_texture_rect->set_texture(main_body_atlas);
  logic_background_node->add_child(main_block_texture_rect);

  auto root_node = godot::Node2D::_new();
  root_node->add_child(logic_background_node);

  auto label = godot::Label::_new();
  label->set_text("Tile");
  label->set("custom_fonts/font", resource_loader->load("res://Assets/Fonts/Montserrat.tres"));
  label->set_position(godot::Vector2(50, 50));
  root_node->add_child(label);
  root_node->set_scale(godot::Vector2(0.2f, 0.2f));

  return root_node;
}

godot::Node2D *LogicEditor::create_node(const LogicNodeType *logic_node_type) {
  auto resource_loader = godot::ResourceLoader::get_singleton();
  auto logic_background_node = godot::Node2D::_new();

  const int grid_size = 16;
  const int connector_width = grid_size * 3;

  const int connector1_y_offset = grid_size * 10;
  const int connector2_y_offset = grid_size * 26 + grid_size * 10;

  const godot::Vector2 output_pos = godot::Vector2(-connector_width, connector1_y_offset);
  const godot::Vector2 input_1_pos = godot::Vector2(512 - connector_width, connector1_y_offset);
  const godot::Vector2 input_2_pos = godot::Vector2(512 - connector_width, connector2_y_offset);

  int logic_node_num_inputs = logic_node_type->connections_in.size();

  auto main_block_texture_rect = godot::TextureRect::_new();
  main_block_texture_rect->set_texture(main_body_atlas);
  logic_background_node->add_child(main_block_texture_rect);

  if (logic_node_num_inputs == 2) {
    auto secondary_block_texture_rect = godot::TextureRect::_new();
    secondary_block_texture_rect->set_position(godot::Vector2(0, 26 * 16));
    secondary_block_texture_rect->set_texture(main_body_atlas);
    logic_background_node->add_child(secondary_block_texture_rect);
  }

  if (logic_node_type->connection_out.connection_type != LogicNodeConnection::NONE) {
    auto out_logic_texture_rect = godot::TextureRect::_new();
    out_logic_texture_rect->set_texture(logic_out_atlas_map[logic_node_type->connection_out.connection_type]);
    out_logic_texture_rect->set_position(output_pos);
    logic_background_node->add_child(out_logic_texture_rect);
  }

  if (logic_node_num_inputs > 0 && logic_node_type->connections_in[0].connection_type != LogicNodeConnection::NONE) {
    auto in_logic_texture_rect = godot::TextureRect::_new();
    in_logic_texture_rect->set_texture(logic_in_atlas_map[logic_node_type->connections_in[0].connection_type]);
    in_logic_texture_rect->set_position(input_1_pos);
    logic_background_node->add_child(in_logic_texture_rect);
  }

  if (logic_node_num_inputs == 2 && logic_node_type->connections_in[1].connection_type != LogicNodeConnection::NONE) {
    auto in_logic_texture_rect = godot::TextureRect::_new();
    in_logic_texture_rect->set_texture(logic_in_atlas_map[logic_node_type->connections_in[1].connection_type]);
    in_logic_texture_rect->set_position(input_2_pos);
    logic_background_node->add_child(in_logic_texture_rect);
  }

  logic_background_node->set_modulate(godot::Color::html(godot::String(logic_node_type->color.c_str())));

  auto root_node = godot::Node2D::_new();
  root_node->set_name(godot::String(logic_node_type->name.c_str()));
  root_node->add_child(logic_background_node);

  auto label = godot::Label::_new();
  label->set_text(logic_node_type->name.c_str());
  label->set("custom_fonts/font", resource_loader->load("res://Assets/Fonts/Montserrat.tres"));
  label->set_position(godot::Vector2(50, 50));
  root_node->add_child(label);
  root_node->set_scale(godot::Vector2(0.2f, 0.2f));

  auto area2d = godot::Area2D::_new();
  area2d->set_name("area2d");

  auto shape = godot::Ref<godot::RectangleShape2D>(godot::RectangleShape2D::_new());
  shape->set_extents(godot::Vector2(150, 100));

  int shape_owner = area2d->create_shape_owner(area2d);
  area2d->shape_owner_add_shape(shape_owner, shape);
  area2d->set_position(godot::Vector2(150, 100));
  area2d->connect("input_event", this, "on_logic_piece_input_event", godot::Array::make(root_node, logic_node_type));

  root_node->add_child(area2d);
  return root_node;
}

void LogicEditor::create_atlas() {
  auto resource_loader = godot::ResourceLoader::get_singleton();
  auto main_atlas_texture = resource_loader->load("res://Assets/Textures/jigsaw_pieces.png");

#define LNC LogicNodeConnection
  std::vector<LNC> logic_order = {LNC::BOOLEAN, LNC::NUMERICAL_COMPARISON, LNC::INVENTORY_ITEM, LNC::NUMBER,
                                  LNC::WORKER_TYPE};

  for (int i = 0; i < 5; i++) {
    auto atlas_out = godot::AtlasTexture::_new();
    atlas_out->set_atlas(main_atlas_texture);
    atlas_out->set_region(godot::Rect2(i * 16 * 3, 0, 16 * 3, 16 * 6));
    logic_out_atlas_map.insert(std::pair<LNC, godot::AtlasTexture *>(logic_order[i], atlas_out));
  }

  for (int z = 0; z < 5; z++) {
    auto atlas_in = godot::AtlasTexture::_new();
    atlas_in->set_atlas(main_atlas_texture);
    atlas_in->set_region(godot::Rect2(z * 16 * 3 + (5 * 16 * 3), 0, 16 * 3, 16 * 6));
    logic_in_atlas_map.insert(std::pair<LNC, godot::AtlasTexture *>(logic_order[z], atlas_in));
  }

  main_body_atlas = godot::AtlasTexture::_new();
  main_body_atlas->set_atlas(main_atlas_texture);
  main_body_atlas->set_region(godot::Rect2(0, 16 * 6, 512, 512 - (16 * 6)));

  auto atlas = godot::AtlasTexture::_new();
  atlas->set_atlas(main_atlas_texture);
}

void LogicEditor::on_logic_piece_input_event(godot::Node *node, godot::InputEvent *input_event, int shape_idx, godot::Node* other) {
  if(input_event->get_class() == "InputEventMouseButton") {
    auto mouse_event = reinterpret_cast<const godot::InputEventMouseButton*>(input_event);
    if (mouse_event->get_button_index() == 1) {
      if (mouse_event->is_pressed()) {
        dragged_node = cast_to<godot::Node2D>(other->duplicate());
        dragged_node->set_position(mouse_event->get_position());
        add_child(dragged_node);
      }
    }
  }
}
