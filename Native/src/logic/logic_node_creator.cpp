//
// Created by jamie on 04/08/19.
//

#include "logic_node_creator.h"

using namespace Refactor;

void LogicNodeCreator::setup() {
  this->create_atlas();
}

std::vector<godot::Control*> LogicNodeCreator::create_toolbox() {
  std::vector<godot::Control*> toolbox;
  auto logic_node_types = LogicNodeTypes::getInstance();
  auto all_types = logic_node_types->toVector();
  for(int i = 1; i < all_types.size(); i++) {
    auto current_type = all_types[i];
    auto visual_node = create_node(current_type);
    visual_node->set_scale(godot::Vector2(0.2, 0.2));

    auto pickable_control = godot::Control::_new();
    pickable_control->add_child(visual_node);
    visual_node->set_position(godot::Vector2(5, 5));
    pickable_control->set_custom_minimum_size(godot::Vector2(150, 150));
    toolbox.emplace_back(pickable_control);
  }
  return toolbox;
}

GhostNode *LogicNodeCreator::create_ghost_node() {
  auto logic_background_node = godot::Node2D::_new();
  auto main_block_texture_rect = godot::TextureRect::_new();
  main_block_texture_rect->set_texture(ghost_atlas);
  logic_background_node->add_child(main_block_texture_rect);

  auto root_node = GhostNode::_new();
  root_node->add_child(logic_background_node);
  root_node->set_scale(godot::Vector2(0.2f, 0.2f));

  return root_node;
}

godot::Node2D *LogicNodeCreator::create_root_node(EntityType entity_type) {
  auto resource_loader = godot::ResourceLoader::get_singleton();
  auto logic_background_node = godot::Node2D::_new();

  auto main_block_texture_rect = godot::TextureRect::_new();
  main_block_texture_rect->set_texture(entry_point_atlas);
  logic_background_node->add_child(main_block_texture_rect);

  logic_background_node->set_modulate(godot::Color::html(godot::String("#42f5cb")));

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

ToolboxNode *LogicNodeCreator::create_node(const LogicNodeType *logic_node_type) {
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

  auto root_node = ToolboxNode::_new();
  root_node->set_logic_node_type(logic_node_type);
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
  area2d->connect("input_event", event_listener, "on_logic_piece_input_event", godot::Array::make(root_node, logic_node_type));

  root_node->add_child(area2d);
  return root_node;
}

/**
 * populates
 * logic_out_atlas_map (lookup map of logic_node_connection to logic_out graphical connector)
 * logic_in_atlas_map  (lookup map of logic_node_connection to logic_in graphical connector)
 * main_body_atlas
 * ghost_atlas
 * entry_point_atlas
 * from the atlas texture
 */
void LogicNodeCreator::create_atlas() {
  auto resource_loader = godot::ResourceLoader::get_singleton();
  auto main_atlas_texture = resource_loader->load("res://Assets/Textures/jigsaw_pieces.png");

#define LNC LogicNodeConnection
  std::vector<LNC> logic_order = {LNC::ACTION, LNC::BOOLEAN, LNC::NUMERICAL_COMPARISON, LNC::INVENTORY_ITEM, LNC::NUMBER,
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

  ghost_atlas = godot::AtlasTexture::_new();
  ghost_atlas->set_atlas(main_atlas_texture);
  ghost_atlas->set_region(godot::Rect2(0, 16 * 6 + 16 * 26, 514, 512 - (16 * 6)));

  entry_point_atlas = godot::AtlasTexture::_new();
  entry_point_atlas->set_atlas(main_atlas_texture);
  entry_point_atlas->set_region(godot::Rect2(512, 16 * 6, 512, 512 - (16 * 6)));
}