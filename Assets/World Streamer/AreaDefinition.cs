using System.Collections.Generic;
using TriInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RaxterBaxter.WorldStreamer
{

    [CreateAssetMenu(fileName = "NewAreaDefinition", menuName = "World Streamer/Area Definition")]
    public class AreaDefinition : ScriptableObject
    {
        [ValidateInput(nameof(CheckScenes))] [Tooltip("List of addressable scenes for this area.")] [SerializeField]
        private List<AssetReference> scenes = new List<AssetReference>();

        [ValidateInput(nameof(CheckAlwaysLoaded))] [Tooltip("The indicator scene for this area.")] [SerializeField]
        private AssetReference _alwaysLoadedScene;

        public List<AssetReference> Scenes => scenes;

        public AssetReference AlwaysLoadedScene => _alwaysLoadedScene;

        #region Validation

        public TriValidationResult CheckIfScene(AssetReference asset)
        {
            if (asset == null)
                return TriValidationResult.Error("Scene is null.");
            if (asset.editorAsset is not SceneAsset)
                return TriValidationResult.Error(asset.editorAsset.name + " is not a valid scene asset.");

            return TriValidationResult.Valid;
        }

        public TriValidationResult CheckAlwaysLoaded() => CheckIfScene(_alwaysLoadedScene);

        public TriValidationResult CheckScenes()
        {
            if (scenes == null || scenes.Count == 0)
                return TriValidationResult.Error("No scenes assigned to this area.");

            foreach (var scene in scenes)
            {
                var result = CheckIfScene(scene);
                if (!result.IsValid)
                    return result;
            }

            return TriValidationResult.Valid;
        }

        #endregion

        public bool Contains(AssetReference scene) => 
            scenes.ContainsAssetReference(scene) || AssetReferenceEqualityComparer.EqualsStatic(scene, AlwaysLoadedScene);
    }
}