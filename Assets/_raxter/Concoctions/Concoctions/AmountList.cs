using System.Collections;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[System.Serializable]
public class AmountList : AmountList.IReadOnly
{
    
    public override string ToString() => string.Join(", ", this);
    
    public interface IReadOnly : IEnumerable<AmountOfIngredient>
    {
        
        int Count { get; }
        int UniqueCount { get; }
        AmountOfIngredient First { get; }
        float TotalAmount { get; }
        bool IsNothing { get; }
        bool IsSinglePure { get; }
        bool IsSingleIngredient { get; }
        AmountOfIngredient Find(AmountOfIngredient aoi);
        AmountOfIngredient Find(string status, string ingredient, string part);
        float GetAmount(AmountOfIngredient aoi);
        float GetAmount(string status, string ingredient, string part);
        
        // filter functions
        public AmountList Filter(string status, string ingredient, string part) => Filter(status, ingredient, part, out _);
        public AmountList Filter(string status, string ingredient, string part, out AmountList residue);
        public AmountList Filter(Ingredient i, out AmountList residue);
        public AmountList FilterMultiple(out AmountList residue, params IEnumerable<Ingredient>[] ingredientLists);
        public AmountList FilterMultiple(out AmountList residue, params Ingredient[] ingredients);
        public AmountList FilterMultiple(params Ingredient[] ingredients);
        public AmountList FilterMultiple(IEnumerable<Ingredient> ingredients);
        public AmountList FilterMultiple(out AmountList residue, IEnumerable<Ingredient> ingredients);
    }

    [InlineProperty]
    [HideReferencePicker]
    [ListDrawerSettings(AlwaysExpanded = true)]
    [SerializeField]
    List<AmountOfIngredient> data = new ();

    public AmountList()
    {
    }

    public AmountList(IReadOnly other)
    {
        foreach (var aoi in other)
        {
            data.Add(new AmountOfIngredient(aoi));
        }
    }
    
    public AmountList(AmountOfIngredient aoi)
    {
        data.Add(new AmountOfIngredient(aoi));
    }

    public int Count => data.Count;

    public int UniqueCount
    {
        get
        {
            HashSet<string> uniqueIngredients = new HashSet<string>();
            foreach (var kv in data)
                uniqueIngredients.Add(kv.ingredient);
            return uniqueIngredients.Count;
        }
    }

    public AmountOfIngredient First => data?[0] ?? null;
    public float TotalAmount
    {
        get
        {
            float total = 0;
            foreach (var kv in data)
                total += kv.amount;
            return total;
        }
    }

    public bool IsNothing => Count == 0;
    public bool IsSinglePure => Count == 1;
    public bool IsSingleIngredient => UniqueCount == 1;

    public float GetAmount(AmountOfIngredient aoi) =>
        Find(aoi)?.amount ?? 0f;
    public float GetAmount(string status, string ingredient, string part) =>
        Find(status, ingredient, part)?.amount ?? 0f;

    public void Add(Ingredient aoi, float amount) =>
        Add(aoi.status, aoi.ingredient, aoi.part, amount);
    public void Add(AmountOfIngredient aoi, float amount) =>
        Add(aoi.status, aoi.ingredient, aoi.part, amount);
    public void Add(string ingredient, float amount) =>
        Add("", ingredient, "", amount);
    public void Add(string ingredient, string part, float amount) =>
        Add("", ingredient, part, amount);

    public void Add(string status, string ingredient, string part, float amount)
    {
        var aoi = Find(status, ingredient, part);
        if (aoi != null)
            aoi.amount += amount;
        else
            data.Add(new AmountOfIngredient(ingredient, part, amount, status));
    }

    public void Add(AmountOfIngredient aoi) => data.Add(new(aoi));

    public AmountOfIngredient FindByName(string name) =>
        data.Find(i => i.Name == name);

    public AmountOfIngredient Find(string status, string ingredient, string part) =>
        data.Find(i => i.ingredient == ingredient && i.status == status && i.part == part);

    public AmountOfIngredient Find(AmountOfIngredient aoi) =>
        Find(aoi.status, aoi.ingredient, aoi.part);
    
    public IEnumerable<AmountOfIngredient> FindAll(string ingredient, string part)
    {
        foreach (var aoi in data)
        {
            if (aoi.ingredient == ingredient && (part == "*" || aoi.part == part))
                yield return aoi;
        }
    }

    public IEnumerator<AmountOfIngredient> GetEnumerator() => data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();
    
    
    public void AddSingle(string ingredient, string part, float amount) => 
        AddSingle("", ingredient, part, amount);
    public void AddSingle(string status, string ingredient, string part, float amount)
    {
        //liquidAmounts[liquid] = liquidAmounts.TryGetValue(liquid, out var conc) ? conc + amount : amount;
        var aoi = Find(status, ingredient, part);
        if (aoi != null)
            aoi.amount += amount;
        else
            Add(new AmountOfIngredient(ingredient, part, amount, status));
    }

