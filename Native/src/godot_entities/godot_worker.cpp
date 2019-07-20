//
// Created by jamie on 20/07/19.
//

#include <Maths.h>
#include "godot_worker.h"

using namespace godot;

void GodotWorker::_register_methods() {
  register_method("init", &GodotWorker::_init);
  register_method("_ready", &GodotWorker::_ready);
  register_method("_process", &GodotWorker::_process);
}

void GodotWorker::_init() {
  Godot::print("Worker init");
}

void GodotWorker::_ready() {
  Godot::print("Worker ready");
}

void GodotWorker::set_destination(Vector3 new_destination) {
  previous_coordinates = this->destination;
  destination = new_destination;
}

void GodotWorker::_process(float delta) {
  if(this->game == nullptr) {
    return;
  }

  auto difference = (destination - previous_coordinates).normalized();
  auto rot = Maths::get_rotation_from_vector(difference);
  auto degrees = rot * 57.2958;
  set_rotation_degrees(Vector3(0, degrees, 0));

  auto world_coords = game->get_world_coords((int)destination.x, (int)destination.z);
  auto travel_difference = Vector3(world_coords.x, 0, world_coords.z) - get_translation();
  set_translation(get_translation() + (travel_difference * 0.1f));
}

void GodotWorker::set_game(Game* new_game) {
  this->game = new_game;
}
