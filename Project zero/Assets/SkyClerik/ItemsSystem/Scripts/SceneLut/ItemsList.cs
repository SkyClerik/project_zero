using System.Collections.Generic;
using UnityEngine;
using UnityEngine.DataEditor;

[System.Serializable]
public class ItemsList
{
    [SerializeField]
    private List<ItemBaseDefinition> _items = new List<ItemBaseDefinition>();

    public List<ItemBaseDefinition> Items { get => _items; set => _items = value; }
}