#include "catch.hpp"
#include "grid.h"
#include <iostream>
#include <Vector3.hpp>

#include <cstdlib>
#include <exception>

class FakeGodotInterface: public GodotInterface {
public:
    void print(const char *message) override {

    }

    void print_error(const char *message, const char *function, const char *file, int line) override {

    }

    bool call_godot() override {
      return false;
    }
};

TEST_CASE( "New entity blocks grid tile", "[Grid]" ) {
    auto grid = Refactor::Grid(5, new FakeGodotInterface());
    grid.add_entity(0, 0,godot::Vector3(0, 0, 0), Refactor::EntityType::WORKER, nullptr);
    REQUIRE( grid.is_blocked(0, 0) == true );
}

TEST_CASE( "Query position by ID", "[Grid]" ) {
    auto grid = Refactor::Grid(5, new FakeGodotInterface());
    auto id = grid.add_entity(1, 1,godot::Vector3(0, 0, 0), Refactor::EntityType::WORKER, nullptr);
    auto coords = grid.get_entity_coordinates(id);
    REQUIRE( coords.x == 1.0f );
    REQUIRE( coords.y == 0.0f );
    REQUIRE( coords.z == 1.0f );
}

TEST_CASE( "Step moves workers", "[Grid]" ) {
    auto grid = Refactor::Grid(5, new FakeGodotInterface());
    auto id = grid.add_entity(0, 0, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, nullptr);
    grid.step();
    auto coords = grid.get_entity_coordinates(id);
    REQUIRE( coords.x == 0.0f );
    REQUIRE( coords.y == 0.0f );
    REQUIRE( coords.z == 1.0f );
}

TEST_CASE( "Workers block worker", "[Grid]" ) {
    auto grid = Refactor::Grid(2, new FakeGodotInterface());
    auto id1 = grid.add_entity(0, 0, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, nullptr);
    auto id2 = grid.add_entity(0, 1, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, nullptr);
    grid.step();

    // Worker 1 is blocked by 2
    auto coords1 = grid.get_entity_coordinates(id1);
    REQUIRE( coords1.x == 0.0f );
    REQUIRE( coords1.y == 0.0f );
    REQUIRE( coords1.z == 0.0f );

    // Worker 2 can't go anywhere
    auto coords2 = grid.get_entity_coordinates(id2);
    REQUIRE( coords2.x == 0.0f );
    REQUIRE( coords2.y == 0.0f );
    REQUIRE( coords2.z == 1.0f );
}

TEST_CASE( "Blocking cascades", "[Grid]" ) {
    auto grid = Refactor::Grid(3, new FakeGodotInterface());
    auto id1 = grid.add_entity(0, 0, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, nullptr);
    auto id2 = grid.add_entity(0, 1, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, nullptr);
    auto id3 = grid.add_entity(0, 2, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, nullptr);
    grid.step();

    // Worker 1 is blocked by 2
    auto coords1 = grid.get_entity_coordinates(id1);
    REQUIRE( coords1.x == 0.0f );
    REQUIRE( coords1.y == 0.0f );
    REQUIRE( coords1.z == 0.0f );

    // Worker 2 is blocked by 3
    auto coords2 = grid.get_entity_coordinates(id2);
    REQUIRE( coords2.x == 0.0f );
    REQUIRE( coords2.y == 0.0f );
    REQUIRE( coords2.z == 1.0f );

    // Worker 3 can't go anywhere
    auto coords3 = grid.get_entity_coordinates(id3);
    REQUIRE( coords3.x == 0.0f );
    REQUIRE( coords3.y == 0.0f );
    REQUIRE( coords3.z == 2.0f );
}

TEST_CASE( "Move onto vacated square", "[Grid]" ) {
    auto grid = Refactor::Grid(2, new FakeGodotInterface());
    auto id1 = grid.add_entity(0, 0, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, nullptr);
    auto id2 = grid.add_entity(0, 1, godot::Vector3(1, 0, 0), Refactor::EntityType::WORKER, nullptr);
    grid.step();

    // Worker 1 is blocked by 2
    auto coords1 = grid.get_entity_coordinates(id1);
    REQUIRE( coords1.x == 0.0f );
    REQUIRE( coords1.y == 0.0f );
    REQUIRE( coords1.z == 1.0f );

    // Worker 2 can't go anywhere
    auto coords2 = grid.get_entity_coordinates(id2);
    REQUIRE( coords2.x == 1.0f );
    REQUIRE( coords2.y == 0.0f );
    REQUIRE( coords2.z == 1.0f );
}