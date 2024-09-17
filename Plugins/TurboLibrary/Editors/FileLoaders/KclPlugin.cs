using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using MapStudio.UI;
using Toolbox.Core;
using System.IO;
using KclLibrary;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;
using TurboLibrary.CollisionEditor;
using ImGuiNET;
using UIFramework;

namespace TurboLibrary
{
    public class KclPlugin : FileEditor, IFileFormat
    {
        public string[] Description => new string[] { "WiiU/Switch Collision" };
        public string[] Extension =>  new string[] { "*.kcl", "*.szs" };

        /// <summary>
        /// Wether or not the file can be saved.
        /// </summary>
        public bool CanSave { get; set; } = true;

        /// <summary>
        /// Information of the loaded file.
        /// </summary>
        public File_Info FileInfo { get; set; }

        public bool Identify(File_Info fileInfo, Stream stream)
        {
            using (var reader = new Toolbox.Core.IO.FileReader(stream, true))
            {
                reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
                return reader.ReadUInt32() == 0x02020000 || fileInfo.Extension == ".kcl";
            }
        }

        public bool IsBigEndian => KclFile.ByteOrder == Syroot.BinaryData.ByteOrder.BigEndian;

        public KCLFile KclFile { get; set; }

        /// <summary>
        /// The collision debug renerer.
        /// </summary>
        public CollisionRender CollisionRender = null;

        CollisionPainterUI CollisionPainter;
        public KclPlugin() { FileInfo = new File_Info() { FileName = "course.kcl" }; }

        public bool UpdateTransformedVertices = false;

        public override bool CreateNew(string menu_name)
        {
            Root.Header = "course.kcl";
            Root.Tag = this;

            FileInfo.FileName = "course.kcl";

            bool bigEndian = menu_name.Contains("WiiU");

            //Empty file
            KclFile = new KCLFile(new List<Triangle>(), FileVersion.Version2, bigEndian);
            UpdateCollisionFile(KclFile);

            return true;
        }

        public void SetAsSwitchKcl()
        {
            KclFile.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
            Root.Header = "course_kcl.szs";

            FileInfo.FileName = "course_kcl.szs";
            FileInfo.Compression = new Yaz0();

            Root.ContextMenus[2].IsChecked = false;
        }

        public void Load(Stream stream) {
            KclFile = new KCLFile(stream);
            UpdateCollisionFile(KclFile);

            CollisionRender.CanSelect = true;
        }

        public void Save(Stream stream) {

            if (UpdateTransformedVertices)
            {
                var t = CollisionRender.Transform.TransformMatrix;
                var matrix = new Matrix4x4(
                    t.M11, t.M12, t.M13, t.M14,
                    t.M21, t.M22, t.M23, t.M24,
                    t.M31, t.M32, t.M33, t.M34,
                    t.M41, t.M42, t.M43, t.M44);

                //Turn into obj, edit then import back
                var obj = KclFile.CreateGenericModel();
                foreach (var mesh in obj.Scene.Models[0].Meshes)
                {
                    foreach (var vertex in mesh.Vertices)
                        vertex.Transform(matrix);
                }
                KclFile = new KCLFile(obj.ToTriangles(), KclFile.Version, IsBigEndian);
                UpdateCollisionFile(KclFile);

                CollisionRender.Transform.TransformMatrix = OpenTK.Matrix4.Identity;

                UpdateTransformedVertices = false;
            }

            KclFile.Save(stream);
        }

        public void UpdateCollisionFile(KCLFile collision)
        {
            KclFile = collision;

            Scene.Init();
            Scene.Objects.Clear();

            //Setup tree node
            Root.Header = FileInfo.FileName;
            Root.Tag = this;
            Root.ContextMenus.Clear();
            Root.ContextMenus.Add(new MenuItemModel("Export", ExportCollision));
            Root.ContextMenus.Add(new MenuItemModel("Replace", ImportCollision));
            Root.ContextMenus.Add(new MenuItemModel("Is Big Endian", () =>
            {
                //Update bom
                if (IsBigEndian)
                    KclFile.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                else
                    KclFile.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;

                Root.ContextMenus[2].IsChecked = IsBigEndian;

            }, "", IsBigEndian));

            Console.WriteLine("Loading collision render");

            //Add or update the existing renderer of the collision
            if (CollisionRender == null) {
                CollisionRender = new CollisionRender(collision);
                this.AddRender(CollisionRender);
            }
            else
                CollisionRender.Reload(collision);

            Console.WriteLine("Loading collision tree");

            //Prepare displayed children in tree
            ReloadTree();

            Scene.Objects.Add(CollisionRender);

            OpenTK.Vector3 ToVec3(System.Numerics.Vector3 v) {
                return new OpenTK.Vector3((float)v.X, (float)v.Y, (float)v.Z);
            }

            Console.WriteLine("Loading collision ray caster");

            //Generate a ray caster to automate collision detection from moving objects
            CollisionRayCaster collisionRayCaster = new CollisionRayCaster();
            foreach (var model in KclFile.Models)
            {
                foreach (var prism in model.Prisms)
                {
                    var tri = model.GetTriangle(prism);
                    collisionRayCaster.AddTri(
                        ToVec3(tri.Vertices[2]) * GLContext.PreviewScale,
                        ToVec3(tri.Vertices[1]) * GLContext.PreviewScale,
                        ToVec3(tri.Vertices[0]) * GLContext.PreviewScale);
                }
            }
            collisionRayCaster.UpdateCache();
            GLContext.ActiveContext.CollisionCaster = collisionRayCaster;

            Console.WriteLine("finished collision");
        }

