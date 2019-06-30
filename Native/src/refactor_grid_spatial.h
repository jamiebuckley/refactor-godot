#ifndef REFACTOR_GRID_SPATIAL_H
#define REFACTOR_GRID_SPATIAL_H

#include <map>
#include <Godot.hpp>
#include <Spatial.hpp>
#include "grid/grid.h"
#include "godot_interface.h"

namespace godot
{
    /*
     * Represents the grid component brought into the Godot engine
     */
    class RefactorGridSpatial : public Spatial, public GodotInterface
    {
        GODOT_CLASS(RefactorGridSpatial, Spatial)

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

        RefactorGridSpatial();
        ~RefactorGridSpatial();

        void _init(); // our initializer called by Godot
        void _process(float delta);
        void set_main_entity(Spatial* _main_entity);

        String add_entity(int x, int z, Vector3, String, Object*);
        bool delete_entity(String id);
        bool is_blocked(int x, int z);
        Vector3 get_entity_coordinates(String id);
        void step();

        void create_worker(int grid_x, int grid_z, Vector3 orientation) override;

        Spatial* main_entity;
    };

} // namespace godot


#endif