using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UnityEngine.Toolbox
{
    public static partial class ToolkitExt
    {
        /// <summary>
        /// Устанавливает видимость `VisualElement` изменяя стиль `visibility`.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="visible">Если `Hidden`, элемент становится НЕ видимым.</param>
        public static void SetVisible<T>(this T element, Visibility visible) where T : VisualElement
        {
            if (element == null)
                return;

            element.style.visibility = visible;
        }

        /// <summary>
        /// Устанавливает видимость `VisualElement` изменяя стиль `visibility`.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="visible">Если `true`, элемент становится видимым (`Visibility.Visible`), если `false` — скрытым (`Visibility.Hidden`).</param>
        public static void SetVisibility<T>(this T element, bool visible) where T : VisualElement
        {
            if (element == null)
                return;

            element.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Устанавливает стиль отображения (`DisplayStyle`) для `VisualElement`.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="displayStyle">Стиль отображения (`DisplayStyle.Flex` для видимости, `DisplayStyle.None` для скрытия).</param>
        public static void SetDisplay<T>(this T element, DisplayStyle displayStyle) where T : VisualElement
        {
            if (element == null)
                return;

            element.style.display = displayStyle;
        }

        /// <summary>
        /// Устанавливает стиль отображения (`DisplayStyle`) для `VisualElement` через флаг.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="displayStyle">Стиль отображения (`DisplayStyle.Flex` для видимости, `DisplayStyle.None` для скрытия).</param>
        public static void SetDisplay<T>(this T element, bool display) where T : VisualElement
        {
            if (element == null)
                return;

            element.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Устанавливает видимость для коллекции `VisualElement` (например, `List<VisualElement>`).
        /// </summary>
        /// <typeparam name="T">Тип коллекции `VisualElement`.</typeparam>
        /// <param name="elements">Коллекция `VisualElement`, для которой нужно установить видимость.</param>
        /// <param name="visible">Если `true`, элементы становятся видимыми, если `false` — скрытыми.</param>
        public static void SetVisibilityAll<T>(this T elements, bool visible) where T : List<VisualElement>
        {
            foreach (var item in elements)
            {
                if (item == null)
                    continue;

                item.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Проверяет, установлен ли у `VisualElement` стиль отображения `DisplayStyle.Flex`.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <returns>Возвращает `true`, если `display` равен `DisplayStyle.Flex`, иначе `false`.</returns>
        public static bool IsDisplay<T>(this T element) where T : VisualElement
        {
            if (element == null)
                return false;

            return element.style.display == DisplayStyle.Flex;
        }

        /// <summary>
        /// Устанавливает фоновое изображение для `VisualElement` из `Sprite`.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="value">Sprite для использования в качестве фонового изображения.</param>
        public static void SetBackgroundImage<T>(this T element, Sprite value) where T : VisualElement
        {
            if (element == null)
                return;

            element.style.backgroundImage = new StyleBackground(value);
        }

        /// <summary>
        /// Создает новый `Button` с заданным текстом и обработчиком события `clicked`.
        /// </summary>
        /// <param name="text">Текст для кнопки.</param>
        /// <param name="callback">Действие, которое будет выполнено при нажатии на кнопку.</param>
        /// <returns>Новый экземпляр `Button`.</returns>
        public static Button CreateButton(string text, System.Action callback)
        {
            Button element = new Button();
            element.text = text;
            element.clicked += callback;
            return element;
        }

        /// <summary>
        /// Устанавливает цвет рамки для `VisualElement` со всех четырех сторон.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="value">Цвет рамки.</param>
        public static void SetBorderColor<T>(this T element, Color value) where T : VisualElement
        {
            element.style.borderTopColor = value;
            element.style.borderBottomColor = value;
            element.style.borderLeftColor = value;
            element.style.borderRightColor = value;
        }

        /// <summary>
        /// Устанавливает радиус скругления углов рамки для `VisualElement` со всех четырех углов.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="value">Радиус скругления в пикселях.</param>
        public static void SetBorderRadius<T>(this T element, int value) where T : VisualElement
        {
            element.style.borderTopLeftRadius = value;
            element.style.borderTopRightRadius = value;
            element.style.borderBottomLeftRadius = value;
            element.style.borderBottomRightRadius = value;
        }

        /// <summary>
        /// Устанавливает толщину рамки для `VisualElement` со всех четырех сторон.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="value">Толщина рамки в пикселях.</param>
        public static void SetBorderWidth<T>(this T element, int value) where T : VisualElement
        {
            element.style.borderTopWidth = value;
            element.style.borderBottomWidth = value;
            element.style.borderLeftWidth = value;
            element.style.borderRightWidth = value;
        }

        /// <summary>
        /// Устанавливает внутренние отступы (padding) для `VisualElement` со всех четырех сторон.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="value">Размер отступа в пикселях.</param>
        public static void SetPadding<T>(this T element, int value) where T : VisualElement
        {
            element.style.paddingTop = value;
            element.style.paddingBottom = value;
            element.style.paddingLeft = value;
            element.style.paddingRight = value;
        }

        /// <summary>
        /// Устанавливает внешние отступы (Margin) для `VisualElement` со всех четырех сторон.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="value">Размер отступа в пикселях.</param>
        public static void SetMargin<T>(this T element, int value) where T : VisualElement
        {
            element.style.marginTop = value;
            element.style.marginBottom = value;
            element.style.marginLeft = value;
            element.style.marginRight = value;
        }

        /// <summary>
        /// Устанавливает фиксированную ширину для `VisualElement`.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="value">Ширина в пикселях.</param>
        public static void SetWidth<T>(this T element, float value) where T : VisualElement
        {
            if (element == null)
                return;

            element.style.width = value;
        }

        /// <summary>
        /// Устанавливает фиксированную ширину и высоту для `VisualElement`.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="width">Ширина в пикселях.</param>
        /// <param name="height">Высота в пикселях.</param>
        public static void SetWidthAndHeight<T>(this T element, float width, float height) where T : VisualElement
        {
            if (element == null)
                return;

            element.style.width = width;
            element.style.height = height;
        }

        /// <summary>
        /// Устанавливает ширину `VisualElement` в процентах от ширины родителя.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="value">Ширина в процентах (например, 50f для 50%).</param>
        public static void SetWidthPercentage<T>(this T element, float value) where T : VisualElement
        {
            if (element == null)
                return;

            element.style.width = Length.Percent(value);
        }

        /// <summary>
        /// Устанавливает высоту `VisualElement` в процентах от высоты родителя.
        /// </summary>
        /// <typeparam name="T">Тип VisualElement.</typeparam>
        /// <param name="element">Целевой `VisualElement`.</param>
        /// <param name="value">Высота в процентах (например, 50f для 50%).</param>
        public static void SetHeightPercentage<T>(this T element, float value) where T : VisualElement
        {
            if (element == null)
                return;

            element.style.height = Length.Percent(value);
        }

        /// <summary>
        /// Безопасно отписывает делегат общего события и возвращает null для обнуления исходной переменной.
        /// </summary>
        /// <typeparam name="TEventType">Тип события.</typeparam>
        /// <param name="element">Объект, от которого отписывается событие.</param>
        /// <param name="callback">Делегат, который нужно отписать.</param>
        /// <returns>Возвращает null.</returns>
        public static EventCallback<TEventType> TryUnregisterCallback<TEventType>(this CallbackEventHandler element, EventCallback<TEventType> callback) where TEventType : EventBase<TEventType>, new()
        {
            if (callback != null && element != null)
            {
                element.UnregisterCallback(callback);
            }
            // Возвращаем null, чтобы можно было написать: callback = element.TryUnregisterCallback(callback);
            return null;
        }

        /// <summary>
        /// Безопасно отписывает делегат события изменения значения и возвращает null для обнуления исходной переменной.
        /// </summary>
        /// <typeparam name="TValue">Тип значения, которое изменяется.</typeparam>
        /// <param name="element">Объект, от которого отписывается событие изменения значения.</param>
        /// <param name="callback">Делегат, который нужно отписать.</param>
        /// <returns>Возвращает null.</returns>
        public static EventCallback<ChangeEvent<TValue>> TryUnregisterValueChangedCallback<TValue>(this INotifyValueChanged<TValue> element, EventCallback<ChangeEvent<TValue>> callback)
        {
            if (callback != null && element != null)
            {
                element.UnregisterValueChangedCallback(callback);
            }
            return null;
        }
    }
}
