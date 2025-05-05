using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Concoctions
{
    [DeclareBoxGroup("All", Title = "$"+nameof(OverallDescriptionFull))]
//[DeclareVerticalGroup("All/Substance")]
    [DeclareHorizontalGroup("All/Substance")]
    [DeclareFoldoutGroup("All/Debug")]
    [System.Serializable]
    public partial class Concoction
    {
        public Concoction() { }

        public Concoction(Concoction concoction)
        {
            solids = new AmountList(concoction.Solids);
            liquids = new AmountList(concoction.Liquids);
        }
    
    
        //*operator with float
        public static Concoction operator *(Concoction c, float f)
        {
            Concoction result = new Concoction(c);
            result.solids *= f;
            result.liquids *= f;
            return result;
        }
    
    
        public AmountOfIngredient AsSingleItem()
        {
            if (solids.Count != 1)
                return null;
            if (liquids.Count != 0)
                return null;
            return new AmountOfIngredient(solids.First);
        }
    
        public AmountOfIngredient AsSingleItem(string ingredient, string part)
        {
            if (solids.Count != 1)
                return null;
            if (liquids.Count != 0)
                return null;
            if (solids.First.ingredient != ingredient)
                return null;
            if (solids.First.part != part)
                return null;
            return new AmountOfIngredient(solids.First);
        }
    
    
        [Title("$"+nameof(SolidsDescriptionFull))]
        [Group("All/Substance")]
        [SerializeField]
        AmountList solids = new ();
        [Title("$"+nameof(LiquidDescriptionFull))]
        [Group("All/Substance")]
        [SerializeField]
        AmountList liquids = new ();

        public AmountList.IReadOnly Solids => solids;
        public AmountList.IReadOnly Liquids => liquids;
    
    
        public enum Temperature {Cold, Tepid, Warm, Hot, Boiling}
    
        public static Concoction Water (int volume)
            =>
                new Concoction
                {
                    liquids = new AmountList {new("Water", volume)},
                };
    
        public static Concoction SingleItem (string item, string part, float amount = 1)
            =>
                new Concoction
                {
                    solids = new AmountList {new(item, part, amount)}
                };
        public static Concoction SingleItem (string status, string item, string part, float amount = 1)
            =>
                new Concoction
                {
                    solids = new AmountList {new(item, part, amount, status)}
                };
    
        public static Concoction Nothing => new();
    
        public static Concoction LiquidConcoction (AmountList liquidAmounts)
            =>
                new Concoction
                {
                    liquids = new AmountList(liquidAmounts),
                };        
        public static Concoction LiquidConcoction (AmountOfIngredient liquidAmount)
            =>
                new Concoction
                {
                    liquids = new AmountList(liquidAmount),
                };
        public static Concoction LiquidConcoction (Ingredient liquidAmount, float amount = 1)
            =>
                new Concoction
                {
                    liquids = new AmountList(new AmountOfIngredient(liquidAmount, 1)),
                };

    
    
        public Concoction ExtractLiquid(float amount, bool extractPartial)
        {
            // create a new concoction with the liquid
            float totalLiquidVolume = 0;
            foreach (var kv in liquids)
            {
                totalLiquidVolume += kv.amount;
            }

            if (totalLiquidVolume < amount)
            {
                if (extractPartial)
                    amount = totalLiquidVolume;
                else
                    return null;
            }
        
            float ratioToExtract = amount / totalLiquidVolume;
            AmountList extractedLiquid = new AmountList();
            foreach (var aoi in liquids)
            {
                //extractedLiquid[kv.Key] = kv.Value * ratioToExtract;
                float extractedAmount = aoi.amount * ratioToExtract;
                if (extractedAmount > 0)
                {
                    extractedLiquid.Add(new AmountOfIngredient(aoi, extractedAmount));
                    aoi.amount -= extractedAmount;
                }
            
            }
        
        
            return LiquidConcoction(extractedLiquid);
        }


        [Group("All/Debug")]
        [Button]
        public void BoilIngredients()
        {
            //TODO boil water ("water + heat => boiling water") then run a mixture step where "boiling water + x => boiled x + x(l)" etc
        
            PerformMixtureStep();
        }

        [Group("All/Debug")]
        // TODO assumes a boil, should rather convers water => boiling water and let the mixture sort itself out
        [Button]
        public void PerformMixtureStep() 
        {
            AmountList sulliedWater = new AmountList();
            // overboil the old concentration turns non water into half the original and half water
            foreach (var liquid in liquids)
            {
                if (liquid.ingredient == "Water")
                    continue;
                string waterStatus = "";
                if (TypeOf(liquid.ingredient) == PlantMatterType)
                    waterStatus = "Herbaceous";
            
                float liquidAmount = liquid.amount/2;
                if (liquid.amount < 0.1f)
                    liquidAmount = liquid.amount;
            
                sulliedWater.AddSingle(waterStatus, "Water", liquidAmount);
                liquid.amount -= liquidAmount;
            }
            liquids.Union(sulliedWater);
            CleanAmounts();
        
            float boilableItems = 0;
            foreach (var kv in solids)
            {
                if (kv.status == "Boiled")
                    continue;
                boilableItems += kv.amount;
            }

            float waterAmount = liquids.GetTotalAmount("Water");
            float boilRatio = Mathf.Min(1, waterAmount / boilableItems);
        
            // convert non boiled items to boiled items, converting water to ingredient water
            Concoction brewwedConcoction = new Concoction();
            foreach (var itemAmount in solids)
            {
                if (itemAmount.status == "Boiled")
                    continue;
                float boilAmount = itemAmount.amount * boilRatio;
                //brewwedConcoction.solids.Add("Boiled", itemAmount.ingredient, boilAmount);
                //brewwedConcoction.solids.Add(itemAmount.ingredient, -boilAmount);
                //brewwedConcoction.liquids.Add("Boiled", itemAmount.ingredient, boilAmount);
                //brewwedConcoction.liquids.Add("Water", -boilAmount);
                brewwedConcoction.solids.Add(itemAmount.WithStatus("Boiled"), boilAmount);
                brewwedConcoction.solids.Add(itemAmount, -boilAmount);
                brewwedConcoction.liquids.Add(itemAmount.WithStatus("Boiled"), boilAmount);
                brewwedConcoction.liquids.Add("Water", -boilAmount);
            
            }
            Union(brewwedConcoction);

            CleanAmounts();
        }

        void CleanAmounts()
        {
            solids.Clean();
            liquids.Clean();
        }


        //=========================================================
    
    
        public void Union(Concoction concoction)
        {
            solids.Union(concoction.solids);
            liquids.Union(concoction.liquids);
        }

    
        public void Exclude(Concoction exclude)
        {
            solids.Exclude(exclude.solids);
            liquids.Exclude(exclude.liquids);
        }
    
        // make equals operator
        public static bool operator ==(Concoction c1, Concoction c2)
        {
            if (c1 is null && c2 is null)
                return true;
            if (c1 is null || c2 is null)
                return false;
            
            if (c1.solids.Count != c2.solids.Count)
                return false;
            if (c1.liquids.Count != c2.liquids.Count)
                return false;
        
            foreach (var c1Amount in c1.solids)
            {
                var c2Amount = c2.solids.Find(c1Amount);
                if (c2Amount == null)
                    return false;
                if (c2Amount.amount != c1Amount.amount)
                    return false;
            }
        
            foreach (var c1Amount in c1.liquids)
            {
                var c2Amount = c2.liquids.Find(c1Amount);
                if (c2Amount == null)
                    return false;
                if (c2Amount.amount != c1Amount.amount)
                    return false;
            }

            return true;
        }

        public static bool operator !=(Concoction c1, Concoction c2)
        {
            return !(c1 == c2);
        }

        public Concoction ExtractCopy(Concoction pattern, ProcessResultBase.ScalingType scalingType, 
            out float ratioExtracted, IngredientWildcardLibrary wildcards) =>
            ExtractCopy(pattern, scalingType, out ratioExtracted, wildcards, out _);
    
        // returns null if not possible
        public Concoction ExtractCopy(
            Concoction pattern, ProcessResultBase.ScalingType scalingType, out float ratioExtracted, 
            IngredientWildcardLibrary wildcards, out Dictionary<string, string> wildcardLookup)
        {
            wildcardLookup = null;

            void ProcessWildcard(AmountOfIngredient pattern, AmountOfIngredient result, ref Dictionary<string, string> lookup)
            {
                void AddWildcard(string key, string value, ref Dictionary<string, string> lookup)
                {
                    lookup ??= new Dictionary<string, string>();
                    if (lookup.ContainsKey(key) && lookup[key] != value)
                        Debug.LogError("Multiple wildcards for " + key + " with different values: " + lookup[key] + " and " + value);
                    lookup[key] = value;
                }
            
                foreach (Ingredient.Type type in System.Enum.GetValues(typeof(Ingredient.Type)))
                {
                    string patternValue = pattern.GetValue(type);
                    string ingredientValue = result.GetValue(type);
                
                    if (patternValue.StartsWith("*"))
                        AddWildcard(patternValue, ingredientValue, ref lookup);
                }
            }

            bool canScale = scalingType != ProcessResultBase.ScalingType.NoScaling;
            float ratioToTake = canScale ? float.MaxValue : 1;
            ratioExtracted = 0;
        
            Concoction extractionCopy = new ();
            foreach (var s in pattern.solids)
            {
                var solid = solids.FindWithPattern(s, wildcards);
                if (solid == null)
                    return null;
                if (!canScale && solid.amount < s.amount)
                    return null;
                if (canScale)
                    ratioToTake = Mathf.Min(ratioToTake, solid.amount / s.amount);
            
                ProcessWildcard(s, solid, ref wildcardLookup);
                extractionCopy.solids.Add(solid, s.amount);
            }
            foreach (var l in pattern.liquids)
            {
                var liquid = liquids.FindWithPattern(l, wildcards);
                if (liquid == null)
                    return null;
                if (!canScale && liquid.amount < l.amount)
                    return null;
                if (canScale)
                    ratioToTake = Mathf.Min(ratioToTake, liquid.amount / l.amount);
            
                ProcessWildcard(l, liquid, ref wildcardLookup);
                extractionCopy.liquids.Add(liquid, l.amount);
            }

            if (scalingType == ProcessResultBase.ScalingType.ScalesDiscretely)
            {
                ratioToTake = Mathf.Floor(ratioToTake);
                if (ratioToTake < 1)
                    return null;
            }
            ratioExtracted = ratioToTake;
            return extractionCopy * ratioToTake;
        }

        public void ReplaceWildcards(Dictionary<string, string> wildcardLookup)
        {
            foreach (var s in solids)
            {
                if (wildcardLookup.ContainsKey(s.ingredient))
                    s.ingredient = wildcardLookup[s.ingredient];
                if (wildcardLookup.ContainsKey(s.status))
                    s.status = wildcardLookup[s.status];
                if (wildcardLookup.ContainsKey(s.part))
                    s.part = wildcardLookup[s.part];
            }
            foreach (var l in liquids)
            {
                if (wildcardLookup.ContainsKey(l.ingredient))
                    l.ingredient = wildcardLookup[l.ingredient];
                if (wildcardLookup.ContainsKey(l.status))
                    l.status = wildcardLookup[l.status];
                if (wildcardLookup.ContainsKey(l.part))
                    l.part = wildcardLookup[l.part];
            }
        }

        public static Concoction FromString(string itemName)
        {
            var split = itemName.Split(' ');
            if (split.Length == 0)
                return Concoction.Nothing;
            if (split.Length == 1)
                return SingleItem(split[0], "", 1);
            else if (split.Length == 2)
                return SingleItem(split[0], split[1], 1);
            else// if (split.Length == 3)
                return SingleItem(split[0], split[1], split[2], 1);
        
        }
    }
}