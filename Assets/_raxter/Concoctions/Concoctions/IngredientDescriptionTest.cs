using Concoctions;
using TriInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientDescriptionTest", menuName = "Black Anna/Ingredient Naming Test")]
public class IngredientDescriptionTest : ScriptableObject
{
    public IngredientWildcardLibrary wildcardLibrary;
    public ConcoctionNamingRules namingRules;
    
    public Concoction concoction;
    
    [ShowInInspector, ReadOnly]
    public Concoction ConcoctionWithRules => 
        new Concoction(concoction) {_wildcards = wildcardLibrary, _namingRules = namingRules};
}
