using System.Collections.Generic;
using TriInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace RaxterBaxter.WorldStreamer
{
    /**
     * This class defines an area and its associated scenes.
     * It is used to define the scenes that will be loaded when the player enters this area.
     *
     * Contains an AlwaysLoaded scene that will always be loaded.
     * This is useful for area bounds and lightweight persistant data like checkpoints
     *
     * Areas bounds are defined by colliders. Please change the layer to the same as the
     * target layer in the World Streamer and add an AreaTag to the collider to define the area.
     */
    [CreateAssetMenu(fileName = "NewAreaDefinition", menuName = "World Streamer/Area Definition")]
    public class AreaDefinition : ScriptableObject
    {
        [ValidateInput(nameof(CheckScenes))]
        [Tooltip("List of addressable scenes for this area.")]
        [SerializeField]
        private List<AssetReference> scenes = new List<AssetReference>();

        [FormerlySerializedAs("_alwaysLoadedScene")]
        [ValidateInput(nameof(CheckAlwaysLoaded))] 
        [Tooltip("The scene for this area that is always loaded (can be the same as other areas'")] 
        [SerializeField]
        private AssetReference alwaysLoadedScene;

        public List<AssetReference> Scenes => scenes;

        public AssetReference AlwaysLoadedScene => alwaysLoadedScene;

        
        #region Validation

        public TriValidationResult CheckIfScene(AssetReference asset)
        {
#if UNITY_EDITOR
            if (asset == null)
                return TriValidationResult.Error("Scene is null.");
            if (asset.editorAsset is not SceneAsset)
                return TriValidationResult.Error(asset.editorAsset.name + " is not a valid scene asset.");
#endif
            return TriValidationResult.Valid;
        }

        public TriValidationResult CheckAlwaysLoaded() => CheckIfScene(alwaysLoadedScene);

        public TriValidationResult CheckScenes()
        {
#if UNITY_EDITOR
            if (scenes == null || scenes.Count == 0)
                return TriValidationResult.Error("No scenes assigned to this area.");

            foreach (var scene in scenes)
            {
                var result = CheckIfScene(scene);
                if (!result.IsValid)
                    return result;
            }
#endif
            return TriValidationResult.Valid;
        }

        #endregion
        

        public bool Contains(AssetReference scene) => 
            scenes.ContainsAssetReference(scene) || AssetReferenceEqualityComparer.EqualsStatic(scene, AlwaysLoadedScene);
    }
}