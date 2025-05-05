
using TriInspector;

[DeclareHorizontalGroup("group", Sizes = new float[] { 48 })]
[DeclareHorizontalGroup("group/amt")]
[System.Serializable]
public class AmountOfIngredient : Ingredient
{
    [Group("group/amt")]
    [PropertyOrder(-1)]
    [LabelWidth(12)]
    [LabelText("#")]
    public float amount;
    
    public AmountOfIngredient () : base()
    {
        amount = 0;
    }
    public AmountOfIngredient (AmountOfIngredient copy) 
    {
        ingredient = copy.ingredient;
        status = copy.status;
        part = copy.part;
        amount = copy.amount;
    }
    
    public AmountOfIngredient(Ingredient copy, float newAmount)
    {
        ingredient = copy.ingredient;
        status = copy.status;
        part = copy.part;
        amount = newAmount;
    }
    
    public AmountOfIngredient(AmountOfIngredient copy, string newStatus)
    {
        ingredient = copy.ingredient;
        status = newStatus;
        part = copy.part;
        amount = copy.amount;
    }
    
    public AmountOfIngredient (string ingredient, float amount)
    {
        this.ingredient = ingredient;
        this.status = "";
        this.part = "";
        this.amount = amount;
    }
    public AmountOfIngredient (string ingredient, string part, float amount, string status = "")
    {
        this.ingredient = ingredient;
        this.status = status;
        this.part = part;
        this.amount = amount;
    }
    
    
    public AmountOfIngredient WithStatus(string withStatus) => new(this, withStatus);
}
    
[DeclareHorizontalGroup("group/base")]
[System.Serializable]
public class Ingredient
{
    
    public enum Type {Status, Ingredient, Part}
    public Ingredient() : this("", "", "")
    {
    }
    
    public Ingredient(string s, string i, string p)
    {
        this.status = s;
        this.ingredient = i;
        this.part = p;
    }
    
    [GroupNext("group/base")]
    [HideLabel]
    public string status;
    [HideLabel]
    public string ingredient; 
    [HideLabel]
    public string part;
    
    public string IngredientPart => (ingredient + " "+ part).Trim();
    public string Name => (status + " " + ingredient + " "+ part).Trim();
    
    //tostring
    public override string ToString()
    {
        return Name;
    }
    
    //== operator
    public static bool operator ==(Ingredient a, Ingredient b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.ingredient == b.ingredient && a.status == b.status && a.part == b.part;
    }
    public static bool operator !=(Ingredient a, Ingredient b)
    {
        if (a is null && b is null) return false;
        if (a is null || b is null) return true;
        return a.ingredient != b.ingredient || a.status != b.status || a.part != b.part;
    }

    // Helper function to get property value based on type
    public string GetValue(Ingredient.Type type)
    {
        switch (type)
        {
            case Ingredient.Type.Status:
                return status;
            case Ingredient.Type.Ingredient:
                return ingredient;
            case Ingredient.Type.Part:
                return part;
            default:
                return null;
        }
    }

    public static bool MatchDirectOrPattern(Ingredient c, Ingredient pattern, IngredientWildcardLibrary wildcards)
    {
        if (c == null || pattern == null)
            return c == pattern;
        
        // Check for direct match first
        if (MatchDirect(c, pattern))
            return true;
        
        // If not a direct match, check for pattern match
        return MatchPattern(c, pattern, wildcards);
    }
    
    public static bool MatchDirect(Ingredient c1, Ingredient c2)
    {
        if (c1 == null || c2 == null)
            return c1 == c2;
        
        if (c1.ingredient != c2.ingredient)
            return false;
        if (c1.status != c2.status)
            return false;
        if (c1.part != c2.part)
            return false;
        return true;
    }
    
    public static bool MatchPattern(Ingredient c, Ingredient pattern, IngredientWildcardLibrary wildcards)
    {
        if (c == null || pattern == null)
            return c == pattern;
            

        // Check all three properties
        foreach (Ingredient.Type type in System.Enum.GetValues(typeof(Ingredient.Type)))
        {
            string patternValue = pattern.GetValue(type);
            string ingredientValue = c.GetValue(type);
            
            // Skip if pattern value is null
            if (patternValue == null)
                continue;
                
            // Direct wildcard match: "*" matches anything
            if (patternValue == "*")
                continue;
                
            // Named wildcard: "*NAME" matches values in wildcard library
            if (patternValue.StartsWith("*"))
            {
                string wildcardName = patternValue.Substring(1); // Remove "*" prefix
                
                // Find matching wildcard in library
                var matchingWildcard = wildcards?.FindWildcardFor(wildcardName, type);
                
                // If matching wildcard found and ingredient value is in possibilities, continue
                if (matchingWildcard != null && matchingWildcard.possibilities.Contains(ingredientValue))
                    continue;
                else
                    return false;
            }
            
            // Direct string comparison
            if (patternValue != ingredientValue)
                return false;
        }
        
        return true;
    }
}
