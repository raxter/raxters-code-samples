using System.Collections.Generic;
using Concoctions;
using TriInspector;
using UnityEngine;

public class ItemInventory : MonoBehaviour
{
    [SerializeField]
    List<Concoction> inventory = new List<Concoction>();

    public int Count => inventory.Count;
    
    [ShowInInspector] private Concoction handingOver;

    public bool clearOnStart = true;

    public Concoction TryHandOverTopItem()
    {
        handingOver = TakeFrom();
        return handingOver;
    }
    public void HandoverSuccessful()
    {
        handingOver = null;
    }
    
    public void HandoverReturned()
    {
        if (handingOver != null)
            AddTo(handingOver);
        handingOver = null;
    }

    void Start()
    {
        if (clearOnStart)
            inventory.Clear();
        
    }
    
    public IReadOnlyList<Concoction> Inventory => inventory;

    public Concoction TopItem => CanTake ? inventory[^1] : null;
    public bool CanTake => inventory.Count > 0;

    public void AddTo(Concoction itemName)
    {
        inventory.Add(itemName);
    }
    
    public bool RemoveFrom(Concoction itemName)
    {
        if (inventory.Contains(itemName))
        {
            inventory.Remove(itemName);
            return true;
        }
        return false;
    }

    public Concoction TakeFrom()
    {
        // take from back FIFO
        if (inventory.Count == 0)
            return null;
        var item = inventory[inventory.Count - 1];
        inventory.RemoveAt(inventory.Count - 1);
        return item;
    }

    public string GetListedDescription(bool topFirst = true, bool doCount = false)
    {
        string handDesc = "";
        Concoction lastDesc = null;
        int count = 0;
        
        string ItemDesc(int c, Concoction conc) => c + "x " + conc.GetFullDescription() + "\n";
        string ItemDescNoCount(Concoction conc) => conc.GetFullDescription() + "\n";
        for (int i = 0; i < inventory.Count; i++)
        {
            var inventoryI = topFirst ? inventory[inventory.Count - 1 - i] : inventory[i];

            if (!doCount)
            {
                handDesc += ItemDescNoCount(inventoryI);
                continue;
            }
            // else
            if (lastDesc == inventoryI)
            {
                count++;
            }
            else
            {
                if (lastDesc != null)
                    handDesc += ItemDesc(count, lastDesc);
                
                lastDesc = inventoryI;
                count = 1;
            }
        }
        if (doCount && lastDesc != null)
            handDesc += ItemDesc(count, lastDesc);

        return handDesc;
    }

    public void MixAllIngredients()
    {
        Concoction all = new Concoction();
        foreach (var c in inventory)
            all.Union(c);
        inventory.Clear();
        inventory.Add(all);
    }

    public void CycleOrder()
    {
        //top item becomes last
        if (inventory.Count <= 1)
            return;
        
        var item = inventory[inventory.Count - 1];
        inventory.RemoveAt(inventory.Count - 1);
        inventory.Insert(0, item);
    }

    public void Clear()
    {
        inventory.Clear();
    }
}