        CollisionPresetData Preset;
        string CurrentPreset = "";

        private void ReloadTree()
        {
            Dictionary<ushort, KclPrism> materials = new Dictionary<ushort, KclPrism>();
            foreach (var model in KclFile.Models)
            {
                foreach (var prism in model.Prisms)
                {
                    if (!materials.ContainsKey(prism.CollisionFlags))
                        materials.Add(prism.CollisionFlags, prism);
                }
            }

            Root.Children.Clear();
            foreach (var mesh in CollisionRender.Meshes)
                Root.AddChild(mesh.UINode);
        }

        public override List<DockWindow> PrepareDocks()
        {
            var docks = base.PrepareDocks();
         //   if (CollisionPainter == null)
            //    CollisionPainter = new CollisionPainterUI(this.Workspace);
           // docks.Add(CollisionPainter);
            return docks;
        }

        public class PrismProperties
        {
            [BindGUI("ID")]
            public int ID { get; set; }

            [BindGUI("Type")]
            public string Type { get; set; }

            [BindGUI("Material")]
            public string Material { get; set; }

            [BindGUI("Sound File")]
            public string SoundFile { get; set; }

            [BindGUI("SpecialFlag")]
            public string SpecialFlag { get; set; }

            NodeBase MatNode;

            public PrismProperties(NodeBase node, ushort id) {
                ID = id;
                MatNode = node;

                int specialFlag = (id >> 8);
                int attributeMaterial = (id & 0xFF);
                int materialIndex = attributeMaterial / 0x20;
                int attributeID = attributeMaterial - (materialIndex * 0x20);
                Type = CollisionCalculator.AttributeList[attributeID];
                Material = CollisionCalculator.AttributeMaterials[attributeID][materialIndex];
                SoundFile = CollisionCalculator.AttributeMaterialSounds[attributeID][materialIndex];
                SpecialFlag = CollisionCalculator.SpecialType[0];

                if (specialFlag == 0x10) SpecialFlag = CollisionCalculator.SpecialType[1];
                if (specialFlag == 0x20) SpecialFlag = CollisionCalculator.SpecialType[2];
                if (specialFlag == 0x40) SpecialFlag = CollisionCalculator.SpecialType[3];
                if (specialFlag == 0x50) SpecialFlag = CollisionCalculator.SpecialType[4];
                if (specialFlag == 0x80) SpecialFlag = CollisionCalculator.SpecialType[5];
            }
        }

        public override List<MenuItemModel> GetEditMenuItems()
        {
            var items = new List<MenuItemModel>();
            items.Add(new MenuItemModel($"        Import Collision", ImportCollision));
            return items;
        }

        public override bool OnFileDrop(string filePath)
        {
            if (filePath.EndsWith(".obj")) {
                ImportCollision(filePath);
                return true;
            }
            return false;
        }

        public void ExportCollision()
        {
            ImguiFileDialog sfd = new ImguiFileDialog();
            sfd.FileName = System.IO.Path.GetFileNameWithoutExtension(FileInfo.FileName);
            sfd.SaveDialog = true;
            sfd.AddFilter(".obj", "Object File");

            if (sfd.ShowDialog()) {
                KclFile.CreateGenericModel().Save(sfd.FilePath);
            }
        }


        public void ImportCollision()
        {
            ImguiFileDialog ofd = new ImguiFileDialog();
            ofd.AddFilter(".obj", "Object File");
            ofd.AddFilter(".dae", "Collada File");
            ofd.AddFilter(".fbx", "Fbx File");

            if (ofd.ShowDialog()) {
                ImportCollision(ofd.FilePath);
            }
        }

        public void ImportCollision(string filePath)
        {
            var importer = PrepareDialog();
            importer.OpenObjectFile(filePath);
        }

        public void ImportCollision(IONET.Core.IOScene scene)
        {
            var importer = PrepareDialog();
            importer.OpenObjectFile(scene);
        }

        private CollisionImporter PrepareDialog()
        {
            var importer = new CollisionImporter(this.IsBigEndian);

            DialogHandler.Show("Collision Importer", 600, 500, () =>
            {
                importer.Render();
            }, (e) =>
            {
                if (e)
                {
                    //Add a collision file if none is present
                    Workspace.ActiveWorkspace.Resources.AddFile(this);

                    this.UpdateCollisionFile(importer.GetCollisionFile());
                    this.CanSave = true;
                }
            });
            return importer;
        }
    }
}
