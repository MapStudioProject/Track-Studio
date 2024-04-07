﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using IONET.Core.Skeleton;
using IONET.Core.Model;
using IONET.Core;
using IONET;
using BfresLibrary;
using BfresLibrary.Helpers;

namespace CafeLibrary.ModelConversion
{
    public class BfresModelExporter
    {
        public static IOScene FromGeneric(ResFile resFile, Model model)
        {
            var scene = new IOScene();
            var daeModel = new IOModel();
            scene.Models.Add(daeModel);

            daeModel.Skeleton = new IOSkeleton();
            foreach (var bone in model.Skeleton.Bones.Values)
            {
                if (bone.ParentIndex != -1)
                    continue;

                daeModel.Skeleton.RootBones.Add(ConvertBones(model.Skeleton, bone));
            }
            foreach (var mat in model.Materials.Values)
            {
                IOMaterial daeMat = new IOMaterial();
                daeMat.Name = mat.Name;
                for (int i = 0; i < mat.TextureRefs?.Count; i++)
                {
                    string sampler = mat.Samplers[i].Name;
                    string name = mat.TextureRefs[i].Name;
                    if (sampler == "_a0")
                    {
                        daeMat.DiffuseMap = new IOTexture();
                        daeMat.DiffuseMap.Name = name;
                        daeMat.DiffuseMap.FilePath = $"{name}.png";
                        daeMat.DiffuseMap.UVChannel = 0;
                        daeMat.DiffuseMap.WrapS = IONET.Core.Model.WrapMode.REPEAT;
                        daeMat.DiffuseMap.WrapT = IONET.Core.Model.WrapMode.REPEAT;
                    }
                }
                scene.Materials.Add(daeMat);
            }
            foreach (var shape in model.Shapes.Values)
            {
                IOMesh daeMesh = new IOMesh();
                daeMesh.Name = shape.Name;
                daeModel.Meshes.Add(daeMesh);

                SetVertexBuffer(daeMesh, resFile, shape, model, daeModel.Skeleton);

                var lod = shape.Meshes[0];
                daeMesh.Polygons.Add(new IOPolygon()
                {
                    Indicies = lod.GetIndices().Select(x => (int)x).ToList(),
                    PrimitiveType = IOPrimitive.TRIANGLE,
                    MaterialName = model.Materials[shape.MaterialIndex].Name,
                });
            }
            return scene;
        }

