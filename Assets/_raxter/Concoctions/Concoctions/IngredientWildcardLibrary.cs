using System.Collections.Generic;
using TriInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientWildcardLibrary", menuName = "Black Anna/Ingredient Wildcard Library", order = 1)]
public class IngredientWildcardLibrary : ScriptableObject
{
    [System.Serializable]
    public class Wildcard
    {
        private string Info => $"Use `*{name}` when used in ingredients";
        [InfoBox("$"+nameof(Info))]
        public string name;
        public Ingredient.Type type;
        public List<string> possibilities = new List<string>();
    }
    
    
    public List<Wildcard> wildcards = new List<Wildcard>();

    public Wildcard FindWildcardFor(string wildcardName, Ingredient.Type type)
    {
        return wildcards.Find(w => w.name == wildcardName && w.type == type);

    }
}