using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheGame
{
    public class Menu : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private Button cont;
        [SerializeField]
        private Text build;
        private List<object> savedGames = new List<object>();    //Temporary placeholder
        #endregion

        #region Methods
        public void OnContinue()
        {
            //Continue last played game
        }

        public void OnNewGame()
        {
            //Starts a new game
        }

        public void OnLoadGame()
        {
            //Brings up saved games menu
        }

        public void OnSettings()
        {
            //Load settings scene/menu
        }

        public void OnCredits()
        {
            //Load credits scene/menu
        }

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