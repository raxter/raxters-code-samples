using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Concoctions
{
    [System.Serializable]
    public class TasteRule
    {
        [FormerlySerializedAs("baseLiquidName")] public string baseLiquidID;
        
        public Ingredient baseLiquid;
        
        [SerializeField] 
        List<string> baseTokens = new() {"[TASTE_LIST]", "[BASE]"};
        
        
        
        [System.Serializable]
        public class BaseAffector
        {
            [SerializeField] 
            Ingredient affector;
            public Ingredient Affector => affector;
            
            [SerializeField] 
            string taste = "Tasty";
            public string Taste => taste;
            
            [SerializeField] TokenTransform tokens;
            public TokenTransform Tokens => tokens;
        }

        public IEnumerable<Ingredient> AllAffectors
        {
            get
            {
                foreach (var ba in affectors)
                    yield return ba.Affector;
            }
        }

        public IEnumerable<string> BaseTokens => baseTokens;

        public List<BaseAffector> affectors;
        
        [SerializeField]
        List<TokenTransform> postAffectTransform = new();
        public IEnumerable<TokenTransform> PostAffectTransform => postAffectTransform;
    }
}