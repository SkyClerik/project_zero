using UnityEngine;

/// <summary>
/// Представляет собой структуру для хранения минимального, текущего и максимального значений целочисленного типа.
/// </summary>
[System.Serializable]
public class IntMM
{
    [SerializeField]
    private int _min;
    [SerializeField]
    private int _cur;
    [SerializeField]
    private int _max;

    /// <summary>
    /// Возвращает минимальное значение.
    /// </summary>
    public int GetMin => _min;
    /// <summary>
    /// Возвращает или устанавливает текущее значение.
    /// </summary>
    public int Current { get => _cur; set => _cur = value; }
    /// <summary>
    /// Возвращает максимальное значение.
    /// </summary>
    public int GetMax => _max;
}