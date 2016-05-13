using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheGame
{
    /// <summary>
    /// Main Menu handler
    /// </summary>
    public class Menu : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private Button cont;    //Continue button
        [SerializeField]
        private Text build;     //Build number label
        private List<object> savedGames = new List<object>();    //Temporary placeholder
        #endregion

        #region Methods
        /// <summary>
        /// Continue button event
        /// </summary>
        public void OnContinue()
        {
            //Continue last played game
        }

        /// <summary>
        /// New Game button event
        /// </summary>
        public void OnNewGame()
        {
            //Starts a new game
        }

        /// <summary>
        /// Load Game button event
        /// </summary>
        public void OnLoadGame()
        {
            //Brings up saved games menu
        }

        /// <summary>
        /// Settings button event
        /// </summary>
        public void OnSettings()
        {
            //Load settings scene/menu
        }

        /// <summary>
        /// Credit button event
        /// </summary>
        public void OnCredits()
        {
            //Load credits scene/menu
        }

        /// <summary>
        /// Quit button event
        /// </summary>
        public void OnQuit() => Application.Quit();
        #endregion

        #region Functions
        private void Awake()
        {
            //Load saved games?
            this.cont.interactable = this.savedGames.Count > 0; //Can't continue if no saved games
            this.build.text = "TheGame v" + GameVersion.Version;
        }
        #endregion
    }
}