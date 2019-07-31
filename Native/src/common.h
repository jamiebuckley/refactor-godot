//
// Created by jamie on 24/06/19.
//
#include <Godot.hpp>
#include <map>
#include <string>

#ifndef REFACTOR_NATIVE_COMMON_H
#define REFACTOR_NATIVE_COMMON_H

namespace Refactor {
    enum EntityType {
        WORKER,
        ENTRANCE,
        EXIT,
        TILE,
        BLOCK,
        LOGIC,
        NONE
    };

    const std::map<EntityType, std::string> entity_type_names = {
            {EntityType::WORKER, "Worker"},
            {EntityType::ENTRANCE, "Entrance"},
            {EntityType::EXIT, "Exit"},
            {EntityType::TILE, "Tile"},
            {EntityType::BLOCK, "Block"},
            {EntityType::LOGIC, "Logic"},
            {EntityType::NONE, "None"},
    };

    const godot::Vector3 ORIENTATION_NORTH = godot::Vector3(0, 0, -1);
    const godot::Vector3 ORIENTATION_EAST = godot::Vector3(1, 0, 0);
    const godot::Vector3 ORIENTATION_SOUTH = godot::Vector3(0, 0, 1);
    const godot::Vector3 ORIENTATION_WEST = godot::Vector3(-1, 0, 0);

    class OrientationUtils {
    public:
        static godot::Vector3 clockwise_of(godot::Vector3 orientation) {
          godot::Godot::print("clockwise");
          if (orientation == ORIENTATION_NORTH) {
            godot::Godot::print("east");
            return ORIENTATION_EAST;
          }
          else if (orientation == ORIENTATION_EAST) {
            godot::Godot::print("south");
            return ORIENTATION_SOUTH;
          }
          else if (orientation == ORIENTATION_SOUTH) {
            godot::Godot::print("west");
            return ORIENTATION_WEST;
          }
          else if (orientation == ORIENTATION_WEST) {
            godot::Godot::print("north");
            return ORIENTATION_NORTH;
          }
          return ORIENTATION_NORTH;
        }

        static godot::Vector3 anti_clockwise_of(godot::Vector3 orientation) {
          if (orientation == ORIENTATION_NORTH) {
            return ORIENTATION_WEST;
          }
          else if (orientation == ORIENTATION_WEST) {
            godot::Godot::print("north");
            return ORIENTATION_SOUTH;
          }
          else if (orientation == ORIENTATION_SOUTH) {
            return ORIENTATION_EAST;
          }
          else if (orientation == ORIENTATION_EAST) {
            return ORIENTATION_NORTH;
          }
          return ORIENTATION_NORTH;
        }
    };
}

#endif //REFACTOR_NATIVE_COMMON_H
