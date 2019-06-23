#include "catch.hpp"
#include "include/refactor_grid.h"
#include <iostream>
#include <Vector3.hpp>

TEST_CASE( "New entity blocks grid tile", "[Grid]" ) {
    auto grid = Refactor::Grid(5);
    grid.add_entity(0, 0, Refactor::EntityType::WORKER, godot::Vector3(0, 0, 0));
    REQUIRE( grid.is_blocked(0, 0) == true );
}

TEST_CASE( "Query position by ID", "[Grid]" ) {
    auto grid = Refactor::Grid(5);
    auto id = grid.add_entity(1, 1, Refactor::EntityType::WORKER, godot::Vector3(0, 0, 0));
    auto coords = grid.get_entity_coordinates(id);
    REQUIRE( coords.x == 1.0f );
    REQUIRE( coords.y == 0.0f );
    REQUIRE( coords.z == 1.0f );
}

TEST_CASE( "Step moves workers", "[Grid]" ) {
    auto grid = Refactor::Grid(5);
    auto id = grid.add_entity(0, 0, Refactor::EntityType::WORKER, godot::Vector3(0, 0, 1));
    grid.step();
    auto coords = grid.get_entity_coordinates(id);
    REQUIRE( coords.x == 0.0f );
    REQUIRE( coords.y == 0.0f );
    REQUIRE( coords.z == 1.0f );
}