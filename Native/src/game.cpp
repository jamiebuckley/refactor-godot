#include <memory>

#include <memory>

#include <sstream>
#include <entities/worker.h>
#include <godot_entities/godot_worker.h>
#include "game.h"
#include "Maths.h"

using namespace godot;

#pragma clang diagnostic push
#pragma ide diagnostic ignored "OCUnusedGlobalDeclarationInspection"
void Game::_register_methods() {
    register_method("init", &Game::_init);
    register_method("_ready", &Game::_ready);
    register_method("_process", &Game::_process);
    register_method("_unhandled_input", &Game::_unhandled_input);
    register_method("_on_build_option_button_press", &Game::_on_build_option_button_press);
    register_method("add_entity", &Game::add_entity);
    register_method("is_blocked", &Game::is_blocked);
    register_method("step", &Game::step);
    register_method("get_entity_coordinates", &Game::get_entity_coordinates);
    register_method("set_main_entity", &Game::set_main_entity);
}
#pragma clang diagnostic pop

Game::Game(): GodotInterface() {

}

Game::~Game(){
  Godot::print("~game");
}

void Game::_init() {
  Godot::print("init");
  this->grid = std::make_shared<Refactor::Grid>(20, this);
}

void Game::_ready() {
  Godot::print("Native::Game::ready");

  this->ui = get_node("/root/RootSpatial/UI");

  // wire up option buttons
  auto buildOptionButtons = this->ui->get_tree()->get_nodes_in_group("BuildOptionButton");
  for(int i = 0; i < buildOptionButtons.size(); i++) {
    auto button_variant = godot::Object::___get_from_variant(buildOptionButtons[i]);
    auto buildOptionButton = cast_to<Button>(button_variant);
    buildOptionButton->connect("pressed", this, "_on_build_option_button_press", Array::make(buildOptionButton));
  }

  // load resources
  auto resource_loader = ResourceLoader::get_singleton();
  picker_scene = resource_loader->load("res://Prototypes/Picker.tscn");
  entrance_scene = resource_loader->load("res://Prototypes/Entrance.tscn");
  exit_scene = resource_loader->load("res://Prototypes/Exit.tscn");
  tile_scene = resource_loader->load("res://Prototypes/DirectionalTile.tscn");
  worker_scene = resource_loader->load("res://Prototypes/Worker.tscn");

  this->picker = reinterpret_cast<Spatial*>(picker_scene->instance());
  get_parent()->add_child(picker);
}


void Game::_process(float delta) {
  Dictionary result = main_entity->call("_get_world_mouse_position");
  if (!result.empty()) {
    this->picker->set_visible(true);
    Vector3 position = result["position"];
    this->picker->set_translation(closest_grid_position(position));
  }

  // Increase pulse timer and step if necessary
  pulse_timer += delta;
  if (pulse_timer > 1.0) {
    grid->step();
    pulse_timer = fmodf(pulse_timer, 1.0f);
  }
}

void Game::_unhandled_input(const InputEvent* event) {
  if(event->get_class() == "InputEventMouseButton") {
    auto mouse_event = reinterpret_cast<const InputEventMouseButton*>(event);
    if (mouse_event->is_pressed()) {
      this->handle_mouse_click(mouse_event);
    }
  }
}

void Game::_on_build_option_button_press(Button* button) {
  if (button_names_to_entity_types.count(button->get_name()) > 0) {
    entity_type = button_names_to_entity_types[button->get_name()];
    Godot::print("Selected entity_type: " + button->get_name());
  }
  else {
    Godot::print("No matching entity type for button " + button->get_name());
  }
}

void Game::set_main_entity(Spatial* spatial) {
  this->main_entity = spatial;
  print(spatial->get_name().alloc_c_string());
}

Refactor::GridEntity * Game::add_entity(int x, int z, Vector3 orientation, String selected_entity_type, Object* entity) {
    Godot::print("add_entity");
    auto grid_entity_type = entity_type_map.find(selected_entity_type);
    if (grid_entity_type == entity_type_map.end()) {
        Godot::print_error("Could not look up entity_type " + selected_entity_type, "add_entity", "refactor_grid_spatial.cpp", 30);
        throw std::logic_error("Tried to add en entity of type " + std::string(selected_entity_type.alloc_c_string()) + " which does not exist");
    }
    auto created_entity = this->grid->add_entity(x, z, orientation, grid_entity_type->second,
                                           reinterpret_cast<Spatial *>(entity));
    return created_entity;
}

bool Game::is_blocked(int x, int z) {
    return grid->is_blocked(x, z);
}

void Game::step() {
    grid->step();
}

