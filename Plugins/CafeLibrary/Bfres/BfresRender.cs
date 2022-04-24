using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core;
using Toolbox.Core.IO;
using Toolbox.Core.ViewModels;

namespace CafeLibrary.Rendering
{
    public class BfresRender : GenericRenderer, IColorPickable, ITransformableObject
    {
        public static float LOD_LEVEL_1_DISTANCE = 10000000 * GLContext.PreviewScale;
        public static float LOD_LEVEL_2_DISTANCE = 100000000 * GLContext.PreviewScale;

        public override bool UsePostEffects => true;

        static bool drawAreaID = false;
        public static bool DrawDebugAreaID
        {
            get
            {
                return drawAreaID;
            }
            set
            {
                if (drawAreaID != value)
                {
                    drawAreaID = value;
                    //Make sure the viewport updates changes
                    GLContext.ActiveContext.UpdateViewport = true;
                }
            }
        }

        public override bool IsSelected 
        {
            get => base.IsSelected;
            set
            {
                base.IsSelected = value;
                foreach (var model in Models) {
                    foreach (BfresMeshRender mesh in model.MeshList) {
                        mesh.IsSelected = value;
                    }
                }
            }
        }

        private BoundingNode _boundingNode;
        public override BoundingNode BoundingNode => _boundingNode;

        public List<BfresSkeletalAnim> SkeletalAnimations = new List<BfresSkeletalAnim>();
        public List<BfresMaterialAnim> MaterialAnimations = new List<BfresMaterialAnim>();
        public List<BfresCameraAnim> CameraAnimations = new List<BfresCameraAnim>();

        public List<BfshaLibrary.BfshaFile> ShaderFiles = new List<BfshaLibrary.BfshaFile>();

        public GLTextureCube ProbeMap;

        public static ShaderProgram DefaultShader => GlobalShaders.GetShader("BFRES", "BFRES/Bfres");
        public static ShaderProgram DebugShader => GlobalShaders.GetShader("BFRES_DEBUG", "BFRES/BfresDebug");

        public bool StayInFrustum = false;

        public bool UseDrawDistance { get; set; }

        public bool CanDisplayInCubemap
        {
            get
            {
               foreach (BfresModelRender model in Models)
                {
                    if (model.Meshes.Any(x => x.IsCubeMap))
                        return true;
                }
                return false;
            }
        }

      /*  public IEnumerable<ITransformableObject> Selectables
        {
            get
            {
                List<BfresMeshRender> meshes = new List<BfresMeshRender>();
                foreach (var model in Models)
                {
                    foreach (BfresMeshRender mesh in model.MeshList)
                        meshes.Add(mesh);
                }
                return meshes;
            }
        }*/

        public Func<bool> FrustumCullingCallback;

        //Render distance for models to cull from far away.
        protected float renderDistanceSquared = 20000000;
        protected float renderDistance = 2000000;

        public EventHandler OnRenderInitialized;

        public bool MeshPicking = false;
        public Func<bool> EnablePicking;

        public BFRES BfresFile;

        public BfresRender() { }

        public BfresRender(BFRES bfres)
        {
            BfresFile = bfres;
        }

        public BfresRender(string filePath, NodeBase parent = null) : base(parent) {

            if (YAZ0.IsCompressed(filePath))
                UpdateModelFromFile(new System.IO.MemoryStream(YAZ0.Decompress(filePath)), filePath);
            else
                UpdateModelFromFile(System.IO.File.OpenRead(filePath), filePath);
        }

        public BfresRender(System.IO.Stream stream, string filePath, NodeBase parent = null) : base(parent)
        {
            UpdateModelFromFile(stream, filePath);
        }

        public BfresRender(BfresLibrary.ResFile resFile, string filePath, NodeBase parent = null) : base(parent)
        {
            Name = filePath;
            BfresLoader.OpenBfres(resFile, this);
            UpdateBoundingBox();
        }

        public bool UpdateModelFromFile(System.IO.Stream stream, string name)
        {
            Name = name;

            this.Transform.TransformUpdated += delegate
            {
                CalculateProbes();
            };

            if (DataCache.ModelCache.ContainsKey(name))
            {
                var cachedModel = DataCache.ModelCache[name] as BfresRender;
                BfresLoader.LoadBfresCached(this, cachedModel);
                UpdateBoundingBox();
                return false;
            }

            BfresLoader.OpenBfres(stream, this);

            if (Models.Count > 0)
            {
                var bounding = ((BfresModelRender)Models[0]).BoundingNode;
                Transform.ModelBounding = bounding.Box;
            }

            if (!DataCache.ModelCache.ContainsKey(name) && Models.Count > 0)
                DataCache.ModelCache.Add(name, this);

            UpdateBoundingBox();
            return Models.Count > 0;
        }

