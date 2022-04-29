using System;
using System.Collections.Generic;
using System.Linq;
using BfresLibrary.Helpers;
using BfresLibrary;
using BfresLibrary.GX2;
using System.IO;
using Syroot.Maths;
using Toolbox.Core.IO;
using Toolbox.Core;
using BfresLibrary.TextConvert;
using IONET.Core.Skeleton;
using IONET.Core.Model;
using IONET.Core;
using IONET;

namespace CafeLibrary.ModelConversion
{
    public class BfresModelImporter
    {
        static System.Numerics.Matrix4x4 GlobalTransform;

        public static Model ImportModel(ResFile resFile, Model model, IOScene scene, string filePath, ModelImportSettings importSettings)
        {
            var fmdl = ConvertScene(resFile, model, scene, importSettings);
            fmdl.Name = Path.GetFileNameWithoutExtension(filePath);
            return fmdl;
        }

        public static Model ImportModel(ResFile resFile, IOScene scene, string filePath, ModelImportSettings importSettings)
        {
            var fmdl = ConvertScene(resFile, new Model(), scene, importSettings);
            fmdl.Name = Path.GetFileNameWithoutExtension(filePath);
            return fmdl;
        }

        public static Model ImportModel(ResFile resFile, string filePath, ModelImportSettings importSettings)
        {
            return ImportModel(resFile, new Model(), filePath, importSettings);
        }

        public static Model ImportModel(ResFile resFile, Model model, string filePath, ModelImportSettings importSettings)
        {
            if (filePath.EndsWith(".bfmdl"))
            {
                model.Import(filePath, resFile);
                return model;
            }

            var settings = new ImportSettings()
            {
                Optimize = true,
                GenerateTangentsAndBinormals = true,
                FlipUVs = importSettings.FlipUVs,
            };

            string ext = Path.GetExtension(filePath);
            MapStudio.UI.ProcessLoading.Instance.Update(10, 100, $"Loading {ext} file.");

            GlobalTransform = System.Numerics.Matrix4x4.Identity;
            IOScene scene = IOManager.LoadScene(filePath, settings);

            var fmdl = ConvertScene(resFile, model, scene, importSettings);
            fmdl.Name = Path.GetFileNameWithoutExtension(filePath);
            return fmdl;
        }

