using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GLFrameworkEngine;
using KclLibrary;
using Toolbox.Core.ViewModels;
using MapStudio.UI;
using System.IO;

namespace TurboLibrary
{
    public class CollisionRender : ITransformableObject, IColorPickable, IDrawable
    {
        public GLTransform Transform { get; set; } = new GLTransform();

        public List<CollisionMesh> Meshes = new List<CollisionMesh>();

        public bool IsHovered { get; set; }
        public bool IsSelected { get; set; }
        public bool CanSelect { get; set; } = false;

        public static bool Overlay = false;
        public static bool DisplayCollision = false;

        KCLFile KCLFile;

        public bool EditMode = false;
        bool IsDisposed = false;

        private bool isVisible = true;
        public bool IsVisible
        {
            get
            {
                return Overlay || DisplayCollision || isVisible;
            }
            set
            {
                isVisible = value;
            }
        }

        public IEnumerable<ITransformableObject> Selectables => this.Meshes;

        public static Dictionary<int, Vector4> AttributeColors = new Dictionary<int, Vector4>()
        {
            { 0, new Vector4(0.5f, 0.5f, 0.5f, 1) }, //road
            { 1, new Vector4(0.55f, 0.55f, 0.55f, 1) }, //road 
            { 2, new Vector4(0.4f, 0.4f, 0.4f, 1) }, //road bumpy
            { 3, new Vector4(0.45f, 0.45f, 0.45f, 1) }, //road 
            { 4, new Vector4(1, 0.8f, 0.2f, 1) }, //road sand
            { 5, new Vector4(1, 0.76f, 0.2f, 1) }, //road sand offroad
            { 6, new Vector4(1, 0.56f, 0.1f, 1) }, //sand
            { 7, new Vector4(1, 0.36f, 0.3f, 1) }, //sticky sound
            { 8, new Vector4(0.35f, 0.35f, 0.30f, 1) }, //offroad
            { 9, new Vector4(0.25f, 0.61f, 1f, 1) }, //road ice particles
            { 10, new Vector4(1, 0.5f, 0, 1) }, //booster
            { 11, new Vector4(1, 0, 1, 1) }, //anti g
            { 12, new Vector4(0.55f, 0.55f, 0.55f, 1) }, //road 
            { 13, new Vector4(0.2f, 0.2f, 1, 1) }, //road wet
            { 14, new Vector4(0.55f, 0.55f, 0.55f, 1) }, //road 
            { 15, new Vector4(0.95f, 0.95f, 0.95f, 1) }, //road semi solid
            { 16, new Vector4(1, 0, 0, 1) }, //latiku boundary
            { 17, new Vector4(1, 1, 1, 1) }, //Wall invisible
            { 18, new Vector4(0.6f, 0, 0, 1) }, //Water wave
            { 19, new Vector4(0.5f, 0.5f, 0.5f, 1) }, //wall
            { 23, new Vector4(0.5f, 0.5f, 0.5f, 1) }, //wall
            { 28, new Vector4(1, 0, 0, 1) }, //latiku boundary 2    
            { 31, new Vector4(1, 1, 0, 1) }, //glider
            { 32, new Vector4(0.85f, 0.95f, 0.95f, 1) }, //road roamy sound
            { 40, new Vector4(0.45f, 0.45f, 0.45f, 1) }, //road 
            { 56, new Vector4(0, 0, 0, 0.5f) }, //unsolid 
            { 60, new Vector4(0, 0, 1, 1) }, //water 
            { 61, new Vector4(0, 0, 1, 1) }, //water 
            { 62, new Vector4(0, 0, 1, 1) }, //water 
            { 63, new Vector4(0.25f, 0.25f, 0, 1) }, //unsolid offroad
            { 64, new Vector4(0.45f, 0.45f, 0.45f, 1) }, //road (stone)
            { 65, new Vector4(0.45f, 0.40f, 0.05f, 1) }, //road (wood)
            { 66, new Vector4(0.65f, 0.65f, 0.65f, 1) }, //road (iron w particles)
            { 67, new Vector4(0.45f, 0.45f, 0.45f, 1) }, //road
            { 68, new Vector4(0.65f, 0.65f, 0.65f, 1) }, //road (iron)
            { 69, new Vector4(0.55f, 0.55f, 0, 1) }, //offroad iron
            { 70, new Vector4(0.85f, 0.85f, 0.85f, 1) }, //offroad snow
            { 81, new Vector4(0.6f, 0.6f, 0.6f, 1) }, //metal wall
            { 92, new Vector4(1, 0.2f, 0, 1) }, //lava
            { 134, new Vector4(0, 1, 0, 1) }, //grass
            { 227, new Vector4(0.95f, 0, 1, 1) }, //snes rr road
            { 4096, new Vector4(0.62f, 1, 0, 1) }, //stunt
            { 4106, new Vector4(1, 0.8f, 0, 1) }, //boost + stunt
            { 4108, new Vector4(1, 0.95f, 0, 1) }, //stunt + glider
        };

