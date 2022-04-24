using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BfresLibrary;
using BfresLibrary.Helpers;
using System.IO;
using System.Numerics;

namespace AGraphicsLibrary
{
    /// <summary>
    /// Represents a collection of area models which determine the boundings of certain effects
    /// These can include bloom, fog, hemi, and also cube map objects from course area.
    /// </summary>
    public class AreaCollection
    {
        private ResFile ResFile;
        private Model Model;

        public List<AreaEnvObject> Objects = new List<AreaEnvObject>();

        public AreaCollection()
        {

        }

        private void CreateEmpty()
        {
            Model = new Model();
            Model.Name = "area";
            ResFile.Models.Add(Model.Name, Model);
        }

        public AreaCollection(byte[] fileData) {
            Load(new ResFile(new MemoryStream(fileData)));
        }

        public AreaCollection(Stream fileData) {
            Load(new ResFile(fileData));
        }

        public AreaEnvObject GetArea(float positionX, float positionY, float positionZ)
        {
            if (Objects.Count == 0)
                return new AreaEnvObject();

            foreach (var area in Objects)
            {
                if (area.HasHit(positionX, positionY, positionZ)) {
                    return area;
                }
            }

            return Objects.FirstOrDefault();
        }

        private void Load(ResFile resFile)
        {
            ResFile = resFile;
            Model = ResFile.Models.Values.FirstOrDefault();
            if (Model == null)
                return;

            foreach (var shape in Model.Shapes.Values)
            {
                //For vertex data of the bounding cube
                var vertexData = Model.VertexBuffers[shape.VertexBufferIndex];
                //Used for mapping env effects
                var materialData = Model.Materials[shape.MaterialIndex];
                //Used for transforming
                var boneData = Model.Skeleton.Bones[shape.BoneIndex];
                Objects.Add(new AreaEnvObject()
                {
                    MaterialData = materialData,
                    MeshData = shape,
                    VertexData = vertexData,
                    TransformData = boneData,
                    ResFile = ResFile,
                });
            }
            foreach (var obj in Objects)
                obj.UpdateMinMax();
        }

        public Stream Save() {
            //Reload the model data
            Model.Shapes.Clear();
            Model.VertexBuffers.Clear();
            Model.Materials.Clear();
            Model.Skeleton.Bones.Clear();
            //Create a dummy root bone automatically
            Model.Skeleton.Bones.Add("dummy_area", new Bone()
            {
                Name = "dummy_area",
                ParentIndex = -1,
                Position = new Syroot.Maths.Vector3F(),
                Scale = new Syroot.Maths.Vector3F(1, 1, 1),
                Rotation = new Syroot.Maths.Vector4F(0, 0, 0, 1),
            });
            //Fill the resource with the object data.
            foreach (var obj in Objects)
            {
                Model.Shapes.Add(obj.MeshData.Name, obj.MeshData);
                Model.VertexBuffers.Add(obj.VertexData);
                if (!Model.Materials.ContainsKey(obj.MaterialData.Name))
                    Model.Materials.Add(obj.MaterialData.Name, obj.MaterialData);
                if (!Model.Skeleton.Bones.ContainsKey(obj.TransformData.Name))
                    Model.Skeleton.Bones.Add(obj.TransformData.Name, obj.TransformData);
            }

            var mem = new MemoryStream();
            ResFile.Save(mem);
            return mem;
        }
    }

    public class AreaEnvObject
    {
        public Shape MeshData { get; set; }
        public VertexBuffer VertexData { get; set; }
        public Bone TransformData { get; set; }
        public Material MaterialData { get; set; }
        public ResFile ResFile { get; set; }

        public int AreaIndex => MeshData.MaterialIndex; //Areas are always split by materials

        public Vector3 Max { get; set; }
        public Vector3 Min { get; set; }

        public AreaEnvObject()
        {
            MeshData = new Shape();
            MeshData.MaterialIndex = 0;
            MaterialData = new Material();
            MaterialData.SetRenderInfo("gsys_areaenv_envmap", "area0");
        }

        public bool HasHit(float positionX, float positionY, float positionZ)
        {
            return (positionX >= Min.X && positionX <= Max.X) &&
                   (positionY >= Min.Y && positionY <= Max.Y) &&
                   (positionZ >= Min.Z && positionZ <= Max.Z);
        }

        public void UpdateMinMax()
        {
            var position = TransformData.Position;
            var rotation = TransformData.Rotation;
            var scale = TransformData.Scale;
            var translateMat = Matrix4x4.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
            var scaleMat = Matrix4x4.CreateScale(new Vector3(scale.X, scale.Y, scale.Z));
            var rotationMat = Matrix4x4.CreateRotationX(rotation.X) *
                              Matrix4x4.CreateRotationY(rotation.Y) *
                              Matrix4x4.CreateRotationZ(rotation.Z);
            var combined = scaleMat * rotationMat * translateMat;
            var vertexHelper = new VertexBufferHelper(VertexData, ResFile.ByteOrder);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            float maxZ = float.MinValue;
            foreach (var attribute in vertexHelper.Attributes)
            {
                if (attribute.Name == "_p0")
                {
                    for (int i = 0; i < attribute.Data.Length; i++)
                    {
                        Vector3 vertexPosition = new Vector3(
                         attribute.Data[i].X,
                         attribute.Data[i].Y,
                         attribute.Data[i].Z);

                        maxX = System.Math.Max(maxX, vertexPosition.X);
                        maxY = System.Math.Max(maxY, vertexPosition.Y);
                        maxZ = System.Math.Max(maxZ, vertexPosition.Z);

                        minX = System.Math.Min(minX, vertexPosition.X);
                        minY = System.Math.Min(minY, vertexPosition.Y);
                        minZ = System.Math.Min(minZ, vertexPosition.Z);
                    }
                }
            }

            Vector3 min = new Vector3(minX, minY, minZ);
            Vector3 max = new Vector3(maxX, maxY, maxZ);
            Min = Vector3.Transform(min, combined) * GLFrameworkEngine.GLContext.PreviewScale;
            Max = Vector3.Transform(max, combined) * GLFrameworkEngine.GLContext.PreviewScale;
        }

        /// <summary>
        /// Gets the name of the obj set using this area object.
        /// This generally refers to material data.
        /// </summary>
        public string GetAreaObjSetName()
        {
            if (MaterialData.RenderInfos.ContainsKey("gsys_areaenv_envobjset"))
                return MaterialData.RenderInfos["gsys_areaenv_envobjset"].GetValueStrings()[0];
            return $"Turbo_area0";
        }

        /// <summary>
        /// Gets the env map name of the area object used for cubemap reflections.
        /// This searches baglcube file for cubemap targets.
        /// <returns></returns>
        public string GetEnvMapName()
        {
            if (MaterialData.RenderInfos.ContainsKey("gsys_areaenv_envmap"))
                return MaterialData.RenderInfos["gsys_areaenv_envmap"].GetValueStrings()[0];
            return $"area{AreaIndex}";
        }

        public string GetLightmapName()
        {
            if (MaterialData.RenderInfos.ContainsKey("gsys_areaenv_lightmap"))
                return MaterialData.RenderInfos["gsys_areaenv_lightmap"].GetValueStrings()[0];
            return "diffuse";
        }

        /// <summary>
        /// Gets the bloom object to use from the course area file.
        /// </summary>
        public string GetBloomName()
        {
            if (MaterialData.RenderInfos.ContainsKey("gsys_areaenv_bloom"))
                return MaterialData.RenderInfos["gsys_areaenv_bloom"].GetValueStrings()[0];
            return "Bloom0";
        }
    }
}
