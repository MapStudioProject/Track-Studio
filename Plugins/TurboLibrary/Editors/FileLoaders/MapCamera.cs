using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using OpenTK;
using Toolbox.Core.IO;
using GLFrameworkEngine;
using OpenTK.Graphics.OpenGL;
using MapStudio.UI;

namespace TurboLibrary.MuuntEditor
{
    public class MapCamera 
    {
        public MinimapEditor Editor = new MinimapEditor();

        public Vector3 Position { get; set; }
        public Vector3 LookAtPosition { get; set; }
        public Vector3 UpAxis { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public byte Unknown { get; set; }

        public bool IsBigEndian = true;

        public MapCamera()
        {
            LookAtPosition = new Vector3(0, 0, -0.017f);
            Position = new Vector3(0, 15000, 0);
            UpAxis = new Vector3(0, 1, 0);
            Width = 6000;
            Height = 6000;
            Unknown = 10;
        }

        public MapCamera(string filePath, bool isWiiU = true)
        {
            Load(File.OpenRead(filePath), isWiiU);
        }

        public MapCamera(Stream stream, bool isWiiU = true)
        {
            Load(stream, isWiiU);
        }

        private void Load(Stream stream, bool isWiiU = true)
        {
            IsBigEndian = isWiiU;
            using (var reader = new FileReader(stream)) {
                reader.SetByteOrder(isWiiU);
                Position = reader.ReadVec3();
                LookAtPosition = reader.ReadVec3();
                UpAxis = reader.ReadVec3();
                Width = reader.ReadSingle();
                Height = reader.ReadSingle();
                Unknown = reader.ReadByte(); //Unused?
            }
        }

        public void Save(string filePath)
        {
            Save(File.OpenWrite(filePath));
        }

        public void Save(Stream stream)
        {
            using (var writer = new FileWriter(stream))
            {
                writer.SetByteOrder(IsBigEndian);
                writer.Write(Position);
                writer.Write(LookAtPosition);
                writer.Write(UpAxis);
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Unknown);
            }
        }

        public Matrix4 ConstructViewMatrix() {
            return Matrix4.LookAt(Position, LookAtPosition, UpAxis);
        }

        public Matrix4 ConstructOrthoMatrix() {
            return Matrix4.CreateOrthographic(Width, Height, -100000, 100000);
        }

    }
}
