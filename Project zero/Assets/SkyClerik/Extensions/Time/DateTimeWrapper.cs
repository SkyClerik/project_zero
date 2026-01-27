using System;

namespace UnityEngine.Toolbox
{
    /// <summary>
    /// Сериализуемая обертка для `System.DateTime`, позволяющая редактировать дату и время в инспекторе Unity.
    /// </summary>
    /// <remarks>
    /// Используется для хранения даты и времени в компонентах MonoBehaviour или ScriptableObject,
    /// которые должны быть сериализованы.
    /// </remarks>
    [Serializable]
    public class DateTimeWrapper
    {
        /// <summary>Компонент года для сериализации.</summary>
        [SerializeField] private int year = 2023;
        /// <summary>Компонент месяца для сериализации.</summary>
        [SerializeField] private int month = 1;
        /// <summary>Компонент дня для сериализации.</summary>
        [SerializeField] private int day = 1;
        /// <summary>Компонент часа для сериализации.</summary>
        [SerializeField] private int hour = 0;
        /// <summary>Компонент минуты для сериализации.</summary>
        [SerializeField] private int minute = 0;
        /// <summary>Компонент секунды для сериализации.</summary>
        [SerializeField] private int second = 0;

        /// <summary>
        /// Получает или устанавливает текущее значение как `System.DateTime`.
        /// </summary>
        /// <value>Объект `System.DateTime`, представляющий текущую дату и время.</value>
        public DateTime Value
        {
            get => new DateTime(year, month, day, hour, minute, second);
            set
            {
                year = value.Year;
                month = value.Month;
                day = value.Day;
                hour = value.Hour;
                minute = value.Minute;
                second = value.Second;
            }
        }

        /// <summary>
        /// Добавляет `TimeSpanWrapper` к `DateTimeWrapper`.
        /// </summary>
        /// <param name="date">Исходный `DateTimeWrapper`.</param>
        /// <param name="duration">`TimeSpanWrapper` для добавления.</param>
        /// <returns>Новый `DateTimeWrapper`, представляющий сумму.</returns>
        public static DateTimeWrapper operator +(DateTimeWrapper date, TimeSpanWrapper duration)
        {
            return new DateTimeWrapper { Value = date.Value + duration.Value };
        }

        /// <summary>
        /// Вычитает `TimeSpanWrapper` из `DateTimeWrapper`.
        /// </summary>
        /// <param name="date">Исходный `DateTimeWrapper`.</param>
        /// <param name="duration">`TimeSpanWrapper` для вычитания.</param>
        /// <returns>Новый `DateTimeWrapper`, представляющий разность.</returns>
        public static DateTimeWrapper operator -(DateTimeWrapper date, TimeSpanWrapper duration)
        {
            return new DateTimeWrapper { Value = date.Value - duration.Value };
        }

        /// <summary>
        /// Вычисляет разницу между двумя `DateTimeWrapper` и возвращает `TimeSpanWrapper`.
        /// </summary>
        /// <param name="date1">Первый `DateTimeWrapper`.</param>
        /// <param name="date2">Второй `DateTimeWrapper`.</param>
        /// <returns>`TimeSpanWrapper`, представляющий разницу во времени.</returns>
        public static TimeSpanWrapper operator -(DateTimeWrapper date1, DateTimeWrapper date2)
        {
            return new TimeSpanWrapper { Value = date1.Value - date2.Value };
        }

        /// <summary>
        /// Добавляет `System.TimeSpan` к `DateTimeWrapper`.
        /// </summary>
        /// <param name="date">Исходный `DateTimeWrapper`.</param>
        /// <param name="duration">`System.TimeSpan` для добавления.</param>
        /// <returns>Новый `DateTimeWrapper`, представляющий сумму.</returns>
        public static DateTimeWrapper operator +(DateTimeWrapper date, TimeSpan duration)
        {
            return new DateTimeWrapper { Value = date.Value + duration };
        }

        /// <summary>
        /// Вычитает `System.TimeSpan` из `DateTimeWrapper`.
        /// </summary>
        /// <param name="date">Исходный `DateTimeWrapper`.</param>
        /// <param name="duration">`System.TimeSpan` для вычитания.</param>
        /// <returns>Новый `DateTimeWrapper`, представляющий разность.</returns>
        public static DateTimeWrapper operator -(DateTimeWrapper date, TimeSpan duration)
        {
            return new DateTimeWrapper { Value = date.Value - duration };
        }
    }