        public CollisionRender(KCLFile kclFile) 
        {
            Init(kclFile);
        }

        private void Init(KCLFile kclFile)
        {
            KCLFile = kclFile;

            CanSelect = false;

            Meshes.Clear();

            Dictionary<ushort, List<Triangle>> meshDiv = new Dictionary<ushort, List<Triangle>>();
            foreach (var model in kclFile.Models)
            {
                foreach (var prism in model.Prisms)
                {
                    var tri = model.GetTriangle(prism);
                    if (!meshDiv.ContainsKey(prism.CollisionFlags))
                        meshDiv.Add(prism.CollisionFlags, new List<Triangle>());

                    meshDiv [prism.CollisionFlags].Add(tri);
                }
            }

            foreach (var mesh in meshDiv)
                Meshes.Add(new CollisionMesh(mesh.Key, mesh.Value));
        }

        public void Reload(KCLFile kclFile)
        {
            Init(kclFile);
        }

        public void DrawColorPicking(GLContext context)
        {
            if (!IsVisible || IsDisposed || !CanSelect)
                return;

            context.CurrentShader = GlobalShaders.GetShader("PICKING");
            foreach (var mesh in Meshes)
            {
                if (EditMode)
                    context.ColorPicker.SetPickingColorFaces(mesh.Faces.Cast<ITransformableObject>().ToList(), context.CurrentShader);
                else
                    context.ColorPicker.SetPickingColor(mesh, context.CurrentShader);

                GL.Disable(EnableCap.CullFace);
                mesh.Draw(context);
                GL.Enable(EnableCap.CullFace);
            }
        }

        public void DrawModel(GLContext context, Pass pass)
        {
            if (!IsVisible || IsDisposed)
                return;

            foreach (var mesh in Meshes)
            {
                if (EditMode && mesh.Faces.Any(x => x.IsHovered) && pass == Pass.OPAQUE)
                    mesh.UpdateIndexBuffer();
            }

            foreach (var mesh in Meshes)
            {
                if (!mesh.IsVisible)
                    continue;

                if (Overlay)
                    mesh.DrawOverlayCollision(context, pass);
                else
                    mesh.DrawSolidCollision(context, pass);
            }
        }

        public void Dispose()
        {
            foreach (var mesh in Meshes)
                mesh.Dispose();
        }
    }
    
    public class CollisionMesh : RenderMesh<CollisionVertex>, ITransformableObject, IRenderNode
    {
        public GLTransform Transform { get; set; } = new GLTransform();

        public bool IsHovered { get; set; }

        public bool IsSelected
        {
            get { return UINode.IsSelected; }
            set { UINode.IsSelected = value; }
        }

        public bool IsVisible
        {
            get { return UINode.IsChecked; }
            set { UINode.IsChecked = value; }
        }

        public bool CanSelect { get; set; } = true;
        public NodeBase UINode { get; set; }

        public CollisionFace[] Faces;

        StandardMaterial SolidMaterial = new StandardMaterial();
        StandardMaterial OverlayMaterial = new StandardMaterial();
        PatternMaterial PatternMaterial = new PatternMaterial();

        const float TransparencyOverlay = 0.3f;

        public BufferObject IndexBuffer;
        public BufferObject SelectionBuffer;

        private bool usePatternShading = false;

        public CollisionMesh(ushort att, List<Triangle> tris) : base(GetVertices(tris), PrimitiveType.Triangles)
        {
            UINode = new NodeBase(att.ToString("X4")) { HasCheckBox = true };
            UINode.Tag = new KclPlugin.PrismProperties(UINode, att);
            UINode.Icon = IconManager.MESH_ICON.ToString();
            UINode.Header += $" {((KclPlugin.PrismProperties)UINode.Tag).Type}";

            LoadAttributeIcon();

            //Index buffer and a seperate selection buffer for selected indices to redraw a selection material
            IndexBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
            SelectionBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);

