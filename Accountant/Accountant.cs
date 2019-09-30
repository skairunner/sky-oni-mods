using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using Harmony;
using static SkyLib.Logger;

namespace Accountant
{
    public class AccountantUpdate : KMonoBehaviour, ISim4000ms
    {
        public void Sim4000ms(float dt)
        {
            Accountant.wss.WebSocketServices.BroadcastAsync(AccountantSocket.GetInventoryString(), null);
        }
    }

    public class Accountant
    {
        static public WebSocketServer wss;
        static public string ModName = "Accountant";
        public static bool addedComponent = false;
        static public AccountantData DATA = new AccountantData();

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
                if (wss == null)
                {
                    LogLine("Starting websocket server.");
                    wss = new WebSocketServer(4920);
                    wss.AddWebSocketService<AccountantSocket>("/");
                    wss.Start();
                    LogLine("Websocket server started.");
                }
                else
                {
                    LogLine("Websocket server already started, skipping.");
                }
            }
        }

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("OnPrefabInit")]
        public static class Game_OnPrefabInit_Patch
        {
            public static void Postfix(PauseScreen __instance)
            {
                LogLine("Hello! After OnPrefabInit. Trying to add AccountantUpdate.");
                Game.Instance.gameObject.AddComponent(typeof(AccountantUpdate));
            }
        }

        [HarmonyPatch(typeof(Game))]
        [HarmonyPatch("OnLoadLevel")]
        public static class Game_OnLoadLevel_Patch
        {
            public static void Postfix(PauseScreen __instance)
            {
                LogLine("Hello! After OnLoadLevel.");
            }
        }
    }
}
