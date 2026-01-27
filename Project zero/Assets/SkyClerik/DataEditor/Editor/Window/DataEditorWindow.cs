using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor.DataEditor;

namespace UnityEngine.DataEditor
{
    public class DataEditorWindow : EditorWindow
    {
        private VisualElement _mainContentArea;
        private ITabController _activeTabController;
        private TabButtonGroup _tabButtonGroup; // Added for managing tabs

        private readonly Dictionary<string, Func<ITabController>> _tabControllerFactories = new Dictionary<string, Func<ITabController>>();
        private readonly Dictionary<Type, string> _definitionTypeToTabName = new Dictionary<Type, string>();

        [MenuItem("Tools/Data Editor")]
        public static void ShowWindow()
        {
            GetWindow<DataEditorWindow>("Data Editor");
        }

        private void OnEnable()
        {
            //EnsureCategoriesExist();
        }

        //public static void EnsureCategoriesExist()
        //{
        //    string[] requiredCategoryNames = { "Эффект", "Характеристика", "Атака", "Навык", "Экипировка", "Прочее" };

        //    var categoryDatabase = LoadOrCreateDatabase<TraitCategoryDatabase>();
        //    var existingNamesInDb = new HashSet<string>(categoryDatabase.Items.Select(c => c.DefinitionName));

        //    bool databaseModified = false;
        //    foreach (var requiredName in requiredCategoryNames)
        //    {
        //        if (!existingNamesInDb.Contains(requiredName))
        //        {
        //            // Create the actual TraitCategoryDefinition asset
        //            var newAsset = CreateInstance<TraitCategoryDefinition>();
        //            newAsset.SetDefinitionName(requiredName); // Use the new method
        //            newAsset.name = requiredName; // Set the asset's file name

        //            // Create asset file (optional, but good practice for organization)
        //            string directory = "Assets/Game/TraitCategories";
        //            if (!AssetDatabase.IsValidFolder(directory))
        //            {
        //                AssetDatabase.CreateFolder("Assets/Game", "TraitCategories");
        //            }
        //            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{requiredName}.asset");
        //            AssetDatabase.CreateAsset(newAsset, assetPath);

        //            // Add the new asset to the database's list
        //            categoryDatabase.Items.Add(newAsset);
        //            databaseModified = true;
        //            Debug.Log($"Created missing Trait Category Definition: {requiredName}");
        //        }
        //    }

        //    if (databaseModified)
        //    {
        //        EditorUtility.SetDirty(categoryDatabase); // Mark database as dirty to save changes
        //        AssetDatabase.SaveAssets();
        //        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        //    }
        //}


        public void CreateGUI()
        {
            var settings = LoadSettings();

            _tabControllerFactories.Clear();
            _definitionTypeToTabName.Clear();

            var root = rootVisualElement;
            root.Clear();
            root.style.flexGrow = 1;

            var tabsContainer = new VisualElement
            {
                name = "tabs-container",
                style = {
                    flexDirection = FlexDirection.Row,
                    height = 30,
                    borderBottomWidth = 1,
                    borderBottomColor = Color.gray
                }
            };
            root.Add(tabsContainer);

            _mainContentArea = new VisualElement
            {
                name = "main-content-area",
                style = { flexGrow = 1 }
            };
            root.Add(_mainContentArea);

            // Collect all tab buttons first
            var tabButtonsList = new List<Button>();

            // Register tabs and collect their buttons
            RegisterTabAndCollectButton("Навыки", () => new SkillsTabController(_mainContentArea, settings), typeof(SkillBaseDefinition), tabButtonsList);
            RegisterTabAndCollectButton("Юниты", () => new UnitsTabController(_mainContentArea, settings), typeof(UnitBaseDefinition), tabButtonsList);
            
            RegisterTabAndCollectButton("Предметы", () => new ItemsTabController(_mainContentArea, settings), typeof(ItemBaseDefinition), tabButtonsList);
            
            RegisterTabAndCollectButton("Классы", () => new ClassTabController(_mainContentArea, settings), typeof(ClassDefinition), tabButtonsList);
            RegisterTabAndCollectButton("Состояния", () => new StateTabController(_mainContentArea, settings), typeof(StateDefinition), tabButtonsList);
            RegisterTabAndCollectButton("Типы", () => new TypeTabController(_mainContentArea, settings), typeof(TypeDefinition), tabButtonsList);
            
            //RegisterTabAndCollectButton("Настройки", () => new SettingsTabController(_mainContentArea, settings), null, tabButtonsList);

            // Initialize TabButtonGroup with collected buttons
            _tabButtonGroup = new TabButtonGroup(tabButtonsList, Color.green, new Border4(l:2, r:2, t:1, b:0), FlexDirection.Row);
            _tabButtonGroup.OnSelectedButtonChanged += OnTabButtonChanged; // Subscribe to its event
            tabsContainer.Add(_tabButtonGroup); // Add the TabButtonGroup to the tabsContainer

            // Set initial selection
            if (tabButtonsList.Any())
            {
                _tabButtonGroup.SetSelected(tabButtonsList.First());
            }
        }

