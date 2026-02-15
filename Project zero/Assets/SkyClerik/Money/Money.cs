using System;
using UnityEngine;

namespace SkyClerik
{
    /// <summary>
    /// Чистый C# класс, который управляет логикой денежного счета.
    /// Является сериализуемым для интеграции с инспектором Unity через родительский компонент.
    /// Не содержит логики, специфичной для MonoBehaviour.
    /// </summary>
    [System.Serializable]
    public class Money
    {
        /// <summary>
        /// Внутреннее событие, которое вызывается при изменении количества денег.
        /// </summary>
        public event Action<long, long> OnMoneyChanged; // long: newAmount, long: delta

        [Tooltip("Начальное и текущее количество денег на счету.")]
        [SerializeField]
        private long _amount;

        /// <summary>
        /// Текущее количество денег на счету. Доступно только для чтения извне.
        /// </summary>
        public long Amount => _amount;

        /// <summary>
        /// Инициализирует кошелек, вызывая событие с текущим количеством денег.
        /// </summary>
        public void Initialize()
        {
            OnMoneyChanged?.Invoke(_amount, 0);
        }
        
        /// <summary>
        /// Проверяет, достаточно ли денег на счету для выполнения операции.
        /// </summary>
        public bool HasEnough(long amountToSpend)
        {
            return HasEnough(amountToSpend, out _);
        }

        /// <summary>
        /// Проверяет, достаточно ли денег на счету, и возвращает недостающую сумму.
        /// </summary>
        public bool HasEnough(long amountToSpend, out long shortfall)
        {
            if (amountToSpend < 0) 
            {
                shortfall = 0;
                return false; 
            }

            if (_amount >= amountToSpend)
            {
                shortfall = 0;
                return true;
            }
            
            shortfall = amountToSpend - _amount;
            return false;
        }

        /// <summary>
        /// Пытается списать указанную сумму со счета.
        /// </summary>
        public bool TrySpend(long amountToSpend)
        {
            return TrySpend(amountToSpend, out _);
        }
        
        /// <summary>
        /// Пытается списать указанную сумму со счета и возвращает недостающую сумму в случае неудачи.
        /// </summary>
        public bool TrySpend(long amountToSpend, out long shortfall)
        {
            if (amountToSpend < 0)
            {
                shortfall = 0;
                return false;
            }

            if (!HasEnough(amountToSpend, out shortfall))
            {
                return false;
            }

            var oldAmount = _amount;
            _amount -= amountToSpend;
            OnMoneyChanged?.Invoke(_amount, _amount - oldAmount);
            return true;
        }

        /// <summary>
        /// Добавляет указанную сумму на счет.
        /// </summary>
        public void Add(long amountToAdd)
        {
            if (amountToAdd <= 0)
            {
                return;
            }
            
            var oldAmount = _amount;
            _amount += amountToAdd;
            OnMoneyChanged?.Invoke(_amount, _amount - oldAmount);
        }

        /// <summary>
        /// Устанавливает точное значение денег на счету.
        /// </summary>
        public void Set(long newAmount)
        {
            if (newAmount < 0)
            {
                newAmount = 0;
            }

            if (_amount == newAmount)
            {
                return;
            }
            
            var oldAmount = _amount;
            _amount = newAmount;
            OnMoneyChanged?.Invoke(_amount, _amount - oldAmount);
        }

        /// <summary>
        /// Пытается перевести деньги с одного счета на другой.
        /// </summary>
        public static bool TryTransfer(Money from, Money to, long amount)
        {
            if (from == null || to == null || amount <= 0)
            {
                return false;
            }

            if (from.TrySpend(amount))
            {
                to.Add(amount);
                return true;
            }

            return false;
        }
    }
}
