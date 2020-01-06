namespace Refactor1.Game.Common
{
    public enum Fabricator
    {
        SMELTER
    }
    
    public class Recipe
    {
        private readonly RecipeElement[] _recipeElement;
        private readonly Fabricator _fabricator;

        public class RecipeElement
        {
            public InventoryItem InventoryItem { get; }
            
            public int Count { get; }

            public RecipeElement(InventoryItem inventoryItem, int count)
            {
                Count = count;
                InventoryItem = inventoryItem;
            }
        }

        public Recipe(Fabricator fabricator, params RecipeElement[] recipeElement)
        {
            _recipeElement = recipeElement;
            _fabricator = fabricator;
        }
    }
}