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
        public void WorkerShouldMoveToEmptyTile()
        {
            var grid = new Refactor1.Game.Grid(2, 1.0f, new MockGodot());
            var worker = grid.AddEntity(null, EntityType.WORKER, new Point2D(0, 0), GameOrientation.North);
            grid.Step();
            Assert.Equal(0,worker.CurrentGridTile.Position.X);
            Assert.Equal(1,worker.CurrentGridTile.Position.Z);
        }
        
        [Fact]
        public void ImmobileWorkersShouldBlock()
        {
            var grid = new Refactor1.Game.Grid(2,1.0f, new MockGodot());
            var worker1 = grid.AddEntity(null, EntityType.WORKER, new Point2D(0, 0), GameOrientation.North);
            var worker2 = grid.AddEntity(null, EntityType.WORKER, new Point2D(0, 1), GameOrientation.North);
            
            grid.Step();
            Assert.Equal(0, worker1.CurrentGridTile.Position.X);
            Assert.Equal(0, worker1.CurrentGridTile.Position.Z);
            
            Assert.Equal(0, worker2.CurrentGridTile.Position.X);
            Assert.Equal(1, worker2.CurrentGridTile.Position.Z);
        }

        [Fact]
        public void WorkersShouldNotCrossPaths()
        {
            var grid = new Refactor1.Game.Grid(2,1.0f, new MockGodot());
            
            var worker1 = grid.AddEntity(null, EntityType.WORKER, new Point2D(0, 0), GameOrientation.North);
            var worker2 = grid.AddEntity(null, EntityType.WORKER, new Point2D(0, 1), GameOrientation.South);
            
            grid.Step();
            Assert.Equal(0, worker1.CurrentGridTile.Position.X);
            Assert.Equal(0, worker1.CurrentGridTile.Position.Z);
            
            Assert.Equal(0, worker2.CurrentGridTile.Position.X);
            Assert.Equal(1, worker2.CurrentGridTile.Position.Z);
            
        }
    }
}