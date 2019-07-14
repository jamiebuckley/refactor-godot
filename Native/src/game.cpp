#include <sstream>
#include "game.h"
#include "Maths.h"
//#include <Spatial.hpp>
//#include <ResourceLoader.hpp>
//#include <PackedScene.hpp>

using namespace godot;

void Game::_register_methods() {
    register_method("init", &Game::_init);
    register_method("_ready", &Game::_ready);
    register_method("_process", &Game::_process);
    register_method("_unhandled_input", &Game::_unhandled_input);
    register_method("_on_build_option_button_press", &Game::_on_build_option_button_press);
    register_method("add_entity", &Game::add_entity);
    register_method("delete_entity", &Game::delete_entity);
    register_method("is_blocked", &Game::is_blocked);
    register_method("step", &Game::step);
    register_method("get_entity_coordinates", &Game::get_entity_coordinates);
    register_method("set_main_entity", &Game::set_main_entity);
}

Game::Game(): GodotInterface() {
  grid = new Grid(20, this);
}

Game::~Game(){

}

void Game::_init() {
  Godot::print("init");
}

void Game::_ready() {
  Godot::print("Native::Game::ready");
  this->ui = get_node("/root/RootSpatial/UI");
  auto buildOptionButtons = this->ui->get_tree()->get_nodes_in_group("BuildOptionButton");
  for(int i = 0; i < buildOptionButtons.size(); i++) {
    Variant v = buildOptionButtons[i];
    auto obj = godot::Object::___get_from_variant(v);
    Button* buildOptionButton = reinterpret_cast<Button*>(obj);
    buildOptionButton->connect("pressed", this, "_on_build_option_button_press", Array::make(buildOptionButton));
  }

  auto resource_loader = ResourceLoader::get_singleton();
  Ref<PackedScene> picker_scene = resource_loader->load("res://Prototypes/Picker.tscn");
  this->picker = reinterpret_cast<Spatial*>(picker_scene->instance());
  get_parent()->add_child(picker);
}


void Game::_process(float delta) {
  Dictionary result = main_entity->call("_get_world_mouse_position");
  if (!result.empty()) {
    this->picker->set_visible(true);
    Vector3 position = result["position"];
    this->picker->set_translation(get_grid_coords(position));
  }
}

void Game::_unhandled_input(const InputEvent* event) {

}

void Game::_on_build_option_button_press(Button* button) {
  Godot::print(button->get_name());
}

void Game::set_main_entity(Spatial* spatial) {
  this->main_entity = spatial;
  print(spatial->get_name().alloc_c_string());
}

String Game::add_entity(int x, int z, Vector3 orientation, String entity_type, Object* entity) {
    Godot::print("add_entity");
    auto grid_entity_type = entity_type_map.find(entity_type);
    if (grid_entity_type == entity_type_map.end()) {
        Godot::print_error("Could not look up entity_type " + entity_type, "add_entity", "refactor_grid_spatial.cpp", 30);
        return String("");
    }
    auto c_string = this->grid->add_entity(x, z, orientation, grid_entity_type->second,
                                           static_cast<Spatial *>(entity));
    return String(c_string.data());
}

bool Game::delete_entity(String id) {
    char* string = id.alloc_c_string();
    std::string c_string = std::string(string);
    return true;
}

bool Game::is_blocked(int x, int z) {
    Godot::print("is blocked");
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

Vector3 Game::get_grid_coords(Vector3 real_coords) {
  float TILE_SIZE = 1.0f;
  auto xToTile = floor(real_coords.x / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2);
  auto zToTile = floor(real_coords.z / TILE_SIZE) * TILE_SIZE  + (TILE_SIZE / 2);
  return Vector3(xToTile, real_coords.y, zToTile);
}

void Game::create_worker(int grid_x, int grid_z, Vector3 orientation) {
  Variant test = this->main_entity->call("add_worker", grid_x, grid_z, orientation);
  auto object = Object::___get_from_variant(test);
  add_entity(grid_x, grid_z, orientation, "Worker", object);
}