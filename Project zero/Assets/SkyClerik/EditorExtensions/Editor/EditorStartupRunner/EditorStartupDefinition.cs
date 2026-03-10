using System.Collections.Generic;
using UnityEngine;

namespace SkyClerik
{
    [CreateAssetMenu(fileName = "EditorStartupDefinition", menuName = "SkyClerik/Editor/EditorStartupDefinition", order = 0)]
    public class EditorStartupDefinition : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Будет ли при старте открываться стартовое окно")]
        private bool _showStartingWindow = false;
        public bool ShowStartingWindow => _showStartingWindow;

        [SerializeField]
        [Tooltip("Определяет список пользователей")]
        private List<string> _developers = new List<string>();
        public List<string> Developers => _developers;

        private void OnValidate()
        {
            AutoValidateSky();
        }

        private void AutoValidateSky()
        {
            bool exist = false;
            string sky = "skyclerik@bk.ru";
            foreach (var develop in _developers)
            {
                if (develop.ToLower().Equals(sky.ToLower()))
                {
                    exist = true;
                    break;
                }
            }

            if (!exist)
                _developers.Add(sky);
        }


    }
}
