#include <sstream>
#include "refactor_grid_spatial.h"

using namespace godot;

void RefactorGridSpatial::_register_methods() {
    register_method("_process", &RefactorGridSpatial::_process);
    register_method("add_entity", &RefactorGridSpatial::add_entity);
    register_method("delete_entity", &RefactorGridSpatial::delete_entity);
    register_method("is_blocked", &RefactorGridSpatial::is_blocked);
    register_method("step", &RefactorGridSpatial::step);
    register_method("get_entity_coordinates", &RefactorGridSpatial::get_entity_coordinates);
    register_method("set_main_entity", &RefactorGridSpatial::set_main_entity);
}

RefactorGridSpatial::RefactorGridSpatial(): GodotInterface() {
  grid = new Grid(20, this);
}

RefactorGridSpatial::~RefactorGridSpatial() {
    // add your cleanup here
}

void RefactorGridSpatial::_init() {

}

void RefactorGridSpatial::set_main_entity(Spatial* spatial) {
  this->main_entity = spatial;
  print(spatial->get_name().alloc_c_string());
}

void RefactorGridSpatial::_process(float delta) {
    Godot::print("process");
}

String RefactorGridSpatial::add_entity(int x, int z, Vector3 orientation, String entity_type, Object* entity) {
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

bool RefactorGridSpatial::delete_entity(String id) {
    char* string = id.alloc_c_string();
    std::string c_string = std::string(string);
    return true;
}

bool RefactorGridSpatial::is_blocked(int x, int z) {
    Godot::print("is blocked");
    return grid->is_blocked(x, z);
}

void RefactorGridSpatial::step() {
    Godot::print("step");
    grid->step();
}

Vector3 RefactorGridSpatial::get_entity_coordinates(String id) {
    char* string = id.alloc_c_string();
    std::string c_string = std::string(string);
    auto coordinates = grid->get_entity_coordinates(string);
    return coordinates;
}

void RefactorGridSpatial::create_worker(int grid_x, int grid_z, Vector3 orientation) {
  Variant test = this->main_entity->call("add_worker", grid_x, grid_z, orientation);
  auto object = Object::___get_from_variant(test);
  add_entity(grid_x, grid_z, orientation, "Worker", object);
}
