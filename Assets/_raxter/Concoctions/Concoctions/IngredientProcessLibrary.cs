using System.Collections.Generic;
using Concoctions;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

public enum Processes
{
    Hand,
    Heat,
    Knife,
    Ladle,
    Strainer,
    Mortar,
    SewingNeedle,
    DryingString,
    Dry,
}

[CreateAssetMenu(fileName = "ConcoctionToolResultLibrary", menuName = "Black Anna/ConcoctionTool Result Library", order = 1)]
public class IngredientProcessLibrary : ScriptableObject
{
    public IngredientWildcardLibrary wildcards;
    public ConcoctionNamingRules namingRules;

    [Button]
    void AssignAllDatabases()
    {
        foreach (var process in processResults)
        {
            process.input._wildcards = wildcards;
            process.input._namingRules = namingRules;
            process.result._wildcards = wildcards;
            process.result._namingRules = namingRules;
        }
        foreach (var process in splitProcessResults)
        {
            process.input._wildcards = wildcards;
            process.input._namingRules = namingRules;
            foreach (var result in process.result)
            {
                result._wildcards = wildcards;
                result._namingRules = namingRules;
            }
        }
    }
    
    [OnValueChanged(nameof(AssignAllDatabases))]
    public List<ProcessResult> processResults = new List<ProcessResult>();
    [OnValueChanged(nameof(AssignAllDatabases))]
    public List<ProcessSplitResult> splitProcessResults = new List<ProcessSplitResult>();

    public bool Process(ref Concoction concoction, Processes tool, IngredientWildcardLibrary wildcards)
    {
        List<ProcessResult> usedProcesses = null;
        return Process(ref concoction, tool, wildcards, ref usedProcesses);
    }

    public bool Process(
        ref Concoction concoction, Processes tool, 
        IngredientWildcardLibrary wildcards, ref List<ProcessResult> usedProcesses)
    {
        bool usedAProcess = false;
        Concoction processed = new Concoction();
        foreach (var process in processResults)
        {
            if (process.tool != tool)
                continue;

            var extracted = 
                concoction.ExtractCopy(process.input, process.scalingType, out var ratioExtracted, 
                    wildcards, out var wildcardLookup);

            if (extracted != null)
            {
                concoction.Exclude(extracted);
                Debug.Log("Process Found: " + process.description);
                
                usedProcesses?.Add(process);
                usedAProcess = true;
                
                if (wildcardLookup == null)
                    processed.Union(process.result * ratioExtracted);
                else
                {
                    var result = process.result * ratioExtracted;
                    result.ReplaceWildcards(wildcardLookup);
                    processed.Union(result);
                }
            }
        }
        if (usedAProcess)
        {
            concoction.Union(processed);
            return true;
        }

        return false;
    }


    public List<Concoction> ProcessMultiResult(Concoction concoction, Processes tool,
        IngredientWildcardLibrary wildcards)
    {
        List<ProcessSplitResult> usedProcesses = null;
        return ProcessMultiResult(concoction, tool, wildcards, ref usedProcesses);
    }
    public List<Concoction> ProcessMultiResult(Concoction concoction, Processes tool,
        IngredientWildcardLibrary wildcards, ref List<ProcessSplitResult> usedProcesses)
    {
        // instead of processing multiple times if possible, we find the first multi result process, and *only* use that
        List<Concoction> processed = new List<Concoction>();
        foreach (var process in splitProcessResults)
        {
            if (process.tool != tool)
                continue;

            var extracted = 
                concoction.ExtractCopy(process.input, process.scalingType, out var ratioExtracted,
                    wildcards, out var wildcardLookup);

            if (extracted != null)
            {
                concoction.Exclude(extracted);
                Debug.Log("Process Found: " + process.description);
                
                usedProcesses?.Add(process);

                if (wildcardLookup == null)
                {
                    return process.result.ConvertAll(c => new Concoction(c)*ratioExtracted);
                }
                else
                {
                    return process.result.ConvertAll(c =>
                    {
                        var result = new Concoction(c)*ratioExtracted;
                        result.ReplaceWildcards(wildcardLookup);
                        return result;
                    });
                }
            }
        }

        return null;
    }
}


//[DeclareFoldoutGroup("group", Title = "$"+nameof(description))]
[System.Serializable]
public abstract class ProcessResultBase
{
    [Group("group")]
    public abstract string description { get; }
    
    public enum ScalingType
    {
        NoScaling,
        ScalesContinuously,
        ScalesDiscretely
    }
    
    [GroupNext("group")]
    public ScalingType scalingType = ScalingType.ScalesContinuously;
    [InlineProperty][HideLabel]
    public Concoction input;
    public Processes tool;
}

[DeclareFoldoutGroup("group", Title = "$"+nameof(description))]
[System.Serializable]
public class ProcessResult : ProcessResultBase
{
    [Group("group")]
    public override string description => input.OverallDescriptionFull +" + " +tool+ " -> "+ result.OverallDescriptionFull;
    [InlineProperty][HideLabel]
    [Group("group")]
    public Concoction result;
}

[DeclareFoldoutGroup("group", Title = "$"+nameof(description))]
[System.Serializable]
public class ProcessSplitResult : ProcessResultBase
{
    [Group("group")]
    public override string description => input.OverallDescriptionFull +" + " +tool+ " -> "+ ResultsDescription;
    string ResultsDescription => string.Join(" + ", result.ConvertAll(c => c.OverallDescriptionFull));
    
    [InlineProperty][HideLabel]
    [Group("group")]
    public List<Concoction> result;
}
