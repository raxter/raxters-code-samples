using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Concoctions
{
    [System.Serializable]
    public class TokenizedString
    {
        [DeclareHorizontalGroup("tokens")]
        [System.Serializable]
        public class Token
        {
            [Group("tokens")]
            public string token;
            [HideLabel]
            [Group("tokens")]
            public string value;

            [HideLabel]
            [ShowInInspector]
            [ReadOnly]
            [Group("tokens")]
            public string RealisedToken
            {
                get
                {
                    if (token.StartsWith("[") && token.EndsWith("]"))
                        return value;
                        
                    return !string.IsNullOrEmpty(value) ? value : token;
                }
            }
            
            public Token (Token other)
            {
                this.token = other.token;
                this.value = other.value;
            }
                
            public Token(string token, string value)
            {
                this.token = token;
                this.value = value;
            }
            public Token(string token)
            {
                this.token = token;
                this.value = null;
            }

            public static string ExtractTag(string token, out string subTokenOption)
            {
                int startIndex = token.IndexOf("[");
                int midIndex = token.IndexOf("|");
                int endIndex = token.IndexOf("]");
                if (startIndex != -1 && endIndex != -1)
                {
                    if (midIndex == -1)
                    {
                        subTokenOption = "";
                        return token.Substring(startIndex + 1, endIndex - startIndex - 1);
                    }
                    else
                    {
                        subTokenOption = token.Substring(midIndex + 1, endIndex - midIndex - 1);
                        return token.Substring(startIndex + 1, midIndex - startIndex - 1);
                    }
                }
                    
                subTokenOption = null;
                return "";
            }
            
            public static string ExtractTag(string token)
            {
                int startIndex = token.IndexOf("[");
                int midIndex = token.IndexOf("|");
                int endIndex = midIndex != -1 ? midIndex : token.IndexOf("]");
                if (startIndex != -1 && endIndex != -1)
                    return token.Substring(startIndex + 1, endIndex - startIndex - 1);
                    
                return "";
            }

            public string TokenStripped => ExtractTag(token);
            public string RealisedStripped => ExtractTag(RealisedToken);
                
                
        }
        
        public TokenizedString()
        {
            tokens = new List<Token>();
        }
        
        public TokenizedString(Token token)
        {
            tokens = new List<Token>();
            tokens.Add(new Token(token));
        }
        
        public TokenizedString(IEnumerable<string> tokens)
        {
            this.tokens = new List<string>(tokens).ConvertAll(x => new Token(x));
        }
            
        [SerializeField] List<Token> tokens;

        public IEnumerable<string> TokensStripped
        {
            get
            {
                foreach (var token in tokens)
                {
                    yield return token.TokenStripped;
                }
            }
        }
        public IEnumerable<string> Tokens
        {
            get
            {
                foreach (var token in tokens)
                {
                    yield return token.token;
                }
            }
        }

        

        private void AddToken(Token token)
        {
            tokens.Add(new Token(token));
        }

        public string Realise()
        {
            List<string> realisedTokens = new List<string>();
            foreach (var t in tokens)
            {
                var rt = t.RealisedToken;
                if (string.IsNullOrEmpty(rt))
                    continue;
                realisedTokens.Add(t.RealisedToken);
            }
                
            return string.Join(" ", realisedTokens).Replace(" |", "").Replace("| ", "");
        }

        public void ReplaceRange(int index, int tokensInputCount, TokenizedString resultTokens)
        {
            tokens.RemoveRange(index, tokensInputCount);
            tokens.InsertRange(index, resultTokens.tokens);
        }
        
        private void ReplaceAny(string toReplace, TokenizedString values)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].TokenStripped == toReplace)
                {
                    tokens.RemoveAt(i);
                    tokens.InsertRange(i, values.tokens);
                    i--;
                    i += values.tokens.Count - 1;
                }
            }
        }
        

        public void GiveValue(string token, string newValue)
        {
            foreach (var t in tokens)
            {
                if (token == t.TokenStripped)
                    t.value = newValue;
            }
        }
        public void GiveValueOfLast(string oldTag, string newValue)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                if (tokens[i].TokenStripped == oldTag)
                {
                    tokens[i].value = newValue;
                    break;
                }
            }
        }

        public bool ProcessPositivesAndNegatives()
        {
            bool changeMade = false;
            while (ProcessPositives() || ProcessNegatives())
                changeMade = true;
            return changeMade;
        }
        bool ProcessPositives()
        {
            bool changeMade = false;
            for (int i = 0; i < tokens.Count - 1; i++)
            {
                if (tokens[i].RealisedToken == tokens[i + 1].token.Trim('+') + "+")
                {
                    // remove the copy
                    tokens.RemoveAt(i);
                    i--;
                    changeMade = true;
                }
            }

            return changeMade;
        }
        bool ProcessNegatives()
        {
            bool changeMade = false;
            for (int i = 0; i < tokens.Count - 1; i++)
            {
                if (tokens[i].RealisedToken == tokens[i + 1].token + "-")
                {
                    // remove
                    tokens.RemoveAt(i);
                    tokens.RemoveAt(i);
                    i--;
                    changeMade = true;
                }
            }
            return changeMade;
        }

        public void Transform(TokenTransform transform)
        {
            int index = FindSubTokenList(this, transform.Input, out int count, out var lookup);

            if (index == -1)
                return;

            ReplaceRange(index, count, new TokenizedString(transform.Result));
        }

        public void Transform(List<string> input, TokenizedString result)
        {
            int index = FindSubTokenList(this, input, out int count, out var lookup);

            if (index == -1)
                return;

            // replace with loopup values
            foreach (var l in lookup)
                result.ReplaceAny(l.Key, l.Value);
            
            ReplaceRange(index, count, result);
        }


        public void ProcessWildcards(Dictionary<string,string> wildcardLookup)
        {
            foreach (var t in tokens)
            {
                if (t.token.StartsWith("*"))
                {
                    if (wildcardLookup.TryGetValue(t.token, out var value))
                    {
                        t.value = value;
                    }
                }
            }
        }
        

        static int FindSubTokenList(TokenizedString tokens, IEnumerable<string> subTokens, 
            out int count, out Dictionary<string, TokenizedString> wildcardLookup)
        {
            for (int i = 0 ; i < tokens.tokens.Count; i++)
            {
                wildcardLookup = new Dictionary<string, TokenizedString>();
                int index = i;
                IEnumerator<string> subTokensEnum = subTokens.GetEnumerator();
                string currentWildcard = null;
                int j = -1;
                count = 0;
                while (subTokensEnum.MoveNext())
                {
                    TokensStart:
                    var subTokenStripped = Token.ExtractTag(subTokensEnum.Current, out var subTokenOption);
                    j++;
                    count++;
                    if (i + j >= tokens.tokens.Count)
                        goto TokensContinue;

                    var tokenIJ = tokens.tokens[i + j];
                    
                    if (subTokenStripped == "*")
                    {
                        currentWildcard = subTokenOption;
                        wildcardLookup[currentWildcard] = new TokenizedString(tokenIJ);
                        continue;
                    }
                    
                    if (tokenIJ.TokenStripped != subTokenStripped)
                    {
                        if (currentWildcard != null)
                        {
                            // we are in a wildcard, so we can skip this token
                            wildcardLookup[currentWildcard].AddToken(tokenIJ);
                            goto TokensStart;
                        }
                        goto TokensContinue;
                    }
                    // since we found a match, we are no longer in the wildcard
                    currentWildcard = null;
                }
                // we got to the end of the sublist without a failure
                return index;

                TokensContinue: ; 
            }

            wildcardLookup = null;
            count = 0;
            return -1;
        }
    }
}