            Faces = new CollisionFace[tris.Count];
            for (int i = 0; i < Faces.Length; i++)
                Faces[i] = new CollisionFace();

            UpdateIndexBuffer();
        }

        private void LoadAttributeIcon()
        {
            var info = UINode.Tag as KclPlugin.PrismProperties;
            //Check if there is a valid attribute set and icon not been loaded
            if (!string.IsNullOrEmpty(info.Type) && !IconManager.HasIcon(info.Type))
            {
                //File path on disk to the icon
                string icon = $"{Toolbox.Core.Runtime.ExecutableDir}\\Lib\\Images\\Collision\\{info.Type}.png";
                if (File.Exists(icon)) //Load the icon if found
                    IconManager.TryAddIcon(System.IO.Path.GetFileNameWithoutExtension(info.Type), File.ReadAllBytes(icon));
            }
            //Set the icon to the tree node
            if (IconManager.HasIcon(info.Type))
                UINode.Icon = info.Type;
        }

        //Draw a normal collision view with half lambert shading
        public void DrawSolidCollision(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE)
                return;

            //A square pattern
            if (usePatternShading)
                PatternMaterial.Render(context, this.Transform);
            else //Standard solid shading with lambert shading.
            {
                //Gray default color
                SolidMaterial.Color = new Vector4(0.7f, 0.7f, 0.7f, 1);
                SolidMaterial.ModelMatrix = this.Transform.TransformMatrix;
                SolidMaterial.HalfLambertShading = true;
                SolidMaterial.hasVertexColors = true;
                SolidMaterial.Render(context);
            }

            GLMaterialBlendState.TranslucentAlphaOne.RenderBlendState();

            GL.Disable(EnableCap.CullFace);

            if (DebugShaderRender.DebugRendering != DebugShaderRender.DebugRender.Default)
                DrawDebugShading(context, Transform.TransformMatrix, this.IsSelected);
            else
                this.DrawWithSelection(context, IsSelected);

            //DrawLineWireframe(context);

            SolidMaterial.Color = new Vector4(1, 0, 0, 1);
            //  this.DrawSelection(context);

            GLMaterialBlendState.Opaque.RenderBlendState();

