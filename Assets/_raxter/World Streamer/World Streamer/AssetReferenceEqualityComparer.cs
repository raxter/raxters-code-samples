using System.Collections.Generic;
using RaxterBaxter.WorldStreamer;
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

public class AreaDefinitionEqualityComparer : IEqualityComparer<AreaDefinition>
{
    public static bool EqualsStatic(AreaDefinition x, AreaDefinition y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        return x.AlwaysLoadedScene.RuntimeKey.Equals(y.AlwaysLoadedScene.RuntimeKey);
    }
        
    public bool Equals(AreaDefinition x, AreaDefinition y) => EqualsStatic(x, y);

    public int GetHashCode(AreaDefinition obj)
    {
        return obj.GetHashCode();
    }
}