using Assets.SimpleLocalization.Scripts;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.SimpleLocalization.Scripts
{
    /// <summary>
    /// Localize text component.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextMeshPro : MonoBehaviour
    {
        public string LocalizationKey;

        public void Start()
        {
            Localize();
            LocalizationManager.OnLocalizationChanged += Localize;
        }

        public void OnDestroy()
        {
            LocalizationManager.OnLocalizationChanged -= Localize;
        }

        [Button]
        private void Localize()
        {
            string localizedText = LocalizationManager.Localize(LocalizationKey);
            GetComponent<TMP_Text>().text = localizedText;
        }

        public void LocolizeUpdate()
        {
            Localize();
        }
    }
}
