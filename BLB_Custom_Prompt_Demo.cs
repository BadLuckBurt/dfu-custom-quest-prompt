using System;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Questing.Actions;

namespace BLB.CustomPromptDemo
{
    
    //this class initializes the mod.
    public class CustomPromptDemoModLoader : MonoBehaviour
    {
        public static Mod mod;
		public static GameObject go;

        //like in the last example, this is used to setup the Mod.  This gets called at Start state.
        [Invoke(StateManager.StateTypes.Start)]
        public static void InitAtStartState(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);

            Debug.Log("Started setup of : " + mod.Title);
            GameManager.Instance.QuestMachine.RegisterAction(new CustomPrompt(null));

            if (!QuestListsManager.RegisterQuestList("CustomPrompt"))
                throw new Exception("Quest list name is already in use, unable to register CustomPrompt quest list.");

            mod.IsReady = true;
        }

        [Invoke(StateManager.StateTypes.Game)]
        public static void InitAtGameState(InitParams initParams)
        {

        }
    }
}
