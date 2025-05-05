using System.Collections.Generic;
using Concoctions;
using TriInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientProcessTest", menuName = "Black Anna/Ingredient Process Test", order = 1)]
public class IngredientProcessTest : ScriptableObject
{
    [Required]
    public IngredientProcessLibrary processLibrary;
    
    [Required]
    public IngredientWildcardLibrary wildcardLibrary;
    
    [Required]
    public ConcoctionNamingRules namingRules;

    void AssignDatabases()
    {
        baseConcoction._wildcards = wildcardLibrary;
        baseConcoction._namingRules = namingRules;
    }
    [OnValueChanged(nameof(AssignDatabases))]
    [InlineProperty]
    [HideLabel]
    public Concoction baseConcoction;
    public Processes tool;
    
    [PropertyOrder(50)]
    [Button]
    void PerformTest()
    {
        usedProcesses.Clear();
        result = new Concoction(baseConcoction) {_wildcards = wildcardLibrary, _namingRules = namingRules};
        processLibrary.Process(ref result, tool, wildcardLibrary, ref usedProcesses);
    }
    [PropertyOrder(65)]
    [Button]
    void ClearResults()
    {
        result = new Concoction();
        usedProcesses.Clear();
    }

    [InlineProperty]
    [HideLabel]
    [PropertyOrder(55)]
    public Concoction result;
    
    [PropertySpace(16)]
    [ShowInInspector, ReadOnly]
    [PropertyOrder(100)]
    List<ProcessResult> usedProcesses = new List<ProcessResult>();
}