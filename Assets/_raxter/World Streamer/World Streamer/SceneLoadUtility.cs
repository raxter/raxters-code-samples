using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace RaxterBaxter.WorldStreamer
{
    // This class is used to load and unload scenes in the editor and play mode
    // And provides unified functions that check the play mode context and runs the appropriate code
    public static class SceneLoadUtility
    {
        // This is a dictionary of all the loaded scenes, and their handles
        [PlayMode][BuildMode]
        private static Dictionary<string, AsyncOperationHandle<SceneInstance>> SceneHandles = new();

        #region Scene Loading 
#if UNITY_EDITOR
        [EditorMode]
        internal static void LoadSceneEditorMode(AssetReference scene) =>
            EditorSceneManager.OpenScene(GetScenePathEditorMode(scene), OpenSceneMode.Additive);
#endif

        [PlayMode]
        internal static void LoadScenePlayMode(AssetReference scene) => LoadSceneBuildMode(scene);

        [BuildMode]
        internal static void LoadSceneBuildMode(AssetReference scene)
        {
            var handle = Addressables.LoadSceneAsync(scene, LoadSceneMode.Additive);
            SceneHandles[scene.AssetGUID] = handle;
        }
        
        #endregion
        
        #region Play+Build Mode Unloading
        [PlayMode]
        internal static void UnloadScenePlayModeViaAddressables(AssetReference scene) => UnloadSceneBuildModeViaAddressables(scene);

        [PlayMode][BuildMode]
        internal static void UnloadSceneBuildModeViaAddressables(AssetReference scene)
        {
            if (SceneHandles.ContainsKey(scene.AssetGUID) == false)
            {
#if UNITY_EDITOR
                // if we are in editor, and we can't find the scene handle, we try unload it by path
                if (Application.isEditor)
                {
                    var loadedSceneReference = GetLoadedSceneReference(scene);
                    UnloadScenePlayModeViaSceneManager(loadedSceneReference.name);
                }
#endif
                return;
            }

            var handle = SceneHandles[scene.AssetGUID];
            Addressables.UnloadSceneAsync(handle);
            SceneHandles.Remove(scene.AssetGUID);
            
            
        }
        
        #endregion
        
        #region Editor Mode Unloading
        
#if UNITY_EDITOR
        
        [EditorMode]
        private static void UnloadScenePlayModeViaSceneManager(string scene) => SceneManager.UnloadSceneAsync(scene);

        [EditorMode]
        internal static void UnloadSceneEditorModeViaEditorSceneManager(AssetReference scene) =>
            UnloadSceneEditorModeViaEditorSceneManager(GetLoadedSceneReference(scene));

        // Throws an exception if canceled
        [EditorMode]
        private static void UnloadSceneEditorModeViaEditorSceneManager(Scene toUnload)
        {
            if (toUnload.isDirty)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.CloseScene(toUnload, false);
                }
                else
                {
                    // canceled
                    throw new SceneUnloadCanceledException("Saving " + toUnload.name + " was cancelled by user");
                }
            }
            else
            {
                // clean scene
                EditorSceneManager.CloseScene(toUnload, true);
                return;
            }
        }

