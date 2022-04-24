using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using BfresLibrary;
using Toolbox.Core;
using BfresLibrary.Helpers;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Toolbox.Core.IO;

namespace CafeLibrary.Rendering
{
    public class BfresLoader
    {
        public static List<BfshaLibrary.BfshaFile> GlobalShaders = new List<BfshaLibrary.BfshaFile>();

        public static Type TargetShader;

        static List<Type> CustomShaders = new List<Type>();

        public static void AddShaderType(Type type)
        {
            if (!CustomShaders.Contains(type))
                CustomShaders.Add(type);
        }

        public static BfresRender Load(System.IO.Stream stream, string name) {
            return new BfresRender(stream, name);
        }

        public static bool LoadBfresCached(BfresRender renderer, BfresRender cachedRender)
        {
            //Render already has models so return
            if (renderer.Models.Count > 0) return true;

            renderer.BoundingSphere = cachedRender.BoundingSphere;

            foreach (var tex in cachedRender.Textures)
                renderer.Textures.Add(tex.Key, tex.Value);
            foreach (BfresModelRender model in cachedRender.Models)
            {
                //Create a new model instance so mesh frustum data and individual skeletons can be stored there
                renderer.Models.Add(BfresModelRender.CreateCache(model));
            }

            foreach (var anim in cachedRender.SkeletalAnimations)
                renderer.SkeletalAnimations.Add(anim.Clone());
            foreach (var anim in cachedRender.MaterialAnimations)
                renderer.MaterialAnimations.Add(anim.Clone());

            return true;
        }

        public static Dictionary<string, GenericRenderer.TextureView> GetTextures(string filePath)
        {
            if (YAZ0.IsCompressed(filePath))
               return GetTextures(new System.IO.MemoryStream(YAZ0.Decompress(filePath)));
            else
                return GetTextures(System.IO.File.OpenRead(filePath));
        }

        public static void LoadAnimations(BfresRender render, string filePath)
        {
            if (render.SkeletalAnimations.Count > 0)
                return;

            if (YAZ0.IsCompressed(filePath))
                LoadAnimations(render, new System.IO.MemoryStream(YAZ0.Decompress(filePath)));
            else
                LoadAnimations(render, System.IO.File.OpenRead(filePath));
        }

        static void LoadAnimations(BfresRender renderer, System.IO.Stream stream)
        {
            ResFile resFile = new ResFile(stream);

            foreach (var anim in resFile.SkeletalAnims.Values)
                renderer.SkeletalAnimations.Add(new BfresSkeletalAnim(anim, renderer.Name));
            foreach (var anim in resFile.ShaderParamAnims.Values)
                renderer.MaterialAnimations.Add(new BfresMaterialAnim(anim, renderer.Name));
            foreach (var anim in resFile.ColorAnims.Values)
                renderer.MaterialAnimations.Add(new BfresMaterialAnim(anim, renderer.Name));
            foreach (var anim in resFile.TexSrtAnims.Values)
                renderer.MaterialAnimations.Add(new BfresMaterialAnim(anim, renderer.Name));
            foreach (var anim in resFile.TexPatternAnims.Values)
                renderer.MaterialAnimations.Add(new BfresMaterialAnim(anim, renderer.Name));
        }

        public static Dictionary<string, GenericRenderer.TextureView> GetTextures(System.IO.Stream stream)
        {
            Dictionary<string, GenericRenderer.TextureView> textures = new Dictionary<string, GenericRenderer.TextureView>();

            ResFile resFile = new ResFile(stream);

            foreach (var tex in resFile.Textures.Values)
                textures.Add(tex.Name, PrepareTexture(resFile, tex));

            return textures;
        }

        public static bool OpenBfres(System.IO.Stream stream, BfresRender renderer)
        {
            //Render already has models so return
            if (renderer.Models.Count > 0) return true;

            return OpenBfres(new ResFile(stream), renderer);
        }

