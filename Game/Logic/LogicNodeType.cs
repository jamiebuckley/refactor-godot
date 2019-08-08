using System;
using System.Collections.Generic;
using Refactor1.Game.Common;
using LC = Refactor1.Game.Logic.LogicNodeConnection;
using LNT = Refactor1.Game.Logic.LogicNodeType;

namespace Refactor1.Game.Logic
{
    public class LogicNodeType : Enumeration
    {

        private static int _currentId = 0;

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

        public static readonly LNT None = new LNT(Colours.White, "None", LC.None, EmptyList());
        public static readonly LNT Root = new LNT(Colours.White, "Root", LC.None, ListOne(LC.Action));
        public static readonly LNT ToggleIf = new LNT(Colours.White, "Toggle If", LC.Action, ListOne(LC.Boolean));
        public static readonly LNT OnIf = new LNT(Colours.White, "On If", LC.Action, ListOne(LC.Boolean));
        public static readonly LNT And = new LNT(Colours.White, "And", LC.Boolean, ListTwo(LC.Boolean, LC.Boolean));
        public static readonly LNT Not = new LNT(Colours.White, "Not", LC.Boolean, ListOne(LC.Boolean));
        public static readonly LNT Or = new LNT(Colours.White, "Or", LC.Boolean, ListTwo(LC.Boolean, LC.Boolean));
        public static readonly LNT Xor = new LNT(Colours.White, "Xor", LC.Boolean, ListTwo(LC.Boolean, LC.Boolean));
        public static readonly LNT WorkerIs = new LNT(Colours.White, "Worker Is", LC.Boolean, ListOne(LC.WorkerType));
        public static readonly LNT WorkerType = new LNT(Colours.White, "Worker Type", LC.WorkerType, EmptyList());
        public static readonly LNT WorkerHas = new LNT(Colours.White, "Worker Has", LC.Boolean, ListTwo(LC.NumericalComparison, LC.InventoryItem));
        public static readonly LNT InventoryItem = new LNT(Colours.White, "Inventory Item", LC.InventoryItem, EmptyList());
        public static readonly LNT CounterIs = new LNT(Colours.White, "Counter Is", LC.Boolean, ListOne(LC.NumericalComparison));
        public static readonly LNT NumericalEquals = new LNT(Colours.White, "Equals", LC.NumericalComparison, ListOne(LC.Number));
        public static readonly LNT NumericalGreaterThan = new LNT(Colours.White, "Greater than", LC.NumericalComparison, ListOne(LC.Number));
        public static readonly LNT NumericalLessThan = new LNT(Colours.White, "Less than", LC.NumericalComparison, ListOne(LC.Number));
        public static readonly LNT Number = new LNT(Colours.White, "Number", LC.Number, EmptyList());
        
        public int Id { get; }
        
        public string Colour { get; }
        
        public string Name { get; }

        public LogicNodeConnection ConnectionOut { get; }
        
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