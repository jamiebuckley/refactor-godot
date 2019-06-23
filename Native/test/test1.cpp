#include "catch.hpp"
#include "include/refactor_grid.h"
#include <iostream>
#include <Vector3.hpp>

TEST_CASE( "Test returns 1", "[test]" ) {
    auto grid = Refactor::Grid(5);
    grid.add_entity(0, 0, Refactor::EntityType::WORKER, godot::Vector3(0, 0, 0));
    REQUIRE( grid.is_blocked(0, 0) == true );
}