using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkyClerik.Inventory;
using SkyClerik.Utils;
using UnityEngine.Toolbox;
using System;

public class TestInv : MonoBehaviour
{
   // private InventoryAPI InvApi;
    private void Start()
    {
      //  InvApi = ServiceProvider.Get<InventoryAPI>();
    }
    
    public void AddItem(int MyID_From_Item)
    {
        var item = new LutContainerWrapper(MyID_From_Item);
        item.TransferItemsToPlayerInventoryContainer();
        
        Debug.Log("What it ? " +  item);
    }
    public Container DeleteItem(int MyID_From_World, int Count)
    {

        return MyInfo[0];
    }
    public bool UseItem(int MyID_Use_Item)
    {
        return true;
    }
    [SerializeField] private Container[] MyInfo;
    
}

[Serializable]
public struct Container
{
    public bool i_did_look;
    public int Count;
    public bool Deleted;
}
//inventory test