using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RaxterBaxter.WorldStreamer
{
    /*
    * This class is responsible for loading and unloading scenes based on the areas that are currently loaded.
    * It uses the AreaGroupDefinition to determine which scenes are in the 'world'.
    * And then polls a loadingTrackerObject to determine which areas should currently be loaded (via the area's area bounds)
    * The class is designed to work in both play mode and edit mode.
    */
    
    [DeclareFoldoutGroup("Hooks")]
    [DeclareFoldoutGroup("Debug")]
    [ExecuteInEditMode]
    public class WorldStreamer : MonoBehaviour
    {
        [SerializeField] private bool performInEditMode = false;

        public enum EditorModeOption
        {
            SimulatePlayMode,
            LoadAllScenes,
            UnloadAllScenes
        }
        
        [Tooltip("To allow for loading all at once or to simulate a build like scenario starting from no loaded scenes.")]
        [SerializeField] private EditorModeOption editorModeOption = EditorModeOption.SimulatePlayMode;
        
        [Tooltip("Set the scene load thread priority to low for smooth loading.")]
        [SerializeField] private bool setLowLoadThreadPriorityOnAwake = true;
        
        [Tooltip("The area group definition to use for this world streamer.")]
        [Required]
        [SerializeField] private AreaGroupDefinition areaGroup;

        [Tooltip("This object will determine what point to test the area bounds")] 
        [SerializeField]
        [Required]
        private GameObject loadingTrackerObject;

        [SerializeField] private LayerMask areaLayerMask;

        public enum LoadingMode
        {
            Simultaneous,
            OneAtATime
        }

        [SerializeField] private LoadingMode additiveLoadingMode = LoadingMode.Simultaneous;

        [Group("Hooks")] [SerializeField] private UnityEvent onGlobalScenesLoaded;

        [Group("Debug")] [ShowInInspector, ReadOnly]
        private List<AreaDefinition> AreasThatShouldBeLoaded = new List<AreaDefinition>();

        [Group("Debug")] [ShowInInspector, ReadOnly]
        private List<List<AssetReference>> ScenesThatShouldBeLoadedPrioritized = new List<List<AssetReference>>();

        [Group("Debug")] [ShowInInspector, ReadOnly]
        private List<List<string>> ScenesThatShouldBeLoadedPrioritized_Readable = new List<List<string>>();
        
        [Group("Debug")] [ShowInInspector, ReadOnly]
        private List<AssetReference> ScenesThatShouldBeLoaded = new List<AssetReference>();
        
        [Group("Debug")] [ShowInInspector, ReadOnly]
        private List<string> ScenesThatShouldBeLoaded_Readable = new List<string>();
        
        private bool notifiedOfGlobalLoaded = false;

        private bool IsEditorMode => Application.isEditor && !Application.isPlaying;
        void Awake()
        {
            // Set the thread priority to low if set in the inspector
            if (setLowLoadThreadPriorityOnAwake)
            {
                Application.backgroundLoadingPriority = ThreadPriority.Low;
            }
        }

        #region Editor mode callbacks to refresh state as scenes load or unload
        
#if UNITY_EDITOR
        void OnEnable()
        {
            if (IsEditorMode)
            {
                EditorSceneManager.sceneOpened += OnSceneOpenedOrClosed;
                EditorSceneManager.sceneClosed += OnSceneOpenedOrClosed;
            }
        }

        private void OnDisable()
        {
            if (IsEditorMode)
            {
                EditorSceneManager.sceneOpened -= OnSceneOpenedOrClosed;
                EditorSceneManager.sceneClosed -= OnSceneOpenedOrClosed;
            }
        }

        private void OnSceneOpenedOrClosed(Scene scene, OpenSceneMode mode) => RepaintAllViews();
        private void OnSceneOpenedOrClosed(Scene scene) => RepaintAllViews();
        
        private void RepaintAllViews() => 
            EditorApplication.delayCall += () => 
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
#endif
        #endregion

        void LateUpdate()
        {
            // Disable execution in prefab mode as scene loading there will cause issues 
#if UNITY_EDITOR
            bool inPrefabMode = false;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            inPrefabMode = prefabStage != null;
            if (inPrefabMode)
                return;
#endif

            if (Application.isPlaying && performInEditMode == false)
                return;

            ReconcileAreas();
            ReconcileScenes();
            ReconcileScenePriority();
            LoadAndUnloadScenes();
        }

        private void ReconcileAreas()
        {
            if (IsEditorMode)
            {
                if (editorModeOption == EditorModeOption.UnloadAllScenes)
                {
                    AreasThatShouldBeLoaded.Clear();
                    return;
                }

                if (editorModeOption == EditorModeOption.LoadAllScenes)
                {
                    AreasThatShouldBeLoaded.Clear();
                    foreach (var area in areaGroup.AreaDefinitions)
                        AreasThatShouldBeLoaded.Add(area);
                    return;
                }
            }

            // Get the position of the loading tracker object
            Vector3 position = loadingTrackerObject?.transform.position ?? transform.position;

            // Get all colliders in the area layer mask
            Collider[] colliders = Physics.OverlapSphere(position, 0.1f, areaLayerMask);

            // Clear the list of areas that should be loaded
            AreasThatShouldBeLoaded.Clear();

            foreach (Collider collider in colliders)
            {
                AreaTag areaTag = collider.GetComponentInParent<AreaTag>();
                if (areaTag != null)
                    AreasThatShouldBeLoaded.Add(areaTag.Area);
            }

        }

        private void ReconcileScenes()
        {
            if (IsEditorMode)
            {
                // Clear the list of scenes that should be loaded
                if (editorModeOption == EditorModeOption.UnloadAllScenes)
                {
                    ScenesThatShouldBeLoadedPrioritized.Clear();
                    ScenesThatShouldBeLoadedPrioritized_Readable.Clear();
                    return;
                }
            }

            ScenesThatShouldBeLoadedPrioritized.ForEach(l => l.Clear());

            if (ScenesThatShouldBeLoadedPrioritized.Count < 1)
                ScenesThatShouldBeLoadedPrioritized.Add(new List<AssetReference>());

            ScenesThatShouldBeLoadedPrioritized[0].AddRange(areaGroup.GlobalScenes);
            ScenesThatShouldBeLoadedPrioritized[0].AddRange(areaGroup.AlwaysLoadedScenes);

            if (ScenesThatShouldBeLoadedPrioritized.Count < 2)
                ScenesThatShouldBeLoadedPrioritized.Add(new List<AssetReference>());

            // Get all the areas that should be loaded
            foreach (var area in AreasThatShouldBeLoaded)
            {
                if (areaGroup.Contains(area))
                {
                    ScenesThatShouldBeLoadedPrioritized[1].AddRange(area.Scenes);
                }
            }

            // Convert the AssetReference to a string for display
            ScenesThatShouldBeLoadedPrioritized_Readable.Clear();
            
#if UNITY_EDITOR
            foreach (var list in ScenesThatShouldBeLoadedPrioritized)
            {
                var readableList = new List<string>();
                foreach (var scene in list)
                    readableList.Add((scene?.editorAsset as SceneAsset)?.ToString() ?? "null");
                ScenesThatShouldBeLoadedPrioritized_Readable.Add(readableList);
            }
#endif
        }


        void ReconcileScenePriority()
        {
            ScenesThatShouldBeLoaded.Clear();
            
            for (int i = 0 ; i < ScenesThatShouldBeLoadedPrioritized.Count; i++)
            {
                ScenesThatShouldBeLoaded.AddRange(ScenesThatShouldBeLoadedPrioritized[i]);
                if (!ScenesThatShouldBeLoadedPrioritized[i].TrueForAll(SceneLoadUtility.IsSceneLoaded))
                {
                    // not all scenes in a priority are loaded, we don't load the rest
                    break;
                }
                
                // if we reach here it means at least hte 0th group loaded, which is always the global group
                if (!notifiedOfGlobalLoaded)
                {
                    notifiedOfGlobalLoaded = true;
                    onGlobalScenesLoaded?.Invoke();
                }
            }
            
#if UNITY_EDITOR
            // Convert the AssetReference to a string for display
            ScenesThatShouldBeLoaded_Readable.Clear();
            foreach (var scene in ScenesThatShouldBeLoaded)
            {
                if (scene?.editorAsset is SceneAsset)
                    ScenesThatShouldBeLoaded_Readable.Add((scene.editorAsset as SceneAsset).ToString());
            }
#endif
        }

        private void LoadAndUnloadScenes()
        {
            bool sceneLoadedOrUnloaded = false;
            
            // Load the scenes that should be loaded
            foreach (var scene in ScenesThatShouldBeLoaded)
            {
                if (SceneLoadUtility.IsSceneLoaded(scene))
                    continue;
                
                if (SceneLoadUtility.IsSceneLoading(scene))
                    continue;
                    
                LoadScene(scene);
                sceneLoadedOrUnloaded = true;
            }

            // Unload the scenes that should be unloaded
            foreach (var scene in SceneLoadUtility.AllLoadedSceneAsAssetReferences())
            {
                // if the scene is null it likely because it is still loading and thus cannot be converted from a scene to an asset reference
                if (scene == null)
                    continue;
                
                // ignore scenes that are not in the area group definition or global, these are probably helper scenes
                if (!areaGroup.ContainsScene(scene))
                    continue;
                
                // ignore scenes that should be loaded
                if (ScenesThatShouldBeLoaded.ContainsAssetReference(scene))
                    continue;
                
                UnloadScene(scene);
                sceneLoadedOrUnloaded = false;
            }

#if UNITY_EDITOR
            // we force a repaint in editor to make sure this class refreshes so that loading or unloading can continue
            // otherwise it can get stuck mid-process until you manually refresh (i.e. move your mouse over the editor)
            if (sceneLoadedOrUnloaded)
            {
                RepaintAllViews();
            }
#endif
        }


        void LoadScene(AssetReference scene)
        {
            if (additiveLoadingMode == LoadingMode.OneAtATime && SceneLoadUtility.AnySceneLoading)
                return;
            
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                // Play mode (while in Editor)
                SceneLoadUtility.LoadScenePlayMode(scene);
            }
            else
            {
                // Editor mode
                SceneLoadUtility.LoadSceneEditorMode(scene);
            }
#else
            // Build
            SceneLoadUtility.LoadSceneBuildMode(scene);
#endif
        }
        
        
        void UnloadScene(AssetReference scene)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                SceneLoadUtility.UnloadScenePlayModeViaAddressables(scene);
            }
            else
            {
                SceneLoadUtility.UnloadSceneEditorModeViaEditorSceneManager(scene);
            }
#else
                SceneLoadUtility.UnloadSceneBuildModeViaAddressables(scene);
#endif
        }
        

    }
}
