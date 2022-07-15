using static KaimiraGames.GameJam.Logging;
using DG.Tweening;
using Unity;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

namespace KaimiraGames.GameJam
{
    public class LoadFirstUtils : BetterMonoBehaviour
    {
        private void Awake()
        {
            // DOTween Setup
            DOTween.Init();
            DOTween.defaultAutoPlay = AutoPlay.None;
            DOTween.useSafeMode = false;
            DOTween.SetTweensCapacity(3000, 500);
            v("DOTween setup complete.");
        }
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    public class LoadFirstUtilsEditor : UnityEditor.Editor
    {
        static LoadFirstUtilsEditor()
        {
            // Set first scene in game to auto-play when play is clicked (from any scene)
            var firstScene = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(UnityEditor.EditorBuildSettings.scenes[0].path);
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = firstScene;
        }
    }
#endif
}