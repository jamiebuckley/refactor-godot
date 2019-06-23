#include "include/refactor_grid_spatial.h"

using namespace godot;

void RefactorGridSpatial::_register_methods() {
    register_method("_process", &RefactorGridSpatial::_process);
    register_method("add_entity", &RefactorGridSpatial::add_entity);
    register_method("delete_entity", &RefactorGridSpatial::delete_entity);
    register_method("is_blocked", &RefactorGridSpatial::is_blocked);
    register_method("step", &RefactorGridSpatial::step);
}

RefactorGridSpatial::RefactorGridSpatial(): grid(10) {

}

RefactorGridSpatial::~RefactorGridSpatial() {
    // add your cleanup here
}

void RefactorGridSpatial::_init() {
    // initialize any variables here
    Godot::print("test");
}

void RefactorGridSpatial::_process(float delta) {

}

String RefactorGridSpatial::add_entity(int x, int z, String entity_type, Vector3 orientation) {
    auto grid_entity_type = entity_type_map.find(entity_type);
    if (grid_entity_type == entity_type_map.end()) {
        Godot::print_error("Could not look up entity_type " + entity_type, "add_entity", "refactor_grid_spatial.cpp", 30);
        return String("");
    }
    auto c_string = this->grid.add_entity(x, z, grid_entity_type->second, orientation);
    return String(c_string.data());
}

bool RefactorGridSpatial::delete_entity(String id) {
    char* string = id.alloc_c_string();
    std::string c_string = std::string(string);
    return true;
}

bool RefactorGridSpatial::is_blocked(int x, int z) {
    return grid.is_blocked(x, z);
}

void RefactorGridSpatial::step() {
    grid.step();
}