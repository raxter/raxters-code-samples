using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace RaxterBaxter.WorldStreamer
{
    [System.Serializable]
    public static class AssetReferenceList
    {
        
        // make a helper function for List<AssetRefernece> to do a custom contains
        public static bool ContainsAssetReference(this List<AssetReference> list, AssetReference item)
        {
            if (list == null || list.Count == 0)
                return false;

            return list.FindIndex(s => AssetReferenceEqualityComparer.EqualsStatic(s, item)) != -1;

        }
        
    }
}