        private bool UpdateProbeLighting = false;

        private void CalculateProbes()
        {
            UpdateProbeLighting = true;
        }

        /// <summary>
        /// Toggles meshes inside the model.
        /// </summary>
        public virtual void ToggleMeshes(string name, bool toggle)
        {
            foreach (var model in Models) {
                foreach (BfresMeshRender mesh in model.MeshList) {
                    if (mesh.Name == name)
                        mesh.IsVisible = toggle;
                }
            }
        }

        public virtual void OnSkeletonUpdated()
        {
            foreach (BfresModelRender model in this.Models)
                model.UpdateSkeletonUniforms();
        }

        /// <summary>
        /// Resets all the animation states to defaults.
        /// Animation value lists are cleared, bones have reset transforms.
        /// </summary>
        public override void ResetAnimations()
        {
            foreach (BfresModelRender model in Models)
            {
                foreach (var mesh in model.Meshes)
                    ((BfresMaterialRender)mesh.MaterialAsset).ResetAnimations();

                model.ResetAnimations();
            }
        }

        public void RemoveSelected(bool undo = true)
        {
            List<BfresMeshRender> selected = new List<BfresMeshRender>();
            foreach (BfresModelRender model in Models)
                selected.AddRange(model.Meshes.Where(x => x.IsSelected).ToList());

            if (selected.Count == 0)
                return;

            int result = MapStudio.UI.TinyFileDialog.MessageBoxInfoYesNo("Are you sure you want to delete these meshes? Operation cannot be undone!");
            if (result == 1)
            {
                foreach (BfresModelRender model in Models)
                {
                    foreach (BfresMeshRender mesh in selected)
                        if (model.Meshes.Contains(mesh)) {
                            model.Meshes.Remove(mesh);
                            mesh.OnRemoved?.Invoke(mesh, EventArgs.Empty);
                        }
                }
            }
        }

        bool drawnOnce = false;

        /// <summary>
        /// Draws the model using a normal material pass.
        /// </summary>
        public override void DrawModel(GLContext control, Pass pass)
        {
            if (!InFrustum || !IsVisible)
                return;

            if (UpdateProbeLighting)
            {
              //  ProbeMap = AGraphicsLibrary.LightingEngine.LightSettings.UpdateProbeCubemap(control, ProbeMap, Transform.Position);
                UpdateProbeLighting = false;

                control.ScreenBuffer.Bind();
            }

            base.DrawModel(control, pass);

            //Make sure cubemaps can look seamless in lower mip levels
            GL.Enable(EnableCap.TextureCubeMapSeamless);

            if (DebugShaderRender.DebugRendering != DebugShaderRender.DebugRender.Default || DrawDebugAreaID)
                control.CurrentShader = DebugShader;
            else
                control.UseSRBFrameBuffer = true;

            if (!drawnOnce)
            {
                OnRenderInitialized?.Invoke(this, EventArgs.Empty);
                drawnOnce = true;
            }

            Transform.UpdateMatrix();
            foreach (BfresModelRender model in Models)
                if (model.IsVisible)
                    model.Draw(control, pass, this);

            //Draw debug boundings
            if (Runtime.RenderBoundingBoxes)
                this.BoundingNode.Box.DrawSolid(control, this.Transform.TransformMatrix, Vector4.One);

           // if (Runtime.RenderBoundingBoxes)
                DrawBoundings(control);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.DepthMask(true);
            GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.Blend);
        }

        public override void DrawColorBufferPass(GLContext control)
        {
            if (!IsVisible)
                return;

            foreach (BfresModelRender model in Models)
                if (model.IsVisible)
                    model.DrawColorBufferPass(control, this);
        }

        /// <summary>
        /// Draws the projected shadow model in light space.
        /// </summary>
        /// <param name="control"></param>
        public override void DrawShadowModel(GLContext control)
        {
            if (!IsVisible)
                return;

            foreach (BfresModelRender model in Models)
                if (model.IsVisible)
                    model.DrawShadowModel(control, this);
        }

        public override void DrawCubeMapScene(GLContext control)
        {
            foreach (BfresModelRender model in Models)
                if (model.IsVisible)
                    model.DrawCubemapModel(control, this);
        }