        static Model ConvertScene(ResFile resFile, Model fmdl, IOScene scene, ModelImportSettings importSettings)
        {
            var model = scene.Models.FirstOrDefault();

            fmdl.Name = model.Name;

            if (importSettings.Replacing)
            {
                fmdl.Materials.Clear();
                fmdl.Shapes.Clear();
                fmdl.VertexBuffers.Clear();
            }

            MapStudio.UI.ProcessLoading.Instance.Update(40, 100, $"Loading bfres materials.");

            foreach (var mat in scene.Materials)
            {
                if (mat.Label == null && mat.Name == null)
                    continue;

                string GetTextureName(IOTexture texture)
                {
                    string name = Path.GetFileNameWithoutExtension(mat.DiffuseMap.FilePath);
                    return name.Replace("%20", " ");
                }

                Material fmat = new Material();
                fmat.Name = mat.Label != null ? mat.Label : mat.Name;
                if (mat.DiffuseMap != null)
                {
                    fmat.Samplers.Add("_a0", new Sampler() { Name = "_a0", TexSampler = new TexSampler()});
                    fmat.TextureRefs.Add(new TextureRef() { Name = GetTextureName(mat.DiffuseMap) });
                }
                if (mat.EmissionMap != null)
                {
                    fmat.Samplers.Add("_e0", new Sampler() { Name = "_e0", TexSampler = new TexSampler() });
                    fmat.TextureRefs.Add(new TextureRef() { Name = GetTextureName(mat.EmissionMap) });
                }
                if (mat.SpecularMap != null)
                {
                    fmat.Samplers.Add("_s0", new Sampler() { Name = "_s0", TexSampler = new TexSampler() });
                    fmat.TextureRefs.Add(new TextureRef() { Name = GetTextureName(mat.SpecularMap) });
                }
                fmdl.Materials.Add(fmat.Name, fmat);
            }
            if (fmdl.Materials.Count == 0)
            {
                Material fmat = new Material();
                fmat.Name = "NewMaterial";
                fmdl.Materials.Add(fmat.Name, fmat);
            }

            if (importSettings.ImportBones)
            {
                fmdl.Skeleton = new Skeleton();
                fmdl.Skeleton.FlagsRotation = SkeletonFlagsRotation.EulerXYZ;

                MapStudio.UI.ProcessLoading.Instance.Update(50, 100, $"Loading bfres bones.");

                foreach (var bone in model.Skeleton.BreathFirstOrder())
                {
                    if (string.IsNullOrEmpty(bone.Name))
                        continue;

                    Vector4F rotation = new Vector4F(
                        bone.RotationEuler.X,
                        bone.RotationEuler.Y,
                        bone.RotationEuler.Z,
                        1.0f);

                    if (fmdl.Skeleton.FlagsRotation == SkeletonFlagsRotation.Quaternion)
                        rotation = new Vector4F(bone.Rotation.X, bone.Rotation.Y, bone.Rotation.Z, bone.Rotation.W);

                    var bfresBone = new Bone()
                    {
                        FlagsRotation = BoneFlagsRotation.EulerXYZ,
                        FlagsTransform = SetBoneFlags(bone),
                        Name = bone.Name,
                        RigidMatrixIndex = -1,  //Gets calculated after
                        SmoothMatrixIndex = -1, //Gets calculated after
                        ParentIndex = (short)model.Skeleton.IndexOf(bone.Parent),
                        Position = new Vector3F(
                             bone.Translation.X,
                             bone.Translation.Y,
                             bone.Translation.Z),
                        Scale = new Vector3F(
                             bone.Scale.X,
                             bone.Scale.Y,
                             bone.Scale.Z),
                        Rotation = rotation,
                        Visible = true,
                    };
                    fmdl.Skeleton.Bones.Add(bone.Name, bfresBone);
                }
            }

            if (fmdl.Skeleton.Bones.Count == 0) {
                fmdl.Skeleton.Bones.Add("Root", new Bone() { Name = "Root" });
            }

            List<int> smoothSkinningIndices = new List<int>();
            List<int> rigidSkinningIndices = new List<int>();

            uint[] skinCounts = new uint[model.Meshes.Count];
            int sindex = 0;

            MapStudio.UI.ProcessLoading.Instance.Update(70, 100, $"calculating skinning data");

            //Determine the rigid and smooth bone skinning
            foreach (var mesh in model.Meshes)
            {
                if (mesh.Vertices.Count == 0)
                    continue;


                //Set the skin count
                uint vertexSkinCount = 0;

                try
                {
                    bool rigidBind = false;
                    //Check if the bone list has only 1 bone or less. If so it uses rigid binding
                    var riggedBones = mesh.Vertices.SelectMany(x => x.Envelope.Weights?.Select(x => x.BoneName)).Distinct().ToList();
                     if (riggedBones == null || riggedBones.Count <= 1)
                        rigidBind = true;

                    if (!rigidBind)
                        CalculateSkinCount(mesh.Vertices);

                    //Todo. This basically reimports meshes with the original skin count to target as
                    /*    if (importSettings.Meshes.Any(x => x.Name == mesh.Name))
                        {
                            var meshSettings = importSettings.Meshes.FirstOrDefault(x => x.Name == mesh.Name);
                            vertexSkinCount = (uint)meshSettings.SkinCount;
                        }*/

                    //Set the skin count for each mesh. This is either calculated or applied via mesh meta info
                    skinCounts[sindex++] = vertexSkinCount;

                    //Transform rigid bindings into worldspace
                    if (rigidBind && riggedBones?.Count == 1)
                    {
                        //For rigid bind types we will fully transform the model into worldspace 
                        var bn = model.Skeleton.BreathFirstOrder().Where(x => x.Name == riggedBones[0]).FirstOrDefault();
                        if (bn != null)
                            mesh.TransformVertices(bn.WorldTransform);

                        //Set the bone into identity as we want these to be applied
                        var bfresBone = fmdl.Skeleton.Bones.Values.Where(x => x.Name == riggedBones[0]).FirstOrDefault();
                        if (bfresBone != null)
                        {
                            bfresBone.Position = new Vector3F(0, 0, 0);
                            bfresBone.Rotation = new Vector4F(0, 0, 0, 1);
                            bfresBone.Scale = new Vector3F(1, 1, 1);
                        }
                        continue;
                    }
                }
                catch
                {

                }

                foreach (var vertex in mesh.Vertices)
                {
                    foreach (var weight in vertex.Envelope.Weights)
                    {
                        var bn = fmdl.Skeleton.Bones.Values.Where(x => x.Name == weight.BoneName).FirstOrDefault();
                        if (bn != null)
                        {
                            int index = fmdl.Skeleton.Bones.IndexOf(bn);

                            //Rigid skinning
                            if (vertexSkinCount == 1)
                            {
                                if (!rigidSkinningIndices.Contains(index))
                                    rigidSkinningIndices.Add(index);
                            }
                            else
                            {
                                if (!smoothSkinningIndices.Contains(index))
                                    smoothSkinningIndices.Add(index);
                            }
                        }
                    }
                }
            }

            //Sort indices
            smoothSkinningIndices.Sort();
            rigidSkinningIndices.Sort();

            //Create a global skinning list. Smooth indices first, rigid indices last
            List<int> skinningIndices = new List<int>();
            skinningIndices.AddRange(smoothSkinningIndices);
            skinningIndices.AddRange(rigidSkinningIndices);

            //Next update the bone's skinning index value
            foreach (var index in smoothSkinningIndices)
            {
                var bone = fmdl.Skeleton.Bones[index];
                bone.SmoothMatrixIndex = (short)smoothSkinningIndices.IndexOf(index);
            }
            //Rigid indices go after smooth indices
            //Here we do not index the global iist as the global list can include the same index in both smooth/rigid
            foreach (var index in rigidSkinningIndices)
            {
                var bone = fmdl.Skeleton.Bones[index];
                bone.RigidMatrixIndex = (short)(smoothSkinningIndices.Count + rigidSkinningIndices.IndexOf(index));
            }

            //Turn them into ushorts for the final list in the binary
            fmdl.Skeleton.MatrixToBoneList = new List<ushort>();
            for (int i = 0; i < skinningIndices.Count; i++)
                fmdl.Skeleton.MatrixToBoneList.Add((ushort)skinningIndices[i]);

            //Generate inverse matrices
            fmdl.Skeleton.InverseModelMatrices = new List<Matrix3x4>();
            foreach (var bone in fmdl.Skeleton.Bones.Values)
            {
                //Set identity types for none smooth bones
                bone.InverseMatrix = new Matrix3x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);

                //Inverse matrices are used for smooth bone skinning
                if (bone.SmoothMatrixIndex == -1)
                    continue;

                var transform = MatrixExenstion.GetMatrixInverted(fmdl.Skeleton, bone);
                //Assign the inverse matrix directly for older versions that store it directly
                bone.InverseMatrix = transform;
                //Add it to the global inverse list
                fmdl.Skeleton.InverseModelMatrices.Add(transform);
            }