        public static bool OpenBfres(ResFile resFile, BfresRender renderer)
        {
            LoadShaders(resFile, renderer);

            foreach (var model in resFile.Models.Values)
                renderer.Models.Add(PrepareModel(renderer, resFile, model));
            foreach (var tex in resFile.Textures.Values)
                renderer.Textures.Add(tex.Name, PrepareTexture(resFile, tex));

            foreach (var anim in resFile.SkeletalAnims.Values)
                renderer.SkeletalAnimations.Add(new BfresSkeletalAnim(anim, renderer.Name));
            foreach (var anim in resFile.ShaderParamAnims.Values)
                renderer.MaterialAnimations.Add(new BfresMaterialAnim(anim, renderer.Name));
            foreach (var anim in resFile.ColorAnims.Values)
                renderer.MaterialAnimations.Add(new BfresMaterialAnim(anim, renderer.Name));
            foreach (var anim in resFile.TexSrtAnims.Values)
                renderer.MaterialAnimations.Add(new BfresMaterialAnim(anim, renderer.Name));
            foreach (var anim in resFile.TexPatternAnims.Values)
                renderer.MaterialAnimations.Add(new BfresMaterialAnim(anim, renderer.Name));

            if (renderer.Models.Count > 0)
            {
                var render = (BfresModelRender)renderer.Models.FirstOrDefault();
                var positions = GetVertices(resFile, render, resFile.Models.Values.FirstOrDefault());
                renderer.BoundingSphere = GLFrameworkEngine.Utils.BoundingSphereGenerator.GenerateBoundingSphere(positions);
            }

            return true;
        }

        public static void TryLoadShader(ResFile resFile, BfresRender renderer, string name)
        {
            string internalName = System.IO.Path.GetFileNameWithoutExtension(name);

            if (renderer.ShaderFiles.Any(x => x.Name == internalName))
                return;

            var data = resFile.ExternalFiles[name].Data;
            renderer.ShaderFiles.Add(new BfshaLibrary.BfshaFile(new System.IO.MemoryStream(data)));
        }

        public static void LoadShaders(ResFile resFile, BfresRender renderer)
        {
            renderer.ShaderFiles.Clear();
            //Find and load any external shader binaries
            if (PluginConfig.UseGameShaders || true)
            {
                for (int i = 0; i < resFile.ExternalFiles.Count; i++)
                {
                    string fileName = resFile.ExternalFiles.Keys.ToList()[i];
                    if (fileName.EndsWith(".bfsha")) {
                        renderer.ShaderFiles.Add(new BfshaLibrary.BfshaFile(new System.IO.MemoryStream(resFile.ExternalFiles[i].Data)));
                    }
                }
            }
            renderer.ShaderFiles.AddRange(GlobalShaders);
        }

        public static void LoadTextures(ResFile resFile, BfresRender renderer)
        {
            foreach (var tex in resFile.Textures)
            {
                renderer.Textures.Add(tex.Key, PrepareTexture(resFile, tex.Value));
            }
        }

        public static BfresModelRender PrepareModel(BfresRender renderer, ResFile resFile, Model model)
        {
            BfresModelRender modelRender = new BfresModelRender();
            modelRender.Name = model.Name;

            //Caustics are drawn with projection in light pre pass
            if (modelRender.Name == "CausticsArea")
                modelRender.IsVisible = false;

            var genericModel = new STGenericModel();
            genericModel.Skeleton = LoadSkeleton(model.Skeleton); 
            modelRender.ModelData = genericModel;
            modelRender.MeshInFrustum = new bool[model.Shapes.Count];
            modelRender.SkeletonRenderer = new SkeletonRenderer(genericModel.Skeleton);

            for (int i = 0; i < model.Shapes.Count; i++)
            {
                modelRender.MeshInFrustum[i] = true;
                var shape = model.Shapes[i];

                var mesh = new BfresMeshRender(i);
                mesh.Name = shape.Name;
                mesh.BoundingNode = CalculateBounding(resFile, modelRender, model, shape);
                mesh.VertexSkinCount = shape.VertexSkinCount;
                mesh.BoneIndex = shape.BoneIndex;

                var mat = model.Materials[shape.MaterialIndex];
                var attributes = BfresGLLoader.LoadAttributes(model, shape, mat);
                var buffer = BfresGLLoader.LoadBufferData(resFile, model, shape, attributes);
                var groups = LoadMeshGroups(mesh, shape);
                var indices = BfresGLLoader.LoadIndexBufferData(shape);

                mesh.LODMeshes = groups;
                mesh.InitVertexBuffer(attributes, buffer, indices);

                var matRender = LoadMaterial(renderer, modelRender, mesh, mat, shape);
                if (matRender.Material.IsTransparent)
                    mesh.Pass = Pass.TRANSPARENT;

                mesh.MaterialAsset = matRender;
                modelRender.Meshes.Add(mesh);

                modelRender.BoundingNode.Include(mesh.BoundingNode);
            }
            return modelRender;
        }

