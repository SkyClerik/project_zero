using System;
using UnityEngine;
using UnityEngine.Toolbox;

namespace SkyClerik
{
    /// <summary>
    /// Фасад для управления кошельком. Предоставляет упрощенный и безопасный доступ 
    /// к функциям класса Money и регистрируется в ServiceProvider.
    /// </summary>
    public class MoneyAPI : MonoBehaviour
    {
        /// <summary>
        /// Вызывается при изменении баланса.
        /// long: новый баланс, long: дельта (изменение).
        /// </summary>
        public event Action<long, long> OnMoneyChanged;

        [Header("Кошелек")]
        [Tooltip("Содержит всю логику и данные о деньгах. Настраивается в инспекторе.")]
        [SerializeField]
        private Money _wallet = new Money();

        #region Unity Lifecycle & Service Registration
        private void Awake()
        {
            ServiceProvider.Register(this);

            if (_wallet != null)
                _wallet.OnMoneyChanged += HandleMoneyChanged;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            SubscribeToEventsForLogging();
#endif
        }

        private void Start()
        {
            // Инициализируем, чтобы UI и другие системы получили начальное значение
            _wallet?.Initialize();
        }

        private void OnDestroy()
        {
            ServiceProvider.Unregister(this);

            if (_wallet != null)
                _wallet.OnMoneyChanged -= HandleMoneyChanged;
        }
        #endregion

        #region Public Facade Methods

        /// <summary>
        /// Возвращает текущий баланс.
        /// </summary>
        public long Amount => _wallet.Amount;

        /// <summary>
        /// Проверяет, достаточно ли денег на счету.
        /// </summary>
        public bool HasEnough(long amount) => _wallet.HasEnough(amount);

        /// <summary>
        /// Проверяет, достаточно ли денег, и возвращает недостающую сумму.
        /// </summary>
        public bool HasEnough(long amount, out long shortfall) => _wallet.HasEnough(amount, out shortfall);

        /// <summary>
        /// Пытается потратить указанную сумму.
        /// </summary>
        /// <returns>True, если трата прошла успешно.</returns>
        public bool TrySpend(long amount) => _wallet.TrySpend(amount);

        /// <summary>
        /// Пытается потратить указанную сумму и возвращает недостачу в случае неудачи.
        /// </summary>
        /// <returns>True, если трата прошла успешно.</returns>
        public bool TrySpend(long amount, out long shortfall) => _wallet.TrySpend(amount, out shortfall);

        /// <summary>
        /// Добавляет указанную сумму на счет. Отрицательные значения игнорируются.
        /// </summary>
        public void Add(long amount) => _wallet.Add(amount);

        /// <summary>
        /// Пытается перевести деньги на счет другого получателя.
        /// </summary>
        /// <param name="recipientAPI">API кошелька-получателя.</param>
        /// <param name="amount">Сумма перевода.</param>
        /// <returns>True, если перевод прошел успешно.</returns>
        public bool TryTransferTo(MoneyAPI recipientAPI, long amount)
        {
            if (recipientAPI == null)
            {
                Debug.LogError("[MoneyAPI] Получатель не может быть null!", this);
                return false;
            }
            // Используем статический метод из Money, передавая ему внутренние кошельки
            return Money.TryTransfer(this._wallet, recipientAPI._wallet, amount);
        }

        #endregion

        /// <summary>
        /// Внутренний обработчик, который транслирует событие из класса Money наружу.
        /// </summary>
        private void HandleMoneyChanged(long newAmount, long delta)
        {
            OnMoneyChanged?.Invoke(newAmount, delta);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// [Только для разработки] Подписывается на событие и выводит изменения в консоль.
        /// </summary>
        private void SubscribeToEventsForLogging()
        {
            OnMoneyChanged += (newAmount, delta) =>
            {
                if (delta > 0)
                    Debug.Log($"<color=cyan>[MoneyAPI]</color> Баланс изменен: <b>{newAmount}</b> (+{delta})");
                else if (delta < 0)
                    Debug.Log($"<color=orange>[MoneyAPI]</color> Баланс изменен: <b>{newAmount}</b> ({delta})");
            };

            Debug.Log("<color=lime>[MoneyAPI]</color> Отладочное логирование кошелька включено.");
        }
#endif
    }
}
