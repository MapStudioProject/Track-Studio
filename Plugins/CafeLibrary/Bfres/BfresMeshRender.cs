using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;
using Toolbox.Core.ViewModels;

namespace CafeLibrary.Rendering
{
    public class BfresMeshRender : GenericPickableMesh, ITransformableObject, IColorPickable, IRenderNode, IDragDropPicking
    {
        public GLTransform Transform { get; set; } = new GLTransform();

        public EventHandler OnRemoved;

        public VertexBufferObject vao;
        //For custom shaders
        public VertexBufferObject customvao;

        public List<BfresPolygonGroupRender> LODMeshes = new List<BfresPolygonGroupRender>();

        public static Func<BoundingNode, bool> DisplaySubMeshFunc;

        public bool IsVisible { get; set; } = true;
        public int VertexSkinCount { get; set; }
        public int BoneIndex { get; set; }
        public bool IsSealPass { get; set; }

        public bool IsDepthShadow { get; set; }
        public bool IsCubeMap { get; set; }
        public bool RenderInCubeMap { get; set; }
        public bool IsHovered { get; set; }
        public bool UseColorBufferPass { get; set; }

        public bool ProjectDynamicShadowMap { get; set; }
        public bool ProjectStaticShadowMap { get; set; }

        public int Priority { get; set; } = 0;

        public static bool DISPLAY_SUB_MESH = true;

        public int Index { get; private set; }

        public bool CanSelect { get; set; } = true;

        public NodeBase UINode { get; set; } = new NodeBase();

        public bool IsMaterialSelected = false;

        public int ForceLevelDetailIndex = -1;

        public bool IsSelected
        {
            get { return UINode.IsSelected || IsMaterialSelected; }
            set { UINode.IsSelected = value; }
        }

        List<BfresGLLoader.VaoAttribute> Attributes;

        public BfresMeshRender(int index) { Index = index; }

        public EventHandler OnDragDroppedOnLeave;
        public EventHandler OnDragDroppedOnEnter;
        public EventHandler OnDragDropped;

        public void DragDroppedOnLeave() {
            OnDragDroppedOnLeave?.Invoke(this, EventArgs.Empty);
        }

        public void DragDroppedOnEnter() {
            OnDragDroppedOnEnter?.Invoke(this, EventArgs.Empty);
        }

        public void DragDropped(object droppedItem) {
            OnDragDropped?.Invoke(droppedItem, EventArgs.Empty);
        }

        public void DrawColorPicking(GLContext control) { }

        public void DrawWithPolygonOffset(ShaderProgram shader, int displayLOD = 0)
        {
            //Seal objects draw ontop of meshes so offset them
            if (IsSealPass)
            {
                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(-1, 1f);
            }
            else
            {
                GL.Disable(EnableCap.PolygonOffsetFill);
            }

            Draw(shader, displayLOD);

            GL.Disable(EnableCap.PolygonOffsetFill);
        }

        public int GetDisplayLevel(GLContext context, BfresRender render)
        {
            var pos = GetClosestPosition(context);
            //forced LOD display (for UI purposes)
            if (ForceLevelDetailIndex != -1 && LODMeshes.Count > ForceLevelDetailIndex)
                return ForceLevelDetailIndex;

            if (!context.Camera.InRange(pos, BfresRender.LOD_LEVEL_1_DISTANCE) && LODMeshes.Count > 1)
                return 1;
            if (!context.Camera.InRange(pos, BfresRender.LOD_LEVEL_2_DISTANCE) && LODMeshes.Count > 2)
                return 2;
            return 0;
        }

        private OpenTK.Vector3 GetClosestPosition(GLContext context)
        {
            OpenTK.Vector3 pos = OpenTK.Vector3.Zero;
            float closestDist = float.MaxValue;

            var vertices = BoundingNode.Box.GetVertices();
            for (int i = 0; i < vertices.Length; i++)
            {
                var distance = (vertices[i] - context.Camera.GetViewPostion()).Length;
                if (distance < closestDist)
                    pos = vertices[i];
            }
            return pos;
        }

        public void DrawCustom(ShaderProgram shader, int lod)
        {
            //Seal objects draw ontop of meshes so offset them
            if (IsSealPass)
            {
                GL.Enable(EnableCap.PolygonOffsetFill);
                GL.PolygonOffset(-1, 1f);
            }
            else
            {
                GL.Disable(EnableCap.PolygonOffsetFill);
            }

            customvao.Enable(shader);
            customvao.Use();

            if (Runtime.RenderSettings.Wireframe)
                DrawModelWireframe(shader, lod);
            else
                DrawSubMesh(lod);

            GL.Disable(EnableCap.PolygonOffsetFill);
        }

        public void Draw(ShaderProgram shader, int displayLOD = 0)
        {
            vao.Enable(shader);
            vao.Use();

            if (Runtime.RenderSettings.Wireframe)
                DrawModelWireframe(shader, displayLOD);
            else
                DrawSubMesh(displayLOD);
        }