Vector3 Game::get_entity_coordinates(String id) {
    char* string = id.alloc_c_string();
    std::string c_string = std::string(string);
    auto coordinates = grid->get_entity_coordinates(string);
    return coordinates;
}

Vector3 Game::closest_grid_position(Vector3 real_coords) {
  auto xToTile = floorf(real_coords.x / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2);
  auto zToTile = floorf(real_coords.z / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2);
  return {xToTile, real_coords.y, zToTile};
}

Vector3 Game::get_grid_coords(godot::Vector3 real_coords) {
  auto offsetWorldX = real_coords.x + (grid->getSize() * TILE_SIZE) / 2;
  auto offsetWorldZ = real_coords.z + (grid->getSize() * TILE_SIZE) / 2;
  return {
            floorf(offsetWorldX / TILE_SIZE),
            real_coords.y,
            floorf(offsetWorldZ / TILE_SIZE)
          };
}

void Game::create_worker(int grid_x, int grid_z, Vector3 orientation) {
  if(!grid->can_place_entity_type(grid_x, grid_z, EntityType::WORKER)) {
    return;
  }

  auto worker_instance = GodotWorker::cast_to<GodotWorker>(worker_scene->instance());
  auto world_coords = get_world_coords(grid_x, grid_z);
  worker_instance->set_translation(Vector3(world_coords.x, 0.0f, world_coords.z));
  auto grid_entity = add_entity(grid_x, grid_z, orientation, "Worker", worker_instance);
  grid_entity->setGodotEntity(worker_instance);

  worker_instance->set_game(this);
  worker_instance->set_destination(Vector3(grid_x, 0, grid_z));
  add_child(worker_instance);
}

void Game::handle_mouse_click(const InputEventMouseButton *mouse_event) {
  // todo: Bug in godot prevents doing this in c++
  Dictionary result = main_entity->call("_get_world_mouse_position");
  if (result.empty()) return;

  Vector3 position = result["position"];
  auto grid_coords = get_grid_coords(position);
  this->handle_grid_coords_click(grid_coords);
}

void Game::handle_grid_coords_click(Vector3 grid_coords) {
  Godot::print("Handle grid coords click");
  if (!entity_type) {
    return;
  }

  if(!grid->can_place_entity_type((int)grid_coords.x, (int)grid_coords.z, entity_type)) {
    return;
  }

  int grid_coords_x = (int)grid_coords.x;
  int grid_coords_z = (int)grid_coords.z;

  // check validity of entrance and exit entity placement
  int minAxis = 0;
  int maxAxis = 19;
  if (entity_type == Refactor::EntityType::ENTRANCE || entity_type == Refactor::EntityType::EXIT) {
    auto validX = (grid_coords_x == minAxis || grid_coords_x == maxAxis) && (grid_coords_z > minAxis && grid_coords_z < maxAxis);
    auto validZ = (grid_coords_z == minAxis || grid_coords_z == maxAxis) && (grid_coords_x > minAxis && grid_coords_x < maxAxis);
    if (!validX && !validZ) {
      // entrance/exit position is not on edge
      return;
    }
  }

  Spatial *instance = nullptr;
  if (entity_type == Refactor::EntityType::ENTRANCE) {
    instance = cast_to<Spatial>(entrance_scene->instance());
  }
  else if (entity_type == Refactor::EntityType::EXIT) {
    instance = cast_to<Spatial>(exit_scene->instance());
  }
  else if (entity_type == Refactor::EntityType::TILE) {
    instance = cast_to<Spatial>(tile_scene->instance());
  }

  if (instance == nullptr) {
    Godot::print_error("Spatial node was null", __FUNCTION__, __FILE__, __LINE__);
  }

  if (entity_type == Refactor::EntityType::ENTRANCE || entity_type == Refactor::EntityType::EXIT) {
    auto orientation = Maths::get_edge_orientation(grid_coords_x, grid_coords_z, 0, 19);
    instance->rotate(Vector3(0, 1, 0), Maths::get_rotation_from_vector(orientation));
    grid->add_entity(grid_coords_x, grid_coords_z, orientation, entity_type, instance);
  }
  else {
      grid->add_entity(grid_coords_x, grid_coords_z, Vector3(1, 0, 0), entity_type, instance);
  }

  instance->set_translation(picker->get_translation());
  add_child(instance);
  // create the entity
}

Vector3 Game::get_world_coords(int x, int z) {
  return {
          (x * TILE_SIZE) - (grid->getSize() * TILE_SIZE) / 2 + (TILE_SIZE / 2),
          0.0f,
          (z * TILE_SIZE) - (grid->getSize() * TILE_SIZE) / 2 + (TILE_SIZE / 2)
          };
}