            int meshIndex = 0;
            foreach (var mesh in model.Meshes)
            {
                if (mesh.Vertices.Count == 0)
                    continue;

                MapStudio.UI.ProcessLoading.Instance.Update(meshIndex, model.Meshes.Count, $"Importing mesh {mesh.Name}");

                var meshSettings = new ModelImportSettings.MeshSettings();

                if (importSettings.Meshes.Any(x => x.Name == mesh.Name))
                {
                    meshSettings = importSettings.Meshes.FirstOrDefault(x => x.Name == mesh.Name);
                    if (meshSettings.SkinCount > 0)
                    {
                        foreach (var v in mesh.Vertices)
                        {
                            //Optimize weights
                            //v.Envelope.Optimize(meshSettings.SkinCount);
                        }
                    }
                }

                var names = fmdl.Shapes.Values.Select(x => x.Name).ToList();

                Shape fshp = new Shape();
                fshp.Name = Utils.RenameDuplicateString(mesh.Name, names, 0, 2);
                fshp.MaterialIndex = 0;
                fshp.BoneIndex = 0;
                fshp.VertexSkinCount = (byte)skinCounts[meshIndex++];

                fshp.SkinBoneIndices = new List<ushort>();
                foreach (var vertex in mesh.Vertices)
                {
                    foreach (var weight in vertex.Envelope.Weights)
                    {
                        var bn = fmdl.Skeleton.Bones.Values.Where(x => x.Name == weight.BoneName).FirstOrDefault();
                        if (bn != null)
                        {
                            ushort index = (ushort)fmdl.Skeleton.Bones.IndexOf(bn);
                            if (!fshp.SkinBoneIndices.Contains(index))
                                fshp.SkinBoneIndices.Add(index);
                        }
                    }
                }

                //Get the original material and map by string key
                string material = mesh.Polygons[0].MaterialName;
                int materialIndex = scene.Materials.FindIndex(x => x.Name == material);
                if (materialIndex != -1)
                    fshp.MaterialIndex = (ushort)materialIndex;
                else
                    Console.WriteLine($"Failed to find material {material}");

                //Generate a vertex buffer
                VertexBuffer buffer = GenerateVertexBuffer(resFile, mesh, fshp,
                    meshSettings, model.Skeleton.BreathFirstOrder(), fmdl.Skeleton, rigidSkinningIndices, smoothSkinningIndices);

                fshp.VertexBufferIndex = (ushort)fmdl.VertexBuffers.Count;
                fshp.VertexSkinCount = (byte)buffer.VertexSkinCount;
                fmdl.VertexBuffers.Add(buffer);

                //Generate boundings for the mesh
                List<IOVertex> vertices = mesh.Vertices;

                var boundingBox = CalculateBoundingBox(vertices, model.Skeleton.BreathFirstOrder(), fshp.VertexSkinCount > 0);
                fshp.SubMeshBoundings.Add(boundingBox); //Create bounding for total mesh
                fshp.SubMeshBoundings.Add(boundingBox); //Create bounding for single sub meshes

                Vector3F min = boundingBox.Center - boundingBox.Extent;
                Vector3F max = boundingBox.Center + boundingBox.Extent;
                float sphereRadius = (float)(boundingBox.Center.Length + boundingBox.Extent.Length);

               // var sphereRadius = GetBoundingSphereFromRegion(new Vector4F(
               //     min.X, min.Y, min.Z, 1), new Vector4F(max.X, max.Y, max.Z, 1));

                fshp.RadiusArray.Add(sphereRadius); //Total radius (per LOD)

                var poly = mesh.Polygons[0];

                Console.WriteLine($"BOUDNING {sphereRadius}");

                //A mesh represents a single level of detail. Here we can create additional level of detail meshes if supported.
                Mesh bMesh = new Mesh();
                bMesh.PrimitiveType = GX2PrimitiveType.Triangles;
                CalculateSubMeshes(resFile, fshp, bMesh, vertices, poly.Indicies.Select(x => x).ToList(), importSettings.EnableSubMesh);

                //Add the lod to the shape
                fshp.Meshes.Add(bMesh);

                if (importSettings.EnableLODs)
                {
                    var lod1 = new Mesh()
                    {
                        FirstVertex = 0,
                        SubMeshes = bMesh.SubMeshes.ToList(),
                        IndexBuffer = bMesh.IndexBuffer,
                        PrimitiveType = bMesh.PrimitiveType,
                    };
                    lod1.SetIndices(bMesh.GetIndices().ToList());
                    fshp.Meshes.Add(lod1);
                    var lod2 = new Mesh()
                    {
                        FirstVertex = 0,
                        SubMeshes = bMesh.SubMeshes.ToList(),
                        IndexBuffer = bMesh.IndexBuffer,
                        PrimitiveType = bMesh.PrimitiveType,
                    };
                    lod2.SetIndices(bMesh.GetIndices().ToList());
                    fshp.Meshes.Add(lod2);
                }

                //Calculate the bounding tree
                // CalculateBoundingTree(fshp);

                //Finally add the shape to the model
                fmdl.Shapes.Add(fshp.Name, fshp);
            }

