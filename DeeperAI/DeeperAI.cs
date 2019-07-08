using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WeNeedToModDeeperEngine;

namespace DeeperAI
{
    public class DeeperAI : IPlugin
    {
        //Declare events and variables
        public ModEngineEvents events = new ModEngineEvents();

        public Manager manager;

        public string Name => "DeeperAI";

        public string Version => "v0.1";

        public void OnApplicationQuit()
        {
            Debug.Log("DeeperAI: Shutting down");
        }

        public void OnApplicationStart()
        {
            Debug.Log("DeeperAI: Starting");
        }

        public void OnFixedUpdate()
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        public void OnLevelWasLoaded(int level)
        {
        }

        public void OnUpdate()
        {
            if (events.KeyPressed(KeyCode.Home)) //Home pressed, start network stuffs
            {
                manager = new Manager();
            }
            if (manager != null && NetworkServer.active) manager.ProccessNet();
        }
    }
}
