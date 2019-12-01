using System;
using System.Collections.Generic;
using Refactor1.Game.Common;
using LC = Refactor1.Game.Logic.LogicNodeConnection;
using LNT = Refactor1.Game.Logic.LogicNodeType;

namespace Refactor1.Game.Logic
{
    public class LogicNodeType : Enumeration
    {

        private static int _currentId;

        private static List<LC> EmptyList()
        {
            return new List<LC>();
        }

        private static List<LC> ListOne(LC lc)
        {
            return new List<LC> {lc};
        }

        private static List<LC> ListTwo(LC lc1, LC lc2)
        {
            return new List<LC> {lc1, lc2};
        }

        public static readonly LNT None = new LNT(Colours.Colour1, "None", LC.None, EmptyList());
        public static readonly LNT Root = new LNT(Colours.Colour2, "Root", LC.None, ListOne(LC.Action));
        public static readonly LNT ToggleIf = new LNT(Colours.Colour3, "Toggle If", LC.Action, ListOne(LC.Boolean));
        public static readonly LNT OnIf = new LNT(Colours.Colour4, "On If", LC.Action, ListOne(LC.Boolean));
        public static readonly LNT And = new LNT(Colours.Colour5, "And", LC.Boolean, ListTwo(LC.Boolean, LC.Boolean));
        public static readonly LNT Not = new LNT(Colours.Colour6, "Not", LC.Boolean, ListOne(LC.Boolean));
        public static readonly LNT Or = new LNT(Colours.Colour7, "Or", LC.Boolean, ListTwo(LC.Boolean, LC.Boolean));
        public static readonly LNT Xor = new LNT(Colours.Colour8, "Xor", LC.Boolean, ListTwo(LC.Boolean, LC.Boolean));
        public static readonly LNT WorkerIs = new LNT(Colours.Colour9, "Worker Is", LC.Boolean, ListOne(LC.WorkerType));
        public static readonly LNT WorkerType = new LNT(Colours.Colour10, "Worker Type", LC.WorkerType, EmptyList());
        public static readonly LNT WorkerHas = new LNT(Colours.Colour11, "Worker Has", LC.Boolean, ListTwo(LC.NumericalComparison, LC.InventoryItem));
        public static readonly LNT InventoryItem = new LNT(Colours.Colour12, "Inventory Item", LC.InventoryItem, EmptyList());
        public static readonly LNT CounterIs = new LNT(Colours.Colour13, "Counter Is", LC.Boolean, ListOne(LC.NumericalComparison));
        public static readonly LNT NumericalEquals = new LNT(Colours.Colour14, "Equals", LC.NumericalComparison, ListOne(LC.Number));
        public static readonly LNT NumericalGreaterThan = new LNT(Colours.Colour15, "Greater than", LC.NumericalComparison, ListOne(LC.Number));
        public static readonly LNT NumericalLessThan = new LNT(Colours.Colour16, "Less than", LC.NumericalComparison, ListOne(LC.Number));
        public static readonly LNT Number = new LNT(Colours.Colour16, "Number", LC.Number, EmptyList());
        
        public int Id { get; }
        
        public string Colour { get; }
        
        public string Name { get; }

        /// <summary>
        /// The 'output' types of the logic node type, e.g. Boolean for a Logical AND node
        /// </summary>
        public LogicNodeConnection ConnectionOut { get; }
        
        /// <summary>
        /// The 'input' types of the logic node type, e.g. two Booleans for a Logical AND node
        /// </summary>
        public List<LogicNodeConnection> ConnectionsIn { get; }

        public LogicNodeType(string colour, string name, LogicNodeConnection connectionOut, List<LogicNodeConnection> connectionsIn) : base(_currentId, name)
        {
            Id = _currentId++;
            Colour = colour;
            Name = name;
            ConnectionOut = connectionOut;
            ConnectionsIn = connectionsIn;
        }
    }
}