using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkyClerik.Inventory;
using SkyClerik.Utils;
using UnityEngine.Toolbox;
using System;
using UnityEngine.DataEditor;

public class TestInv : MonoBehaviour
{
    private InventoryAPI InvApi;

    public int ID, Count;
    private void Start()
    {
        InvApi = ServiceProvider.Get<InventoryAPI>();



    }
    
    public void AddItem(int MyID_From_Item)
    {
        var chiki = InvApi.TryAddItemsToPlayerInventory(MyID_From_Item, out ItemBaseDefinition item);
        if (chiki)
        {
            Debug.Log("name " + item.DefinitionName);
        }
    }
    public void DeleteItem()
    {
       var result =  InvApi.TryRemoveItemInPlayerInventory(ID, Count);
        Debug.Log("NASHEL? " + result.IDidLook + " NotEnough? " + result.NotEnough + " Deleted? " + result.IsDeleted);
       
    }
    private void ChikiPencle(ItemBaseDefinition item)
    {
        if(item.ID == ID_Push_Delete)
        {
            Debug.Log("черепашки ниндзя воруют Ваш " + item.DefinitionName + " в количестве одной штуки");
            InvApi.TryRemoveItem(item,1 ,ItemContainer.ItemRemoveReason.Destroy);
            InvApi.OnItemGiven -= ChikiPencle;
            InvApi.CloseAll();

        }
        else
        {
            Debug.Log("Черепашки ниндзя против этого!");
            
        }
       
    }
    public int ID_Push_Delete;
    public void OpenUseInventory()
    {
        if (InvApi.IsInventoryVisible)
        {
            InvApi.OnItemGiven -= ChikiPencle;
            InvApi.CloseAll();
        }
        else
        {
            InvApi.OnItemGiven += ChikiPencle;
            InvApi.OpenInventoryFromGiveItem(ID_Push_Delete, tracing: false); //light cell 
        }
    }

    public void SaveM()
    {
        InvApi.SaveInventory();
        Debug.Log("я пытался сохранить");
    }

    public void LoadM()
    {
        InvApi.LoadInventory_();
        Debug.Log("я пытался загрузить");
    }

  
    
}

