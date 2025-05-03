using UnityEngine;

namespace RaxterBaxter.WorldStreamer
{
    public class DisplayScenesGUI : MonoBehaviour
    {
        void OnGUI()
        {
            foreach (var scene in SceneLoadUtility.AllSceneManagerScenes)
            {
                // show as editor layout list
                GUILayout.Label(scene.name);
            }
            
        }
    }
}
