
namespace KaimiraGames.GameJam
{
    using UnityEditor;
    using UnityEditor.SceneManagement;

    [InitializeOnLoad]
    public class UnityAutoSave
    {
        // Static constructor that gets called when unity fires up.
        static UnityAutoSave()
        {
            // If we're about to run the scene...
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                {
                    EditorSceneManager.SaveOpenScenes();
                    AssetDatabase.SaveAssets();
                }
            };
        }
    }
}