        public static BfresMeshRender AddMesh(ResFile resFile, BfresRender renderer, BfresModelRender modelRender, Model model, Shape shape)
        {
            var mesh = new BfresMeshRender(modelRender.Meshes.Count);
            mesh.Name = shape.Name;
            mesh.BoundingNode = CalculateBounding(resFile, modelRender, model, shape);
            mesh.VertexSkinCount = shape.VertexSkinCount;
            mesh.BoneIndex = shape.BoneIndex;

            var mat = model.Materials[shape.MaterialIndex];
            var attributes = BfresGLLoader.LoadAttributes(model, shape, mat);
            var buffer = BfresGLLoader.LoadBufferData(resFile, model, shape, attributes);
            var groups = LoadMeshGroups(mesh, shape);
            var indices = BfresGLLoader.LoadIndexBufferData(shape);

            mesh.LODMeshes = groups;
            mesh.InitVertexBuffer(attributes, buffer, indices);
            modelRender.BoundingNode.Include(mesh.BoundingNode);
            modelRender.Meshes.Add(mesh);

            modelRender.MeshInFrustum = new bool[model.Shapes.Count];
            for (int i = 0; i < model.Shapes.Count; i++)
                modelRender.MeshInFrustum[i] = true;

            return mesh;
        }

        public static void UpdateVertexBuffer(ResFile resFile, Model model, Shape shape, Material mat, BfresMeshRender mesh)
        {
            var attributes = BfresGLLoader.LoadAttributes(model, shape, mat);
            var buffer = BfresGLLoader.LoadBufferData(resFile, model, shape, attributes);
            var groups = LoadMeshGroups(mesh, shape);
            var indices = BfresGLLoader.LoadIndexBufferData(shape);

            mesh.LODMeshes = groups;
            mesh.UpdateVertexBuffer(attributes, buffer, indices);
        }

        public static void UpdateAttributes(Model model, Shape shape, Material mat, BfresMeshRender mesh)
        {
            var attributes = BfresGLLoader.LoadAttributes(model, shape, mat);
            mesh.UpdateAttributes(attributes);
        }

        public static void UpdateMaterial(BfresMaterialRender matRender, BfresRender render, FMAT fmat,
            BfresModelRender model, BfresMeshRender meshRender, Material mat)
        {
            if (matRender != null)
                matRender.Dispose();

            matRender = new BfresMaterialRender(render, model);
            if (render.ShaderFiles.Any(x => x.Name == mat.ShaderAssign.ShaderArchiveName))
            {
                if (TargetShader != null)
                    matRender = (BfresMaterialRender)Activator.CreateInstance(TargetShader, render, model);
            }

            foreach (var type in CustomShaders)
            {
                var shaderMat = (ShaderRenderBase)Activator.CreateInstance(TargetShader, render, model);
                if (shaderMat.UseRenderer(mat, mat.ShaderAssign.ShaderArchiveName, mat.ShaderAssign.ShadingModelName))
                    matRender = shaderMat;
            }

            fmat.MaterialAsset = matRender;

            matRender.Material = fmat;
            matRender.Name = mat.Name;
            matRender.ReloadRenderState(mat, meshRender);

            if (matRender is BfshaRenderer)
                ((BfshaRenderer)matRender).TryLoadShader(render, meshRender);

            if (matRender.Material.IsTransparent)
                meshRender.Pass = Pass.TRANSPARENT;

            meshRender.MaterialAsset = matRender;
        }

