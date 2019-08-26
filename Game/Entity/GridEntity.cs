using Godot;
using Refactor1.Game.Common;

namespace Refactor1.Game.Entity
{
    public class GridEntity
    {
        public EntityType EntityType { get; set; }
            
        public Node GodotEntity { get; set; }
            
        public Grid.GridTile CurrentGridTile { get; set; }
            
        public GameOrientation Orientation { get; set; }
    }
}