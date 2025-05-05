using System;
using System.Collections.Generic;
using TriInspector;

namespace Concoctions
{
    public partial class Concoction
    {
        
        [NonSerialized]
        public ConcoctionNamingRules _namingRules;
        [NonSerialized]
        public IngredientWildcardLibrary _wildcards;
        
        
        private ConcoctionNamingRules NamingRules => _namingRules ?? IConcoctionDataManager.NamingRules;
        private IngredientWildcardLibrary Wildcards => _wildcards ?? IConcoctionDataManager.Wildcards;
        
        
        public string GetFullDescription() => 
            GetDescription(NamingRules, Wildcards).fullDesc;

        [InlineEditor]
        public class Description
        {
            public static Description Water => new Description("Water", "liquid");
            public static Description Nothing => new Description("Nothing", "nothing");

            public int StrengthValue() => StrengthValue(specifier);
            static int StrengthValue(string specifier)
            {
                if (specifier == "Concentrated")
                    return 5;
                if (specifier == "Strong")
                    return 4;
                if (specifier == "")
                    return 3;
                if (specifier == "Weak")
                    return 2;
                if (specifier == "Impotent")
                    return 1;
                return -1;
            }
        
            static string StrengthSpecifier(int strength)
            {
                return strength switch
                {
                    4 => "Concentrated",
                    3 => "Strong",
                    2 => "",
                    1 => "Weak",
                    0 => "Impotent",
                    _ => "undefined"
                };
            }

            public string fullDesc;
            public string broadDesc;
            public string specifier;
            public string type;

            public Description(string fullDesc, string broadDesc, string specifier, string type)
            {
                this.fullDesc = fullDesc;
                this.broadDesc = broadDesc;
                this.specifier = specifier;
                this.type = type;
            }

            public Description(string fullDesc, string broadDesc, string specifier)
            {
                this.fullDesc = fullDesc;
                this.broadDesc = broadDesc;
                this.specifier = specifier;
                this.type = "unspecified";
            }

            public Description(string broadDesc, string type)
            {
                this.fullDesc = broadDesc;
                this.broadDesc = broadDesc;
                this.specifier = "";
                this.type = type;
            }

            public Description(string broadDesc)
            {
                this.fullDesc = broadDesc;
                this.broadDesc = broadDesc;
                this.specifier = "";
                this.type = "unspecified";
            }

            public Description(AmountOfIngredient ingredient)
            {
                fullDesc = ingredient.Name;
                broadDesc = ingredient.ingredient;
                specifier = ingredient.status;
                type = TypeOf(ingredient.ingredient);
            }

            public static Description LiquidDesc(AmountOfIngredient singleLiquid)
            {
                return new Description(
                    ("Liquid" + " " + singleLiquid.Name).Trim(),
                    singleLiquid.ingredient,
                    "",
                    "liquid");
            }
        }

        public string OverallDescriptionFullWithAmount
        {
            get
            {
                var singleItem = AsSingleItem();
                if (singleItem != null)
                    return singleItem.amount + " " + OverallDescriptionFull;
                return OverallDescriptionFull;
            }
        }
    
        public string OverallDescriptionFull => OverallDescription.fullDesc;
        public string SolidsDescriptionFull => SolidsDescription.fullDesc;
        public string LiquidDescriptionFull => LiquidDescription.fullDesc;

        [Group("All/Debug")]
        [ShowInInspector, ReadOnly]
        [HideReferencePicker]
        public Description OverallDescription => GetDescription(NamingRules, Wildcards);