        static BfresMaterialRender LoadMaterial(BfresRender render, BfresModelRender model, BfresMeshRender meshRender, Material mat, Shape shape)
        {
            BfresMaterialRender matRender = new BfresMaterialRender(render, model);
            if (render.ShaderFiles.Count > 0)
            {
                if (TargetShader != null)
                    matRender = (BfresMaterialRender)Activator.CreateInstance(TargetShader, render, model);
            }
            matRender.Material = new FMAT();
            matRender.Name = mat.Name;
            matRender.Material.ReloadMaterial(mat);
            matRender.ReloadRenderState(mat, meshRender);

            if (matRender is BfshaRenderer)
                ((BfshaRenderer)matRender).TryLoadShader(render, meshRender);

            return matRender;
        }

        static List<BfresPolygonGroupRender> LoadMeshGroups(BfresMeshRender meshRender, Shape shape)
        {
            List<BfresPolygonGroupRender> groups = new List<BfresPolygonGroupRender>();
            int offset = 0;
            foreach (var mesh in shape.Meshes) {
                groups.Add(new BfresPolygonGroupRender(meshRender, shape, mesh, 0, offset));
                int stride = 4;
                if (mesh.IndexFormat == BfresLibrary.GX2.GX2IndexFormat.UInt16 ||
                    mesh.IndexFormat == BfresLibrary.GX2.GX2IndexFormat.UInt16LittleEndian)
                {
                    stride = 2;
                }
                offset += (int)mesh.IndexCount * stride;
            }
            return groups;
        }

        static Vector3[] GetVertices(ResFile resFile, BfresModelRender modelRender, Model model)
        {
            List<Vector3> vertices = new List<Vector3>();
            foreach (var shape in model.Shapes.Values)
            {
                VertexBufferHelper helper = new VertexBufferHelper(
                     model.VertexBuffers[shape.VertexBufferIndex], resFile.ByteOrder);

                var positions = helper.Attributes.FirstOrDefault(x => x.Name == "_p0");
                var indices = helper.Attributes.FirstOrDefault(x => x.Name == "_i0");

                for (int i = 0; i < positions.Data.Length; i++)
                {
                    var position = new Vector3(positions.Data[i].X, positions.Data[i].Y, positions.Data[i].Z);
                    position *= GLContext.PreviewScale;

                    //Calculate in worldspace
                    if (shape.VertexSkinCount == 1)
                    {
                        var index = (int)model.Skeleton.MatrixToBoneList[(int)indices.Data[i].X];
                        var transform = modelRender.ModelData.Skeleton.Bones[index].Transform;
                        position = Vector3.TransformPosition(position, transform);
                    }
                    if (shape.VertexSkinCount == 0)
                    {
                        var transform = modelRender.ModelData.Skeleton.Bones[shape.BoneIndex].Transform;
                        position = Vector3.TransformPosition(position, transform);
                    }
                    vertices.Add(position);
                }
            }
            return vertices.ToArray();
        }

