using UnityEngine.DataEditor;
using SkyClerik.CraftingSystem;
using UnityEngine.UIElements;

namespace UnityEditor.DataEditor
{
    public class RecipesTabController : ITabController
    {
        private VisualElement _container;
        private DataEditorSettings _settings;

        private ListViewPanel<CraftingRecipe, RecipeDatabase> _recipesListView;
        private RecipeDetailPanel _recipeDetailPanel;

        public RecipesTabController(VisualElement container, DataEditorSettings settings)
        {
            _settings = settings;
            _container = container;
        }

        public void LoadTab()
        {
            _container.Clear();
            var root = new VisualElement { name = "recipes-tab-root", style = { flexGrow = 1 } };
            _container.Add(root);

            var splitView = new TwoPaneSplitView(0, 300, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            var leftPanel = new VisualElement();
            splitView.Add(leftPanel);

            _recipeDetailPanel = new RecipeDetailPanel(() => _recipesListView.RebuildList());
            splitView.Add(_recipeDetailPanel);

            _recipesListView = new ListViewPanel<CraftingRecipe, RecipeDatabase>(
                leftPanel,
                () => DataEditorWindow.CreateNewAsset<CraftingRecipe>("Recipes")
            );
            _recipesListView.OnDefinitionSelected += _recipeDetailPanel.DisplayDetails;
            _recipesListView.RebuildList();
        }

        public void Unload()
        {
            _recipesListView = null;
            _recipeDetailPanel = null;
            _container.Clear();
        }
    }
}