        public void SwitchToTab(Type definitionType)
        {
            var tabType = _definitionTypeToTabName.Keys.FirstOrDefault(key => key.IsAssignableFrom(definitionType));
            if (tabType != null && _definitionTypeToTabName.TryGetValue(tabType, out string tabName))
            {
                SwitchToTab(tabName);
            }
            else
            {
                Debug.LogWarning($"No tab mapping found for type: {definitionType.Name}");
            }
        }

        public void SwitchToTab(string tabName)
        {
            // Now we don't look up in _tabButtons, we only use the tabName
            if (_tabControllerFactories.TryGetValue(tabName, out Func<ITabController> factory))
            {
                var controller = factory();
                // We pass a dummy button here, as SwitchTab no longer uses activeTabButton
                // The actual active state is managed by TabButtonGroup
                SwitchTab(new Button { text = tabName }, controller);
            }
            else
            {
                Debug.LogWarning($"Tab '{tabName}' is not registered.");
            }
        }

        private void SwitchTab(Button tabButton, ITabController controller)
        {
            _activeTabController?.Unload();

            // The TabButtonGroup handles button highlighting, so we no longer need to do it here
            // if (_activeTabButton != null)
            // {
            //     _activeTabButton.style.backgroundColor = StyleKeyword.Null;
            // }

            // _activeTabButton = tabButton; // No longer needed
            // _activeTabButton.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f); // No longer needed

            _activeTabController = controller;
            _activeTabController.LoadTab();
        }

        private void OnTabButtonChanged(Button selectedButton)
        {
            // The userData is not set for the buttons created for TabButtonGroup directly in DataEditorWindow
            // So we rely on the button's text which corresponds to the tabName
            SwitchToTab(selectedButton.text);
        }

        private void RegisterTabAndCollectButton(string tabName, Func<ITabController> controllerFactory, Type definitionType, List<Button> tabButtonsList)
        {
            var button = new Button { text = tabName };
            tabButtonsList.Add(button); // Add button to the list for TabButtonGroup

            _tabControllerFactories[tabName] = controllerFactory;
            if (definitionType != null)
            {
                _definitionTypeToTabName[definitionType] = tabName;
            }
        }

        public static TDb LoadOrCreateDatabase<TDb>() where TDb : ScriptableObject
        {
            var settings = LoadSettings();
            string folderPath = settings.DatabaseAssetPath;

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string[] folders = folderPath.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string nextPath = $"{currentPath}/{folders[i]}";
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = nextPath;
                }
            }

            string dbPath = $"{folderPath}/{typeof(TDb).Name}.asset";
            TDb database = AssetDatabase.LoadAssetAtPath<TDb>(dbPath);

            if (database == null)
            {
                database = CreateInstance<TDb>();
                AssetDatabase.CreateAsset(database, dbPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return database;
        }

        public static T CreateNewAsset<T>(string subfolder) where T : ScriptableObject
        {
            var settings = LoadSettings();
            string parentFolderPath = settings.DataEntityAssetPath;

            if (!AssetDatabase.IsValidFolder(parentFolderPath))
            {
                string[] folders = parentFolderPath.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string nextPath = $"{currentPath}/{folders[i]}";
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = nextPath;
                }
            }

            string specificFolderPath = $"{parentFolderPath}/{subfolder}";
            if (!AssetDatabase.IsValidFolder(specificFolderPath))
            {
                AssetDatabase.CreateFolder(parentFolderPath, subfolder);
            }

            T asset = CreateInstance<T>();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{specificFolderPath}/New {typeof(T).Name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static DataEditorSettings LoadSettings()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(DataEditorSettings).Name}");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<DataEditorSettings>(path);
            }

            DataEditorSettings settings = CreateInstance<DataEditorSettings>();
            string settingsFolderPath = "Assets/DataEditor/Settings";

            if (!AssetDatabase.IsValidFolder("Assets/DataEditor"))
            {
                AssetDatabase.CreateFolder("Assets", "DataEditor");
            }
            if (!AssetDatabase.IsValidFolder(settingsFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/DataEditor", "Settings");
            }

            string settingsPath = $"{settingsFolderPath}/{typeof(DataEditorSettings).Name}.asset";
            AssetDatabase.CreateAsset(settings, settingsPath);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}