        public Description GetDescription() => GetDescription(NamingRules, Wildcards);
        public Description GetDescription(ConcoctionNamingRules namingRules, IngredientWildcardLibrary wildcard)
        {
            if (liquids.IsNothing)
            {
                if (solids.IsNothing)
                    return Description.Nothing;
                if (solids.IsSinglePure)
                {
                    return new Description(solids.First.Name);
                }

                return new Description("A number of ingredients", "ingredients", "");
            }

            if (liquids.IsSingle())
            {
                if (Solids.IsNothing)
                {
                    return GetLiquidDescription(namingRules, wildcard);
                }

                string ingredientDesc = solids.Count == 1 ? solids.First.Name : "ingredients";
            
            
                return new Description(
                    $"{ingredientDesc} in "+GetLiquidDescription(namingRules, wildcard).fullDesc,
                    $"{ingredientDesc} in "+GetLiquidDescription(namingRules, wildcard).broadDesc,
                    "",
                    "liquid");
            }
            // else - we have concentrations

            if (Solids.IsNothing)
            {
                return GetLiquidDescription(namingRules, wildcard);
            }
            else
            {
                var solidDesc = GetSolidsDescription(wildcard);
                var liquidDesc = GetLiquidDescription(namingRules, wildcard);

                var combinedDesc = new Description(
                    liquidDesc.fullDesc +
                    (solidDesc != null ? " with bits of " + solidDesc.fullDesc : ""),
                    "combined");

                return combinedDesc;
            }
        }

        [Group("All/Debug")]
        [ShowInInspector, ReadOnly]
        [HideReferencePicker]
        public Description LiquidDescription => GetLiquidDescription(NamingRules, Wildcards);

        [Group("All/Debug")]
        [ShowInInspector]
        public AmountList LiquidConcentrations =>
            liquids.Concentrations;

        [Group("All/Debug")]
        [ShowInInspector]
        public AmountList NonWaterLiquidConcentrations =>
            LiquidConcentrations.Residue("*", "Water", "");

        [Group("All/Debug")]
        [ShowInInspector]
        public AmountList WatersConcentrations =>
            LiquidConcentrations.Filter("*", "Water", "");

        public IEnumerable<Ingredient> TastelessWaterIngredients
        {
            get
            {
                yield return new Ingredient("", "Water", "");
            }
        }
        public IEnumerable<Ingredient> BitterWaterIngredients
        {
            get
            {
                yield return new Ingredient("Boiled", "Plant Matter", "");
            }
        }
        public IEnumerable<Ingredient> HerbaceousWaterIngredients
        {
            get
            {
                yield return new Ingredient("Herbaceous", "Water", "");
            }
        }
    
        public Description GetLiquidDescription(ConcoctionNamingRules namingRules, IngredientWildcardLibrary wildcard)
        {
            return new Description(namingRules?.GetBaseLiquidName(this, wildcard) ?? "<Unnamable Liquid>");
        }
        
        [Group("All/Debug")]
        [ShowInInspector, ReadOnly]
        [HideReferencePicker]
        public Concoction.Description SolidsDescription => GetSolidsDescription(Wildcards);

        [Group("All/Debug")]
        [ShowInInspector, ReadOnly]
        [HideReferencePicker]
        public ConcoctionNamingRules ConcoctionNamingRules => NamingRules;
        [Group("All/Debug")]
        [ShowInInspector, ReadOnly]
        [HideReferencePicker]
        public IngredientWildcardLibrary WildcardLibrary => Wildcards;

        public Description GetSolidsDescription(IngredientWildcardLibrary wildcard)
        {
            if (solids.IsNothing)
                return Description.Nothing;
            if (solids.Count == 1)
            {
                return new Description(solids.First);
            }

            return new Description("A number of ingredients", "ingredients", "");
        }

        public static string TypeOf(string ingredient)
        {
            string[] split = ingredient.Split(' ');

            if (split.Length == 1)
                return BaseTypeOf(ingredient);
            if (split.Length == 2)
                return split[0].ToLower() + " " + BaseTypeOf(split[1]);

            return UndefinedType;
        }

        public static string BaseTypeOf(string baseIngredient)
        {
            return baseIngredient switch
            {
                "Water" => "water",
                "Allheal" => PlantMatterType,
                _ => UndefinedType
            };
        }

        private const string UndefinedType = "undefined type";
        private const string PlantMatterType = "plant matter";
    }
}