#if UNITY_EDITOR
using UnityEditor;

namespace UnityEngine.Toolbox.Editor
{
    /// <summary>
    /// Этот класс автоматически сбрасывает состояние ServiceProvider при выходе из режима игры в редакторе.
    /// </summary>
    [InitializeOnLoad]
    public static class ServiceProviderCleaner
    {
        static ServiceProviderCleaner()
        {
            // Подписываемся на событие изменения состояния PlayMode.
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Если мы выходим из режима игры (нажимаем "стоп")...
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                // ...вызываем метод очистки нашего сервис-локатора.
                ServiceProvider.ClearAllServices();
            }
        }
    }
}
#endif