            return fmdl;
        }

        static void CalculateSubMeshes(ResFile resFile, Shape fshp, Mesh mesh, List<IOVertex> vertices, List<int> indices, bool enableSubMesh)
        {
            List<uint> indexList = new List<uint>();
            indexList = indices.Select(x => (uint)x).ToList();

            //If a mesh's vertex data is split into parts, we can create sub meshes with their own boundings
            CalculateMeshDivision(vertices, fshp, mesh, indices, ref indexList, enableSubMesh);

            //Finally setup the full index list of the entire mesh
            GX2IndexFormat Format = GX2IndexFormat.UInt16;
            if (resFile.IsPlatformSwitch)
            {
                Format = GX2IndexFormat.UInt16LittleEndian;
                if (indexList.Any(x => x > ushort.MaxValue))
                    Format = GX2IndexFormat.UInt32LittleEndian;
            }
            else
            {
                if (indexList.Any(x => x > ushort.MaxValue))
                    Format = GX2IndexFormat.UInt32;
            }

            mesh.SetIndices(indexList, Format);
        }

        static void CalculateMeshDivision(List<IOVertex> vertices, Shape fshp, Mesh mesh, List<int> indices, ref List<uint> indexList, bool enableSubMesh)
        {
            bool divide = false;
            if (enableSubMesh)
            {
                var bb = fshp.SubMeshBoundings[0];
                int numSubMeshes = 2;

                fshp.SubMeshBoundingIndices.Clear();
                fshp.SubMeshBoundingNodes.Clear();
                fshp.SubMeshBoundings.Clear();
                for (int i = 0; i < numSubMeshes; i++)
                {
                    ushort index = (ushort)i;

                    fshp.SubMeshBoundingIndices.Add(index);
                    fshp.SubMeshBoundings.Add(bb);
                    fshp.SubMeshBoundingNodes.Add(new BoundingNode()
                    {
                        NextSibling = index,
                        LeftChildIndex = index,
                        RightChildIndex = index,
                        Unknown = index,
                        SubMeshCount = 1,
                        SubMeshIndex = index,
                    });
                }

                //Single mesh
                mesh.SubMeshes.Add(new SubMesh()
                {
                    Offset = 0,
                    Count = (uint)indices.Count,
                });
                mesh.SubMeshes.Add(new SubMesh()
                {
                    Offset = 0,
                    Count = 1,
                });
            }
            else if (!divide)
            {
                //Single mesh
                mesh.SubMeshes.Add(new SubMesh()
                {
                    Offset = 0,
                    Count = (uint)indices.Count,
                });
                fshp.SubMeshBoundingIndices.Add(0);
                fshp.SubMeshBoundingNodes.Add(new BoundingNode()
                {
                    LeftChildIndex = 0,
                    RightChildIndex = 0,
                    NextSibling = 0,
                    SubMeshIndex = 0,
                    Unknown = 0,
                    SubMeshCount = 1,
                });
            }
            else
            {
                //Divided up. Update the index list, boundings and sub mesh lists
                indexList.Clear();
                var divided = PolygonDivision.Divide(vertices, indices, new PolygonDivision.PolygonSettings()
                {

                });
                int offset = 0;

                fshp.SubMeshBoundingNodes.Clear();
                fshp.SubMeshBoundings.Clear();

                foreach (var root in divided)
                    AddSubMesh(root, fshp, mesh, ref offset, ref indexList);
            }

        }

