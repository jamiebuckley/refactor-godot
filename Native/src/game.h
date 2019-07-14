#ifndef REFACTOR_GRID_SPATIAL_H
#define REFACTOR_GRID_SPATIAL_H

#include <Godot.hpp>
#include <Node.hpp>
#include <PackedScene.hpp>
#include <ResourceLoader.hpp>
#include <SceneTree.hpp>
#include <Viewport.hpp>
#include <InputEvent.hpp>
#include <World.hpp>
#include <PhysicsDirectSpaceState.hpp>
#include <gen/InputEventMouseButton.hpp>
#include <stdexcept>
#include <gen/Button.hpp>
#include <map>

#include "grid/grid.h"
#include "godot_interface.h"

namespace godot
{
    /*
     * Represents the grid component brought into the Godot engine
     */
    class Game : public Spatial, public GodotInterface
    {
        GODOT_CLASS(Game, Spatial)

    private:
        float time_passed;
        Refactor::Grid* grid;

        const std::map<String, Refactor::EntityType> entity_type_map = {
            { String("Worker"), Refactor::EntityType::WORKER },
            { String("Entrance"), Refactor::EntityType::ENTRANCE },
            { String("Exit"), Refactor::EntityType::EXIT },
            { String("Tile"), Refactor::EntityType::TILE },
            { String("Block"), Refactor::EntityType::BLOCK },
        };

    public:
        static void _register_methods();

        Game();
        ~Game();

        void _init();
        void _ready();
        void _process(float delta);
        void _unhandled_input(const InputEvent* event);
        void _on_build_option_button_press(Button* button);

        void set_main_entity(Spatial* _main_entity);

        String add_entity(int x, int z, Vector3, String, Object*);
        bool delete_entity(String id);
        bool is_blocked(int x, int z);
        Vector3 get_entity_coordinates(String id);
        Vector3 get_grid_coords(Vector3 real_coords);
        void step();

        void create_worker(int grid_x, int grid_z, Vector3 orientation) override;
        Spatial* main_entity;

    private:
        Spatial* picker;
        Node* ui;
    };

} // namespace godot


#endif