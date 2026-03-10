using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.SimpleLocalization;
using Assets.SimpleLocalization.Scripts;

public class NewExample : MonoBehaviour
{
    public void SetLocalization(string localization)
    {
        LocalizationManager.Language = localization;
    }

}
