using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class AssetReferenceEqualityComparer : IEqualityComparer<AssetReference>
{
    public static bool EqualsStatic(AssetReference x, AssetReference y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        return x.RuntimeKey.Equals(y.RuntimeKey);
    }
        
    public bool Equals(AssetReference x, AssetReference y) => EqualsStatic(x, y);

    public int GetHashCode(AssetReference obj)
    {
        return obj.RuntimeKey.GetHashCode();
    }
}