#endif
        #endregion
        
        #region Scene Access

        [EditorMode][PlayMode][BuildMode]
        internal static IEnumerable<AssetReference> AllLoadedSceneAsAssetReferences()
        {
            foreach (var s in AllSceneManagerScenes)
                yield return GetAssetReferenceSceneFromScene(s);
        }
        
        [EditorMode][PlayMode][BuildMode]
        internal static bool IsSceneLoaded(AssetReference scene)
        {
            foreach (var s in AllSceneManagerScenes)
            {
                var assetReferenceSceneFromScene = GetAssetReferenceSceneFromScene(s);
                if (s.isLoaded && assetReferenceSceneFromScene != null && assetReferenceSceneFromScene.AssetGUID.Equals(scene.AssetGUID))
                    return true;
            }

            return false;
        }
        
        [BuildMode][PlayMode][EditorMode]
        internal static bool IsSceneLoading(AssetReference scene)
        {
#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                return false;
#endif
            
            return SceneHandles.ContainsKey(scene.AssetGUID); // handle exists but is not managed means it is loading or unloading
        }
        
        
        [EditorMode][PlayMode][BuildMode]
        internal static int SceneCount
        {
            get
            {
#if UNITY_EDITOR
                return EditorSceneManager.sceneCount;
#else
                return SceneManager.sceneCount;
#endif
            }
        }
        
        [EditorMode][PlayMode][BuildMode]
        private static Scene GetSceneAt(int i)
        {
#if UNITY_EDITOR
            return EditorSceneManager.GetSceneAt(i);
#else
                return SceneManager.GetSceneAt(i);
#endif
        }
    
        [EditorMode][PlayMode][BuildMode]
        internal static IEnumerable<Scene> AllSceneManagerScenes
        {
            get
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    yield return GetSceneAt(i);
            }
        }

        [EditorMode][PlayMode][BuildMode]
        internal static bool AnySceneLoading
        {
            get
            {
                foreach (var s in AllSceneManagerScenes)
                    if (s.isLoaded == false)
                        return true;
                return false;
            }
        }
        
        #endregion
        
        #region Private scene retrieval methods
        
        [EditorMode][PlayMode][BuildMode]
        private static Scene GetLoadedSceneReference(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = GetSceneAt(i);
                if (scene.name.Equals(sceneName))
                    return scene;
            }

            return default;
        }
        
        
#if UNITY_EDITOR
        [EditorMode]
        private static Scene GetLoadedSceneReference(AssetReference sceneReference)
        {
            string scenePath = GetScenePathEditorMode(sceneReference);

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = GetSceneAt(i);
                if (scenePath.Equals(scene.path))
                    return scene;
            }

            return default;

        }
        private static string GetScenePathEditorMode(AssetReference sceneReference)
        {
            var scenePathEditorMode = AssetDatabase.GetAssetPath(sceneReference.editorAsset);
            return scenePathEditorMode;
        }
        
#endif
        
        // Can potentially return null if the scene is still loading
        [EditorMode][PlayMode][BuildMode]
        private static AssetReference GetAssetReferenceSceneFromScene(Scene scene) 
        {
            if (Application.isPlaying)
            {
                var assetRef = GetAssetReferenceSceneFromScenePlayMode(scene);
#if UNITY_EDITOR
                // check if null and if so, return the edit mode version if in editor, otherwise return null
                if (Application.isEditor && assetRef == null)
                {
                    return GetAssetReferenceSceneFromSceneEditMode(scene);
                }
                
#endif
                return assetRef;
            }
            else
            {
#if UNITY_EDITOR
                return GetAssetReferenceSceneFromSceneEditMode(scene);
#else
                return null;
#endif
            }
        }
        
#if UNITY_EDITOR
        [EditorMode]
        private static AssetReference GetAssetReferenceSceneFromSceneEditMode(Scene scene)
        {
            var path = scene.path;
            var activeSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(activeSceneAsset, out var guid, out long _))
            {
                var newRef = new AssetReference(guid);
                return newRef;
            }

            return null;
        }
#endif
    
        [PlayMode][EditorMode][BuildMode]
        private static AssetReference GetAssetReferenceSceneFromScenePlayMode(Scene scene)
        {
            string guidFound = string.Empty;
            // Iterate like this, rather than making a new collection, or using LINQ on it
            // New Collection/LINQ was causing GC. Iterating safer.
            foreach (var kvp in SceneHandles)
            {
                if (kvp.Value.IsDone && kvp.Value.Result.Scene == scene)
                {
                    guidFound = kvp.Key;
                    break;
                }
            }
            return string.IsNullOrEmpty(guidFound) == false ? new AssetReference(guidFound) : null;
        }

        #endregion

            
    }
    
    #region Exceptions and Function tags
    // This exception is thrown when a scene unload is canceled by the user
    internal class SceneUnloadCanceledException : Exception
    {
        public SceneUnloadCanceledException(string message) : base(message)
        {
        }
    }

    // This is a marker attribute to indicate that the method should only be used in editor mode
    internal class EditorModeAttribute : Attribute
    {
    }

    // This is a marker attribute to indicate that the method should only be used in build mode
    internal class BuildModeAttribute : Attribute
    {
    }

    // This is a marker attribute to indicate that the method should only be used in play mode
    internal class PlayModeAttribute : Attribute
    {
    }
    
    #endregion
}