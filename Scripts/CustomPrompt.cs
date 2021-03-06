// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2020 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility;
using FullSerializer;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop.Game.Questing.Actions
{
    /// <summary>
    /// Prompt which displays a custom dialog that executes a different task based on user input.
    /// </summary>
    public class CustomPrompt : ActionTemplate
    {
        int id;

        Dictionary<string, Symbol> buttons = new Dictionary<string, Symbol>();
        string[] buttonKeys;

        public override string Pattern
        {
            //customprompt (?id\d+)(?<buttons>\s?[a-zA-Z0-9._]+\s[a-zA-Z0-9_.]+){2,}
            //get { return @"customprompt (?id\d+)(?<buttons>\s?[a-zA-Z0-9._]+\s[a-zA-Z0-9_.]+){2,}|" +
                         //@"customprompt (?idName\w+)(?<buttons>\s?[a-zA-Z0-9._]+\s[a-zA-Z0-9_.]+){2,}"; }
            get { return @"customprompt (?<id>\d+)\s(?<buttons>[A-Za-z0-9_\s]+)|" + 
                        @"customprompt (?<idName>\w+)\s(?<buttons>[A-Za-z0-9_\s]+)"; }
        }

        public CustomPrompt(Quest parentQuest)
            : base(parentQuest)
        {
            allowRearm = false;
        }

        public override IQuestAction CreateNew(string source, Quest parentQuest)
        {
            // Source must match pattern
            Match match = Test(source);
            if (!match.Success)
                return null;

            // Factory new prompt
            CustomPrompt prompt = new CustomPrompt(parentQuest);
            prompt.id = Parser.ParseInt(match.Groups["id"].Value);
            string[] promptButtons = match.Groups["buttons"].Value.Split(' ');
            prompt.buttonKeys = new string[promptButtons.Length / 2];
            int j = 0;
            for(int i = 0; i < promptButtons.Length; i = i + 2) {
                Debug.Log("adding button: " + promptButtons[i]);
                prompt.buttonKeys[j] = promptButtons[i];
                prompt.buttons.Add(promptButtons[i], new Symbol(promptButtons[i + 1]));
                j++;
            }
            Debug.Log("Finished adding buttons");

            // Resolve static message back to ID
            string idName = match.Groups["idName"].Value;
            if (prompt.id == 0 && !string.IsNullOrEmpty(idName))
            {
                Table table = QuestMachine.Instance.StaticMessagesTable;
                prompt.id = Parser.ParseInt(table.GetValue("id", idName));
            }

            return prompt;
        }

        public override void Update(Task caller)
        {
            DaggerfallMessageBox messageBox = CreateCustomMessagePrompt(ParentQuest, id);
            if (messageBox != null)
            {
                Debug.Log("Created messagebox");
                Debug.Log("Adding buttons (" + buttonKeys.Length.ToString() + ")");
                for(int i = 0; i < buttonKeys.Length; i++)
                {
                    Debug.Log("Creating button: " + buttonKeys[i]);
                    messageBox.AddCustomButton(id + i, buttonKeys[i], false);
                }
                Debug.Log("Adding click event");
                messageBox.OnCustomButtonClick += MessageBox_OnCustomButtonClick;
                Debug.Log("Showing messagebox");
                messageBox.Show();
            }
            SetComplete();
        }

        /// <summary>
        /// Creates a custom prompt from quest message.
        /// Caller must set events and call Show() when ready.
        /// </summary>
        public DaggerfallMessageBox CreateCustomMessagePrompt(Quest quest, int id) 
        {
            Message message = quest.GetMessage(id);
            if (message != null)
                return CreateCustomMessagePrompt(message);
            else
                return null;
        }

        /// <summary>
        /// Creates a custom prompt from quest message.
        /// Caller must set events and call Show() when ready.
        /// </summary>
        public DaggerfallMessageBox CreateCustomMessagePrompt(Message message)
        {
            TextFile.Token[] tokens = message.GetTextTokens();
            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(DaggerfallUI.UIManager);
            messageBox.SetTextTokens(tokens);
            messageBox.ClickAnywhereToClose = false;
            messageBox.AllowCancel = false;
            messageBox.ParentPanel.BackgroundColor = Color.clear;

            return messageBox;
        }

        private void MessageBox_OnCustomButtonClick(DaggerfallMessageBox sender, string messageBoxButton) 
        {
            Symbol task;
            buttons.TryGetValue(messageBoxButton, out task);
            ParentQuest.StartTask(task);
            sender.CloseWindow();
        }

        #region Serialization

        [fsObject("v1")]
        public struct SaveData_v1
        {
            public int id;
            public Dictionary<string, Symbol> buttons;
        }

        public override object GetSaveData()
        {
            SaveData_v1 data = new SaveData_v1();
            data.id = id;
            data.buttons = buttons;

            return data;
        }

        public override void RestoreSaveData(object dataIn)
        {
            if (dataIn == null)
                return;

            SaveData_v1 data = (SaveData_v1)dataIn;
            id = data.id;
            buttons = data.buttons;
        }

        #endregion
    }
}