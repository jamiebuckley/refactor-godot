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
#include <gen/InputEventKey.hpp>
#include <GlobalConstants.hpp>
#include <stdexcept>
#include <gen/Button.hpp>
#include <map>
#include <memory>

#include "grid/grid.h"
#include "godot_interface.h"

namespace godot
{
    /*
     * Represents the grid component brought into the Godot engine
     */
class Game : public Spatial, public Refactor::GodotInterface
    {
        GODOT_CLASS(Game, Spatial)

    private:
        float time_passed;
        std::shared_ptr<Refactor::Grid> grid;

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

        Refactor::GridEntity * add_entity(int x, int z, Vector3, String, Object*);
        bool is_blocked(int x, int z);
        Vector3 get_entity_coordinates(String id);
        Vector3 closest_grid_position(Vector3 real_coords);
        Vector3 get_grid_coords(Vector3 real_coords);
        Vector3 get_world_coords(int x, int z);
        void step();

        void create_worker(int grid_x, int grid_z, Vector3 orientation) override;
        Spatial* main_entity;

    private:
        Spatial* picker;
        Node* ui;
        Refactor::EntityType entity_type = Refactor::EntityType::NONE;
        void handle_mouse_click(const InputEventMouseButton *mouse_event);
        void handle_grid_coords_click(Vector3 grid_coords);

        std::map<String, Refactor::EntityType> button_names_to_entity_types = {
                { "BOptDirectionalTileButton", Refactor::EntityType::TILE },
                { "BOptEntranceButton", Refactor::EntityType::ENTRANCE },
                { "BOptExitButton", Refactor::EntityType::EXIT }
        };

        Ref<PackedScene> picker_scene;
        Ref<PackedScene> entrance_scene;
        Ref<PackedScene> exit_scene;
        Ref<PackedScene> tile_scene;
        Ref<PackedScene> worker_scene;

        float TILE_SIZE = 1.0f;
        float pulse_timer = 0.0f;
};

} // namespace godot


#endif