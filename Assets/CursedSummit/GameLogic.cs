using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace CursedSummit
{
    /// <summary>
    /// Game scenes
    /// </summary>
    public enum GameScenes
    {
        LOADING = 0,
        MENU = 1
    }

    /// <summary>
    /// General Game utility logic. Access through instance member
    /// </summary>
    [DisallowMultipleComponent]
    public class GameLogic : MonoBehaviour
    {
        #region Instance
        /// <summary>
        /// Current "immortal" GameLogic instance
        /// </summary>
        public static GameLogic Instance { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Loads and launches the game into the given scene
        /// </summary>
        /// <param name="scene">Scene to load</param>
        internal void LoadScene(GameScenes scene)
        {
            Debug.Log("Loading scene: " + scene);
            SceneManager.LoadScene((int)scene);
        }

        /// <summary>
        /// Loads and launches the given scene atop of the current loaded scenes
        /// </summary>
        /// <param name="scene">Scene to load</param>
        internal void LoadSceneLayered(GameScenes scene)
        {
            Debug.Log("Loading layered scene: " + scene);
            SceneManager.LoadScene((int)scene, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Unloads the given scene
        /// </summary>
        /// <param name="scene">Scene to unload</param>
        internal void UnloadScene(GameScenes scene)
        {
            Debug.Log("Unloading scene: " + scene);
            SceneManager.UnloadScene((int)scene);
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Closes the instance of the game
        /// </summary>
        internal static void Quit()
        {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        #endregion
    }
}
