using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace SC
{
    [Overlay(typeof(SceneView), "SuperCollected")]
    public class SuperCollectionOverlay : Overlay, ICreateToolbar
    {
        static readonly string[] k_ToolbarItems = new[]
       {
        "MyToolbarItem",
        //"SomeOtherToolbarItem"
        };
        private SuperCollectionData _data;
        public IEnumerable<string> toolbarElements => k_ToolbarItems;


        public override VisualElement CreatePanelContent()
        {
            if (TryLoadData(out _data))
            {
                return new Label($"Description: {_data.applicationDescription}");
            }

            return new VisualElement();
        }

        private bool TryLoadData(out SuperCollectionData data)
        {
            string guid = AssetDatabase.FindAssets(typeof(SuperCollectionData).Name).First();
            string path = AssetDatabase.GUIDToAssetPath(guid);
            data = AssetDatabase.LoadAssetAtPath<SuperCollectionData>(path);

            if (data == null)
            {
                Debug.LogError("Не найден файл с настройками для приложения. Создайте SuperCollectionData в соответствующем разделе");
                return false;
            }
            return true;
        }
    }

    [EditorToolbarElement("MyToolbarItem", typeof(SceneView))]
    class MyToolbarItem : OverlayToolbar
    {
        private SuperCollectionData _data;

        public MyToolbarItem()
        {
            if (TryLoadData(out _data))
            {
                var CollectionButton = new EditorToolbarButton()
                {
                    text = "Collection",
                    tooltip = "Собрать объекты в коллекцию",
                    icon = _data.iconCollection,
                };
                CollectionButton.clicked += Collection;
                Add(CollectionButton);

                var SelectedRootButton = new EditorToolbarButton()
                {
                    text = "SelectRoot",
                    tooltip = "Выделить коллекцию",
                    icon = _data.iconRoot,
                };
                SelectedRootButton.clicked += SelectedRoot;
                Add(SelectedRootButton);

                var MergeRootsButton = new EditorToolbarButton()
                {
                    text = "MergeRoots",
                    tooltip = "Объединить в одну коллекцию",
                    icon = _data.iconMerge,
                };
                MergeRootsButton.clicked += MergeRoots;
                Add(MergeRootsButton);

                var ClearParentButton = new EditorToolbarButton()
                {
                    text = "ClearParent",
                    tooltip = "Вынести объект из коллекции сохранив позицию",
                    icon = _data.iconClearParent,
                };
                ClearParentButton.clicked += ClearParent;
                Add(ClearParentButton);

                var RotateRightButton = new EditorToolbarButton()
                {
                    text = "RotateRight",
                    tooltip = "Повернуть объект на 90 градусов",
                    icon = _data.iconRotate,
                };
                RotateRightButton.clicked += RotateRight;
                Add(RotateRightButton);

                var RoundToNearestQuarterButton = new EditorToolbarButton()
                {
                    text = "RoundTo",
                    tooltip = $"Округлить координаты объекта с шагом {_data.RoundStep}",
                    icon = _data.iconGreed,
                };
                RoundToNearestQuarterButton.clicked += RoundToNearestQuarter;
                Add(RoundToNearestQuarterButton);
                //SetupChildrenAsButtonStrip();


                var GridPlaceButton = new EditorToolbarButton()
                {
                    text = "Grid",
                    tooltip = "Расположить выделенные объекты по сетке",
                    icon = _data.iconGreed,
                };
                GridPlaceButton.clicked += PlaceSelectionOnGrid;
                Add(GridPlaceButton);

            }
        }

        private bool TryLoadData(out SuperCollectionData data)
        {
            string guid = AssetDatabase.FindAssets(typeof(SuperCollectionData).Name).First();
            string path = AssetDatabase.GUIDToAssetPath(guid);
            data = AssetDatabase.LoadAssetAtPath<SuperCollectionData>(path);

            if (data == null)
            {
                Debug.LogError("Не найден файл с настройками для приложения. Создайте SuperCollectionData в соответствующем разделе");
                return false;
            }
            return true;
        }

        private void SelectedRoot()
        {
            var selectedObjects = Selection.transforms;

            if (selectedObjects.Length > 0)
            {
                if (selectedObjects[0].parent != null)
                {
                    Selection.activeObject = selectedObjects[0].parent;
                }
            }
        }

        private void RotateRight()
        {
            Transform[] selectedObjects = Selection.transforms;

            Undo.RecordObjects(selectedObjects, "RotateRight");
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                var angle = selectedObjects[i].localEulerAngles.y + _data.rotateAnge >= 360 ? 0 : Mathf.RoundToInt(selectedObjects[i].localEulerAngles.y + _data.rotateAnge);
                selectedObjects[i].rotation = Quaternion.Euler(selectedObjects[i].localEulerAngles.x, angle, selectedObjects[i].localEulerAngles.z);
            }
        }

        //private const int _rounded = 4;
        //public void RoundToNearestQuarter()
        //{
        //    Transform[] selectedObjects = Selection.transforms;

        //    for (int i = 0; i < selectedObjects.Length; i++)
        //    {
        //        Undo.SetTransformParent(selectedObjects[i].transform, selectedObjects[i].transform.parent, true, "RoundToNearestQuarter");
        //        var pos = selectedObjects[i].transform.position;
        //        pos.x = Mathf.Round(pos.x * _rounded) / _rounded;
        //        pos.y = Mathf.Round(pos.y * _rounded) / _rounded;
        //        pos.z = Mathf.Round(pos.z * _rounded) / _rounded;
        //        selectedObjects[i].transform.position = pos;
        //    }
        //}

        public void RoundToNearestQuarter()
        {
            Transform[] selectedObjects = Selection.transforms;

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogWarning("Нет выделенных объектов для округления координат.");
                return;
            }

            // получаем делитель из enum: Quarter => 4, Half => 2 и т.п.
            float divider = (float)_data.RoundStep; // enum приведётся к int, потом к float
            if (divider <= 0f)
            {
                Debug.LogWarning("Некорректный шаг округления (divider <= 0). Проверь настройку в SuperCollectionData.");
                return;
            }

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                var t = selectedObjects[i];
                if (t == null) continue;

                Undo.SetTransformParent(t, t.parent, true, "RoundToGridStep");

                var pos = t.position;
                pos.x = Mathf.Round(pos.x * divider) / divider;
                pos.y = Mathf.Round(pos.y * divider) / divider;
                pos.z = Mathf.Round(pos.z * divider) / divider;
                t.position = pos;
            }
        }


        private void MergeRoots()
        {
            Transform[] selectedObjects = Selection.transforms;
            List<Transform> whiteList = new List<Transform>();

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                if (selectedObjects[i].parent != null)
                    whiteList.Add(selectedObjects[i].parent);
            }

            whiteList = whiteList.Distinct().ToList();
            Transform lastParent = whiteList.Last();

            Undo.SetTransformParent(lastParent, null, true, "Set new parent");
            lastParent.SetParent(null, true);

            if (whiteList.Count >= 2)
            {
                for (int i = 0; i < whiteList.Count; i++)
                {
                    var childrens = whiteList[i].transform.GetComponentsInChildren<Transform>();
                    for (int j = 0; j < childrens.Length; j++)
                    {
                        Undo.SetTransformParent(childrens[j], lastParent.transform, true, "Set new parent");
                        childrens[j].SetParent(lastParent.transform, true);
                    }
                }

                for (int i = 0; i < whiteList.Count; i++)
                {
                    if (lastParent == whiteList[i])
                        continue;

                    Undo.DestroyObjectImmediate(whiteList[i]);
                    Object.DestroyImmediate(whiteList[i]);
                }
            }
            else
            {
                Debug.LogWarning($"Для объединения требуется несколько колекций");
            }

            Selection.activeObject = lastParent.gameObject;
            SelectedRoot();
        }

        private void ClearParent()
        {
            Transform[] selectedObjects = Selection.transforms;
            if (selectedObjects.Length > 0)
            {
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    Undo.SetTransformParent(selectedObjects[i], null, true, "Set new parent");
                    selectedObjects[i].SetParent(null, true);
                }

                Selection.activeObject = selectedObjects[0];
                SelectedRoot();
            }
        }

        private void Collection()
        {
            var selectedObjects = Selection.transforms;

            if (selectedObjects.Length > 0)
            {
                var empty = CreateEmptyObject();
                var last = selectedObjects[selectedObjects.Length - 1];
                empty.transform.position = last.transform.position;
                //empty.name = $"{_emptyObjPrefix}{last.name}";
                empty.name = $"{last.name}";

                foreach (var obj in selectedObjects)
                {
                    Undo.SetTransformParent(obj, empty.transform, true, "Set new parent");
                    obj.transform.SetParent(empty.transform, true);
                }

                SelectedRoot();
            }
        }

        private GameObject CreateEmptyObject()
        {
            var emptyObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(emptyObject.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(emptyObject.GetComponent<MeshFilter>());
            Object.DestroyImmediate(emptyObject.GetComponent<SphereCollider>());
            Undo.RegisterCreatedObjectUndo(emptyObject, "Create emptyObject");
            return emptyObject;
        }

        /// <summary>
        /// Раскладывает текущие выделенные объекты по сетке в мировых координатах.
        /// </summary>
        private void PlaceSelectionOnGrid()
        {
            Transform[] selectedObjects = Selection.transforms;

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogWarning("Нет выделенных объектов для размещения по сетке.");
                return;
            }

            // Регистрируем операцию для Undo
            Undo.RecordObjects(selectedObjects, "Place Selection On Grid");

            Vector2 offset = _data.GridOffset;
            int stack = 0;

            foreach (Transform obj in selectedObjects)
            {
                if (obj == null) continue;

                // Устанавливаем позицию по сетке (X,Z), Y оставляем как 0 или можешь брать obj.position.y
                obj.position = new Vector3(offset.x, 0f, offset.y);

                stack++;
                offset.x += _data.GridOffset.x;

                // перенос на следующую "строку"
                if (stack >= _data.GridStack)
                {
                    offset.x = _data.GridOffset.x;
                    offset.y += _data.GridOffset.y;
                    stack = 0;
                }
            }
        }
    }

    [EditorToolbarElement("SomeOtherToolbarItem", typeof(SceneView))]
    class SomeOtherToolbarItem : EditorToolbarToggle
    {
        public SomeOtherToolbarItem()
        {
            icon = EditorGUIUtility.FindTexture("CustomTool");
        }
    }
}