        static void AddSubMesh(PolygonDivision.PolygonOctree subMesh, Shape shape, Mesh mesh,
      ref int offset, ref List<uint> indexList)
        {
            indexList.AddRange(subMesh.TriangleIndices.Select(x => (uint)x).ToList());

            offset += subMesh.TriangleIndices.Count;

            mesh.SubMeshes.Add(new SubMesh()
            {
                Offset = (uint)offset,
                Count = (uint)indexList.Count,
            });

            foreach (var child in subMesh.Children)
                AddSubMesh(child, shape, mesh, ref offset, ref indexList);
        }

        private static float CalculateRadius(float horizontalLeg, float verticalLeg)
        {
            return (float)Math.Sqrt((horizontalLeg * horizontalLeg) + (verticalLeg * verticalLeg));
        }

        private static float GetBoundingSphereFromRegion(Vector4F min, Vector4F max)
        {
            // The radius should be the hypotenuse of the triangle.
            // This ensures the sphere contains all points.
            Vector4F lengths = max - min;
            return CalculateRadius(lengths.X / 2.0f, lengths.Y / 2.0f);
        }

        private static BoneFlagsTransform SetBoneFlags(IOBone bn)
        {
            BoneFlagsTransform flags = BoneFlagsTransform.None;
            if (bn.Translation == System.Numerics.Vector3.Zero)
                flags |= BoneFlagsTransform.TranslateZero;
            if (bn.RotationEuler == System.Numerics.Vector3.Zero)
                flags |= BoneFlagsTransform.RotateZero;
            if (bn.Scale == System.Numerics.Vector3.One)
                flags |= BoneFlagsTransform.ScaleOne;
            return flags;
        }

        private static byte CalculateSkinCount(List<IOVertex> vertices)
        {
            uint numSkinning = 0;
            for (int v = 0; v < vertices.Count; v++)
                numSkinning = Math.Max(numSkinning, (uint)vertices[v].Envelope.Weights.Count);
            return (byte)numSkinning;
        }