            GL.Enable(EnableCap.CullFace);
        }

        //Draw using a translucent overlay
        public void DrawOverlayCollision(GLContext context, Pass pass)
        {
            if (pass != Pass.TRANSPARENT)
                return;

            OverlayMaterial.Color = new Vector4(1, 1, 1, TransparencyOverlay);
            OverlayMaterial.ModelMatrix = this.Transform.TransformMatrix;
            OverlayMaterial.hasVertexColors = true;

            GLMaterialBlendState.TranslucentAlphaOne.RenderBlendState();

            //Draw filled faces
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(-5f, 1f);

            OverlayMaterial.Render(context);
            this.DrawDefault(context);

            OverlayMaterial.Color = new Vector4(1, 0, 0, TransparencyOverlay);
            OverlayMaterial.Render(context);
            this.DrawSelection(context);

            //Draw lines
            OverlayMaterial.Color = new Vector4(0, 0, 0, 1);

            GL.LineWidth(1);
            GL.Enable(EnableCap.PolygonOffsetLine);
            GL.PolygonOffset(-10f, 1f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            OverlayMaterial.Render(context);
            this.DrawDefault(context);

            //Reset
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.PolygonOffsetLine);
            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(0f, 0f);
            GL.Enable(EnableCap.CullFace);

            GLMaterialBlendState.Opaque.RenderBlendState();
        }

        private void DrawLineWireframe(GLContext context)
        {
            GL.LineWidth(1);
            GL.Enable(EnableCap.PolygonOffsetLine);
            GL.PolygonOffset(-0.08f, 1f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            //Gray default color
            SolidMaterial.Color = new Vector4(1, 1, 1, 1);
            SolidMaterial.Render(context);

            this.Draw(context);

            //Reset
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.PolygonOffsetLine);
            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(0f, 0f);
            GL.Enable(EnableCap.CullFace);
        }

        //Draw unselected
        public void DrawDefault(GLContext context)
        {
            PrepareAttributes(context.CurrentShader);
            BindVAO();

            IndexBuffer.Bind();
            GL.DrawElements(BeginMode.Triangles, IndexBuffer.DataCount, DrawElementsType.UnsignedInt, 0);
        }

        //Draw Selected
        public void DrawSelection(GLContext context)
        {
            if (SelectionBuffer.DataCount == 0)
                return;

            PrepareAttributes(context.CurrentShader);
            BindVAO();

            SelectionBuffer.Bind();
            GL.DrawElements(BeginMode.Triangles, SelectionBuffer.DataCount, DrawElementsType.UnsignedInt, 0);
        }

        public void UpdateIndexBuffer()
        {
            List<int> indexBuffer = new List<int>();
            List<int> selIndexBuffer = new List<int>();

            //Create a normal index buffer and selection buffer
            int vertexIndex = 0;
            for (int i = 0; i < Faces.Length; i++)
            {
                if (Faces[i].IsSelected || Faces[i].IsHovered)
                {
                    selIndexBuffer.Add(vertexIndex++);
                    selIndexBuffer.Add(vertexIndex++);
                    selIndexBuffer.Add(vertexIndex++);
                }
                else
                {
                    indexBuffer.Add(vertexIndex++);
                    indexBuffer.Add(vertexIndex++);
                    indexBuffer.Add(vertexIndex++);
                }
            }
            IndexBuffer.SetData(indexBuffer.ToArray(), BufferUsageHint.StaticDraw);
            SelectionBuffer.SetData(selIndexBuffer.ToArray(), BufferUsageHint.StaticDraw);
        }

        static CollisionVertex[] GetVertices(List<Triangle> triangles)
        {
            List<CollisionVertex> vertices = new List<CollisionVertex>();
            for (int i = 0; i < triangles.Count; i++)
            {
                var triangle = triangles[i];
                var id = triangle.Attribute;
                var color = Vector4.One;

                if (CollisionRender.AttributeColors.ContainsKey(id))
                    color = CollisionRender.AttributeColors[id];

                int specialFlag = (id >> 8);
                int attributeMaterial = (id & 0xFF);
                int materialIndex = attributeMaterial / 0x20;
                int attributeID = attributeMaterial - (materialIndex * 0x20);

                if (CollisionCalculator.AttributeColors.Count > attributeID && CollisionCalculator.AttributeList.Length > attributeID)
                {
                    var attribute = CollisionCalculator.AttributeList[attributeID];
                    if (CollisionCalculator.AttributeColors.ContainsKey(attribute))
                        color = CollisionCalculator.AttributeColors[attribute];
                }
                if (CollisionCalculator.AttributeMatColors.Count > attributeID && materialIndex < 8)
                {
                    var material = CollisionCalculator.AttributeMaterials[attributeID][materialIndex];
                    if (CollisionCalculator.AttributeMatColors.ContainsKey(material))
                        color = CollisionCalculator.AttributeMatColors[material];
                }

                for (int j = 0; j < 3; j++)
                {
                    vertices.Add(new CollisionVertex()
                    {
                        Position = new Vector3(
                            triangle.Vertices[j].X,
                            triangle.Vertices[j].Y,
                            triangle.Vertices[j].Z),
                        Normal = new Vector3(
                            triangle.Normal.X,
                            triangle.Normal.Y,
                            triangle.Normal.Z),
                        Color = color,
                    });
                }
            }
            return vertices.ToArray();
        }
    }

    public struct CollisionFace : ITransformableObject
    {
        public int Index;

        public GLTransform Transform { get; set; }

        public bool IsHovered { get; set; }

        public bool IsSelected { get; set; }

      public  bool CanSelect { get; set; }
    }

    public struct CollisionVertex
    {
        [RenderAttribute(GLConstants.VPosition, VertexAttribPointerType.Float, 0)]
        public Vector3 Position;

        [RenderAttribute(GLConstants.VNormal, VertexAttribPointerType.Float, 12)]
        public Vector3 Normal;

        [RenderAttribute(GLConstants.VColor, VertexAttribPointerType.Float, 24)]
        public Vector4 Color;

        public CollisionVertex(Vector3 position, Vector3 normal, Vector4 color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }
    }

    class PatternMaterial
    {
        public void Render(GLContext context, GLTransform transform)
        {
            var shader = GlobalShaders.GetShader("KCL");
            context.CurrentShader = shader;
            shader.SetTransform(GLConstants.ModelMatrix, transform);
        }
    }
}
