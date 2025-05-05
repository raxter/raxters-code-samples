using UnityEngine.Serialization;

namespace Concoctions
{
    [System.Serializable]
    public class LiquidBaseReplaceRule
    {
        public Ingredient mainIngredient;

        [FormerlySerializedAs("baseLiquid")] public string baseLiquidID;

        public TokenTransform tokens;
    }
}