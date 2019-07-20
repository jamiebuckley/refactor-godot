#include "catch.hpp"
#include "grid/grid.h"
#include <iostream>
#include <Vector3.hpp>
#include <memory>
#include <cstdlib>
#include <exception>

#include "godot_entities/godot_worker.h"

class FakeGodotInterface: public Refactor::GodotInterface {
public:
    void print(const char *message) override {

    }

    void print_error(const char *message, const char *function, const char *file, int line) override {

    }

    bool call_godot() override {
      return false;
    }
};

godot::GodotWorker worker;

TEST_CASE( "New real_entity blocks grid tile", "[Grid]" ) {
    auto grid = std::make_shared<Refactor::Grid>(5, new FakeGodotInterface());
    grid->add_entity(0, 0,godot::Vector3(0, 0, 0), Refactor::EntityType::WORKER, nullptr);
    REQUIRE(grid->is_blocked(0, 0));
}

TEST_CASE( "Query position by ID", "[Grid]" ) {
    auto grid = std::make_shared<Refactor::Grid>(5, new FakeGodotInterface());
    auto entity = grid->add_entity(1, 1,godot::Vector3(0, 0, 0), Refactor::EntityType::WORKER, &worker);
    auto coords = grid->get_entity_coordinates(entity->getId());
    REQUIRE( coords.x == 1.0f );
    REQUIRE( coords.y == 0.0f );
    REQUIRE( coords.z == 1.0f );
}

TEST_CASE( "Step moves workers", "[Grid]" ) {
    auto grid = std::make_shared<Refactor::Grid>(5, new FakeGodotInterface());
    auto entity = grid->add_entity(0, 0, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, &worker);
    grid->step();
    auto coords = grid->get_entity_coordinates(entity->getId());
    REQUIRE( coords.x == 0.0f );
    REQUIRE( coords.y == 0.0f );
    REQUIRE( coords.z == 1.0f );
}

TEST_CASE( "Workers block worker", "[Grid]" ) {
    auto grid = std::make_shared<Refactor::Grid>(2, new FakeGodotInterface());
    auto entity1 = grid->add_entity(0, 0, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, &worker);
    auto entity2 = grid->add_entity(0, 1, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, &worker);
    grid->step();

    // Worker 1 is blocked by 2
    auto coords1 = grid->get_entity_coordinates(entity1->getId());
    REQUIRE( coords1.x == 0.0f );
    REQUIRE( coords1.y == 0.0f );
    REQUIRE( coords1.z == 0.0f );

    // Worker 2 can't go anywhere
    auto coords2 = grid->get_entity_coordinates(entity2->getId());
    REQUIRE( coords2.x == 0.0f );
    REQUIRE( coords2.y == 0.0f );
    REQUIRE( coords2.z == 1.0f );
}

TEST_CASE( "Blocking cascades", "[Grid]" ) {
    auto grid = std::make_shared<Refactor::Grid>(3, new FakeGodotInterface());
    auto entity1 = grid->add_entity(0, 0, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, &worker);
    auto entity2 = grid->add_entity(0, 1, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, &worker);
    auto entity3 = grid->add_entity(0, 2, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, &worker);
    grid->step();

    // Worker 1 is blocked by 2
    auto coords1 = grid->get_entity_coordinates(entity1->getId());
    REQUIRE( coords1.x == 0.0f );
    REQUIRE( coords1.y == 0.0f );
    REQUIRE( coords1.z == 0.0f );

    // Worker 2 is blocked by 3
    auto coords2 = grid->get_entity_coordinates(entity2->getId());
    REQUIRE( coords2.x == 0.0f );
    REQUIRE( coords2.y == 0.0f );
    REQUIRE( coords2.z == 1.0f );

    // Worker 3 can't go anywhere
    auto coords3 = grid->get_entity_coordinates(entity3->getId());
    REQUIRE( coords3.x == 0.0f );
    REQUIRE( coords3.y == 0.0f );
    REQUIRE( coords3.z == 2.0f );
}

TEST_CASE( "Move onto vacated square", "[Grid]" ) {
    auto grid = std::make_shared<Refactor::Grid>(2, new FakeGodotInterface());
    auto entity1 = grid->add_entity(0, 0, godot::Vector3(0, 0, 1), Refactor::EntityType::WORKER, &worker);
    auto entity2 = grid->add_entity(0, 1, godot::Vector3(1, 0, 0), Refactor::EntityType::WORKER, &worker);
    grid->step();

    // Worker 1 is blocked by 2
    auto coords1 = grid->get_entity_coordinates(entity1->getId());
    REQUIRE( coords1.x == 0.0f );
    REQUIRE( coords1.y == 0.0f );
    REQUIRE( coords1.z == 1.0f );

    // Worker 2 can't go anywhere
    auto coords2 = grid->get_entity_coordinates(entity2->getId());
    REQUIRE( coords2.x == 1.0f );
    REQUIRE( coords2.y == 0.0f );
    REQUIRE( coords2.z == 1.0f );
}