    /// <summary>
    /// Сериализуемая обертка для `System.TimeSpan`, позволяющая редактировать продолжительность времени в инспекторе Unity.
    /// </summary>
    /// <remarks>
    /// Используется для хранения продолжительности времени в компонентах MonoBehaviour или ScriptableObject,
    /// которые должны быть сериализованы.
    /// </remarks>
    [Serializable]
    public class TimeSpanWrapper
    {
        /// <summary>Компонент дней для сериализации.</summary>
        [SerializeField] private int days = 0;
        /// <summary>Компонент часов для сериализации.</summary>
        [SerializeField] private int hours = 0;
        /// <summary>Компонент минут для сериализации.</summary>
        [SerializeField] private int minutes = 0;
        /// <summary>Компонент секунд для сериализации.</summary>
        [SerializeField] private int seconds = 0;

        /// <summary>
        /// Получает или устанавливает текущее значение как `System.TimeSpan`.
        /// </summary>
        /// <value>Объект `System.TimeSpan`, представляющий текущую продолжительность времени.</value>
        public TimeSpan Value
        {
            get => new TimeSpan(days, hours, minutes, seconds);
            set
            {
                days = value.Days;
                hours = value.Hours;
                minutes = value.Minutes;
                seconds = value.Seconds;
            }
        }

        /// <summary>
        /// Складывает два `TimeSpanWrapper`.
        /// </summary>
        /// <param name="a">Первый `TimeSpanWrapper`.</param>
        /// <param name="b">Второй `TimeSpanWrapper`.</param>
        /// <returns>Новый `TimeSpanWrapper`, представляющий сумму.</returns>
        public static TimeSpanWrapper operator +(TimeSpanWrapper a, TimeSpanWrapper b)
        {
            return new TimeSpanWrapper { Value = a.Value + b.Value };
        }

        /// <summary>
        /// Вычитает один `TimeSpanWrapper` из другого.
        /// </summary>
        /// <param name="a">Исходный `TimeSpanWrapper`.</param>
        /// <param name="b">`TimeSpanWrapper` для вычитания.</param>
        /// <returns>Новый `TimeSpanWrapper`, представляющий разность.</returns>
        public static TimeSpanWrapper operator -(TimeSpanWrapper a, TimeSpanWrapper b)
        {
            return new TimeSpanWrapper { Value = a.Value - b.Value };
        }

        /// <summary>
        /// Умножает `TimeSpanWrapper` на целочисленный множитель.
        /// </summary>
        /// <param name="a">Исходный `TimeSpanWrapper`.</param>
        /// <param name="multiplier">Множитель.</param>
        /// <returns>Новый `TimeSpanWrapper`, представляющий результат умножения.</returns>
        public static TimeSpanWrapper operator *(TimeSpanWrapper a, int multiplier)
        {
            return new TimeSpanWrapper { Value = a.Value * multiplier };
        }

        /// <summary>
        /// Делит `TimeSpanWrapper` на целочисленный делитель.
        /// </summary>
        /// <param name="a">Исходный `TimeSpanWrapper`.</param>
        /// <param name="divisor">Делитель.</param>
        /// <returns>Новый `TimeSpanWrapper`, представляющий результат деления.</returns>
        /// <exception cref="DivideByZeroException">Вызывается, если делитель равен нулю.</exception>
        public static TimeSpanWrapper operator /(TimeSpanWrapper a, int divisor)
        {
            if (divisor == 0) throw new DivideByZeroException();
            return new TimeSpanWrapper { Value = a.Value / divisor };
        }

        /// <summary>
        /// Добавляет `System.TimeSpan` к `TimeSpanWrapper`.
        /// </summary>
        /// <param name="a">Исходный `TimeSpanWrapper`.</param>
        /// <param name="b">`System.TimeSpan` для добавления.</param>
        /// <returns>Новый `TimeSpanWrapper`, представляющий сумму.</returns>
        public static TimeSpanWrapper operator +(TimeSpanWrapper a, TimeSpan b)
        {
            return new TimeSpanWrapper { Value = a.Value + b };
        }

