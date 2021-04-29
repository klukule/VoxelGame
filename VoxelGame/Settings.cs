using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VoxelGame
{
    public class Settings
    {
        public static string SaveLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VoxelGame", "Settings.json");

        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public float RefreshRate { get; set; }
        public int BitsPerPixel { get; set; }
        public float FieldOfView { get; set; }
        public bool Fullscreen { get; set; }

        public InputSettings Input { get; set; }

        public void ApplyWindowSettings(DisplayResolution res, bool fullscreen)
        {
            Fullscreen = fullscreen;
            WindowWidth = res.Width;
            WindowHeight = res.Height;
            RefreshRate = res.RefreshRate;
            BitsPerPixel = res.BitsPerPixel;

            Program.Window.ClientSize = new System.Drawing.Size(WindowWidth, WindowHeight);
            Program.Window.ClientRectangle = new System.Drawing.Rectangle(0, 0, WindowWidth, WindowHeight);

            if (Fullscreen)
                DisplayDevice.GetDisplay(DisplayIndex.Default).ChangeResolution(res);
            else
                DisplayDevice.GetDisplay(DisplayIndex.Default).RestoreResolution();

            Program.Window.WindowState = Fullscreen ? OpenTK.WindowState.Fullscreen : OpenTK.WindowState.Normal;

            Save();
        }

        public void UpdateAll()
        {
            Program.Window.WindowState = Fullscreen ? OpenTK.WindowState.Fullscreen : OpenTK.WindowState.Normal;
            Program.Window.ClientSize = new System.Drawing.Size(WindowWidth, WindowHeight);
            Program.Window.ClientRectangle = new System.Drawing.Rectangle(0, 0, WindowWidth, WindowHeight);

            if (Fullscreen)
                DisplayDevice.GetDisplay(DisplayIndex.Default).ChangeResolution(DisplayDevice.GetDisplay(DisplayIndex.Default)
                    .SelectResolution(WindowWidth, WindowHeight, BitsPerPixel, RefreshRate));
            else
                DisplayDevice.GetDisplay(DisplayIndex.Default).RestoreResolution();


        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            if (!Directory.Exists(Path.GetDirectoryName(SaveLocation)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SaveLocation));
            }

            File.WriteAllText(SaveLocation, json);
        }

        public static Settings Load()
        {
            if (File.Exists(SaveLocation))
            {
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SaveLocation));
            }

            return new Settings()
            {
                WindowWidth = DisplayDevice.GetDisplay(DisplayIndex.Default).Width,
                WindowHeight = DisplayDevice.GetDisplay(DisplayIndex.Default).Height,
                RefreshRate = DisplayDevice.GetDisplay(DisplayIndex.Default).RefreshRate,
                FieldOfView = 75,
                Input = new InputSettings()
            };
        }
    }
}
