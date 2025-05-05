using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Concoctions
{
    [CreateAssetMenu(fileName = "ConcoctionNamingRules", menuName = "Concoctions/Concoction Naming Rules")]
    public class ConcoctionNamingRules : ScriptableObject
    {
        public List<LiquidBaseReplaceRule> liquidRules;
        public List<TasteRule> tasteRules;
        public List<TasteDescription> tasteDescriptions;
        
        public TasteDescription GetTasteDescription(string tasteDescID)
        {
            return tasteDescriptions.Find(x => x.name == tasteDescID);
        }

        public IngredientWildcardLibrary wildcards;

        void AssignDatabases()
        {
            testConcoction._namingRules = this;
            testConcoction._wildcards = wildcards;
        }
        [OnValueChanged(nameof(AssignDatabases))]
        public Concoction testConcoction;
        
        [ShowInInspector, ReadOnly] public string LiquidBaseName => 
            GetBaseLiquidName(testConcoction, wildcards);
        
        [ShowInInspector, ReadOnly] public TokenizedString LiquidBaseTokenValues => 
            GetBaseLiquidTokenizedString(testConcoction, wildcards);
        
        public AmountList residueDebug;

        Dictionary<string, TasteDescription> GetTasteDescs(IEnumerable<string> source)
        {
            Dictionary<string, TasteDescription> tasteDescriptions = new Dictionary<string, TasteDescription>();
            
            foreach (var token in source)
            {
                if (token.StartsWith("[") && token.EndsWith("]"))
                {
                    string tasteDescID = token.Trim('[', ']');
                    var desc = GetTasteDescription(tasteDescID);
                    if (desc != null)
                        tasteDescriptions[tasteDescID] = desc;
                }
            }
            return tasteDescriptions;
        }

        public string GetBaseLiquidName(Concoction concoction, IngredientWildcardLibrary wildcards)
        {
            var tokenList = GetBaseLiquidTokenizedString(concoction, wildcards);
            return tokenList?.Realise() ?? "---";
        }
        public TokenizedString GetBaseLiquidTokenizedString(Concoction concoction, IngredientWildcardLibrary wildcards)
        {

            foreach (var baseDef in tasteRules)
            {
                // filter out the base liquid
                var pureBaseLiquid = concoction.Liquids.Filter(
                    baseDef.baseLiquid, out var residue);
                var allCoBaseLiquids = concoction.Liquids.FilterMultiple(
                    out var completeResidue, baseDef.AllAffectors);

                float totalBaseLiquid = pureBaseLiquid.TotalAmount + allCoBaseLiquids.TotalAmount;

                TokenizedString tokens = new(baseDef.BaseTokens);
                
                if (totalBaseLiquid > 0)
                {
                    foreach (var affector in baseDef.affectors)
                    {
                        var affectorLiquid = residue.Filter(affector.Affector, out var affectorResidue);
                        residue = affectorResidue;
                        float totalAffectorLiquid = affectorLiquid.TotalAmount;
                        float concentration = totalAffectorLiquid / totalBaseLiquid;

                        if (concentration <= 0)
                            continue;

                        ApplyAffector(ref tokens, affector, concentration);
                    }
                }

                tokens.ProcessPositivesAndNegatives();
                // post affect processes
                foreach (var postTrans in baseDef.PostAffectTransform)
                {
                    tokens.Transform(postTrans);
                    tokens.ProcessPositivesAndNegatives();
                }
                

                tokens.GiveValue("AND", "|,");
                tokens.GiveValueOfLast("AND", "and");

                residue.Add(baseDef.baseLiquid, totalBaseLiquid);
                residueDebug = new AmountList(residue);
                foreach (var rule in liquidRules)
                {
                    if(rule.baseLiquidID == baseDef.baseLiquidID)
                    {
                        Concoction liquidResidue = Concoction.LiquidConcoction(residue);
                        Concoction liquidMainIngredient = Concoction.LiquidConcoction(rule.mainIngredient);
                        Concoction extracted = liquidResidue.ExtractCopy(liquidMainIngredient, 
                            ProcessResultBase.ScalingType.ScalesContinuously, 
                            out _, wildcards, out var lookup);
                        
                        if (extracted == null)
                            continue;
                        
                        var mainLiquid = residue.FilterMultiple(out _, extracted.Liquids);
                        float concentration = mainLiquid.TotalAmount / totalBaseLiquid;
                        if (concentration <= 0)
                            continue;
                        
                        var resultTokens = new TokenizedString(rule.tokens.GetResultTokens());
                        resultTokens.ProcessWildcards(lookup);
                        ApplyTasteDescriptions(resultTokens, concentration);
                        tokens.Transform(rule.tokens.GetInputTokens(), resultTokens);
                    }
                }
                
                tokens.GiveValue("BASE", baseDef.baseLiquid.Name);
                
                return tokens;
            }

            return null;
        }
        


        void ApplyAffector(ref TokenizedString tokenList, TasteRule.BaseAffector affector, float concentration)
        {
            
            TokenizedString resultTokens = new (affector.Tokens.GetResultTokens());
            
            ApplyTasteDescriptions(resultTokens, concentration);
            
            resultTokens.GiveValue("TASTE", affector.Taste);

            tokenList.Transform(affector.Tokens.GetInputTokens(), resultTokens);

        }
        
        void ApplyTasteDescriptions(TokenizedString tokenizedString, float concentration)
        {
            var tasteDescs = GetTasteDescs(tokenizedString.Tokens);
            foreach (var desc in tasteDescs)
            {
                // replace the token with the taste description
                string tasteDescID = desc.Key;
                var tasteDescription = desc.Value;
                tokenizedString.GiveValue(tasteDescID, tasteDescription.GetTaste(concentration));
            }
        }

    }
}