        private static Dictionary<string, AABB> CalculateBoneAABB(List<IOVertex> vertices, List<IOBone> bones)
        {
            Dictionary<string, AABB> skinnedBoundings = new Dictionary<string, AABB>();
            for (int i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[i];

                foreach (var boneID in vertex.Envelope.Weights)
                {
                    if (!skinnedBoundings.ContainsKey(boneID.BoneName))
                        skinnedBoundings.Add(boneID.BoneName.ToString(), new AABB());

                    var transform = bones.FirstOrDefault(x => x.Name == boneID.BoneName).WorldTransform;
                    System.Numerics.Matrix4x4.Invert(transform, out System.Numerics.Matrix4x4 inverted);

                    //Get the position in local coordinates
                    var position = vertices[i].Position;
                    position = System.Numerics.Vector3.Transform(position, inverted);

                    var bounding = skinnedBoundings[boneID.BoneName];
                    bounding.minX = Math.Min(bounding.minX, position.X);
                    bounding.minY = Math.Min(bounding.minY, position.Y);
                    bounding.minZ = Math.Min(bounding.minZ, position.Z);
                    bounding.maxX = Math.Max(bounding.maxX, position.X);
                    bounding.maxY = Math.Max(bounding.maxY, position.Y);
                    bounding.maxZ = Math.Max(bounding.maxZ, position.Z);
                }
            }
            return skinnedBoundings;
        }

        class AABB
        {
            public float minX = float.MaxValue;
            public float minY = float.MaxValue;
            public float minZ = float.MaxValue;
            public float maxX = float.MinValue;
            public float maxY = float.MinValue;
            public float maxZ = float.MinValue;

            public OpenTK.Vector3 Max => new OpenTK.Vector3(maxX, maxY, maxZ);
            public OpenTK.Vector3 Min => new OpenTK.Vector3(minX, minY, minZ);

        }

        private static Bounding CalculateBoundingBox(List<IOVertex> vertices, List<IOBone> bones, bool isSmoothSkinning)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;

            if (isSmoothSkinning)
            {
                var boundings = CalculateBoneAABB(vertices, bones);
                //Find largest bounding box
                foreach (var bounding in boundings.Values)
                {
                    minX = Math.Min(minX, bounding.minX);
                    minY = Math.Min(minY, bounding.minY);
                    minZ = Math.Min(minZ, bounding.minZ);
                    maxX = Math.Max(maxX, bounding.maxX);
                    maxY = Math.Max(maxY, bounding.maxY);
                    maxZ = Math.Max(maxZ, bounding.maxZ);
                }
                return CalculateBoundingBox(
                    new Vector3F(minX, minY, minZ),
                    new Vector3F(maxX, maxY, maxZ));
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                minX = Math.Min(minX, vertices[i].Position.X);
                minY = Math.Min(minY, vertices[i].Position.Y);
                minZ = Math.Min(minZ, vertices[i].Position.Z);
                maxX = Math.Max(maxX, vertices[i].Position.X);
                maxY = Math.Max(maxY, vertices[i].Position.Y);
                maxZ = Math.Max(maxZ, vertices[i].Position.Z);
            }