        static void SetVertexBuffer(IOMesh mesh, ResFile resFile, Shape shape, Model model, IOSkeleton skeleton)
        {
            //Load the vertex buffer into the helper to easily access the data.
            VertexBufferHelper helper = new VertexBufferHelper(model.VertexBuffers[shape.VertexBufferIndex], resFile.ByteOrder);

            // Calculate used buffers
            int indexWeightBuffers = (int)Math.Ceiling(shape.VertexSkinCount / 4.0f);

            //Get all the necessary attributes
            var positions = TryGetValues(helper, "_p0");
            var normals = TryGetValues(helper, "_n0");
            var texCoords = TryGetChannelValues(helper, "_u");
            var colors = TryGetChannelValues(helper, "_c");
            var tangents = TryGetValues(helper, "_t0");
            var bitangents = TryGetValues(helper, "_b0");

            
            List<Syroot.Maths.Vector4F[]> weightsList = new List<Syroot.Maths.Vector4F[]>();
            List<Syroot.Maths.Vector4F[]> indicesList = new List<Syroot.Maths.Vector4F[]>();
            for (int i = 0; i < indexWeightBuffers; i++)
            {
                weightsList.Add(TryGetValues(helper, $"_w{i}"));
                indicesList.Add(TryGetValues(helper, $"_i{i}"));
            }

            //Get the position attribute and use the length for the vertex count
            for (int v = 0; v < positions.Length; v++)
            {
                IOVertex vertex = new IOVertex();
                vertex.Position = ToVec3(positions[v]);
                mesh.Vertices.Add(vertex);

                if (normals.Length > 0)
                    vertex.Normal = new Vector3(normals[v].X, normals[v].Y, normals[v].Z);

                if (texCoords.Length > 0)
                {
                    for (int i = 0; i < texCoords.Length; i++)
                        vertex.SetUV(texCoords[i][v].X, 1 - texCoords[i][v].Y, i);
                }
                if (colors.Length > 0)
                {
                    for (int i = 0; i < colors.Length; i++)
                        vertex.SetColor(colors[i][v].X, colors[i][v].Y,
                                       colors[i][v].Z, colors[i][v].W, i);
                }

                if (tangents.Length > 0)
                    vertex.Tangent = new Vector3(tangents[v].X, tangents[v].Y, tangents[v].Z);
                if (bitangents.Length > 0)
                    vertex.Binormal = new Vector3(bitangents[v].X, bitangents[v].Y, bitangents[v].Z);

                for (int i = 0; i < shape.VertexSkinCount; i++)
                {
                    if (i > 3)
                        break;

                    for (int j = 0; j < indexWeightBuffers; j++)
                    {
                        //Skip 0 weights
                        if (weightsList[j].Length > 0 && weightsList[j][v][i] == 0)
                            continue;

                        int index = model.Skeleton.MatrixToBoneList[(int)indicesList[j][v][i]];

                        var daeWeight = new IOBoneWeight();
                        daeWeight.BoneName = model.Skeleton.Bones[index].Name;
                        daeWeight.Weight = weightsList[j].Length > 0 ? weightsList[j][v][i] : 1.0f;
                        vertex.Envelope.Weights.Add(daeWeight);

                        if (shape.VertexSkinCount == 1)
                        {
                            var bone = skeleton.BreathFirstOrder()[index];
                            vertex.Position = Vector3.Transform(vertex.Position, bone.WorldTransform);
                            vertex.Normal = Vector3.Transform(vertex.Normal, bone.WorldTransform);
                        }
                    }
                }
            }
        }

        static IOBone ConvertBones(Skeleton skeleton, Bone bone, IOBone parent = null)
        {
            var daeBone = new IOBone();
            daeBone.Name = bone.Name;
            daeBone.Parent = parent;
            daeBone.Translation = ToVec3(bone.Position);
            if (bone.FlagsRotation == BoneFlagsRotation.EulerXYZ)
                daeBone.RotationEuler = new Vector3(bone.Rotation.X, bone.Rotation.Y, bone.Rotation.Z);
            else
                daeBone.Rotation = new Quaternion(bone.Rotation.X, bone.Rotation.Y, bone.Rotation.Z, bone.Rotation.W);

            daeBone.Scale = ToVec3(bone.Scale);

            int index = skeleton.Bones.IndexOf(bone);
            foreach (var b in skeleton.Bones.Values)
            {
                if (b.ParentIndex == index)
                    daeBone.AddChild(ConvertBones(skeleton, b, daeBone));
            }
            return daeBone;
        }

        static System.Numerics.Vector3 ToVec3(Syroot.Maths.Vector3F vec) {
            return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
        }

        static System.Numerics.Vector2 ToVec2(Syroot.Maths.Vector2F vec) {
            return new System.Numerics.Vector2(vec.X, vec.Y);
        }

        static System.Numerics.Vector3 ToVec3(Syroot.Maths.Vector4F vec) {
            return new System.Numerics.Vector3(vec.X, vec.Y, vec.Z);
        }

        //Gets attributes with more than one channel
        static Syroot.Maths.Vector4F[][] TryGetChannelValues(VertexBufferHelper helper, string attribute)
        {
            List<Syroot.Maths.Vector4F[]> channels = new List<Syroot.Maths.Vector4F[]>();
            for (int i = 0; i < 10; i++)
            {
                if (helper.Contains($"{attribute}{i}"))
                    channels.Add(helper[$"{attribute}{i}"].Data);
                else
                    break;
            }
            return channels.ToArray();
        }

        //Gets the attribute data given the attribute key.
        static Syroot.Maths.Vector4F[] TryGetValues(VertexBufferHelper helper, string attribute)
        {
            if (helper.Contains(attribute))
                return helper[attribute].Data;
            else
                return new Syroot.Maths.Vector4F[0];
        }
    }
}
