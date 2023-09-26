using BepInEx;
using HarmonyLib;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomGraffiti
{
    [BepInPlugin("dance.tari.bombrushcyberfunk.customgraffiti", "CustomGraffiti", "0.0.0.1")]
    [BepInProcess("Bomb Rush Cyberfunk.exe")]
    public class CustomGraffitiMod : BaseUnityPlugin
    {
        private static Harmony _harmonyInstance;
        private static CustomGraffitiMod _modInstance;
        public static CustomGraffitiMod Instance => _modInstance;

        public static List<CustomGraffiti> LoadedGraffiti;

        public static BepInEx.Logging.ManualLogSource Log => _modInstance.Logger;

        public CustomGraffitiMod()
        {
            _modInstance = this;
            _harmonyInstance = new Harmony("dance.tari.bombrushcyberfunk.customgraffiti.patch");
            LoadedGraffiti = new List<CustomGraffiti>();
        }

        // Add Graffiti

        public static CustomGraffiti AddCustomGraffitiFromFilePath(string filePath)
        {
            string folder = Path.GetDirectoryName(filePath);

            folder = folder.Substring(folder.LastIndexOf('\\') + 1, folder.Length - folder.LastIndexOf('\\') - 1);

            GraffitiSize size = GraffitiSize.S;
            // figure out what size our graffiti is via folder path
            switch (folder)
            {
                case "Small":
                    size = GraffitiSize.S;
                    break;
                case "Medium":
                    size = GraffitiSize.M;
                    break;
                case "Large":
                    size = GraffitiSize.L;
                    break;
                case "ExtraLarge":
                    size = GraffitiSize.XL;
                    break;
            }

            return AddCustomGraffitiFromFilePath(filePath, size);
        }

        public static CustomGraffiti AddCustomGraffitiFromFilePath(string filePath, GraffitiSize size)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string ext = Path.GetExtension(filePath);

            //
            string[] name = fileName.Split('_');

            string graffitiName;
            GraffitiArt.Combos graffitiCombo = GraffitiArt.Combos.NONE;

            if (name.Length >= 2)
            {
                string comboString = name[0];
                GraffitiArt.Combos combo = ParseCombo(comboString);

                graffitiCombo = combo;
                graffitiName = string.Join("_", name, 1, name.Length - 1);
            }
            else
            {
                graffitiName = fileName;
            }

            // If our graffiti is invalid
            if (graffitiCombo == GraffitiArt.Combos.NONE && size != GraffitiSize.S)
            {
                string combo = "";
                switch (size)
                {
                    case GraffitiSize.M:
                        combo = "1234";
                        break;
                    case GraffitiSize.L:
                        combo = "12345";
                        break;
                    case GraffitiSize.XL:
                        combo = "123456";
                        break;
                }
                Log.LogError($"Unable to add graffiti {fileName}. Graffiti larger than small graffiti need a combo in the file name, such as '{combo}_{fileName}{ext}'. The underscore to separate the combo from the name is required!");
                return null;
            }

            // get texture -- if invalid we can't make a custom graffiti
            Texture2D texture = LoadTexture(filePath);

            if (texture == null)
                return null;

            // create our new custom graffiti instance
            CustomGraffiti graffiti = new CustomGraffiti();

            graffiti.Name = graffitiName;

            graffiti.Combo = graffitiCombo;

            // apply the texture to our instance
            graffiti.Texture = texture;

            // user specified size, override folder path
            graffiti.Size = size;

            LoadedGraffiti.Add(graffiti);
            return graffiti;
        }

        // API

        public static List<CustomGraffiti> GetCustomGraffitiBySize(GraffitiSize size)
        {
            List<CustomGraffiti> list = new List<CustomGraffiti>();

            foreach (CustomGraffiti graffiti in LoadedGraffiti)
            {
                if (graffiti.Size == size)
                {
                    list.Add(graffiti);
                }
            }

            return list;
        }

        public static List<CustomGraffiti> GetCustomGraffitiByCombo(GraffitiArt.Combos combo)
        {
            List<CustomGraffiti> list = new List<CustomGraffiti>();

            foreach (CustomGraffiti graffiti in LoadedGraffiti)
            {
                if (graffiti.Combo == combo)
                {
                    list.Add(graffiti);
                }
            }

            return list;
        }

        public static List<CustomGraffiti> GetCustomGraffitiByName(string name)
        {
            List<CustomGraffiti> list = new List<CustomGraffiti>();

            foreach (CustomGraffiti graffiti in LoadedGraffiti)
            {
                if (graffiti.Name == name)
                {
                    list.Add(graffiti);
                }
            }

            return list;
        }

        // Utility

        public static Texture2D LoadTexture(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            string ext = Path.GetExtension(filePath);

            if (ext != ".jpg" && ext != ".png")
            {
                Instance.Logger.LogWarning($"Cannot load graffiti: {fileName}{ext}. Only .png and .jpg files are accepted.");
                return null;
            }

            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(File.ReadAllBytes(filePath)))
            {
                tex.Apply();

                //Instance.Logger.LogInfo($"Custom graffiti {fileName}{ext} loaded.");

                return tex;
            }

            Instance.Logger.LogWarning($"Could not load image data of {fileName}{ext}.");
            return null;
        }

        public static GraffitiArt.Combos ParseCombo(string numericComboString)
        {
            if (int.TryParse(numericComboString, out int combo))
            {
                GraffitiArt.Combos comboEnum = (GraffitiArt.Combos)combo;

                if (Enum.IsDefined(typeof(GraffitiArt.Combos), comboEnum))
                {
                    return comboEnum;
                }
            }

            return GraffitiArt.Combos.NONE;
        }

        // Framerate stuff


        private int _desiredFramerate = Screen.currentResolution.refreshRate + 1;

        private GraffitiLoader _loaderInstance;
        private bool _initialized = false;

        private void Uninitialize()
        {
            if (!_initialized)
            {
                return;
            }

            _initialized = false;
            Log.LogWarning("Uninitialized.");
            LoadedGraffiti.Clear();
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            Log.LogWarning("Initialized.");

            string dir = Path.GetDirectoryName(Info.Location);
            string graffitiFolder = "Graffiti";

            string graffitiFolderPath = Path.Combine(dir, graffitiFolder) + "/";

            string smallGraffitiFolder = Path.Combine(graffitiFolderPath, "Small") + "/";
            string mediumGraffitiFolder = Path.Combine(graffitiFolderPath, "Medium") + "/";
            string largeGraffitiFolder = Path.Combine(graffitiFolderPath, "Large") + "/";
            string xlGraffitiFolder = Path.Combine(graffitiFolderPath, "ExtraLarge") + "/";

            if (!Directory.Exists(graffitiFolderPath))
                Directory.CreateDirectory(graffitiFolderPath);

            if (!Directory.Exists(smallGraffitiFolder))
                Directory.CreateDirectory(smallGraffitiFolder);

            if (!Directory.Exists(mediumGraffitiFolder))
                Directory.CreateDirectory(mediumGraffitiFolder);

            if (!Directory.Exists(largeGraffitiFolder))
                Directory.CreateDirectory(largeGraffitiFolder);

            if (!Directory.Exists(xlGraffitiFolder))
                Directory.CreateDirectory(xlGraffitiFolder);

            string[] smallGraffitiFiles = Directory.GetFiles(smallGraffitiFolder);
            string[] mediumGraffitiFiles = Directory.GetFiles(mediumGraffitiFolder);
            string[] largeGraffitiFiles = Directory.GetFiles(largeGraffitiFolder);
            string[] xlGraffitiFiles = Directory.GetFiles(xlGraffitiFolder);

            List<string> files = new List<string>();
            files.AddRange(smallGraffitiFiles);
            files.AddRange(mediumGraffitiFiles);
            files.AddRange(largeGraffitiFiles);
            files.AddRange(xlGraffitiFiles);

            foreach (string file in files)
            {
                AddCustomGraffitiFromFilePath(file);
            }

            foreach (CustomGraffiti customGraffiti in LoadedGraffiti)
            {
                Logger.LogInfo($"{customGraffiti.Name}, Size: {customGraffiti.Size}, Combo: {customGraffiti.Combo}");
                customGraffiti.FixShader();
                customGraffiti.AddToGraffitiArtInfo();
            }

            Logger.LogInfo("Plugin Custom Graffiti successfully initialized.");
        }

        // Unity Functions

        private void Awake()
        {
            _harmonyInstance?.PatchAll();

            Logger.LogInfo("Plugin Custom Graffiti patched successfully.");

            /*
            var methods = _harmonyInstance.GetPatchedMethods();
            foreach (var method in methods)
            {
                Logger.LogInfo($"Patched: {method.Name}");
            }
            */
        }

        private void OnDestroy()
        {
            _harmonyInstance?.UnpatchSelf();
            LoadedGraffiti.Clear();
        }

        private void Update()
        {
            // Turns out BRC doesn't really cap your framerate, so your GPU will become a space heater
            // This bit of code does cap your framerate. Reason it's in here is because for some reason it gets updated twice.
            // This bit of code will essentially set your framerate to your max available framerate depending on your screen's refresh rate.

            if (Application.targetFrameRate != _desiredFramerate)
            {
                Logger.LogInfo($"Forcing framerate to {_desiredFramerate}");
                Application.targetFrameRate = _desiredFramerate;
            }

            // For context, since I'm using an older version of .NET Framework (4.7.2), I don't have access to "On" for events.
            // This update loop will simply check if there's a graffiti loader instance available, if it is, we inject and load our graffiti into it.
            // This happens basically every time we load a new area of the game, so it refreshes often.
            // It's not great, but it works.

            // Setup Loader Instance


            if (_loaderInstance == null) // uninitialize if we haven't received our graffiti loader
            {
                Uninitialize();
                GraffitiLoader loader = CustomGraffiti.GetGraffitiLoader();

                if (loader == null)
                {
                    return;
                }
                else
                {
                    _loaderInstance = loader;
                    // cool, we have the graffiti loader
                    // now we wait until graffiti info is available
                }
            }

            if (_loaderInstance.GraffitiArtInfo != null) // we have our graffiti info!
            {
                if (_loaderInstance.GraffitiArtInfo.graffitiArt != null && !_initialized) // we have an available list, and we need to initialize once
                {
                    Initialize();
                    _initialized = true;
                }
                else if (_loaderInstance.GraffitiArtInfo.graffitiArt == null) // if the list is suddenly null, uninitialize
                {
                    Uninitialize();
                }
            }
            else // initialize if our graffiti info instance is destroyed (asset bundle is unloaded)
            {
                Uninitialize();
            }
        }
    }
}
