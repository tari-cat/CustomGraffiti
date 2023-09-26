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

        public CustomGraffitiMod()
        {
            _modInstance = this;
            _harmonyInstance = new Harmony("dance.tari.bombrushcyberfunk.customgraffiti.patch");
            LoadedGraffiti = new List<CustomGraffiti>();
        }

        private void Awake()
        {
            _harmonyInstance?.PatchAll();
        }

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

            // get texture -- if invalid we can't make a custom graffiti
            Texture2D texture = LoadTexture(filePath);

            if (texture == null)
                return null;

            // create our new custom graffiti instance
            CustomGraffiti graffiti = new CustomGraffiti();

            // apply the texture to our instance
            graffiti.Texture = texture;

            // user specified size, override folder path
            graffiti.Size = size;

            //
            string[] name = fileName.Split('_');

            if (name.Length >= 2)
            {
                string comboString = name[0];
                GraffitiArt.Combos combo = ParseCombo(comboString);

                graffiti.Combo = combo;
                graffiti.Name = string.Join("_", name, 1, name.Length - 1);
            }
            else
            {
                graffiti.Name = fileName;
            }

            LoadedGraffiti.Add(graffiti);
            return graffiti;
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

                Instance.Logger.LogInfo($"Custom graffiti {fileName}{ext} loaded.");

                return tex;
            }

            Instance.Logger.LogWarning($"Could not load image data of {fileName}{ext}.");
            return null;
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

        // Framerate stuff


        private int _desiredFramerate = Screen.currentResolution.refreshRate + 1;

        private GraffitiLoader _loaderInstance;
        private bool _initialized = false;

        private void Update()
        {
            // framerate check

            if (Application.targetFrameRate != _desiredFramerate)
            {
                Logger.LogInfo($"Forcing framerate to {_desiredFramerate}");
                Application.targetFrameRate = _desiredFramerate;
            }


            // Setup Loader Instance
            if (_loaderInstance == null)
            {
                _initialized = false;
                LoadedGraffiti.Clear();
                GraffitiLoader loader = CustomGraffiti.GetGraffitiLoader();

                if (loader == null)
                {
                    return;
                }
                else
                {
                    _loaderInstance = loader;
                }
            }

            if (_loaderInstance.GraffitiArtInfo != null)
            {
                if (_loaderInstance.GraffitiArtInfo.graffitiArt != null && !_initialized)
                {
                    Initialize();
                    _initialized = true;
                }
                else if (_loaderInstance.GraffitiArtInfo.graffitiArt == null)
                {
                    Uninitialize();
                }
            }
            else
            {
                Uninitialize();
            }
        }

        private void Uninitialize()
        {
            if (!_initialized)
            {
                return;
            }

            _initialized = false;
            Debug.LogWarning("Uninitialized.");
            LoadedGraffiti.Clear();
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            Debug.LogWarning("Initialized.");

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

            //Logger.LogInfo("------------------------");

            Logger.LogInfo("Loaded custom graffiti:");

            foreach (CustomGraffiti customGraffiti in LoadedGraffiti)
            {
                Logger.LogInfo($"{customGraffiti.Name} ({customGraffiti.Texture}), Size: {customGraffiti.Size}, Combo: {customGraffiti.Combo}, SO: {customGraffiti.AppEntry}");
                customGraffiti.FixShader();
                customGraffiti.AddToGraffitiArtInfo();
            }

            //Logger.LogInfo("------------------------");

            Logger.LogInfo("Plugin Custom Graffiti successfully initialized.");
        }

        private void OnDestroy()
        {
            _harmonyInstance?.UnpatchSelf();
        }
    }
}