        static GLFrameworkEngine.BoundingNode CalculateBounding(ResFile resFile, BfresModelRender modelRender, Model model, Shape shape)
        {
            VertexBufferHelper helper = new VertexBufferHelper(
                model.VertexBuffers[shape.VertexBufferIndex], resFile.ByteOrder);

            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            var positions = helper.Attributes.FirstOrDefault(x => x.Name == "_p0");
            var indices = helper.Attributes.FirstOrDefault(x => x.Name == "_i0");

            for (int i = 0; i < positions.Data.Length; i++)
            {
                var position = new Vector3(positions.Data[i].X, positions.Data[i].Y, positions.Data[i].Z);
                position *= GLContext.PreviewScale;

                //Calculate in worldspace
                if (shape.VertexSkinCount == 1)
                {
                    var index = (int)model.Skeleton.MatrixToBoneList[(int)indices.Data[i].X];
                    var transform = modelRender.ModelData.Skeleton.Bones[index].Transform;
                    position = Vector3.TransformPosition(position, transform);
                }
                if (shape.VertexSkinCount == 0)
                {
                    var transform = modelRender.ModelData.Skeleton.Bones[shape.BoneIndex].Transform;
                    position = Vector3.TransformPosition(position, transform);
                }

                min.X = MathF.Min(min.X, position.X);
                min.Y = MathF.Min(min.Y, position.Y);
                min.Z = MathF.Min(min.Z, position.Z);
                max.X = MathF.Max(max.X, position.X);
                max.Y = MathF.Max(max.Y, position.Y);
                max.Z = MathF.Max(max.Z, position.Z);
            }
 
            return new GLFrameworkEngine.BoundingNode()
            {
                Radius = shape.RadiusArray.FirstOrDefault(),
                Center = (max - min) / 2f,
                Box = BoundingBox.FromMinMax(min, max),
        };
        }

        static GenericRenderer.TextureView PrepareTexture(ResFile resFile, TextureShared tex)
        {
            if (tex is BfresLibrary.WiiU.Texture)
            {
                FtexTexture ftex = new FtexTexture(resFile, (BfresLibrary.WiiU.Texture)tex);
                return new GenericRenderer.TextureView(ftex);
            }
            else
            {
                var texture = (BfresLibrary.Switch.SwitchTexture)tex;
                BntxTexture bntxTexture = new BntxTexture(texture.BntxFile, texture.Texture);
                return new GenericRenderer.TextureView(bntxTexture);
            }
        }

        public static STGenericTexture GetTexture(ResFile resFile, TextureShared tex)
        {
            if (tex is BfresLibrary.WiiU.Texture)
            {
                return new FtexTexture(resFile, (BfresLibrary.WiiU.Texture)tex);
            }
            else
            {
                var texture = (BfresLibrary.Switch.SwitchTexture)tex;
                return new BntxTexture(texture.BntxFile, texture.Texture);
            }
        }

        static STSkeleton LoadSkeleton(Skeleton Skeleton)
        {
            STSkeleton skeleton = new STSkeleton();

            //Set the remap table
            skeleton.RemapTable.Clear();
            if (Skeleton.MatrixToBoneList != null)
            {
                for (int i = 0; i < Skeleton.MatrixToBoneList.Count; i++)
                    skeleton.RemapTable.Add(Skeleton.MatrixToBoneList[i]);
            }

            foreach (var bone in Skeleton.Bones.Values)
            {
                var genericBone = new STBone(skeleton)
                {
                    Name = bone.Name,
                    ParentIndex = bone.ParentIndex,
                    Position = new OpenTK.Vector3(
                        bone.Position.X,
                        bone.Position.Y,
                        bone.Position.Z) * GLFrameworkEngine.GLContext.PreviewScale,
                    Scale = new OpenTK.Vector3(
                        bone.Scale.X,
                        bone.Scale.Y,
                        bone.Scale.Z),
                };

                if (Skeleton.FlagsScaling == SkeletonFlagsScaling.Maya)
                    genericBone.UseSegmentScaleCompensate = true;

                if (bone.FlagsRotation == BoneFlagsRotation.EulerXYZ)
                {
                    genericBone.EulerRotation = new OpenTK.Vector3(
                        bone.Rotation.X, bone.Rotation.Y, bone.Rotation.Z);
                }
                else
                    genericBone.Rotation = new OpenTK.Quaternion(
                         bone.Rotation.X, bone.Rotation.Y,
                         bone.Rotation.Z, bone.Rotation.W);

                skeleton.Bones.Add(genericBone);
            }

            skeleton.Reset();
            return skeleton;
        }
    }
}