        /// <summary>
        /// Вычитает `System.TimeSpan` из `TimeSpanWrapper`.
        /// </summary>
        /// <param name="a">Исходный `TimeSpanWrapper`.</param>
        /// <param name="b">`System.TimeSpan` для вычитания.</param>
        /// <returns>Новый `TimeSpanWrapper`, представляющий разность.</returns>
        public static TimeSpanWrapper operator -(TimeSpanWrapper a, TimeSpan b)
        {
            return new TimeSpanWrapper { Value = a.Value - b };
        }

        /// <summary>
        /// Сравнивает два `TimeSpanWrapper` на "больше".
        /// </summary>
        /// <param name="a">Первый `TimeSpanWrapper`.</param>
        /// <param name="b">Второй `TimeSpanWrapper`.</param>
        /// <returns>Возвращает `true`, если первый `TimeSpanWrapper` больше второго.</returns>
        public static bool operator >(TimeSpanWrapper a, TimeSpanWrapper b)
        {
            return a.Value > b.Value;
        }

        /// <summary>
        /// Сравнивает два `TimeSpanWrapper` на "меньше".
        /// </summary>
        /// <param name="a">Первый `TimeSpanWrapper`.</param>
        /// <param name="b">Второй `TimeSpanWrapper`.</param>
        /// <returns>Возвращает `true`, если первый `TimeSpanWrapper` меньше второго.</returns>
        public static bool operator <(TimeSpanWrapper a, TimeSpanWrapper b)
        {
            return a.Value < b.Value;
        }

        /// <summary>
        /// Сравнивает два `TimeSpanWrapper` на "больше или равно".
        /// </summary>
        /// <param name="a">Первый `TimeSpanWrapper`.</param>
        /// <param name="b">Второй `TimeSpanWrapper`.</param>
        /// <returns>Возвращает `true`, если первый `TimeSpanWrapper` больше или равен второму.</returns>
        public static bool operator >=(TimeSpanWrapper a, TimeSpanWrapper b)
        {
            return a.Value >= b.Value;
        }

        /// <summary>
        /// Сравнивает два `TimeSpanWrapper` на "меньше или равно".
        /// </summary>
        /// <param name="a">Первый `TimeSpanWrapper`.</param>
        /// <param name="b">Второй `TimeSpanWrapper`.</param>
        /// <returns>Возвращает `true`, если первый `TimeSpanWrapper` меньше или равен второму.</returns>
        public static bool operator <=(TimeSpanWrapper a, TimeSpanWrapper b)
        {
            return a.Value <= b.Value;
        }

        /// <summary>
        /// Сравнивает два `TimeSpanWrapper` на равенство.
        /// </summary>
        /// <param name="a">Первый `TimeSpanWrapper`.</param>
        /// <param name="b">Второй `TimeSpanWrapper`.</param>
        /// <returns>Возвращает `true`, если оба `TimeSpanWrapper` равны.</returns>
        public static bool operator ==(TimeSpanWrapper a, TimeSpanWrapper b)
        {
            return a.Value == b.Value;
        }

        /// <summary>
        /// Сравнивает два `TimeSpanWrapper` на неравенство.
        /// </summary>
        /// <param name="a">Первый `TimeSpanWrapper`.</param>
        /// <param name="b">Второй `TimeSpanWrapper`.</param>
        /// <returns>Возвращает `true`, если `TimeSpanWrapper` не равны.</returns>
        public static bool operator !=(TimeSpanWrapper a, TimeSpanWrapper b)
        {
            return a.Value != b.Value;
        }

        /// <summary>
        /// Определяет, равен ли указанный объект текущему объекту `TimeSpanWrapper`.
        /// </summary>
        /// <param name="obj">Объект, который требуется сравнить с текущим объектом.</param>
        /// <returns>Возвращает `true`, если указанный объект равен текущему объекту, иначе `false`.</returns>
        public override bool Equals(object obj)
        {
            if (obj is TimeSpanWrapper other)
            {
                return this == other;
            }
            return false;
        }

        /// <summary>
        /// Служит хэш-функцией по умолчанию.
        /// </summary>
        /// <returns>Хэш-код для текущего объекта.</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}