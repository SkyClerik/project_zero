#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SkyClerik.Editor
{
    class FindMissingComponents
    {
        static List<GameObject> offenders = new List<GameObject>();

        [MenuItem("SkyClerik/GameObject/Find & Select Missing Components")]
        static void FindAndSelectMissingComponents()
        {
            offenders.Clear();
            Transform[] allTransformsInScene = GameObject.FindObjectsOfType<Transform>(true);

            foreach (var item in allTransformsInScene)
            {
                checkObject(item.gameObject);
            }

            if (offenders.Count > 0)
            {
                Selection.objects = offenders.Distinct().ToArray();
                Debug.Log($"Found {offenders.Count} objects with missing components: {string.Join(", ", offenders.Select(go => go.name))}");
            }
            else
            {
                Debug.Log("No objects with missing components found in the current scene. You're doing great! ✨");
            }
        }

        static void checkObject(GameObject go)
        {
            Component[] comps = go.GetComponents<Component>();
            foreach (var item in comps)
            {
                if (item == null)
                {
                    offenders.Add(go);
                    break;
                }
            }
        }
    }
}
#endif