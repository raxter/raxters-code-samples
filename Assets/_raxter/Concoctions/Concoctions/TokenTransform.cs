using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Concoctions
{
    [DeclareHorizontalGroup("lists")]
    [System.Serializable]
    public class TokenTransform
    {
        [Group("lists")]
        [SerializeField] 
        List<string> input = new List<string>();
        
        [Group("lists")]
        [SerializeField] 
        List<string> result = new List<string>();
        
        public int InputCount => input.Count;
        public int ResultCount => result.Count;
        public IEnumerable<string> InputStripped
        {
            get
            {
                foreach (var i in input)
                    yield return TokenizedString.Token.ExtractTag(i);
            }
        }

        public IEnumerable<string> Input => input;
        public IEnumerable<string> Result => result;

        public List<string> GetResultTokens()
        {
            return new List<string>(result);
        }

        public List<string> GetInputTokens()
        {
            return new List<string>(input);
        }
    }
}