using System;
using Refactor1;
using Refactor1.Game.Common;
using Xunit;

namespace RefactorTests
{
    public class Tests
    {
        class MockGodot : GodotInterface
        {
            public void CreateWorker(Point2D position, GameOrientation orientation)
            {
                
            }
        }
        
        [Fact]
        public void Test1()
        {
            var grid = new Refactor1.Game.Grid(2, new MockGodot());
            var worker = grid.AddEntity(null, EntityType.WORKER, new Point2D(0, 0), GameOrientation.South);
            grid.Step();
            Assert.Equal(worker.CurrentGridTile.Position.X, 0);
            Assert.Equal(worker.CurrentGridTile.Position.Z, 1);
        }
        
        [Fact]
        public void Test2()
        {
            var grid = new Refactor1.Game.Grid(2, new MockGodot());
            var worker1 = grid.AddEntity(null, EntityType.WORKER, new Point2D(0, 0), GameOrientation.South);
            var worker2 = grid.AddEntity(null, EntityType.WORKER, new Point2D(0, 1), GameOrientation.South);
            
            grid.Step();
            Assert.Equal(worker1.CurrentGridTile.Position.X, 0);
            Assert.Equal(worker1.CurrentGridTile.Position.Z, 0);
            
            Assert.Equal(worker2.CurrentGridTile.Position.X, 0);
            Assert.Equal(worker2.CurrentGridTile.Position.Z, 1);
        }
    }
}