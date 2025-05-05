using System.Collections.Generic;

namespace Concoctions
{
    [System.Serializable]
    public class TasteDescription
    {
        [System.Serializable]
        public class TasteStrength
        {
            public string name;
            public float minValue;
        }

        public string name;
        public List<TasteStrength> strengths;
        
        
        public string GetTaste(float strength)
        {
            string descString = "";
            float strongestStrength = float.NegativeInfinity;
            foreach (var strengthDesc in strengths)
            {
                if (strengthDesc.minValue <= strength && strengthDesc.minValue > strongestStrength)
                {
                    strongestStrength = strengthDesc.minValue;
                    descString = strengthDesc.name;
                }
            }
        
            return descString;
        }
        
    }
}