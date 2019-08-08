using System;
using System.Collections.Generic;
using Godot;
using Refactor1.Game.Common;

namespace Refactor1.Game.Logic
{
    public class LogicNodeCreator
    {
        private Dictionary<LogicNodeConnection, AtlasTexture> _connectorOutAtlasMap = new Dictionary<LogicNodeConnection, AtlasTexture>();
        private Dictionary<LogicNodeConnection, AtlasTexture> _connectorInAtlasMap = new Dictionary<LogicNodeConnection, AtlasTexture>();
        private AtlasTexture _mainBodyAtlas;
        private AtlasTexture _ghostAtlas;

        Node2D CreateNode(LogicNodeType logicNodeType)
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

//
//  if (logic_node_num_inputs > 0 && logic_node_type->connections_in[0].connection_type != LogicNodeConnection::NONE) {
//    auto in_logic_texture_rect = godot::TextureRect::_new();
//    in_logic_texture_rect->set_texture(logic_in_atlas_map[logic_node_type->connections_in[0].connection_type]);
//    in_logic_texture_rect->set_position(input_1_pos);
//    logic_background_node->add_child(in_logic_texture_rect);
//  }
//
//  if (logic_node_num_inputs == 2 && logic_node_type->connections_in[1].connection_type != LogicNodeConnection::NONE) {
//    auto in_logic_texture_rect = godot::TextureRect::_new();
//    in_logic_texture_rect->set_texture(logic_in_atlas_map[logic_node_type->connections_in[1].connection_type]);
//    in_logic_texture_rect->set_position(input_2_pos);
//    logic_background_node->add_child(in_logic_texture_rect);
//  }
//
//  logic_background_node->set_modulate(godot::Color::html(godot::String(logic_node_type->color.c_str())));
//
//  auto root_node = ToolboxNode::_new();
//  root_node->set_logic_node_type(logic_node_type);
//  root_node->set_name(godot::String(logic_node_type->name.c_str()));
//  root_node->add_child(logic_background_node);
//
//  auto label = godot::Label::_new();
//  label->set_text(logic_node_type->name.c_str());
//  label->set("custom_fonts/font", resource_loader->load("res://Assets/Fonts/Montserrat.tres"));
//  label->set_position(godot::Vector2(50, 50));
//  root_node->add_child(label);
//
//  root_node->set_scale(godot::Vector2(0.2f, 0.2f));
//
//  auto area2d = godot::Area2D::_new();
//  area2d->set_name("area2d");
//
//  auto shape = godot::Ref<godot::RectangleShape2D>(godot::RectangleShape2D::_new());
//  shape->set_extents(godot::Vector2(150, 100));
//
//  int shape_owner = area2d->create_shape_owner(area2d);
//  area2d->shape_owner_add_shape(shape_owner, shape);
//  area2d->set_position(godot::Vector2(150, 100));
//  area2d->connect("input_event", event_listener, "on_logic_piece_input_event", godot::Array::make(root_node, logic_node_type));
//
//  root_node->add_child(area2d);
//  return root_node;
            throw new NotImplementedException();
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
            
            for (int i = 0; i < 5; i++)
            {
                var atlasOut = new AtlasTexture();
                atlasOut.SetAtlas(logic_pieces_texture);
                atlasOut.SetRegion(new Rect2(i * connectorWidth, 0, connectorWidth, connectorHeight));
                _connectorOutAtlasMap.Add(logicOrder[i], atlasOut);
                
                var atlasIn = new AtlasTexture();
                atlasIn.SetAtlas(logic_pieces_texture);
                atlasIn.SetRegion(new Rect2(i * connectorWidth + offset, 0, connectorWidth, connectorHeight));
                _connectorOutAtlasMap.Add(logicOrder[i], atlasIn);
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