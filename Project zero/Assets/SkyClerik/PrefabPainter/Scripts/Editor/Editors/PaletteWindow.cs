using CollisionBear.WorldEditor.Brushes;
using CollisionBear.WorldEditor.Extensions;
using CollisionBear.WorldEditor.Utils;
using SkyClerik;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CollisionBear.WorldEditor
{
    public class PaletteWindow : EditorWindow
    {
        private const float LeftOffset = 2;
        private const float ButtonPadding = 3;
        private const float RightOffset = 24;
        private const float ScrollBarWidth = 16;

        static readonly Vector2 WindowMinSize = new Vector2(200, 150);
        private PrefabPainterDefinition _data;

        [MenuItem(KalderaEditorUtils.WindowBasePath)]
        public static void ShowWindow()
        {
            var window = GetWindow<PaletteWindow>();
            window.minSize = WindowMinSize;

            if (EditorStartupRunner.TryGetData(typeof(PrefabPainterDefinition), out ScriptableObject data))
            {
                window._data = (PrefabPainterDefinition)data;
                window.titleContent = new GUIContent(window._data.TitleGuiContent);
                window.ShowUtility();
            }
            else
            {
                window = null;
            }
        }

        public static void RefreshAllWindows()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<PaletteWindow>())
            {
                window.UpdateAvailableAssets();
                window.Repaint();
            }
        }

        [SerializeField]
        private SelectableAsset SelectedAsset;

        [SerializeField]
        private Palette SelectedPalette;

        [SerializeField]
        private ScenePlacer ScenePlacer = new ScenePlacer();

        [SerializeField]
        public SelectionSettings SelectionSettings = new SelectionSettings();

        [SerializeField]
        private Vector2 CurrentWindowScroll;

        private Dictionary<KeyCode, Palette> ShortKeysIndexMapping;

        [SerializeField]
        private bool IsPlacementModeHintsOpen;
        [SerializeField]
        private bool IsRaycastModeHintsOpen;

        [SerializeField]
        private List<SelectableAsset> AvailableAssets = new List<SelectableAsset>();

        //[SerializeField]
        //private GUIContent[] AvailableAssetGuiContent = new GUIContent[0];

        [SerializeField]
        private List<string> AvailableAssetContent = new List<string>();

        [SerializeField]
        private List<Palette> AvailablePalettes = new List<Palette>();

        [SerializeField]
        private GUIContent[] AvailablePalettesGuiContent = new GUIContent[0];

        private VisualElement _areaSelectPalette;
        private VisualElement _brushTypeToolArea;
        private VisualElement _toolOptionArea;
        private VisualElement _paletteArea;
        private int _selectedIndex = 0;

        private bool IsMousePressed = false;
        private double LastTimestamp;

        void OnEnable()
        {
            wantsMouseEnterLeaveWindow = true;

            if (ScenePlacer == null)
            {
                ScenePlacer = new ScenePlacer();
            }

            ScenePlacer.OnEnable();

            LastTimestamp = EditorApplication.timeSinceStartup;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            SceneView.duringSceneGui += OnSceneView;
            EditorSceneManager.sceneClosing += SceneChange;
            AssemblyReloadEvents.afterAssemblyReload += ClearSelection;

            UpdateAvailableAssets();
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneView;
            EditorSceneManager.sceneClosing -= SceneChange;
            AssemblyReloadEvents.afterAssemblyReload -= ClearSelection;
        }

        public void UpdateAvailableAssets()
        {
            UpdateShortKeys();
            AvailableAssets = GetAvailableSelectableAssets();
            //AvailableAssetGuiContent = GetSelectableAssetsContent(AvailableAssets);

            AvailableAssetContent.Clear();
            for (int i = 0; i < AvailableAssets.Count; i++)
            {
                AvailableAssetContent.Add(AvailableAssets[i].name);
            }

            // TODO: Repeated code from 412. Unify
            if (SelectedAsset is PaletteSet paletteSet)
            {
                AvailablePalettes = paletteSet.Categories.Where(c => c != null).ToList();
                AvailablePalettesGuiContent = AvailablePalettes.Select(c => new GUIContent(c.name)).ToArray();
            }

            if (!AvailablePalettes.Contains(SelectedPalette))
            {
                SelectedPalette = AvailablePalettes.Count == 0 ? null : AvailablePalettes[0];
            }
        }

        protected void SceneChange(Scene scene, bool removingScene)
        {
            ClearSelection();
        }

        //protected void SelectionChanged()
        //{
        //    ClearSelection();
        //}

        protected bool IsSelectableAsset(SelectableAsset selectedAsset)
        {
            if (selectedAsset == null || !(selectedAsset is PaletteSet))
            {
                return false;
            }

            var selectedAssetSet = (PaletteSet)SelectedAsset;
            if (selectedAssetSet.Categories == null)
            {
                return false;
            }

            return true;
        }

        protected void UpdateShortKeys()
        {
            ShortKeysIndexMapping = new Dictionary<KeyCode, Palette>();

            if (!IsSelectableAsset(SelectedAsset))
            {
                return;
            }

            var selectedAssetSet = (PaletteSet)SelectedAsset;
            for (int i = 0; i < selectedAssetSet.Categories.Count; i++)
            {
                var category = selectedAssetSet.Categories[i];
                if (category == null)
                {
                    continue;
                }

                if (category.ShortKey != KeyCode.None && !ShortKeysIndexMapping.ContainsKey(category.ShortKey))
                {
                    ShortKeysIndexMapping.Add(category.ShortKey, selectedAssetSet.Categories[i]);
                }
            }
        }

        public void ClearSelection()
        {
            ScenePlacer.ClearSelection();
        }

        public void OnSceneView(SceneView sceneView)
        {
            sceneView.Repaint();

            Event currentEvent = Event.current;

            if (!ScenePlacer.CurrentBrush.ShowBrush(ScenePlacer))
            {
                if (IsSpaceKey(currentEvent))
                {
                    ScenePlacer.RestoreSelection();
                    currentEvent.Use();
                    Repaint();
                }

                return;
            }

            if (ScenePlacer.CurrentBrush == null)
            {
                ClearSelection();
                return;
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var adjustedInworldPosition = ScenePlacer.GetInWorldPoint(currentEvent.GUIPointToRay());
            if (adjustedInworldPosition.HasValue)
            {
                ScenePlacer.DrawBrushHandle(adjustedInworldPosition.Value);
            }

            HandleInput(currentEvent, adjustedInworldPosition);
            HandleShortCuts(currentEvent);
        }

        private void HandleInput(Event currentEvent, Vector3? adjustedInworldPosition)
        {
            if (currentEvent.type == EventType.Layout)
            {
                return;
            }
            else if (currentEvent.type == EventType.MouseEnterWindow)
            {
                IsMousePressed = false;
                ScenePlacer.UpdatePlacements();
                currentEvent.Use();
                return;
            }
            else if (currentEvent.type == EventType.MouseLeaveWindow)
            {
                // Ensures any drag motions is stopped if the mouse pointer leaves Unity's editor window
                if (IsMousePressed)
                {
                    ScenePlacer.EndPlacement();
                    ScenePlacer.DestroyPlacementObjects();
                    currentEvent.Use();
                    return;
                }
            }
            else

                if (currentEvent.isMouse && currentEvent.button == 0)
                {
                    if (currentEvent.type == EventType.MouseDown)
                    {
                        IsMousePressed = true;
                    }
                    else if (currentEvent.type == EventType.MouseUp)
                    {
                        IsMousePressed = false;
                    }
                }

            if (!adjustedInworldPosition.HasValue)
            {
                return;
            }

            HandleToolInput(currentEvent, adjustedInworldPosition.Value);
        }

        private void HandleToolInput(Event currentEvent, Vector3 adjustedInworldPosition)
        {
            if (currentEvent.type == EventType.Repaint)
            {
                return;
            }

            var deltaTime = EditorApplication.timeSinceStartup - LastTimestamp;
            LastTimestamp = EditorApplication.timeSinceStartup;

            // Resumes brush after placement
            if (currentEvent.isMouse)
            {
                if (!ScenePlacer.HasPlacementSelection)
                {
                    ScenePlacer.GeneratePlacement();
                    ScenePlacer.MovePosition(currentEvent.mousePosition, adjustedInworldPosition);
                }
            }

            // Start placement 
            if (!currentEvent.shift && currentEvent.isMouse && currentEvent.IsPureMouseStartClick())
            {
                ScenePlacer.StartPlacement(currentEvent.mousePosition);
                currentEvent.Use();
                return;
            }

            // Active drag
            if (!currentEvent.shift && currentEvent.isMouse && currentEvent.IsPureMouseDrag())
            {
                ScenePlacer.ActiveDragPlacement(adjustedInworldPosition, deltaTime);
                ScenePlacer.UpdatePlacements();
                currentEvent.Use();
                return;
            }

            // Toggling shift key
            if (currentEvent.keyCode == KeyCode.LeftShift && IsMousePressed && !currentEvent.alt && !currentEvent.control)
            {
                if (currentEvent.type == EventType.KeyDown)
                {
                    ScenePlacer.ShiftDragPlacement(adjustedInworldPosition, deltaTime);
                    ScenePlacer.UpdatePlacements();
                    currentEvent.Use();
                    return;
                }
                else if (currentEvent.type == EventType.KeyUp)
                {
                    ScenePlacer.ActiveDragPlacement(adjustedInworldPosition, deltaTime);
                    ScenePlacer.UpdatePlacements();
                    currentEvent.Use();
                    return;
                }
            }

            // Passive drag 
            if (!currentEvent.shift && IsMousePressed && currentEvent.NoModifiers())
            {
                ScenePlacer.PassiveDragPlacement(adjustedInworldPosition, deltaTime);
                currentEvent.Use();
                return;
            }

            // Shift drag
            if (currentEvent.shift && !currentEvent.alt && !currentEvent.control)
            {
                if (IsMousePressed)
                {
                    ScenePlacer.ShiftDragPlacement(adjustedInworldPosition, deltaTime);
                    currentEvent.Use();
                    return;
                }
            }

            // End placement
            if (currentEvent.isMouse && currentEvent.IsPureMouseEndClick())
            {
                ScenePlacer.EndPlacement();
                ScenePlacer.DestroyPlacementObjects();
                currentEvent.Use();
                return;
            }

            // Cycle prefab variant tool 
            if (KalderaSpawnEffectsSettings.CyclePrefabHotkey.EventMatch(currentEvent))
            {
                ScenePlacer.CurrentBrush.CycleVariant(KalderaSpawnEffectsSettings.CyclePrefabHotkey.GetDelta(currentEvent), ScenePlacer);
                ScenePlacer.MovePosition(currentEvent.mousePosition, ScenePlacer.PlacementPosition);
                currentEvent.Use();
                return;
            }

            // Scale tool 
            if (KalderaSpawnEffectsSettings.ScalePrefabHotkey.EventMatch(currentEvent))
            {
                ScenePlacer.CurrentBrush.ScaleBrush(-currentEvent.delta.y, ScenePlacer);
                ScenePlacer.UpdatePlacements();
                currentEvent.Use();
                return;
            }

            // Hover
            if (currentEvent.shift && !currentEvent.alt && !currentEvent.control)
            {
                if (!IsMousePressed)
                {
                    ScenePlacer.ShiftHoverPlacement(adjustedInworldPosition, deltaTime);
                    currentEvent.Use();
                    return;
                }
            }

            // Move
            if (currentEvent.NoModifiers())
            {
                ScenePlacer.MovePosition(currentEvent.mousePosition, adjustedInworldPosition);
            }
        }

        private bool IsSpaceKey(Event currentEvent) => currentEvent.isKey && currentEvent.keyCode == KeyCode.Space && currentEvent.type == EventType.KeyDown;

        private void HandleShortCuts(Event currentEvent)
        {
            if (currentEvent.type == EventType.Layout || currentEvent.type == EventType.Repaint)
            {
                return;
            }

            if (ScenePlacer?.CurrentBrush == null)
            {
                return;
            }

            if (EditorGUIUtility.editingTextField)
            {
                return;
            }

            //if (currentEvent.type == EventType.KeyDown) {
            //    return;
            //}

            if (currentEvent.shift)
            {
                foreach (var key in ShortKeysIndexMapping.Keys)
                {
                    if (currentEvent.keyCode == key)
                    {
                        SelectedPalette = ShortKeysIndexMapping[key];
                        currentEvent.Use();
                        return;
                    }
                }
            }

            foreach (var brush in ScenePlacer.GetBrushMapping())
            {
                if (brush.KeyBinding.EventMatch(currentEvent))
                {
                    PressBrushFromHotkey(brush, currentEvent);
                    return;
                }
            }

            if (ScenePlacer.CurrentBrush.HandleKeyEvents(currentEvent, ScenePlacer))
            {
                currentEvent.Use();
                return;
            }
        }

        public void PressBrushFromHotkey(IBrushButton button, Event currentEvent)
        {
            button.OnButtonPress(this);
            NotifyChange();
            currentEvent.Use();
        }

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            AreaSelectPalette();
            AreaDrawSetPlacementMode();
            AreaDrawSetRaycastMode();

            UpdatePaletteAreas();
        }

        private void UpdatePaletteAreas()
        {
            AreaDrawBrushTypeTool();
            //AreaToolOption();
            AreaViewPalette();
        }

        private void AreaSelectPalette()
        {
            if (_areaSelectPalette != null)
                return;

            _areaSelectPalette = new VisualElement();
            _areaSelectPalette.style.flexDirection = FlexDirection.Row;
            _areaSelectPalette.style.justifyContent = Justify.SpaceBetween;
            _areaSelectPalette.style.alignItems = Align.Center;
            rootVisualElement.Add(_areaSelectPalette);

            //---

            var paletteField = new PopupField<string>("Palette", AvailableAssetContent, 0);
            paletteField.style.flexShrink = 1;
            paletteField.value = AvailableAssetContent[0];
            _areaSelectPalette.Add(paletteField);

            paletteField.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                paletteField.value = evt.newValue;
                for (int i = 0; i < AvailableAssetContent.Count; i++)
                {
                    _selectedIndex = i;
                    if (AvailableAssetContent[i] == evt.newValue)
                    {
                        SetSelectedAsset(AvailableAssets[_selectedIndex]);
                        SelectedPalette = GetSelectedWindowCategory(SelectedAsset);
                        UpdatePaletteAreas();
                    }
                }
            });

            //---

            Button inProjectButton = new Button();
            SetSprite(inProjectButton, _data.Dir);
            inProjectButton.clicked += () =>
            {
                Debug.Log($"selectedIndex : {_selectedIndex}");
                ShowAssetInProject(AvailableAssets[_selectedIndex]);
            };
            var minSize = new StyleLength(16);
            inProjectButton.style.minWidth = minSize;
            inProjectButton.style.minHeight = minSize;
            _areaSelectPalette.Add(inProjectButton);
        }

        private void AreaDrawSetPlacementMode()
        {
            VisualElement placementModeRoot = new VisualElement();
            placementModeRoot.style.flexDirection = FlexDirection.Row;
            placementModeRoot.style.justifyContent = Justify.SpaceBetween;
            placementModeRoot.style.alignItems = Align.Center;
            rootVisualElement.Add(placementModeRoot);

            //---

            var guiContent = ScenePlacer.GetPlacementModeGuiContent();
            List<string> content = new List<string>();
            for (int i = 0; i < guiContent.Length; i++)
                content.Add(guiContent[i].text);

            var paletteField = new PopupField<string>("Collider mode", content, 0);
            paletteField.style.flexShrink = 1;
            paletteField.value = content[0];
            placementModeRoot.Add(paletteField);

            paletteField.RegisterCallback<ChangeEvent<string>>((EventCallback<ChangeEvent<string>>)((evt) =>
            {
                paletteField.value = evt.newValue;
                var placementModeIndex = 0;
                for (int i = 0; i < content.Count; i++)
                {
                    placementModeIndex = i;
                    if (content[i] == evt.newValue)
                    {
                        if (placementModeIndex != ScenePlacer.SelectionSettings.PlacementModeIndex)
                        {
                            SetPlacementMode(placementModeIndex);
                        }
                    }
                }
            }));
        }

        private void AreaDrawSetRaycastMode()
        {
            VisualElement raycastModeRoot = new VisualElement();
            raycastModeRoot.style.flexDirection = FlexDirection.Row;
            raycastModeRoot.style.justifyContent = Justify.SpaceBetween;
            raycastModeRoot.style.alignItems = Align.Center;
            rootVisualElement.Add(raycastModeRoot);

            //---

            var guiContent = ScenePlacer.GeRaycastModeGuiContent();
            List<string> content = new List<string>();
            for (int i = 0; i < guiContent.Length; i++)
                content.Add(guiContent[i].text);

            var paletteField = new PopupField<string>("Raycast mode", content, 0);
            paletteField.style.flexShrink = 1;
            paletteField.value = content[0];
            raycastModeRoot.Add(paletteField);

            paletteField.RegisterCallback<ChangeEvent<string>>((EventCallback<ChangeEvent<string>>)((evt) =>
            {
                paletteField.value = evt.newValue;
                var raycastModeIndex = 0;
                for (int i = 0; i < content.Count; i++)
                {
                    raycastModeIndex = i;
                    if (content[i] == evt.newValue)
                    {
                        if (raycastModeIndex != ScenePlacer.SelectionSettings.RaycastModeIndex)
                        {
                            SetRaycastNode(raycastModeIndex);
                        }
                    }
                }
            }));
        }

        private void AreaDrawBrushTypeTool()
        {
            _brushTypeToolArea?.Clear();

            _brushTypeToolArea = new VisualElement();
            _brushTypeToolArea.pickingMode = PickingMode.Ignore;
            _brushTypeToolArea.style.flexGrow = 0;
            _brushTypeToolArea.style.backgroundColor = _data.BlackA;
            rootVisualElement.Add(_brushTypeToolArea);

            Label paletteLabelContent = new Label();
            var currentIndex = Mathf.Min(ScenePlacer.SelectionSettings.SelectedBrushIndex, ScenePlacer.GetBrushMapping().Length - 1);
            paletteLabelContent.text = $"{KalderaEditorUtils.SelectToolsContent.text}   {ScenePlacer.GetBrushMapping()[currentIndex].KeyBinding.SettingName}";
            _brushTypeToolArea.Add(paletteLabelContent);

            VisualElement buttonLineArea = new VisualElement();
            buttonLineArea.pickingMode = PickingMode.Ignore;
            buttonLineArea.style.flexGrow = 0;
            buttonLineArea.style.flexDirection = FlexDirection.Row;
            buttonLineArea.style.flexWrap = Wrap.Wrap;
            _brushTypeToolArea.Add(buttonLineArea);

            var brushes = ScenePlacer.GetBrushMapping();

            foreach (var brush in brushes)
            {
                Button but = new Button();
                but.style.backgroundImage = new StyleBackground(brush.GetButtonTexture());
                but.clicked += () =>
                {
                    brush.OnButtonPress(this);
                    NotifyChange();
                };
                but.style.width = _data.IconButtonSize;
                but.style.height = _data.IconButtonSize;
                buttonLineArea.Add(but);
            }
        }

        private void AreaToolOption()
        {
            _toolOptionArea?.Clear();

            _toolOptionArea = new VisualElement();
            _toolOptionArea.pickingMode = PickingMode.Ignore;
            rootVisualElement.Add(_toolOptionArea);

            var settings = ScenePlacer.SelectionSettings;
            var foldout = new Foldout() { text = "Tool options" };
            foldout.value = false;
            _toolOptionArea.Add(foldout);

            VisualElement drawAdditionalArea = new VisualElement();
            drawAdditionalArea.style.flexDirection = FlexDirection.Row;
            foldout.contentContainer.Add(drawAdditionalArea);

            Button but2 = new Button();
            but2.tooltip = "settings.ParentObjectsToBaseObject = !settings.ParentObjectsToBaseObject;";
            but2.style.backgroundImage = new StyleBackground(KalderaEditorUtils.ParentObjectToBaseObjectContent.image as Texture2D);
            but2.clicked += () => { settings.ParentObjectsToBaseObject = !settings.ParentObjectsToBaseObject; };
            but2.style.width = _data.IconButtonSize;
            but2.style.height = _data.IconButtonSize;
            drawAdditionalArea.Add(but2);

            Button but3 = new Button();
            but3.tooltip = "settings.OrientToGroundNormal = !settings.OrientToGroundNormal;";
            but3.style.backgroundImage = new StyleBackground(KalderaEditorUtils.OrientToGroundNormalContent.image as Texture2D);
            but3.clicked += () => { settings.OrientToGroundNormal = !settings.OrientToGroundNormal; };
            but3.style.width = _data.IconButtonSize;
            but3.style.height = _data.IconButtonSize;
            drawAdditionalArea.Add(but3);

            VisualElement objectLimitArea = new VisualElement();
            objectLimitArea.style.flexDirection = FlexDirection.Row;
            foldout.contentContainer.Add(objectLimitArea);

            SliderInt sliderInt = new SliderInt();
            sliderInt.tooltip = "sliderInt.value = settings.ObjectLimit";
            sliderInt.style.flexGrow = 1;
            sliderInt.lowValue = 0;
            sliderInt.value = settings.ObjectLimit;
            sliderInt.highValue = 10000;
            objectLimitArea.Add(sliderInt);

            IntegerField integerField = new IntegerField();
            integerField.style.flexGrow = 0;
            integerField.value = settings.ObjectLimit;
            objectLimitArea.Add(integerField);


            sliderInt.RegisterCallback<ChangeEvent<int>>((evt) =>
            {
                sliderInt.value = evt.newValue;
                integerField.value = evt.newValue;
            });

            integerField.RegisterCallback<ChangeEvent<int>>((evt) =>
            {
                sliderInt.value = evt.newValue;
                integerField.value = evt.newValue;
            });
        }

        private void AreaViewPalette()
        {
            _paletteArea?.Clear();

            _paletteArea = new VisualElement();
            _paletteArea.pickingMode = PickingMode.Ignore;
            rootVisualElement.Add(_paletteArea);

            VisualElement buttonLineArea = new VisualElement();
            buttonLineArea.pickingMode = PickingMode.Ignore;
            buttonLineArea.style.flexDirection = FlexDirection.Row;
            _paletteArea.Add(buttonLineArea);

            VisualElement topElement = new VisualElement();
            topElement.pickingMode = PickingMode.Ignore;
            _paletteArea.Add(topElement);

            VisualElement bottomElement = new VisualElement();
            bottomElement.style.flexGrow = 1;
            bottomElement.style.flexDirection = FlexDirection.ColumnReverse;
            _paletteArea.Add(bottomElement);

            ScrollView scrollView = new ScrollView();
            scrollView.style.backgroundColor = _data.BlackA;
            topElement.Add(scrollView);

            foreach (var group in SelectedPalette.Groups)
            {
                Label title = new Label();
                title.text = group.GroupName;
                scrollView.Add(title);

                VisualElement groupArea = new VisualElement();
                groupArea.style.flexDirection = FlexDirection.Row;
                groupArea.style.flexWrap = Wrap.Wrap;
                scrollView.Add(groupArea);

                foreach (var items in group.Items)
                {
                    var itemInfo = PreviewRenderingUtility.GetInfoForItem(items);

                    if (itemInfo.image == null && itemInfo.name == null)
                        continue;

                    Button prefabButton = new Button();

                    if (itemInfo.image != null)
                        prefabButton.style.backgroundImage = new StyleBackground(itemInfo.image);

                    if (itemInfo.name != null)
                        prefabButton.tooltip = itemInfo.name;

                    prefabButton.style.width = _data.PreviewButtonSize;
                    prefabButton.style.height = _data.PreviewButtonSize;
                    prefabButton.clicked += () =>
                    {
                        if (Event.current.shift)
                        {
                            ScenePlacer.SelectionSettings.ToggleSelectedItem(items);
                        }
                        else
                        {
                            ScenePlacer.SelectionSettings.SetSelectedItem(items);
                        }
                        NotifyChange();
                    };
                    groupArea.Add(prefabButton);
                }
            }

            Label downHelper = new Label();
            downHelper.text = "Hold down shift to select/deselect several prefabs";
            bottomElement.Add(downHelper);
        }

        void OnGUI()
        {
            //OnGuiClearBrush(Event.current);
            HandleShortCuts(Event.current);

            //DrawSelectedAsset();// Palette
            //SelectedPalette = GetSelectedWindowCategory(SelectedAsset);
            //EditorGUILayout.Space();

            //using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            //{
            //    DrawSetPlacementMode();
            //    DrawSetRaycastMode();
            //}

            //if (!ValidatePlacementSettings())
            //{
            //    return;
            //}

            //EditorGUILayout.Space();
            //DrawBrushTools();
            //EditorGUILayout.Space();
            //DrawScrollWrapper();

            //DrawVersionFooter();
            //DrawLiteVersionNote();
        }

        private void SetSelectedAsset(SelectableAsset newSelectedAsset)
        {
            if (SelectedAsset == newSelectedAsset)
            {
                return;
            }

            SelectedAsset = newSelectedAsset;
            UpdateShortKeys();

            if (SelectedAsset == null)
            {
                return;
            }

            if (!(SelectedAsset is PaletteSet paletteCollection))
            {
                AvailablePalettes = new List<Palette>();
                AvailablePalettesGuiContent = new GUIContent[0];
                return;
            }

            AvailablePalettes = paletteCollection.Categories.Where(c => c != null).ToList();

            AvailablePalettesGuiContent = AvailablePalettes.Select(c => new GUIContent(c.name)).ToArray();
        }

        private void DrawSetPlacementMode()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var placementModeIndex = EditorGUILayout.Popup(KalderaEditorUtils.SelectPlacementModeContent, ScenePlacer.SelectionSettings.PlacementModeIndex, ScenePlacer.GetPlacementModeGuiContent());
                if (placementModeIndex != ScenePlacer.SelectionSettings.PlacementModeIndex)
                {
                    SetPlacementMode(placementModeIndex);
                }

                EditorGUILayout.LabelField(KalderaEditorUtils.MoreInformationContent, GUILayout.Width(14));
                IsPlacementModeHintsOpen = EditorGUILayout.Toggle(IsPlacementModeHintsOpen, StylesUtility.BoldFoldoutStyle, GUILayout.Width(14));
            }

            if (IsPlacementModeHintsOpen)
            {
                EditorGUILayout.HelpBox($"{ScenePlacer.CurrentPlacementMode.Name}\n{ScenePlacer.CurrentPlacementMode.HintText}", MessageType.Info);
            }

            ScenePlacer.CurrentPlacementMode.DrawEditor(this);
            EditorGUILayout.Space();
        }

        private void DrawSetRaycastMode()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var raycastModeIndex = EditorGUILayout.Popup(KalderaEditorUtils.SelectRaycastModeContent, ScenePlacer.SelectionSettings.RaycastModeIndex, ScenePlacer.GeRaycastModeGuiContent());
                if (raycastModeIndex != ScenePlacer.SelectionSettings.RaycastModeIndex)
                {
                    SetRaycastNode(raycastModeIndex);
                }

                EditorGUILayout.LabelField(KalderaEditorUtils.MoreInformationContent, GUILayout.Width(14));
                IsRaycastModeHintsOpen = EditorGUILayout.Toggle(IsRaycastModeHintsOpen, StylesUtility.BoldFoldoutStyle, GUILayout.Width(14));
            }

            if (IsRaycastModeHintsOpen)
            {
                EditorGUILayout.HelpBox($"{ScenePlacer.CurrentRaycastMode.Name}\n{ScenePlacer.CurrentRaycastMode.HintText}", MessageType.Info);
            }

            ScenePlacer.CurrentRaycastMode.DrawEditor(this);
            EditorGUILayout.Space();
        }



        //private void DrawScrollWrapper()
        //{
        //    using (var scrollScope = new EditorGUILayout.ScrollViewScope(CurrentWindowScroll))
        //    {
        //        CurrentWindowScroll = scrollScope.scrollPosition;

        //        if (SelectedPalette == null)
        //        {
        //            return;
        //        }

        //        DrawCategory(SelectedPalette);
        //    }
        //}

        //private void DrawLiteVersionNote()
        //{
        //    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        //    {
        //        EditorGUILayout.LabelField("This is free and trimmed down version of Kaldera Prefab Painter.");
        //        EditorGUILayout.LabelField("You can purchase the full version from Unity's Asset Store.");
        //        if (GUILayout.Button("Kaldera Prefab Painter (Full version)", EditorStyles.linkLabel))
        //        {
        //            Application.OpenURL("https://assetstore.unity.com/packages/tools/level-design/kaldera-prefab-painter-249492");
        //        }
        //    }
        //}

        private List<SelectableAsset> GetAvailableSelectableAssets()
        {
            return AssetDatabase.FindAssets(string.Format("t:{0}", typeof(SelectableAsset).FullName))
                .Select(a => AssetDatabase.GUIDToAssetPath(a))
                .Select(a => AssetDatabase.LoadAssetAtPath<SelectableAsset>(a))
                .OrderBy(a => a.name)
                .ToList();
        }

        //private GUIContent[] GetSelectableAssetsContent(List<SelectableAsset> selectableAssets)
        //{
        //    return selectableAssets
        //    .Select(a => new GUIContent(a.GetName()))
        //    .ToArray();
        //}

        private void SetPlacementMode(int placementModeIndex)
        {
            ScenePlacer.CurrentPlacementMode = ScenePlacer.GetPlacementModes()[placementModeIndex];
            ScenePlacer.SelectionSettings.PlacementModeIndex = placementModeIndex;
        }

        private void SetRaycastNode(int raycastModeIndex)
        {
            ScenePlacer.CurrentRaycastMode = ScenePlacer.GetRaycastModes()[raycastModeIndex];
            ScenePlacer.SelectionSettings.RaycastModeIndex = raycastModeIndex;
        }

        public void SetBrushType(BrushBase brush)
        {
            ScenePlacer.SelectionSettings.SelectedBrushIndex = brush.Index;
            Repaint();
        }

        //private bool ValidatePlacementSettings()
        //{
        //    var result = true;

        //    if (AvailableAssets.Count == 0)
        //    {
        //        return false;
        //    }

        //    if (SelectedAsset == null)
        //    {
        //        EditorGUILayout.HelpBox(KalderaEditorUtils.SelectPaletteToolTip, MessageType.Warning);
        //        result = false;
        //    }

        //    var validatePlacementMessage = ScenePlacer.CurrentPlacementMode.ValidatePlacementMode();
        //    if (validatePlacementMessage != null)
        //    {
        //        EditorGUILayout.HelpBox(validatePlacementMessage, MessageType.Warning);
        //        result = false;
        //    }

        //    return result;
        //}

        //private SelectableAsset GetSelectableAsset()
        //{
        //    if (AvailableAssets.Count == 0)
        //    {
        //        EditorGUILayout.HelpBox(KalderaEditorUtils.NoPaletteToolTip, MessageType.Warning);
        //        return null;
        //    }

        //    var currentIndex = Mathf.Max(AvailableAssets.IndexOf(SelectedAsset), 0);

        //    if (currentIndex >= AvailableAssetGuiContent.Length)
        //    {
        //        return AvailableAssets[AvailableAssetGuiContent.Length - 1];
        //    }

        //    using (new EditorGUILayout.HorizontalScope())
        //    {
        //        var selectedIndex = EditorGUILayout.Popup(GetPaletteSelectionContent(AvailableAssets, currentIndex), currentIndex, AvailableAssetGuiContent);
        //        if (GUILayout.Button(KalderaEditorUtils.ShowInProjectContent, GUILayout.Width(KalderaEditorUtils.MiniButtonWidth), GUILayout.Height(KalderaEditorUtils.MiniButtonHeight)))
        //        {
        //            Debug.Log($"selectedIndex : {selectedIndex}");
        //            ShowAssetInProject(AvailableAssets[selectedIndex]);
        //        }

        //        return AvailableAssets[selectedIndex];
        //    }
        //}

        private Palette GetSelectedWindowCategory(SelectableAsset selectedAsset)
        {
            if (selectedAsset == null)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.LabelField(KalderaEditorUtils.SelectPaletteContent);
                }

                return null;
            }
            else if (selectedAsset is Palette windowCategory)
            {
                return windowCategory;
            }
            else if (selectedAsset is PaletteSet windowSet)
            {
                return CategoryDropdownFromSet(windowSet);
            }
            else
            {
                Debug.LogError($"Selected Palette {selectedAsset.name} is neither a Palette nor a Palette Collection");
                return null;
            }
        }

        private void ShowAssetInProject(SelectableAsset selectableAsset)
        {
            EditorGUIUtility.PingObject(selectableAsset);
            Selection.activeObject = selectableAsset;
        }

        //private GUIContent GetPaletteSelectionContent(List<SelectableAsset> availableAssets, int index)
        //{
        //    if (availableAssets[index] is Palette)
        //    {
        //        return KalderaEditorUtils.SelectPaletteContent;
        //    }
        //    else
        //    {
        //        return KalderaEditorUtils.SelectPaletteCollectionContent;
        //    }
        //}

        protected Palette CategoryDropdownFromSet(PaletteSet paletteCollection)
        {
            if (AvailablePalettes.Count == 0)
            {
                EditorGUILayout.HelpBox(KalderaEditorUtils.EmptySetTooltip, MessageType.Warning);
                return null;
            }

            var currentItem = 0;
            if (SelectedPalette != null)
            {
                currentItem = AvailablePalettes.IndexOf(SelectedPalette);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUIContent PaletteLabelContent = new GUIContent("Палетка");
                var currentIndex = Mathf.Max(0, EditorGUILayout.Popup(PaletteLabelContent, currentItem, AvailablePalettesGuiContent));
                if (GUILayout.Button(KalderaEditorUtils.ShowInProjectContent, GUILayout.Width(KalderaEditorUtils.MiniButtonWidth), GUILayout.Height(KalderaEditorUtils.MiniButtonHeight)))
                {
                    ShowAssetInProject(AvailablePalettes[currentIndex]);
                }

                return AvailablePalettes[currentIndex];
            }
        }

        //protected void DrawCategory(Palette palette)
        //{
        //    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        //    {
        //        EditorGUILayout.LabelField(KalderaEditorUtils.PaletteLabelContent, EditorStyles.boldLabel);
        //        if (palette.HasAnyGroupWithItems())
        //        {
        //            foreach (var group in palette.Groups)
        //            {
        //                DrawGroup(palette, group);
        //            }
        //            EditorGUILayout.Space();
        //        }
        //        else
        //        {
        //            EditorGUILayout.LabelField("Palette contains no groups with items in them.", EditorStyles.miniBoldLabel);
        //            if (GUILayout.Button("Start create and populate groups for this palette", EditorStyles.miniButton))
        //            {
        //                EditorGUIUtility.PingObject(SelectedPalette);
        //                Selection.activeObject = SelectedPalette;
        //            }
        //            EditorGUILayout.Space();
        //        }

        //        EditorGUILayout.LabelField("Hold down shift to select/deselect several prefabs", EditorStyles.miniLabel);

        //    }
        //}

        //protected void DrawGroup(Palette palette, PaletteGroup group)
        //{
        //    var windowWidth = GetWindowWidth();
        //    var buttonsInRow = GetButtonCountInRow(windowWidth, KalderaEditorUtils.IconButtonSize);

        //    var totalIndex = 0;
        //    var currentIndex = 0;
        //    var currentRow = 0;

        //    EditorGUILayout.LabelField(GetGroupName(group));

        //    while (totalIndex < group.Items.Count)
        //    {
        //        using (new GUI.GroupScope(GUILayoutUtility.GetRect(windowWidth, KalderaEditorUtils.IconButtonSize)))
        //        {
        //            for (int rowIndex = 0; rowIndex < buttonsInRow; rowIndex++)
        //            {
        //                if (totalIndex >= group.Items.Count)
        //                {
        //                    break;
        //                }

        //                if (DrawItem(palette, group, rowIndex, currentIndex))
        //                {
        //                    currentIndex++;
        //                }

        //                totalIndex++;
        //            }

        //            GUI.color = Color.white;
        //        }
        //        currentRow++;
        //    }
        //}

        //private bool DrawItem(Palette palette, PaletteGroup group, int rowIndex, int totalIndex)
        //{
        //    if (!group.Items[totalIndex].HasVariants())
        //    {
        //        return false;
        //    }

        //    var drawRectatangle = GetRectForPosition(rowIndex, KalderaEditorUtils.IconButtonSize);

        //    var paletteItem = group.Items[totalIndex];

        //    // Make sure the currently selected button is blue
        //    EditorCustomGUILayout.SetGuiColorState(ScenePlacer.SelectionSettings.SelectedItems.Contains(paletteItem));

        //    var guiContent = PreviewRenderingUtility.GetGuiContentForItem(paletteItem);
        //    if (guiContent == null)
        //    {
        //        return false;
        //    }

        //    if (GUI.Button(drawRectatangle, guiContent, StylesUtility.IconButtonStyle))
        //    {
        //        if (Event.current.button == 0)
        //        {
        //            SelectItem(paletteItem);
        //        }
        //        else if (Event.current.button == 1)
        //        {
        //            OpenPrefabInPaletteAsset(palette, group, paletteItem);
        //        }
        //        else if (Event.current.button == 2)
        //        {
        //            OpenPrefabItem(paletteItem);
        //        }
        //    }

        //    return true;
        //}

        //private void DrawVersionFooter()
        //{
        //    EditorGUILayout.LabelField(KalderaEditorUtils.VersionContent);
        //}

        private float GetWindowWidth() => position.width - (LeftOffset + RightOffset + ScrollBarWidth);
        private int GetButtonCountInRow(float windowWidth, float iconButtonSize) => Mathf.FloorToInt(windowWidth / iconButtonSize);

        private Rect GetRectForPosition(int rowIndex, float iconButtonSize)
        {
            return new Rect(LeftOffset + (rowIndex * (KalderaEditorUtils.IconButtonSize + ButtonPadding)), 0, iconButtonSize, iconButtonSize);
        }

        //private void SelectItem(PaletteItem assetItem)
        //{
        //    if (Event.current.shift)
        //    {
        //        ScenePlacer.SelectionSettings.ToggleSelectedItem(assetItem);
        //    }
        //    else
        //    {
        //        ScenePlacer.SelectionSettings.SetSelectedItem(assetItem);
        //    }
        //    NotifyChange();
        //}

        //private void OpenPrefabItem(PaletteItem assetItem)
        //{
        //    if (!assetItem.HasVariants())
        //    {
        //        return;
        //    }

        //    var variantObject = assetItem.FirstObject();
        //    EditorGUIUtility.PingObject(variantObject);
        //    AssetDatabase.OpenAsset(variantObject);
        //}

        //private void OpenPrefabInPaletteAsset(Palette palette, PaletteGroup group, PaletteItem assetItem)
        //{
        //    EditorGUIUtility.PingObject(palette);
        //    AssetDatabase.OpenAsset(palette);

        //    group.IsOpenInEditor = true;
        //    assetItem.IsOpenInEditor = true;
        //}

        //private string GetGroupName(PaletteGroup group)
        //{
        //    if (group.GroupName == string.Empty)
        //    {
        //        return "[Nameless group]";
        //    }
        //    else
        //    {
        //        return group.GroupName;
        //    }
        //}

        //private void DrawBrushTools()
        //{
        //    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        //    {
        //        DrawBrushTypeTool();

        //        var settings = ScenePlacer.SelectionSettings;
        //        settings.OptionsExtended = EditorGUILayout.Foldout(settings.OptionsExtended, "Tool Options", StylesUtility.BoldFoldoutStyle);

        //        if (!settings.OptionsExtended)
        //        {
        //            return;
        //        }

        //        ScenePlacer.CurrentBrush.DrawBrushEditor(ScenePlacer);
        //        ScenePlacer.CurrentBrush.DrawAdditionalSettings(ScenePlacer, settings);
        //    }

        //    EditorGUILayout.Space();
        //}

        //private void DrawBrushTypeTool()
        //{
        //    using (new EditorGUILayout.HorizontalScope())
        //    {
        //        EditorGUILayout.LabelField(KalderaEditorUtils.SelectToolsContent, EditorStyles.boldLabel, GUILayout.Width(KalderaEditorUtils.OptionLabelWidth));
        //        var currentIndex = Mathf.Min(ScenePlacer.SelectionSettings.SelectedBrushIndex, ScenePlacer.GetBrushMapping().Length - 1);
        //        EditorGUILayout.LabelField(ScenePlacer.GetBrushMapping()[currentIndex].KeyBinding.SettingName);

        //    }

        //    var brushes = ScenePlacer.GetBrushMapping();

        //    var windowWidth = GetWindowWidth();
        //    var buttonsInRow = GetButtonCountInRow(windowWidth, KalderaEditorUtils.IconButtonSize);

        //    var totalIndex = 0;
        //    var currentRow = 0;

        //    while (totalIndex < brushes.Length)
        //    {
        //        using (new GUI.GroupScope(GUILayoutUtility.GetRect(windowWidth, KalderaEditorUtils.IconButtonSize)))
        //        {
        //            for (int rowIndex = 0; rowIndex < buttonsInRow; rowIndex++)
        //            {
        //                if (totalIndex >= brushes.Length)
        //                {
        //                    break;
        //                }

        //                var brush = brushes[totalIndex];

        //                EditorCustomGUILayout.SetGuiBackgroundColorState(ScenePlacer.CurrentBrush == brush);
        //                var drawRectatangle = GetRectForPosition(rowIndex, KalderaEditorUtils.IconButtonSize);

        //                using (new EditorGUI.DisabledGroupScope(brush.Disabled))
        //                {
        //                    if (GUI.Button(drawRectatangle, brush.GetButtonContent()))
        //                    {
        //                        brush.OnButtonPress(this);
        //                        NotifyChange();
        //                    }
        //                }

        //                totalIndex++;
        //            }

        //            GUI.color = Color.white;
        //        }
        //        currentRow++;
        //    }

        //    EditorCustomGUILayout.RestoreGuiColor();
        //}

        private void NotifyChange()
        {
            ScenePlacer.NotifyChange();
        }

        private void SetSprite(VisualElement target, CastomGuiContent content)
        {
            target.style.backgroundImage = new StyleBackground(content.sprite);

            if (content.sprite == null)
                Debug.Log($"Для {content.name} не указан SPRITE");
        }
    }
}