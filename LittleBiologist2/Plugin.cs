using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx;
using MonoMod.RuntimeDetour;
using System.Reflection;
using RWCustom;
using System.IO;
using System.Text.RegularExpressions;

namespace LittleBiologist
{
    [BepInPlugin("Harvie.LittleBiologist", "LittleBiologist", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        #region others
        public static bool TestFunction => true; //启用测试功能

        public static Plugin instance;

        public MouseEventTrigger mouseEventTrigger;
        public KeyDownEventTrigger keyDownEventTrigger;
        #endregion
        //testFunction
        public bool beastMasterHookOn;
        public bool GetBeastMaster;

        public void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            instance = this;
        }

        public void Update()
        {
            if (mouseEventTrigger != null)
            {
                if (Input.GetMouseButtonDown(0)) mouseEventTrigger(0);
                if (Input.GetMouseButtonDown(1)) mouseEventTrigger(1);
                if (Input.GetMouseButtonDown(2)) mouseEventTrigger(2);
            }
            if (keyDownEventTrigger != null)
            {
                if (Input.anyKeyDown)
                {
                    keyDownEventTrigger();
                }
            }
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            Init();
        }

        public void Init()//初始化函数
        {
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
            On.HUD.HUD.InitMultiplayerHud += HUD_InitMultiplayerHud;
            On.HUD.HUD.InitSafariHud += HUD_InitSafariHud;
            Log("LittleBiologist Inited");
        }

        private void HUD_InitSafariHud(On.HUD.HUD.orig_InitSafariHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig.Invoke(self, cam);
            self.AddPart(new LBioHUD(self, cam));
        }

        private void HUD_InitMultiplayerHud(On.HUD.HUD.orig_InitMultiplayerHud orig, HUD.HUD self, ArenaGameSession session)
        {
            orig.Invoke(self, session);
            self.AddPart(new LBioHUD(self, session.room.game.cameras[0]));
        }


        #region hooks
        private void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig.Invoke(self, cam);
            self.AddPart(new LBioHUD(self, cam));
            Log("HUD_InitSinglePlayerHud");
        }

        public static void Log(string s)
        {
            Debug.Log("[LittleBiologist]" + s);
        }


        #endregion
        public delegate void MouseEventTrigger(int button_id);
        public delegate void KeyDownEventTrigger();
    }

    public static class LBioExpandFunc
    {
        public static Vector2 GetVector2(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.y);
        }

        public static float ManhattanDist(Vector2 a, Vector2 b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }

    public static class Hook_BeastMaster
    {
        public static Hook hook;

        static FieldInfo isSpawningModeInfo;
        static FieldInfo playerInfo;
        static FieldInfo spawningCreatureInfo;

        public static void HookOn(object beastMasterInstance)
        {
            isSpawningModeInfo = beastMasterInstance.GetType().GetField("isSpawningMode", BindingFlags.Instance | BindingFlags.Public);
            playerInfo = beastMasterInstance.GetType().GetField("player", BindingFlags.Instance | BindingFlags.NonPublic);
            spawningCreatureInfo = beastMasterInstance.GetType().GetField("spawningCreature", BindingFlags.Instance | BindingFlags.Public);

            

            hook = new Hook(
                beastMasterInstance.GetType().GetMethod("ManageSelection",BindingFlags.Instance | BindingFlags.Public),
                typeof(Hook_BeastMaster).GetMethod("ManageSelectionHook",BindingFlags.Static | BindingFlags.Public)
                );
        }

        public static void ManageSelectionHook(Action<object, Vector2, Vector2,RainWorld,RainWorldGame> orig,object self, Vector2 centerPoint, Vector2 camOffset, RainWorld RainWorldself, RainWorldGame game)
        {
            orig.Invoke(self, centerPoint, camOffset, RainWorldself, game);
            Vector2 vector = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - centerPoint;
            var isSpawningMode = (bool)isSpawningModeInfo.GetValue(self);
            var player = (Player)playerInfo.GetValue(self);
            var spawningCreature = (CreatureTemplate.Type)spawningCreatureInfo.GetValue(self);

            if (spawningCreature != null && isSpawningMode && player != null && player.room != null && Input.GetMouseButtonDown(0))
            {
                IntVector2 tilePosition = player.room.GetTilePosition(new Vector2(Input.mousePosition.x, Input.mousePosition.y) + camOffset);
                WorldCoordinate pos = new WorldCoordinate(player.abstractCreature.pos.room, tilePosition.x, tilePosition.y, player.abstractCreature.pos.abstractNode);
                EntityID newID = player.room.game.GetNewID();

                var newCreature = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger), null, pos, new EntityID(-1,26456));
                player.room.abstractRoom.entities.Add(newCreature);
                newCreature.RealizeInRoom();
            }
        }

        public static void RemoveHook()
        {
            hook.Dispose();
            hook = null;
        }
    }

    public static class GetTranslatorText
    {
        public static Dictionary<string, string> shortStrings = new Dictionary<string, string>();

        public static void GetTexts()
        {
            Plugin.Log("Start Get texts");
            List<string> origPaths = loadShortStringPath(InGameTranslator.LanguageID.English);
            List<string> transPaths = loadShortStringPath(InGameTranslator.LanguageID.Chinese);

            foreach(var path in origPaths)
            {
                Plugin.Log("Get orig path:" + path);
            }
            foreach (var path in transPaths)
            {
                Plugin.Log("Get transPaths path:" + path);
            }

            LoadShortStrings(origPaths);
            LoadShortStrings(transPaths);

            string outOrigPath = AssetManager.ResolveFilePath("Strings/Origs.txt");
            string outTransPath = AssetManager.ResolveFilePath("Strings/Trans.txt");


            Plugin.Log("Get outOrigPath:" + outOrigPath);
            Plugin.Log("Get outTransPath:" + outTransPath);


            string[] keys = shortStrings.Keys.ToArray();
            string origText = "";
            string transText = "";
            foreach(var key in keys)
            {
                origText += key + "\n";
                transText += shortStrings[key] + "\n";
            }
            origText.TrimEnd();
            transText.TrimEnd();

            File.WriteAllText(outOrigPath, origText);
            File.WriteAllText(outTransPath, transText);
        }

        public static List<string> loadShortStringPath(InGameTranslator.LanguageID id)
        {
            List<string> list = new List<string>();
            list.Add(Custom.RootFolderDirectory() + string.Concat(new string[]
            {
                Path.DirectorySeparatorChar.ToString(),
                "Text",
                Path.DirectorySeparatorChar.ToString(),
                "Text_",
                LocalizationTranslator.LangShort(id),
                Path.DirectorySeparatorChar.ToString(),
                "strings.txt"
            }).ToLowerInvariant());
            return list;
        }

        public static void LoadShortStrings(List<string> paths)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                string text = File.ReadAllText(paths[i], Encoding.UTF8);
                if (text[0] == '1')
                {
                    text = Custom.xorEncrypt(text, 12467);
                }
                else if (text[0] == '0')
                {
                    text = text.Remove(0, 1);
                }
                string[] array = Regex.Split(text, "\r\n");
                for (int j = 0; j < array.Length; j++)
                {
                    if (array[j].Contains("///"))
                    {
                        array[j] = array[j].Split(new char[]
                        {
                        '/'
                        })[0].TrimEnd(Array.Empty<char>());
                    }
                    string[] array2 = array[j].Split(new char[]
                    {
                    '|'
                    });
                    if (array2.Length >= 2 && !string.IsNullOrEmpty(array2[1]))
                    {
                        shortStrings[array2[0]] = array2[1];
                    }
                }
            }
        }
    }
}