        /// <summary>
        /// Draw gbuffer pass for storing normals and depth information
        /// </summary>
        /// <param name="control"></param>
        public override void DrawGBuffer(GLContext control)
        {
            if (!InFrustum || !IsVisible)
                return;

            foreach (BfresModelRender model in Models)
            {
                if (model.IsVisible)
                    model.DrawGBuffer(control, this);
            }
        }

        SphereRender sphere = null;

        public void DrawBoundings(GLContext control)
        {
            foreach (BfresModelRender model in Models)
            {
                if (!model.IsVisible)
                    continue;

                //Go through each bounding in the current displayed mesh
                var bounding = model.BoundingNode;

                var shader = GlobalShaders.GetShader("PICKING");
                control.CurrentShader = shader;
                control.CurrentShader.SetVector4("color", new Vector4(1));

                Matrix4 transform = Transform.TransformMatrix;
                bounding.UpdateTransform(transform);

                GL.LineWidth(2);

                var bnd = bounding.Box;

                //Culling debugger
                if (BfresMeshRender.DISPLAY_SUB_MESH)
                {
                    foreach (BfresMeshRender mesh in model.Meshes)
                    {
                        if (!mesh.IsVisible)
                            continue;

                        int ind = 0;
                        foreach (var bb in mesh.LODMeshes[0].Boundings)
                        {
                            control.CurrentShader.SetVector4("color", new Vector4(1));
                            if (!mesh.LODMeshes[0].InFustrum[ind++])
                            {
                                control.CurrentShader.SetVector4("color", new Vector4(1, 0, 0, 1));
                                BoundingBoxRender.Draw(control, bb.Box.Min, bb.Box.Max);
                            }
                            else
                            {
                                //   BoundingBoxRender.Draw(control, bb.Box.Min, bb.Box.Max);
                            }
                        }
                    }

                }
                /* foreach (BfresMeshRender mesh in model.Meshes)
                 {
                     if (!mesh.IsVisible)
                         continue;

                     foreach (var bb in mesh.LODMeshes[0].Boundings)
                     {
                         if (sphere == null)
                             sphere = new SphereRender(1.0f, 8, 8, PrimitiveType.LineLoop);

                         var mat = new StandardMaterial();
                         mat.ModelMatrix = Matrix4.CreateScale(bb.Radius) * Matrix4.CreateTranslation(bb.Center);
                         mat.Render(control);
                         sphere.Draw(control);
                     }
                 }*/
            }
        }

        /// <summary>
        /// Draws the model in the color picking pass.
        /// </summary>
        /// <param name="control"></param>
        public void DrawColorPicking(GLContext control)
        {
            if (!InFrustum || !IsVisible || (EnablePicking != null && !EnablePicking()))
                return;

            Transform.UpdateMatrix();

            var shader = GlobalShaders.GetShader("PICKING");
            control.CurrentShader = shader;

            if (!MeshPicking)
                control.ColorPicker.SetPickingColor(this, shader);

            foreach (BfresModelRender model in Models)
            {
                if (model.IsVisible)
                    model.DrawColorPicking(control, this);
            }
        }

        /// <summary>
        /// Checks for when the current render is in the fustrum of the camera
        /// Returns true if in view.
        /// </summary>
        public override bool IsInsideFrustum(GLContext context)
        {
            if (StayInFrustum) return true;

            if (FrustumCullingCallback != null) {
                return FrustumCullingCallback.Invoke();
            }

            InFrustum = false;

            //Todo check for actor objects that handle box culling differently
            foreach (BfresModelRender model in Models) {
                model.UpdateFrustum(context, this);
                /*if (model.MeshInFrustum.Any(x => x))
                    InFrustum = true;*/
            }

            // Draw distance map objects
            //if (UseDrawDistance)
             //   return context.Camera.InRange(renderDistanceSquared, Transform.Position);
            return true;
        }

        public void UpdateBoundingBox()
        {
            _boundingNode = new BoundingNode(new Vector3(float.MaxValue), new Vector3(float.MinValue));
            foreach (var model in Models) {
                if (!model.IsVisible)
                    continue;

                foreach (var mesh in model.MeshList) {
                    _boundingNode.Include(mesh.BoundingNode);
                }
            }
            this.Transform.TransformUpdated += delegate {
                _boundingNode.UpdateTransform(this.Transform.TransformMatrix);
            };
        }
    }
}
