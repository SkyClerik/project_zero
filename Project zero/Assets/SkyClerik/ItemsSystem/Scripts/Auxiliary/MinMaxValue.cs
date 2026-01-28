using UnityEngine;

[System.Serializable]
public class IntMM
{
    [SerializeField]
    private int _min;
    [SerializeField]
    private int _cur;
    [SerializeField]
    private int _max;

    public int GetMin => _min;
    public int Current { get => _cur; set => _cur = value; }
    public int GetMax => _max;
}