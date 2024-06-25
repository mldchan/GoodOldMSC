﻿using HutongGames.PlayMaker;

using MSCLoader;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Reflection;

using System.Text;

using UnityEngine;

namespace GoodOldMSC.Mods.OldCarSounds {
    public class OldCarSounds {
        public SatsumaOcs satsumaOcs;

        public void OnNewGame()
        {
            if (!LoadGameOnMenu)
            {
                return;
            }

            LoadGameOnMenu = false;
            Application.LoadLevel(3);
        }

        public void OnLoad(Mod mod)
        {
            if (File.Exists(Path.Combine(ModLoader.GetModSettingsFolder(mod), "log.log")))
            {
                File.Delete(Path.Combine(ModLoader.GetModSettingsFolder(mod), "log.log"));
            }

            // Called once, when mod is loading after game is fully loaded
            PrintF("Starting Loading of OldCarSounds...", "load");

            // Load asset bundle
            PrintF("Loading AssetBundle", "load");

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Stream stream = executingAssembly.GetManifestResourceStream("OldCarSounds.Resources.oldsound.unity3d");
            if (stream != null)
            {
                byte[] shit = new byte[stream.Length];
                stream.Read(shit, 0, shit.Length);

                AssetBundle assetBundle = AssetBundle.CreateFromMemoryImmediate(shit);

                if (EngineSoundsTypeSettings.GetValue() == 2)
                {
                    PrintF("Loading audio files from old builds...", "load");
                    Clip2 = assetBundle.LoadAsset<AudioClip>("idle_sisa");
                    Clip1 = assetBundle.LoadAsset<AudioClip>("idle");
                }

                // Assemble sounds
                if (AssembleSounds.GetValue())
                {
                    PrintF("Loading audio files for assembly sounds...", "load");
                    Clip3 = assetBundle.LoadAsset("assemble") as AudioClip;
                }

                _noSel = assetBundle.LoadAsset<Material>("nosel");

                // Music
                if (OldRadioSongsSettings.GetValue())
                {
                    PrintF("Loading radio songs...", "load");
                    Radio1 = assetBundle.LoadAsset("oldradio") as GameObject;

                    if (ModLoader.CheckSteam())
                    {
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("mustamies"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("oldradiosong"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song2"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song3"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song4"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song5"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song6"));
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("song7"));
                    }
                    else
                    {
                        RadioCore.Clips.Add(assetBundle.LoadAsset<AudioClip>("pirate"));
                    }

                    // Import custom songs
                    string path = Path.Combine(ModLoader.GetModAssetsFolder(mod), "radiosongs");
                    if (File.Exists(path))
                    {
                        foreach (string name in Directory.GetFiles(path))
                        {
                            PrintF("Loading: " + name);
                            WWW www = new WWW("file:///" + name);
                            RadioCore.Clips.Add(www.GetAudioClip(true, false));
                        }
                    }
                }

                // Dashboard texture
                if (OldDashTexturesSettings.GetValue())
                {
                    PrintF("Loading black material for dashboard");
                    Material1 = assetBundle.LoadAsset<Material>("black");
                }

                // Selection textures if chosen to
                if (SelectionSelectionSettings.GetValue())
                {
                    PrintF("Loading selection material");
                    SelMaterial = assetBundle.LoadAsset<Material>("selection");
                }

                // Unload the asset bundle to reduce memory usage
                PrintF("Unloading AssetBundle", "load");
                assetBundle.Unload(false);
            }
            else
            {
                PrintF("Error while loading mod resources. Your copy of OCS is corrupted.", "ERR", true);
            }

            // Get the GameObject of Satsuma.
            Satsuma = GameObject.Find("SATSUMA(557kg, 248)");

            // Add the component that does the load stuff
            PrintF("Adding component for Satsuma", "load");
            satsumaOcs = Satsuma.AddComponent<SatsumaOcs>();

            // Old RPM Gauge
            if (OldRpmGaugeSettings.GetValue())
            {
                GameObject object1 = UnityEngine.Object.FindObjectsOfType<GameObject>()
                    .First(sdf => sdf.name.IndexOf("rpm gauge", StringComparison.OrdinalIgnoreCase) >= 0);
                object1.SetActive(true);
                object1.AddComponent<RPMGauge>();
            }

            // Create a new instance of stopwatch
            PrintF("Starting stopwatch for info text", "load");
            _stopwatch = new Stopwatch();

            // Start a stopwatch for the lake time info thing
            _stopwatch.Start();

            PrintF("Fully loaded!", "load", true);
        }

        public void ModSettings(Mod mod)
        {
            /* UPDATED SETTINGS DOCS
            AssembleSounds = new Settings("assembleSounds", "Assemble Sounds", false);
            DisableDoorSoundsSettings = new Settings("doorSounds", "Disable Door Sounds", false);
            DisableFootSoundsSettings = new Settings("footSounds", "Disable Foot Sounds", false);
            DisableKnobSoundsSettings = new Settings("knobSounds", "Disable Knob Sounds", false);
            OldDashTexturesSettings = new Settings("oldDash", "Old Dashboard", false);
            InfoTextSettings = new Settings("info", "Information Text", false);
            OldRadioSongsSettings = new Settings("radio", "Old Radio", false);
            ShiftDelaySelectionSettings = new Settings("shiftDelay", "Shift Delay Selection", 0);
            KeySoundSelectionSettings = new Settings("keySound", "Key Sound Selection", 0);
            SelectionSelectionSettings = new Settings("selection", "Green selections", false);
            EngineSoundsTypeSettings = new Settings("sounds", "Engine sound type", 0);
            OldRpmGaugeSettings = new Settings("rpmgauge", "Old RPM Gauge", false);
            OldDelaySettings = new Settings("oldrev", "Old engine revving", false);

            Settings.AddSlider(this, ShiftDelaySelectionSettings, 0, 2, new[] {
                "No change",
                "Build 172",
                "No delay"
            });
            Settings.AddSlider(this, KeySoundSelectionSettings, 0, 2, new[] {
                "No change",
                "Old key sounds (2016)",
                "No key sounds (2014)"
            });
            Settings.AddSlider(this, EngineSoundsTypeSettings, 0, 2, new[] {
                "No engine sound change",
                "Lower pitch (2016)",
                "Old alpha (2014)"
            });
            Settings.AddCheckBox(this, SelectionSelectionSettings);
            Settings.AddCheckBox(this, AssembleSounds);
            Settings.AddCheckBox(this, DisableDoorSoundsSettings);
            Settings.AddCheckBox(this, DisableFootSoundsSettings);
            Settings.AddCheckBox(this, DisableKnobSoundsSettings);
            Settings.AddCheckBox(this, OldDashTexturesSettings);
            Settings.AddCheckBox(this, InfoTextSettings);
            Settings.AddCheckBox(this, OldRadioSongsSettings);
            Settings.AddCheckBox(this, OldRpmGaugeSettings);*/

            AssembleSounds = Settings.AddCheckBox(mod, "assembleSounds", "Assemble Sounds", false);
            DisableDoorSoundsSettings = Settings.AddCheckBox(mod, "doorSounds", "Disable Door Sounds", false);
            DisableFootSoundsSettings = Settings.AddCheckBox(mod, "footSounds", "Disable Foot Sounds", false);
            DisableKnobSoundsSettings = Settings.AddCheckBox(mod, "knobSounds", "Disable Knob Sounds", false);
            OldDashTexturesSettings = Settings.AddCheckBox(mod, "oldDash", "Old Dashboard", false);
            InfoTextSettings = Settings.AddCheckBox(mod, "info", "Information Text", false);
            OldRadioSongsSettings = Settings.AddCheckBox(mod, "radio", "Old Radio", false);
            ShiftDelaySelectionSettings = Settings.AddSlider(mod, "shiftDelay", "Shift Delay Selection", 0, 2, 0, textValues: new[] {
                "No change",
                "Build 172",
                "No delay"
            });
            KeySoundSelectionSettings = Settings.AddSlider(mod, "keySound", "Key Sound Selection", 0, 2, 0, textValues: new[] {
                "No change",
                "Old key sounds (2016)",
                "No key sounds (2014)"
            });
            SelectionSelectionSettings = Settings.AddCheckBox(mod, "selection", "Green selections", false);
            EngineSoundsTypeSettings = Settings.AddSlider(mod, "sounds", "Engine sound type", 0, 2, 0, textValues: new[] {
                "No engine sound change",
                "Lower pitch (2016)",
                "Old alpha (2014)"
            });
            OldRpmGaugeSettings = Settings.AddCheckBox(mod, "rpmgauge", "Old RPM Gauge", false);
            OldDelaySettings = Settings.AddCheckBox(mod, "oldrev", "Old engine revving", false);

        }

        public void OnGUI()
        {

            // Use GUI statements
            // Not anything else
            // Called every render

            if (ModLoader.GetCurrentScene() != CurrentScene.Game)
            {
                return;
            }

            if (!InfoTextSettings.GetValue())
            {
                return;
            }

            float fps = (float)Math.Round(1f / Time.unscaledDeltaTime, 2);
            float wrenchSize = FsmVariables.GlobalVariables.GetFsmFloat("ToolWrenchSize").Value;
            GUI.Label(new Rect(0, 0, 1000, 20), $"FPS: {fps}");
            GUI.Label(new Rect(0, 20, 1000, 20), $"Wrench size: {wrenchSize}");
            GUI.Label(new Rect(0, 40, 1000, 20),
                $"Lake run current time: {_stopwatch.Elapsed.Minutes}:{_stopwatch.Elapsed.Seconds}:{_stopwatch.Elapsed.Milliseconds}");
            GUI.Label(new Rect(0, 60, 1000, 20), "Lake run last time: ");
        }

        private Camera mainCam;

        public void Update()
        {

            // Color buttons green if looking at them

            if (Camera.main == null)
            {
                return;
            }
            if (mainCam == null)
            {
                mainCam = Camera.main;
            }

            foreach (RaycastHit hit in Physics.RaycastAll(mainCam.ScreenPointToRay(Input.mousePosition), 3f))
            {

                // Power knob
                if (SatsumaOcs.powerKnob != null)
                {
                    if (hit.collider.gameObject.name == SatsumaOcs.powerKnob.name)
                    {
                        if (!SelectionSelectionSettings.GetValue())
                        {
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Radio";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                        }
                        else
                        {
                            SatsumaOcs.powerKnob.GetComponent<Renderer>().material = SelMaterial;
                        }

                        break;
                    }

                    SatsumaOcs.powerKnob.GetComponent<Renderer>().material = _noSel;
                }

                // Volume knob
                if (SatsumaOcs.volumeKnob != null)
                {
                    if (hit.collider.gameObject.name == SatsumaOcs.volumeKnob.name)
                    {
                        if (!SelectionSelectionSettings.GetValue())
                        {
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Volume";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                        }
                        else
                        {
                            SatsumaOcs.volumeKnob.GetComponent<Renderer>().material = SelMaterial;
                        }

                        float wheel = Input.GetAxis("Mouse ScrollWheel");
                        if (wheel >= 0.01f)
                        {
                            SatsumaOcs.radioCoreInstance.IncreaseVolume();
                        }

                        if (wheel <= -0.01f)
                        {
                            SatsumaOcs.radioCoreInstance.DecreaseVolume();
                        }

                        break;
                    }

                    SatsumaOcs.volumeKnob.GetComponent<Renderer>().material = _noSel;
                }

                // Switch knob
                if (SatsumaOcs.switchKnob != null)
                {
                    if (hit.collider.gameObject.name == SatsumaOcs.switchKnob.name)
                    {
                        if (SelectionSelectionSettings.GetValue())
                        {
                            SatsumaOcs.switchKnob.GetComponent<Renderer>().material = SelMaterial;
                        }
                        else
                        {
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "Next";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = true;
                        }

                        break;
                    }

                    SatsumaOcs.switchKnob.GetComponent<Renderer>().material = _noSel;
                }

                if (SatsumaOcs.knobChoke != null && SatsumaOcs.triggerChoke != null)
                {

                    // Check other knobs
                    if (hit.collider.gameObject == SatsumaOcs.triggerChoke)
                    {
                        if (SelectionSelectionSettings.GetValue())
                        {

                            // Color is now green
                            SatsumaOcs.knobChoke.GetComponent<Renderer>().material = SelMaterial;

                            // Disable the little subtitle and stuff in center
                            // of the screen.
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;

                            // This "break" will exit forEach statement.
                            // Not triggering any code under }
                        }

                        break;
                    }

                    // If we don't exit in the last if statement,
                    // this will run setting the color back to normal
                    SatsumaOcs.knobChoke.GetComponent<Renderer>().material = Material1;
                }

                if (SatsumaOcs.knobHazards != null && SatsumaOcs.triggerHazard != null)
                {
                    if (hit.collider.gameObject == SatsumaOcs.triggerHazard)
                    {
                        if (SelectionSelectionSettings.GetValue())
                        {
                            SatsumaOcs.knobHazards.GetComponent<Renderer>().material = SelMaterial;
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                        }

                        break;
                    }

                    SatsumaOcs.knobHazards.GetComponent<Renderer>().material = Material1;
                }

                if (SatsumaOcs.knobLights != null && SatsumaOcs.triggerLightModes != null)
                {
                    if (hit.collider.gameObject == SatsumaOcs.triggerLightModes)
                    {
                        if (SelectionSelectionSettings.GetValue())
                        {
                            SatsumaOcs.knobLights.GetComponent<Renderer>().material = SelMaterial;
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                        }

                        break;
                    }

                    SatsumaOcs.knobLights.GetComponent<Renderer>().material = Material1;
                }

                if (SatsumaOcs.knobWipers != null && SatsumaOcs.triggerButtonWiper != null)
                {
                    if (hit.collider.gameObject == SatsumaOcs.triggerButtonWiper)
                    {
                        if (SelectionSelectionSettings.GetValue())
                        {
                            SatsumaOcs.knobWipers.GetComponent<Renderer>().material = SelMaterial;
                            FsmVariables.GlobalVariables.GetFsmString("GUIinteraction").Value = "";
                            FsmVariables.GlobalVariables.GetFsmBool("GUIuse").Value = false;
                        }

                        break;
                    }

                    SatsumaOcs.knobWipers.GetComponent<Renderer>().material = Material1;
                }
            }

            // If we click
            if (Input.GetMouseButtonDown(0))
            {
                {
                    foreach (RaycastHit hit in Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition),
                                 3f))
                    {
                        if (SatsumaOcs.radioCoreInstance != null)
                        {
                            switch (hit.collider.gameObject.name)
                            {

                                // Aiming at the power knob
                                // Toggle radio
                                case "trigger_ocs_power1" when SatsumaOcs.radioCoreInstance.@on:
                                    SatsumaOcs.radioCoreInstance.DisableRadio();
                                    break;

                                case "trigger_ocs_power1":
                                    SatsumaOcs.radioCoreInstance.EnableRadio();
                                    break;

                                // Aiming at the switch song knob
                                // Change song
                                case "trigger_ocs_switch1":
                                    SatsumaOcs.radioCoreInstance.NextClip();
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Write to logs.
        ///     Note that if you have the NoLog version of OldCarSounds, it will not do anything.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="module">Where the message is coming from. By default it's SYSTEM.</param>
        /// <param name="console">
        ///     If it should be displayed in the console even if it's not
        ///     debugging mode.
        /// </param>
        /// <exception cref="IOException">Cannot write to logs.</exception>
        public static void PrintF(string text, string module = "SYSTEM", bool console = false)
        {
#if !DISABLE_LOG
            try
            {
                string modConfigFolder =
                    ModLoader.GetModSettingsFolder(ModLoader.LoadedMods.First(x => x.ID == nameof(OldCarSounds)));
                StreamWriter writer = new StreamWriter(Path.Combine(modConfigFolder, "log.log"), true);
                StringBuilder builder = new StringBuilder().Append(DateTime.Now).Append(" [").Append(module.ToUpper())
                    .Append("]: ").Append(text);
                writer.WriteLine(builder.ToString());
                writer.Close();

                if (console)
                {
                    switch (module.ToUpper())
                    {
                        case "ERROR":
                        case "ERR":
                            ModConsole.Error(builder.ToString());
                            break;

                        case "WARN":
                        case "WARNING":
                            ModConsole.Warning(builder.ToString());
                            break;

                        default:
                            ModConsole.Print(builder.ToString());
                            break;
                    }
                }
#if DEBUG
                else {
                    switch (module.ToUpper()) {
                        case "ERROR":
                        case "ERR":
                            ModConsole.Error(builder.ToString());
                            break;

                        case "WARN":
                        case "WARNING":
                            ModConsole.Warning(builder.ToString());
                            break;

                        default:
                            ModConsole.Print(builder.ToString());
                            break;
                    }
                }
#endif
            }
            catch (Exception) { }
#endif
        }

        public static string GameObjectPath(GameObject go)
        {
            string s = "";
            GameObject temp1 = go;
            while (true)
            {
                s = temp1.name + "/" + s;
                if (temp1.transform.parent == null)
                {
                    break;
                }

                temp1 = temp1.transform.parent.gameObject;
            }

            return s;
        }

        #region VARIABLES

        public static SettingsCheckBox AssembleSounds;
        public static SettingsCheckBox OldDashTexturesSettings;
        public static SettingsCheckBox InfoTextSettings;
        public static SettingsCheckBox OldRadioSongsSettings;
        public static SettingsCheckBox DisableKnobSoundsSettings;
        public static SettingsCheckBox DisableDoorSoundsSettings;
        public static SettingsCheckBox DisableFootSoundsSettings;
        public static SettingsCheckBox OldRpmGaugeSettings;
        public static SettingsSliderInt ShiftDelaySelectionSettings;
        public static SettingsSliderInt KeySoundSelectionSettings;
        public static SettingsCheckBox SelectionSelectionSettings;
        public static SettingsSliderInt EngineSoundsTypeSettings;
        public static SettingsCheckBox OldDelaySettings;

        public static SettingsCheckBox OldRadioEnabled;
        public static SettingsCheckBox OldRadioSongsEnabled;
        public static SettingsCheckBox NewRadioSongsEnabled;

        public static AudioClip Clip1;

        public static AudioClip Clip2;

        public static AudioClip Clip3;

        public static GameObject Radio1, Satsuma;

        public static Material Material1, SelMaterial;

        public static bool LoadGameOnMenu;

        private Stopwatch _stopwatch;
        private Material _noSel;

        #endregion VARIABLES
    }
}