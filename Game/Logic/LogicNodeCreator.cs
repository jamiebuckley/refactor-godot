using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Refactor1.Game.Common;
using Array = Godot.Collections.Array;

namespace Refactor1.Game.Logic
{
    public class LogicNodeCreator
    {
        private System.Collections.Generic.Dictionary<LogicNodeConnection, AtlasTexture> _connectorOutAtlasMap = new System.Collections.Generic.Dictionary<LogicNodeConnection, AtlasTexture>();
        private System.Collections.Generic.Dictionary<LogicNodeConnection, AtlasTexture> _connectorInAtlasMap = new System.Collections.Generic.Dictionary<LogicNodeConnection, AtlasTexture>();
        private AtlasTexture _mainBodyAtlas;
        private AtlasTexture _ghostAtlas;

        public void Initialise()
        {
            CreateAtlas();
        }

        public GraphicalLogicNode CreateNode(LogicNodeType logicNodeType)
        {
            var backgroundNode = new Node2D();

            int grid_size = 16;
            int connectorWidth = grid_size * 3;

            int connector1YOffset = grid_size * 10;
            int connector2YOffset = grid_size * 26 + grid_size * 10;
              
            Vector2 outputPos = new Vector2(-connectorWidth, connector1YOffset);
            Vector2 input1Pos = new Vector2(512 - connectorWidth, connector1YOffset);
            Vector2 input2Pos = new Vector2(512 - connectorWidth, connector2YOffset);

            int numInputs = logicNodeType.ConnectionsIn.Count;
            
            var mainBlockTexture = new TextureRect();
            mainBlockTexture.SetTexture(_mainBodyAtlas);
            backgroundNode.AddChild(mainBlockTexture);

            if (numInputs == 2)
            {
                var secondaryBlockTextureRect = new TextureRect();
                secondaryBlockTextureRect.SetPosition(new Vector2(0, 26 * 16));
                secondaryBlockTextureRect.SetTexture(_mainBodyAtlas);
                backgroundNode.AddChild(secondaryBlockTextureRect);
            }

            if (logicNodeType.ConnectionOut != LogicNodeConnection.None)
            {
                var outTextureRect = new TextureRect();
                outTextureRect.SetTexture(_connectorOutAtlasMap[logicNodeType.ConnectionOut]);
                outTextureRect.SetPosition(outputPos);
                backgroundNode.AddChild(outTextureRect);
            }

            if (numInputs > 0 && logicNodeType.ConnectionsIn[0] != LogicNodeConnection.None)
            {
                var logicInTextureRect = new TextureRect();
                logicInTextureRect.SetTexture(_connectorInAtlasMap[logicNodeType.ConnectionsIn[0]]);
                logicInTextureRect.SetPosition(input1Pos);
                backgroundNode.AddChild(logicInTextureRect);
            }
            
            if (numInputs == 2 && logicNodeType.ConnectionsIn[1] != LogicNodeConnection.None)
            {
                var logicInTextureRect = new TextureRect();
                logicInTextureRect.SetTexture(_connectorInAtlasMap[logicNodeType.ConnectionsIn[1]]);
                logicInTextureRect.SetPosition(input2Pos);
                backgroundNode.AddChild(logicInTextureRect);
            }
            
            backgroundNode.SetModulate(new Color(logicNodeType.Colour));
            
            var rootNode = new GraphicalLogicNode();
            rootNode.AddChild(backgroundNode);
            
            var label = new Label();
            label.SetText(logicNodeType.Name);
            label.Set("custom_fonts/font", ResourceLoader.Load("res://Assets/Fonts/Montserrat.tres"));
            label.SetPosition(new Vector2(50, 50));
            rootNode.AddChild(label);
            
            var area2D = new Area2D();
            area2D.SetName("area2d");
            
            var shape = new RectangleShape2D();
            shape.SetExtents(new Vector2(512, 512));

            int shapeOwner = area2D.CreateShapeOwner(area2D);
            area2D.ShapeOwnerAddShape(shapeOwner, shape);
            area2D.SetPosition(new Vector2(512, 512));
            rootNode.AddChild(area2D);
            
            rootNode.Area2d = area2D;
            rootNode.Type = logicNodeType;
            
            return rootNode;
        }

        void CreateAtlas()
        {
            var logic_pieces_texture = ResourceLoader.Load<Texture>("res://Assets/Textures/jigsaw_pieces.png");

            var logicOrder = new List<LogicNodeConnection>
            {
                LogicNodeConnection.Action,
                LogicNodeConnection.Boolean,
                LogicNodeConnection.NumericalComparison,
                LogicNodeConnection.InventoryItem,
                LogicNodeConnection.Number,
                LogicNodeConnection.WorkerType
            };

            var chunk = 16;
            var connectorWidth = chunk * 3;
            var connectorHeight = chunk * 6;
            var offset = logicOrder.Count * connectorWidth;

            var mainBlockWidth = 512;
            var mainBlockHeight = 512 - connectorHeight;
            
            for (int i = 0; i < 6; i++)
            {
                var atlasOut = new AtlasTexture();
                atlasOut.SetAtlas(logic_pieces_texture);
                atlasOut.SetRegion(new Rect2(i * connectorWidth, 0, connectorWidth, connectorHeight));
                _connectorOutAtlasMap.Add(logicOrder[i], atlasOut);
                
                var atlasIn = new AtlasTexture();
                atlasIn.SetAtlas(logic_pieces_texture);
                atlasIn.SetRegion(new Rect2(i * connectorWidth + offset, 0, connectorWidth, connectorHeight));
                _connectorInAtlasMap.Add(logicOrder[i], atlasIn);
            }
            
            _mainBodyAtlas = new AtlasTexture();
            _mainBodyAtlas.SetAtlas(logic_pieces_texture);
            _mainBodyAtlas.SetRegion(new Rect2(0, connectorHeight, mainBlockWidth, mainBlockHeight));

            _ghostAtlas = new AtlasTexture();
            _ghostAtlas.SetAtlas(logic_pieces_texture);
            _ghostAtlas.SetRegion(new Rect2(0, connectorHeight + (chunk * 26), mainBlockWidth, mainBlockHeight));
        }
    }
}