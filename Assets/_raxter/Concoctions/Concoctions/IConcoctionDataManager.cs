using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Concoctions
{
    public interface IConcoctionDataManager
    {
        public static IEnumerable<T> FindAll<T>(bool includeInactive = false)
        {
            for (int i = 0 ; i < SceneManager.sceneCount ; i++)
            {
                var sceneAt = SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded)
                    continue;
                
                var rootObjects = sceneAt.GetRootGameObjects();
                foreach (var rootObject in rootObjects)
                {
                    foreach( var childInterface in rootObject.GetComponentsInChildren<T>(includeInactive))
                    {
                        yield return childInterface;
                    }
                }
            }
        }

        public static T Find<T>(bool includeInactive = false)
        {
            foreach (var t in FindAll<T>(includeInactive))
                return t;
            return default;
        }
        
        static IConcoctionDataManager()
        {
            // Subscribe to scene changes
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        // Reset the flag when the hierarchy changes
        static void OnHierarchyChanged() => _hasSearchedAllObjects = false;

        // Reset the flag when a new scene is loaded
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => 
            _hasSearchedAllObjects = false;

        private static bool _hasSearchedAllObjects = false;
        private static IConcoctionDataManager _instance;
        // While not an ideal system, it is a requirement that in play mode
        // the ConcoctionManager must register itself in the Awake or Start function
        public static IConcoctionDataManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                
                if (!Application.isPlaying && !_hasSearchedAllObjects)
                {
                    // this is horribly inefficient, I know
                    _instance = Find<IConcoctionDataManager>(true);
                    
                    // this flag will prevent it getting out of hand
                    _hasSearchedAllObjects = true;
                }

                return _instance;

            }
            set
            {
                _instance = value;
            }
        }
        public static IngredientWildcardLibrary Wildcards => Instance?.WildcardLibrary;
        public IngredientWildcardLibrary WildcardLibrary { get; }
        public static ConcoctionNamingRules NamingRules => Instance?.ConcoctionNamingRules;
        public ConcoctionNamingRules ConcoctionNamingRules { get; }
    }

}