            return CalculateBoundingBox(
                new Vector3F(minX, minY, minZ),
                new Vector3F(maxX, maxY, maxZ));
        }

        private static Bounding CalculateBoundingBox(Vector3F min, Vector3F max)
        {
            Vector3F center = max + min;

            Console.WriteLine($"min {min}");
            Console.WriteLine($"max {max}");

            float xxMax = GetExtent(max.X, min.X);
            float yyMax = GetExtent(max.Y, min.Y);
            float zzMax = GetExtent(max.Z, min.Z);

            Vector3F extend = new Vector3F(xxMax, yyMax, zzMax);

            return new Bounding()
            {
                Center = new Vector3F(center.X, center.Y, center.Z),
                Extent = new Vector3F(extend.X, extend.Y, extend.Z),
            };
        }



        private static float GetExtent(float max, float min)
        {
            return (float)Math.Max(Math.Sqrt(max * max), Math.Sqrt(min * min));
        }

        private static VertexBuffer GenerateVertexBuffer(ResFile resFile, IOMesh mesh, Shape fshp, ModelImportSettings.MeshSettings settings,
           List<IOBone> bones, Skeleton fskl, List<int> rigidIndices, List<int> smoothIndices)
        {
            List<IOVertex> vertices = mesh.Vertices;

            VertexBufferHelper vertexBufferHelper = new VertexBufferHelper(
                new VertexBuffer(), resFile.ByteOrder);

            List<Vector4F> Positions = new List<Vector4F>();
            List<Vector4F> Normals = new List<Vector4F>();
            List<Vector4F> BoneWeights = new List<Vector4F>();
            List<Vector4F> BoneIndices = new List<Vector4F>();
            List<Vector4F> Tangents = new List<Vector4F>();
            List<Vector4F> Bitangents = new List<Vector4F>();

            int numTexCoords = vertices.Max(x => x.UVs.Count);
            int numColors = vertices.Max(x => x.Colors.Count);

            Vector4F[][] TexCoords = new Vector4F[numTexCoords][];
            Vector4F[][] Colors = new Vector4F[numColors][];

            for (int c = 0; c < numColors; c++)
                Colors[c] = new Vector4F[vertices.Count];
            for (int c = 0; c < numTexCoords; c++)
                TexCoords[c] = new Vector4F[vertices.Count];

            List<string> missingBones = new List<string>();
            bool hasWeights = vertices.Any(x => x.Envelope.Weights?.Count > 0);
            for (int v = 0; v < vertices.Count; v++)
            {
                var vertex = vertices[v];

                var position = vertex.Position;
                var normal = vertex.Normal;

                //Reset rigid skinning types to local space
                if (fshp.VertexSkinCount == 0 && bones.Count > 0)
                {
                    var transform = bones[fshp.BoneIndex].WorldTransform;
                    System.Numerics.Matrix4x4.Invert(transform, out System.Numerics.Matrix4x4 inverted);
                    position = System.Numerics.Vector3.Transform(position, inverted);
                    normal = System.Numerics.Vector3.TransformNormal(normal, inverted);
                }
                //Reset rigid skinning types to local space
                if (fshp.VertexSkinCount == 1)
                {
                    var bone = bones.FirstOrDefault(x => x.Name == vertex.Envelope.Weights[0].BoneName);
                    var transform = bone.WorldTransform;
                    System.Numerics.Matrix4x4.Invert(transform, out System.Numerics.Matrix4x4 inverted);
                    position = System.Numerics.Vector3.Transform(position, inverted);
                    normal = System.Numerics.Vector3.TransformNormal(normal, inverted);
                }

               // position = System.Numerics.Vector3.Transform(position, GlobalTransform);
               // normal = System.Numerics.Vector3.Transform(normal, GlobalTransform);

                Positions.Add(new Vector4F(
                    position.X,
                    position.Y,
                    position.Z, 0));

                Normals.Add(new Vector4F(
                    normal.X,
                    normal.Y,
                    normal.Z, 0));

                if (settings.Tangent.Enable)
                {
                    Tangents.Add(new Vector4F(
                        vertex.Tangent.X,
                        vertex.Tangent.Y,
                        vertex.Tangent.Z, 0));
                }

                if (settings.Bitangent.Enable)
                {
                    Bitangents.Add(new Vector4F(
                        vertex.Binormal.X,
                        vertex.Binormal.Y,
                        vertex.Binormal.Z, 0));
                }

                for (int i = 0; i < vertex.UVs?.Count; i++)
                {
                    TexCoords[i][v] = new Vector4F(
                        vertex.UVs[i].X,
                        1 - vertex.UVs[i].Y,
                        0, 0);
                }

                for (int i = 0; i < vertex.Colors?.Count; i++)
                {
                    Colors[i][v] = new Vector4F(
                        vertex.Colors[i].X,
                        vertex.Colors[i].Y,
                        vertex.Colors[i].Z,
                        vertex.Colors[i].W);
                }

                int[] indices = new int[4];
                float[] weights = new float[4];
                for (int j = 0; j < vertex.Envelope.Weights?.Count; j++)
                {
                    int index = Array.FindIndex(fskl.Bones.Values.ToArray(), x => x.Name == vertex.Envelope.Weights[j].BoneName);
                    if (index == -1)
                    {
                        if (!missingBones.Contains(vertex.Envelope.Weights[j].BoneName))
                            missingBones.Add(vertex.Envelope.Weights[j].BoneName);
                        continue;
                    }

                    //Check for the index in the proper skinning index lists
                    if (fshp.VertexSkinCount > 1)
                    {
                        indices[j] = smoothIndices.IndexOf(index);
                        weights[j] = vertex.Envelope.Weights[j].Weight;
                    }
                    else
                    {
                        //Rigid indices start after smooth indices in the global index list
                        //Smooth indices can have the same bone index as a rigid one, so it's best to index the specific list
                        indices[j] = smoothIndices.Count + rigidIndices.IndexOf(index);
                        weights[j] = vertex.Envelope.Weights[j].Weight;
                    }
                }

                if (hasWeights && settings.BoneIndices.Enable && fshp.VertexSkinCount > 0)
                {
                    BoneWeights.Add(new Vector4F(weights[0], weights[1], weights[2], weights[3]));
                    BoneIndices.Add(new Vector4F(indices[0], indices[1], indices[2], indices[3]));
                }
            }

            if(missingBones.Count > 0)
            {
                foreach (var bone in missingBones)
                    StudioLogger.WriteWarning($"Missing bone {bone} on mesh {mesh.Name}");
            }

            List<VertexBufferHelperAttrib> attributes = new List<VertexBufferHelperAttrib>();
            attributes.Add(new VertexBufferHelperAttrib()
            {
                Name = "_p0",
                Data = Positions.ToArray(),
                Format = settings.Position.Format,
            });

            if (Normals.Count > 0)
            {
                attributes.Add(new VertexBufferHelperAttrib()
                {
                    Name = "_n0",
                    Data = Normals.ToArray(),
                    Format = settings.Normal.Format,
                });
            }

            if (Tangents.Count > 0)
            {
                attributes.Add(new VertexBufferHelperAttrib()
                {
                    Name = "_t0",
                    Data = Tangents.ToArray(),
                    Format = settings.Tangent.Format,
                });
            }

            if (Bitangents.Count > 0)
            {
                attributes.Add(new VertexBufferHelperAttrib()
                {
                    Name = "_b0",
                    Data = Bitangents.ToArray(),
                    Format = settings.Bitangent.Format,
                });
            }

            for (int i = 0; i < TexCoords.Length; i++)
            {
                if (settings.UseTexCoord[i])
                {
                    attributes.Add(new VertexBufferHelperAttrib()
                    {
                        Name = $"_u{i}",
                        Data = TexCoords[i],
                        Format = settings.UVs.Format,
                    });
                }
            }

            for (int i = 0; i < Colors.Length; i++)
            {
                if (settings.UseColor[i])
                {
                    attributes.Add(new VertexBufferHelperAttrib()
                    {
                        Name = $"_c{i}",
                        Data = Colors[i],
                        Format = settings.Colors.Format,
                    });
                }
            }

            if (BoneIndices.Count > 0)
            {
                attributes.Add(new VertexBufferHelperAttrib()
                {
                    Name = "_i0",
                    Data = BoneIndices.ToArray(),
                    Format = settings.BoneIndices.Format,
                });
            }

            if (BoneWeights.Count > 0)
            {
                attributes.Add(new VertexBufferHelperAttrib()
                {
                    Name = "_w0",
                    Data = BoneWeights.ToArray(),
                    Format = settings.BoneWeights.Format,
                });
            }

            foreach (var att in settings.AttributeLayout)
            {
                var attribute = attributes.FirstOrDefault(x => x.Name == att.Name);
                if (attribute != null)
                    attribute.BufferIndex = att.BufferIndex;
            }

            vertexBufferHelper.Attributes = attributes;
            var buffer = vertexBufferHelper.ToVertexBuffer();
            buffer.VertexSkinCount = (byte)fshp.VertexSkinCount;
            return buffer;
        }

        public class MeshSettings
        {
            public bool UseNormal { get; set; }

            public bool UseBoneWeights { get; set; }
            public bool UseBoneIndices { get; set; }
            public bool UseTangents { get; set; }
            public bool UseBitangents { get; set; }

            public GX2AttribFormat PositionFormat = GX2AttribFormat.Format_32_32_32_Single;
            public GX2AttribFormat NormalFormat = GX2AttribFormat.Format_10_10_10_2_SNorm;
            public GX2AttribFormat TexCoordFormat = GX2AttribFormat.Format_16_16_Single;
            public GX2AttribFormat ColorFormat = GX2AttribFormat.Format_16_16_16_16_Single;
            public GX2AttribFormat TangentFormat = GX2AttribFormat.Format_8_8_8_8_SNorm;
            public GX2AttribFormat BitangentFormat = GX2AttribFormat.Format_8_8_8_8_SNorm;

            public GX2AttribFormat BoneIndicesFormat = GX2AttribFormat.Format_8_8_8_8_UInt;
            public GX2AttribFormat BoneWeightsFormat = GX2AttribFormat.Format_8_8_8_8_UNorm;
        }
    }
}
