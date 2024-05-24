using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Toolbox.Core;

namespace CafeLibrary.Rendering
{
    public class BfresModelRender : ModelAsset
    {
        public Matrix4 ModelTransform = Matrix4.Identity;

        static GLMaterialBlendState DefaultBlendState = new GLMaterialBlendState();

        public override IEnumerable<GenericPickableMesh> MeshList => Meshes;

        public List<BfresMeshRender> Meshes = new List<BfresMeshRender>();
        public bool[] MeshInFrustum = new bool[0];

        public BoundingNode BoundingNode = new BoundingNode();

        public SkeletonRenderer SkeletonRenderer;

        public void ResetAnimations() {
            foreach (var bone in ModelData.Skeleton.Bones)
                bone.Visible = true;

            ModelData.Skeleton.Reset();
        }

        public static BfresModelRender CreateCache(BfresModelRender model)
        {
            BfresModelRender modelCache = new BfresModelRender();
            modelCache.SkeletonRenderer = model.SkeletonRenderer;
            modelCache.Name = model.Name;
            modelCache.ModelData = new STGenericModel();
            modelCache.BoundingNode = model.BoundingNode;
            var skeletonCache = model.ModelData.Skeleton;
/*
            var skeleton = new STSkeleton();
            modelCache.ModelData.Skeleton = skeleton;

            for (int i = 0; i < skeletonCache.Bones.Count; i++)
            {
                skeleton.Bones.Add(new STBone(skeleton)
                {
                    Name = skeletonCache.Bones[i].Name,
                    Position = skeletonCache.Bones[i].Position,
                    Rotation = skeletonCache.Bones[i].Rotation,
                    Scale = skeletonCache.Bones[i].Scale,
                    ParentIndex = skeletonCache.Bones[i].ParentIndex,
                });
            }*/

             modelCache.ModelData = model.ModelData;
            modelCache.MeshInFrustum = new bool[model.Meshes.Count];
            for (int i = 0; i < model.Meshes.Count; i++)
                modelCache.MeshInFrustum[i] = true;
            modelCache.Meshes.AddRange(model.Meshes);

            return modelCache;
        }

        public void UpdateSkeletonUniforms()
        {

        }