    public float GetTotalAmount(string ingredient)
    {
        float total = 0;
        foreach (var kv in data)
        {
            if (kv.ingredient == ingredient)
                total += kv.amount;
        }
        return total;
    }

    public void Clean()
    {
        // remove items and liquids that are now 0
        data.RemoveAll(i => i.amount <= 0 || float.IsNaN(i.amount));
    }

    public bool IsSingle() => UniqueCount == 1;

    public bool IsSingle(string ingredient, string part = "")
    {
        if (UniqueCount != 1)
            return false;
        foreach (var aoi in FindAll(ingredient, part))
            return true;
        
        return false;
    }
    
    public bool IsSingle(string status, string ingredient, string part)
    {
        if (Count != 1)
            return false;
        var itemAmount = Find(status, ingredient, part);
        return itemAmount != null;
    }


    public AmountList Residue(string status, string ingredient, string part)
    {
        Filter(status, ingredient, part, out AmountList residue);
        return residue;
    }
    public AmountList Filter(string status, string ingredient, string part) => Filter(status, ingredient, part, out _);
    public AmountList Filter(string status, string ingredient, string part, out AmountList residue)
     => Filter(new Ingredient(status, ingredient, part), out residue);
    public AmountList Filter(Ingredient i, out AmountList residue)
    {
        var ingredient = i.ingredient;
        var status = i.status;
        var part = i.part;
        
        residue = new AmountList();
        AmountList filtered = new AmountList();
        foreach (var aoi in data)
        {
            var isFilter = aoi.ingredient == ingredient && (status == "*" || aoi.status == status) && (part == "*" || aoi.part == part);
            if (isFilter)
                filtered.Add(aoi);
            else
                residue.Add(aoi);
        }

        return filtered;
    }

    public AmountList FilterMultiple(out AmountList residue, params IEnumerable<Ingredient>[] ingredientLists)
    {
        IEnumerable<Ingredient> MultiEnum()
        {
            foreach (var list in ingredientLists)
                foreach (var ingredient in list)
                    yield return ingredient;
        }
        return FilterMultiple(out residue, MultiEnum());
    }

    public AmountList FilterMultiple(out AmountList residue, params Ingredient[] ingredients)
    {
        return FilterMultiple(out residue, ingredients);
    }
    public AmountList FilterMultiple(params Ingredient[] ingredients)
    {
        return FilterMultiple(out _, ingredients);
    }
    public AmountList FilterMultiple(IEnumerable<Ingredient> ingredients)
    {
        return FilterMultiple(out _, ingredients);
    }
    public AmountList FilterMultiple(out AmountList residue, IEnumerable<Ingredient> ingredients)
    {
        AmountList filtered = new AmountList();
        residue = new AmountList();

        foreach (var aoi in data)
        {
            var isFilter = false;
            
            foreach (var i in ingredients)
            {
                var ingredient = i.ingredient;
                var status = i.status;
                var part = i.part;
                
                isFilter = aoi.ingredient == ingredient && (status == "*" || aoi.status == status) && (part == "*" || aoi.part == part);
                if (isFilter)
                    break;
            }
            
            if (isFilter)
                filtered.Add(aoi);
            else
                residue.Add(aoi);
        }
        return filtered;
    }
    
    public AmountList Concentrations => this / TotalAmount;
    
    public static AmountList operator *(AmountList a, float b)
    {
        AmountList result = new AmountList();
        foreach (var aoi in a)
            result.Add(aoi, aoi.amount * b);
        return result;
    }
    public static AmountList operator /(AmountList a, float b)
    {
        AmountList result = new AmountList();
        foreach (var aoi in a)
            result.Add(aoi, aoi.amount / b);
        return result;
    }

    public AmountOfIngredient FindWithPattern(Ingredient pattern, IngredientWildcardLibrary wildcards)
    {
        foreach (var aoi in data)
        {
            if (Ingredient.MatchPattern(aoi, pattern, wildcards))
                return aoi;
        }

        return null;
    }

    public void Union(AmountList items)
    {
        foreach (var aoi in items)
        {
            //itemCounts[kv.Key] = itemCounts.TryGetValue(kv.Key, out var count) ? count + kv.Value : kv.Value;
            var itemAmount = Find(aoi);
            if (itemAmount != null)
                itemAmount.amount += aoi.amount;
            else
                Add(new AmountOfIngredient(aoi));
        }
    }

    public void Exclude(AmountList exclude)
    {
        foreach (var aoi in exclude)
        {
            var itemAmount = Find(aoi);
            if (itemAmount != null)
                itemAmount.amount -= aoi.amount;
        }

        Clean();
    }
}