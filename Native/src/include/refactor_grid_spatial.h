#ifndef REFACTOR_GRID_SPATIAL_H
#define REFACTOR_GRID_SPATIAL_H

#include <map>
#include <Godot.hpp>
#include <Spatial.hpp>
#include "refactor_grid.h"

namespace godot
{
    /*
     * Represents the grid component brought into the Godot engine
     */
    class RefactorGridSpatial : public Spatial
    {
        GODOT_CLASS(RefactorGridSpatial, Spatial)

    private:
        float time_passed;
        Refactor::Grid grid;

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

        String add_entity(int x, int z, String entity_type, Vector3);
        bool delete_entity(String id);
        bool is_blocked(int x, int z);
        Vector3 get_entity_coordinates(String id);
        void step();
    };

} // namespace godot


#endif