        public void UpdateFrustum(GLContext context, BfresRender render)
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (Meshes[i].VertexSkinCount == 0 && Meshes[i].BoneIndex == 0) //only check static meshes atm as rigged boudnings are not checked
                    MeshInFrustum[i] = IsMeshInFustrum(context, render, Meshes[i]);
                else
                    MeshInFrustum[i] = true;
            }
        }

        public void Draw(GLContext context, Pass pass, BfresRender parentRender)
        {
            if (!IsVisible)
                return;

            if (DebugShaderRender.DebugRendering != DebugShaderRender.DebugRender.Default) {
                DrawMeshes(context, parentRender, pass, RenderPass.DEBUG);
            }
            else
            {
                foreach (var mesh in Meshes) {
                    if (pass != mesh.Pass || !mesh.IsVisible || mesh.UseColorBufferPass)
                        continue;

                    RenderMesh(context, parentRender, mesh);
                }

                /* if (Meshes.Where(x => x.UseColorBufferPass).ToList().Count > 0)
                 {
                     GLFrameworkEngine.ScreenBufferTexture.FilterScreen(context);
                     foreach (var mesh in Meshes.Where(x => x.UseColorBufferPass))
                     {
                         if (pass != mesh.Pass || !mesh.IsVisible)
                             continue;

                         RenderMesh(context, parentRender, mesh);
                     }
                 }*/
            }

            //Reset blend state
            DefaultBlendState.RenderAlphaTest();
            DefaultBlendState.RenderBlendState();
            DefaultBlendState.RenderDepthTest();

            GL.DepthMask(true);
        }

        public void DrawShadowModel(GLContext context, BfresRender parentRender)
        {
            if (!IsVisible)
                return;

            DrawMeshes(context, parentRender, Pass.OPAQUE, RenderPass.SHADOW_DYNAMIC);
            DrawMeshes(context, parentRender, Pass.TRANSPARENT, RenderPass.SHADOW_DYNAMIC);
        }

        public void DrawCubemapModel(GLContext context, BfresRender parentRender)
        {
            if (!IsVisible)
                return;

            foreach (var mesh in Meshes) {
                if (!mesh.IsCubeMap && !mesh.RenderInCubeMap)
                    continue;

                RenderMesh(context, parentRender, mesh);
            }
        }

        public void DrawGBuffer(GLContext context, BfresRender parentRender)
        {
            if (!IsVisible)
                return;

            DrawMeshes(context, parentRender, Pass.OPAQUE, RenderPass.GBUFFER);
            DrawMeshes(context, parentRender, Pass.TRANSPARENT, RenderPass.GBUFFER);
        }

        public void DrawColorBufferPass(GLContext context, BfresRender parentRender)
        {
            if (!Meshes.Any(x => x.UseColorBufferPass))
                return;

            if (DebugShaderRender.DebugRendering != DebugShaderRender.DebugRender.Default)
            {
                DrawMeshes(context, parentRender, Pass.OPAQUE, RenderPass.DEBUG);
                DrawMeshes(context, parentRender, Pass.TRANSPARENT, RenderPass.DEBUG);
                return;
            }
            //Draw objects that use the color buffer texture
            DrawMeshes(context, parentRender, Pass.OPAQUE, RenderPass.COLOR_COPY);
            DrawMeshes(context, parentRender, Pass.TRANSPARENT, RenderPass.COLOR_COPY);
        }

        enum RenderPass
        {
            DEFAULT,
            DEBUG,
            COLOR_COPY,
            SHADOW_DYNAMIC,
            GBUFFER,
        }

        private void DrawMeshes(GLContext context, BfresRender parentRender, Pass pass, RenderPass renderMode)
        {
            foreach (var mesh in Meshes)
            {
                if (mesh.Pass != pass || !mesh.IsVisible ||
                     renderMode == RenderPass.COLOR_COPY && !mesh.UseColorBufferPass ||
                     renderMode == RenderPass.SHADOW_DYNAMIC && !mesh.ProjectDynamicShadowMap)
                    continue;

                if (renderMode == RenderPass.DEFAULT && mesh.UseColorBufferPass)
                    return;

                if (renderMode == RenderPass.SHADOW_DYNAMIC)
                {
                    var frustum = context.Scene.ShadowRenderer.GetShadowFrustum();
                    if (FrustumHelper.CubeInFrustum(frustum, BoundingNode.GetCenter(), BoundingNode.GetRadius()))
                    {
                        ((BfresMaterialRender)mesh.MaterialAsset).RenderShadowMaterial(context);

                        DrawMesh(context.CurrentShader, parentRender, mesh, false);
                        ResourceTracker.NumShadowDrawCalls += mesh.LODMeshes[0].DrawCalls.Count;
                    }
                }
                else if (renderMode == RenderPass.GBUFFER)
                {
                    ((BfresMaterialRender)mesh.MaterialAsset).RenderGBuffer(context, parentRender, mesh);

                    DrawMesh(context.CurrentShader, parentRender, mesh);
                }
                else if (renderMode == RenderPass.DEBUG)
                {
                    if (DebugShaderRender.DebugRendering != DebugShaderRender.DebugRender.Diffuse)
                        context.UseSRBFrameBuffer = false;
                    else
                        context.UseSRBFrameBuffer = true;

                    context.CurrentShader = BfresRender.DebugShader;
                    BfresRender.DebugShader.Enable();

                    ((BfresMaterialRender)mesh.MaterialAsset).SetRenderState();
                    ((BfresMaterialRender)mesh.MaterialAsset).RenderDebugMaterials(context,
                        parentRender, context.CurrentShader, mesh);

                    context.CurrentShader.SetBoolToInt("DrawAreaID", BfresRender.DrawDebugAreaID);
                    context.CurrentShader.SetInt("AreaIndex", ((BfresMaterialRender)mesh.MaterialAsset).AreaIndex);
                    context.CurrentShader.SetBoolToInt("isSelected", parentRender.IsSelected || mesh.IsSelected);


                    //Selection clear color
                    if (parentRender.MeshPicking)
                    {
                        if (mesh.IsSelected)
                            context.EnableSelectionMask();
                        else
                            context.DisableSelectionMask();
                    }
                    else
                    {
                        if (parentRender.IsSelected)
                            context.EnableSelectionMask();
                        else
                            context.DisableSelectionMask();
                    }

                    DrawMesh(context.CurrentShader, parentRender, mesh);
                }
                else
                    RenderMesh(context, parentRender, mesh);
            }
        }

        public void DrawColorPicking(GLContext control, BfresRender parentRender)
        {
            if (!IsVisible)
                return;

            GL.Enable(EnableCap.DepthTest);
            foreach (BfresMeshRender mesh in this.Meshes)
            {
                if (!mesh.IsVisible)
                    continue;

                ((BfresMaterialRender)mesh.MaterialAsset).SetRenderState();

                //Draw the mesh
                if (parentRender.MeshPicking)
                    control.ColorPicker.SetPickingColor(mesh, control.CurrentShader);

                DrawMesh(control.CurrentShader, parentRender, mesh);

                control.CurrentShader.SetInt("UseSkinning", 0);
            }
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }

        public void RenderMesh(GLContext context, BfresRender parentRender, BfresMeshRender mesh)
        {
            //Selection clear color
            if (parentRender.MeshPicking)
            {
                if (mesh.IsSelected)
                    context.EnableSelectionMask();
                else
                    context.DisableSelectionMask();
            }
            else
            {
                if (parentRender.IsSelected)
                    context.EnableSelectionMask();
                else
                    context.DisableSelectionMask();
            }

            if (PluginConfig.UseGameShaders && mesh.MaterialAsset is BfshaRenderer && !BfresRender.DrawDebugAreaID) {
                DrawCustomShaderRender(context, parentRender, mesh, 0);
            }
            else //Draw default if not using game shader rendering.
            {
                context.CurrentShader = BfresRender.DefaultShader;

                ((BfresMaterialRender)mesh.MaterialAsset).RenderDefaultMaterials(context, parentRender.Transform, context.CurrentShader, mesh);
                DrawMesh(context.CurrentShader, parentRender, mesh, true);
            }
        }

        private void DrawCustomShaderRender(GLContext context, BfresRender parentRender, BfresMeshRender mesh, int stage = 0)
        {
            var materialAsset = ((BfshaRenderer)mesh.MaterialAsset);
            if (!materialAsset.HasValidProgram) {
                materialAsset.DrawEmptyMaterial(context, parentRender.Transform.TransformMatrix, mesh);
                return;
            }

            materialAsset.ShaderIndex = stage;
            materialAsset.CheckProgram(context, mesh, stage);

            if (materialAsset.GLShaderInfo == null)
                return;

            context.CurrentShader = materialAsset.Shader;
            ((BfshaRenderer)mesh.MaterialAsset).ParentRenderer = parentRender;
          ((BfshaRenderer)mesh.MaterialAsset).Render(context, this, parentRender.Transform, materialAsset.Shader, mesh);
            //Draw the mesh
            int lod = mesh.GetDisplayLevel(GLContext.ActiveContext, parentRender);
            mesh.DrawCustom(context.CurrentShader, lod);
        }

        private void DrawMesh(ShaderProgram shader, BfresRender parentRender, BfresMeshRender mesh, bool usePolygonOffset = false)
        {
            if (!MeshInFrustum[mesh.Index])
                return;

            //hidden by bone vis
            if (!this.ModelData.Skeleton.Bones[mesh.BoneIndex].Visible)
                return;

            bool enableSkinning = true;

            if (mesh.VertexSkinCount > 0 && enableSkinning)
                SetModelMatrix(shader.program, ModelData.Skeleton, mesh.VertexSkinCount > 1);

            var worldTransform = parentRender.Transform.TransformMatrix * ModelTransform;
            var transform = this.ModelData.Skeleton.Bones[mesh.BoneIndex].Transform;
            shader.SetMatrix4x4("RigidBindTransform", ref transform);
            shader.SetMatrix4x4("mtxMdl", ref worldTransform);
            shader.SetInt("SkinCount", mesh.VertexSkinCount);
            shader.SetInt("UseSkinning", enableSkinning ? 1 : 0);

            int lod = mesh.GetDisplayLevel(GLContext.ActiveContext, parentRender);            

            //Draw the mesh
            if (usePolygonOffset)
                mesh.DrawWithPolygonOffset(shader, lod);
            else
                mesh.Draw(shader, lod);
        }

        /// <summary>
        /// Checks for when the given mesh render is in the fustrum of the camera
        /// Returns true if in view.
        /// </summary>
        private bool IsMeshInFustrum(GLContext control, BfresRender parentRender, GenericPickableMesh mesh)
        {
            if (parentRender.StayInFrustum)
                return true;

            var msh = (BfresMeshRender)mesh;
            var bone = ModelData.Skeleton.Bones[msh.BoneIndex].Transform;
            mesh.BoundingNode.UpdateTransform(bone * parentRender.Transform.TransformMatrix);
            return control.Camera.InFustrum(mesh.BoundingNode);
        }

        private void SetModelMatrix(int programID, STSkeleton skeleton, bool useInverse = true)
        {
            GL.Uniform1(GL.GetUniformLocation(programID, "UseSkinning"), 1);

            for (int i = 0; i < skeleton.Bones.Count; i++)
            {
                Matrix4 transform = skeleton.Bones[i].Transform;
                //Check if the bone is smooth skinning aswell for accuracy purposes.
                if (useInverse)
                    transform = skeleton.Bones[i].Inverse * skeleton.Bones[i].Transform;
                GL.UniformMatrix4(GL.GetUniformLocation(programID, String.Format("bones[{0}]", i)), false, ref transform);
            }
        }

        public void Dispose()
        {
            foreach (var mesh in Meshes)
                mesh.Dispose();
        }
    }
}
