using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Toolbox;
using UnityEngine.UIElements;

/// <summary>
/// Пользовательский редактор для `DateTimeWrapper` и `TimeSpanWrapper`,
/// предоставляющий удобный интерфейс для редактирования даты и времени в инспекторе Unity
/// с использованием UI Toolkit.
/// </summary>
/// <remarks>
/// Этот редактор заменяет стандартное отображение `[Serializable]` полей на
/// более структурированный набор полей для года, месяца, дня, часа, минуты и секунды.
/// </remarks>
[CustomEditor(typeof(DateTimeWrapper))]
public class DateTimeWrapperEditor : Editor
{
    /// <summary>
    /// Создает и возвращает VisualElement, который будет отображаться как пользовательский GUI инспектора для `DateTimeWrapper`.
    /// </summary>
    /// <returns>Корневой `VisualElement` для пользовательского инспектора.</returns>
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        // Получаем SerializedObject
        var serializedObject = this.serializedObject;

        // Создаём UIElements для каждого поля
        var dateTimeProperty = serializedObject.FindProperty("dateTime"); // Assuming 'dateTime' is the name of the SerializedProperty in the target object
        var timeSpanProperty = serializedObject.FindProperty("timeSpan"); // Assuming 'timeSpan' is the name of the SerializedProperty in the target object

        // Отрисовка DateTimeWrapper (аналогично вашему PropertyDrawer)
        var dateTimeContainer = CreateDateTimeGUI(dateTimeProperty);
        root.Add(dateTimeContainer);

        // Отрисовка TimeSpanWrapper
        var timeSpanContainer = CreateTimeSpanGUI(timeSpanProperty);
        root.Add(timeSpanContainer);

        // Применяем изменения
        root.Bind(serializedObject);

        return root;
    }

    /// <summary>
    /// Создает GUI-элементы для редактирования свойств `DateTimeWrapper`.
    /// </summary>
    /// <param name="property">`SerializedProperty`, представляющий `DateTimeWrapper`.</param>
    /// <returns>`VisualElement`, содержащий поля для редактирования даты и времени.</returns>
    private VisualElement CreateDateTimeGUI(SerializedProperty property)
    {
        var container = new VisualElement();

        var year = property.FindPropertyRelative("year");
        var month = property.FindPropertyRelative("month");
        var day = property.FindPropertyRelative("day");
        var hour = property.FindPropertyRelative("hour");
        var minute = property.FindPropertyRelative("minute");
        var second = property.FindPropertyRelative("second");

        var yearField = new IntegerField("Год") { value = year.intValue };
        var monthField = new IntegerField("Месяц") { value = month.intValue };
        var dayField = new IntegerField("День") { value = day.intValue };
        var hourField = new IntegerField("Час") { value = hour.intValue };
        var minuteField = new IntegerField("Минута") { value = minute.intValue };
        var secondField = new IntegerField("Секунда") { value = second.intValue };

        yearField.RegisterValueChangedCallback(evt => year.intValue = evt.newValue);
        monthField.RegisterValueChangedCallback(evt => month.intValue = Mathf.Clamp(evt.newValue, 1, 12));
        dayField.RegisterValueChangedCallback(evt => day.intValue = Mathf.Clamp(evt.newValue, 1, 31));
        hourField.RegisterValueChangedCallback(evt => hour.intValue = Mathf.Clamp(evt.newValue, 0, 23));
        minuteField.RegisterValueChangedCallback(evt => minute.intValue = Mathf.Clamp(evt.newValue, 0, 59));
        secondField.RegisterValueChangedCallback(evt => second.intValue = Mathf.Clamp(evt.newValue, 0, 59));

        var row1 = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };
        row1.Add(yearField);
        row1.Add(monthField);
        row1.Add(dayField);

        var row2 = new VisualElement { style = { flexDirection = FlexDirection.Row } };
        row2.Add(hourField);
        row2.Add(minuteField);
        row2.Add(secondField);

        foreach (var field in new[] { yearField, monthField, dayField, hourField, minuteField, secondField })
        {
            field.style.marginRight = 5;
            field.style.flexGrow = 1;
        }

        container.Add(row1);
        container.Add(row2);

        return container;
    }

    /// <summary>
    /// Создает GUI-элементы для редактирования свойств `TimeSpanWrapper`.
    /// </summary>
    /// <param name="property">`SerializedProperty`, представляющий `TimeSpanWrapper`.</param>
    /// <returns>`VisualElement`, содержащий поля для редактирования продолжительности времени.</returns>
    private VisualElement CreateTimeSpanGUI(SerializedProperty property)
    {
        var container = new VisualElement();

        var days = property.FindPropertyRelative("days");
        var hours = property.FindPropertyRelative("hours");
        var minutes = property.FindPropertyRelative("minutes");
        var seconds = property.FindPropertyRelative("seconds");

        var daysField = new IntegerField("Дни") { value = days.intValue };
        var hoursField = new IntegerField("Часы") { value = hours.intValue };
        var minutesField = new IntegerField("Минуты") { value = minutes.intValue };
        var secondsField = new IntegerField("Секунды") { value = seconds.intValue };

        daysField.RegisterValueChangedCallback(evt => days.intValue = Mathf.Max(0, evt.newValue));
        hoursField.RegisterValueChangedCallback(evt => hours.intValue = Mathf.Clamp(evt.newValue, 0, 23));
        minutesField.RegisterValueChangedCallback(evt => minutes.intValue = Mathf.Clamp(evt.newValue, 0, 59));
        secondsField.RegisterValueChangedCallback(evt => seconds.intValue = Mathf.Clamp(evt.newValue, 0, 59));

        var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
        foreach (var field in new[] { daysField, hoursField, minutesField, secondsField })
        {
            field.style.marginRight = 5;
            field.style.flexGrow = 1;
            row.Add(field);
        }

        container.Add(row);

        return container;
    }
}