        private void DrawModelWireframe(ShaderProgram shader, int displayLOD = 0)
        {
            // use vertex color for wireframe color
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);
            DrawSubMesh(displayLOD);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        private void DrawSubMesh(int displayLOD = 0)
        {
            BfresPolygonGroupRender polygonGroup = LODMeshes[displayLOD];

            if (polygonGroup.DrawCalls.Count > 1 && DISPLAY_SUB_MESH && DisplaySubMeshFunc != null)
            {
                bool hasCulling = false;
                for (int i = 0; i < polygonGroup.DrawCalls.Count; i++)
                {
                    var bounding = polygonGroup.Boundings[i];
                    polygonGroup.InFustrum[i] = DisplaySubMeshFunc(bounding);
                    if (!polygonGroup.InFustrum[i])
                        hasCulling = true;
                }
                //Here we want to only draw individual sub meshes if any being culled
                //This could be improved more with combining merged sub mesh draw calls but this is fine for now
                if (hasCulling)
                {
                    for (int i = 0; i < polygonGroup.DrawCalls.Count; i++)
                    {
                        var draw = polygonGroup.DrawCalls[i];
                        if (polygonGroup.InFustrum[i])
                        {
                            GL.DrawElements(OpenGLHelper.PrimitiveTypes[polygonGroup.PrimitiveType],
                               (int)draw.Count, polygonGroup.DrawElementsType, draw.Offset);

                            ResourceTracker.NumDrawCalls += 1;
                        }
                    }
                }
                else
                {
                    GL.DrawElements(OpenGLHelper.PrimitiveTypes[polygonGroup.PrimitiveType],
                     (int)polygonGroup.FaceCount, polygonGroup.DrawElementsType, polygonGroup.Offset);

                    ResourceTracker.NumDrawCalls += 1;
                }
            }
            else
            {
                GL.DrawElements(OpenGLHelper.PrimitiveTypes[polygonGroup.PrimitiveType],
                   (int)polygonGroup.FaceCount, polygonGroup.DrawElementsType, polygonGroup.Offset);

                ResourceTracker.NumDrawCalls += 1;
            }

            ResourceTracker.NumDrawTriangles += (polygonGroup.FaceCount / 3);

            //Reset to default depth settings after draw
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.DepthRange(0.0, 1.0);
            GL.DepthMask(true);
        }

        int indexBuffer = -1;
        int vaoBuffer = -1;

        public void InitVertexBuffer(List<BfresGLLoader.VaoAttribute> attributes, byte[] bufferData, byte[] indices)
        {
            Attributes = attributes;

            if (Attributes.Count == 0)
                throw new Exception("Failed to generate attributes!");

            //Load vaos
            int[] buffers = new int[2];
            GL.GenBuffers(2, buffers);

            indexBuffer = buffers[0];
            vaoBuffer = buffers[1];

            vao = new VertexBufferObject(vaoBuffer, indexBuffer);
            customvao = new VertexBufferObject(vaoBuffer, indexBuffer);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length, indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vaoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferData.Length, bufferData, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            UpdateVaoAttributes();
        }

        public void UpdateAttributes(List<BfresGLLoader.VaoAttribute> attributes) {
            Attributes = attributes;
        }

        public void UpdateVertexBuffer(List<BfresGLLoader.VaoAttribute> attributes, byte[] bufferData, byte[] indices)
        {
            Attributes = attributes;

            if (Attributes.Count == 0)
                throw new Exception("Failed to generate attributes!");

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length, indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vaoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferData.Length, bufferData, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            UpdateVaoAttributes();
        }

        public void UpdateVaoAttributes(Dictionary<string, int> attributeToLocation)
        {
            customvao.Clear();

            var strideTotal = Attributes.Sum(x => x.Stride);
            for (int i = 0; i < Attributes.Count; i++)
            {
                var att = Attributes[i];
                if (!attributeToLocation.ContainsKey(att.name))
                {
                    Console.WriteLine($"attributeToLocation does not contain {att.name}. skipping");
                    continue;
                }

                customvao.AddAttribute(
                    attributeToLocation[att.name],
                    att.ElementCount,
                    att.Type,
                    false,
                    strideTotal,
                    att.Offset);
            }
            customvao.Initialize();
        }

        public void UpdateVaoAttributes(Dictionary<string, string> attributeToUniform)
        {
            customvao.Clear();

            var strideTotal = Attributes.Sum(x => x.Stride);
            for (int i = 0; i < Attributes.Count; i++)
            {
                var att = Attributes[i];
                if (!attributeToUniform.ContainsKey(att.name))
                    continue;

                Console.WriteLine($"att {att.name} DataTarget {att.vertexAttributeName} shader {attributeToUniform[att.name]}");

                customvao.AddAttribute(
                    attributeToUniform[att.name],
                    att.ElementCount,
                    att.Type,
                    false,
                    strideTotal,
                    att.Offset);
            }

            customvao.Initialize();
        }

        public void UpdateVaoAttributes()
        {
            vao.Clear();

            var strideTotal = Attributes.Sum(x => x.Stride);
            for (int i = 0; i < Attributes.Count; i++)
            {
                var att = Attributes[i];
                vao.AddAttribute(
                    att.UniformName,
                    att.ElementCount,
                    att.Type,
                    false,
                    strideTotal,
                    att.Offset);
            }
            vao.Initialize();
        }

        public override void Dispose()
        {
            vao.Dispose();
            base.Dispose();
        }
    }
}
