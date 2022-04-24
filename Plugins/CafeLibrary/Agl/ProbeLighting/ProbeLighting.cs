using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AampLibraryCSharp;
using OpenTK;

namespace AGraphicsLibrary
{
    public partial class ProbeLighting
    {
        /// <summary>
        /// Index used for when a probe is not used for calculating coefficents in the grid.
        /// </summary>
        public static readonly uint UnusedIndex = 4294377462;

        public ProbeGrid RootGrid { get; set; }
        public ProbeParams Params { get; set; }

        public List<ProbeVolume> Boxes = new List<ProbeVolume>();

        public void LoadValues(AampFile aamp)
        {
            Params = new ProbeParams();
            foreach (var val in aamp.RootNode.paramObjects)
            {
                if (val.HashString == "root_grid")
                    RootGrid = LoadGridData(val.paramEntries);
                if (val.HashString == "param_obj") {
                    foreach (var param in val.paramEntries)
                    {
                        if (param.HashString == "version")
                            Params.Version = (uint)param.Value;
                        if (param.HashString == "dir_light_indirect")
                            Params.IndirectDirectionLight = (float)param.Value;
                        if (param.HashString == "point_light_indirect")
                            Params.IndirectPointLight = (float)param.Value;
                        if (param.HashString == "spot_light_indirect")
                            Params.IndirectSpotLight = (float)param.Value;
                        if (param.HashString == "emission_scale")
                            Params.EmissionScale = (float)param.Value;
                    }
                }
            }

            foreach (var val in aamp.RootNode.childParams)
            {
                ProbeVolume box = new ProbeVolume();
                Boxes.Add(box);

                foreach (var param in val.paramObjects)
                {
                    if (param.HashString == "grid")
                        box.Grid = LoadGridData(param.paramEntries);
                    if (param.HashString == "param_obj")
                    {
                        foreach (var p in param.paramEntries)
                        {
                            if (p.HashString == "index")
                                box.Index = (uint)p.Value;
                            if (p.HashString == "type")
                                box.Type = (uint)p.Value;
                        }
                    }
                    if (param.HashString == "sh_data_buffer")
                        box.DataBuffer = LoadDataBuffer(param.paramEntries);
                    if (param.HashString == "sh_index_buffer")
                        box.IndexBuffer = LoadIndexBuffer(param.paramEntries);
                }
            }
        }

        private ProbeGrid LoadGridData(ParamEntry[] paramEntries)
        {
            ProbeGrid grid = new ProbeGrid();

            foreach (var entry in paramEntries)
            {
                if (entry.HashString == "aabb_min_pos")
                    grid.Min = (Syroot.Maths.Vector3F)entry.Value;
                if (entry.HashString == "aabb_max_pos")
                    grid.Max = (Syroot.Maths.Vector3F)entry.Value;
                if (entry.HashString == "voxel_step_pos")
                    grid.Step = (Syroot.Maths.Vector3F)entry.Value;
            }

            grid.Setup();
            return grid;
        }

        private SHDataBuffer LoadDataBuffer(ParamEntry[] paramEntries)
        {
            SHDataBuffer buffer = new SHDataBuffer();
            foreach (var entry in paramEntries)
            {
                if (entry.HashString == "type")
                    buffer.Type = (uint)entry.Value;
                if (entry.HashString == "max_sh_data_num")
                    buffer.MaxDataNum = (uint)entry.Value;
                if (entry.HashString == "used_data_num")
                    buffer.UsedDataNum = (uint)entry.Value;
                if (entry.HashString == "per_probe_float_num")
                    buffer.PerProbeFloatNum = (uint)entry.Value;
                if (entry.HashString == "data_buffer")
                    buffer.DataBuffer = (float[])entry.Value;
            }
            return buffer;
        }

        private SHIndexBuffer LoadIndexBuffer(ParamEntry[] paramEntries)
        {
            SHIndexBuffer buffer = new SHIndexBuffer();
            foreach (var entry in paramEntries)
            {
                if (entry.HashString == "type")
                    buffer.Type = (uint)entry.Value;
                if (entry.HashString == "max_index_num")
                    buffer.MaxIndicesNum = (uint)entry.Value;
                if (entry.HashString == "used_index_num")
                    buffer.UsedIndicesNum = (uint)entry.Value;
                if (entry.HashString == "index_buffer")
                {
                    var indices = (uint[])entry.Value;
                    buffer.IndexBuffer = new ushort[indices.Length * 2];
                    for (int i = 0; i < indices.Length; i++) {
                        //Indices are ushorts packed into uints
                        buffer.IndexBuffer[i] =      (ushort)(indices[i] >> 16);
                        buffer.IndexBuffer[i + 1] =  (ushort)(indices[i] & 0xFFFF);
                    }
                }
            }
            return buffer;
        }
    }

    public class ProbeParams
    {
        public uint Version { get; set; }
        public float IndirectDirectionLight { get; set; }
        public float IndirectPointLight { get; set; }
        public float IndirectSpotLight { get; set; }
        public float EmissionScale { get; set; }
    }

    public class ProbeVolume
    {
        /// <summary>
        /// The index of the current probe box.
        /// </summary>
        public uint Index { get; set; }

        /// <summary>
        /// The type of probe object used.
        /// </summary>
        public uint Type { get; set; }

        /// <summary>
        /// The grid to determine the boundry used for probe lighting.
        /// </summary>
        public ProbeGrid Grid { get; set; }

        /// <summary>
        /// The index buffer used to lookup probe color values.
        /// </summary>
        public SHIndexBuffer IndexBuffer { get; set; }

        /// <summary>
        /// The data buffer used for probe color/lighting calculations.
        /// </summary>
        public SHDataBuffer DataBuffer { get; set; }

        public float[] GetSHData(int index, int step)
        {
            uint dataIndex = IndexBuffer.GetSHDataIndex(index, step);
            bool isValid = IndexBuffer.IsIndexValueValid(dataIndex);
            if (isValid) {
                return DataBuffer.GetSHData((int)dataIndex);
            }
            return new float[DataBuffer.PerProbeFloatNum];
        }

        public int CalculateIndexCount()
        {
            //Get number of probes
            var size = Grid.Max - Grid.Min;
            var stride = size / Grid.Step;
            stride.X = MathF.Ceiling(stride.X);
            stride.Y = MathF.Ceiling(stride.Y);
            stride.Z = MathF.Ceiling(stride.Z);
            return (int)(stride.X * stride.Y * stride.Z) * 8; //8 probes in each
        }
    }

    public struct SHIndexBuffer
    {
        /// <summary>
        /// The buffer type. 0 for index buffer, 1 for data buffer
        /// </summary>
        public uint Type { get; set; }

        /// <summary>
        /// The total amount of indices being used in the data buffer.
        /// </summary>
        public uint UsedIndicesNum { get; set; }

        /// <summary>
        /// The max amount of indices in the data buffer.
        /// </summary>
        public uint MaxIndicesNum { get; set; }

        /// <summary>
        /// A list of indices that determine the data to pull from the data buffer.
        /// </summary>
        public ushort[] IndexBuffer { get; set; }

        public uint GetSHDataIndex(int index, int step) {
            return IndexBuffer[index * 8 + step];
        }

        public bool IsIndexValueInvisible(uint index) {
            return IsTypeU16() ? index == 0xfff5 : index == 0xfffffff5;
        }

        public bool IsIndexValueEmpty(uint index) {
            return IsTypeU16() ? index == 0xfff6 : index == 0xfffffff6;
        }

        public bool IsIndexValueValid(uint index) {
            return IsTypeU16() ? index < 0xfff5 : index < 0xfffffff5;
        }

        public bool IsTypeU16() {
            return true;
        }

        public uint GetLargestIndex()
        {
            uint index = 0;
            for (int i = 0; i < IndexBuffer.Length; i++)
            {
                if (!IsIndexValueValid(IndexBuffer[i]))
                    continue;

                index = Math.Max(index, IndexBuffer[i]);
            }
            return index;
        }
    }

    public struct SHDataBuffer
    {
        /// <summary>
        /// The buffer type. 0 for index buffer, 1 for data buffer
        /// </summary>
        public uint Type { get; set; }

        /// <summary>
        /// The total amount of indices being used in the data buffer.
        /// </summary>
        public uint UsedDataNum { get; set; }

        /// <summary>
        /// The max amount of data in the data buffer.
        /// </summary>
        public uint MaxDataNum { get; set; }

        /// <summary>
        /// The total amount floats used by a single probe.
        /// Typically uses 27 spherical harmonics.
        /// </summary>
        public uint PerProbeFloatNum { get; set; }

        /// <summary>
        /// The data buffer used to store color values for each probe.
        /// </summary>
        public float[] DataBuffer { get; set; }

        public float[] GetSHData(int dataIndex)
        {
            //Note the game is hardcoded to 27
            int startIndex =  (int)(dataIndex * 27);

            float[] shData = new float[27];
            for (int i = 0; i < 27; i++) {
                shData[i] = DataBuffer[startIndex + i];
            }
            return shData;
        }
    }

    public struct ProbeGrid
    {
        /// <summary>
        /// The bounding voxel grid min value.
        /// </summary>
        public Syroot.Maths.Vector3F Min { get; set; }

        /// <summary>
        /// The bounding voxel grid max value.
        /// </summary>
        public Syroot.Maths.Vector3F Max { get; set; }

        /// <summary>
        /// The bounding voxel step value for spacing each probe.
        /// </summary>
        public Syroot.Maths.Vector3F Step { get; set; }

        public Syroot.Maths.Vector3F Size => (Max - Min);

        public Syroot.Maths.Vector3F Stride;

        public Syroot.Maths.Vector3F CalculatedMin;
        public Syroot.Maths.Vector3F CalculatedMax;

        public Vector3 GetProbeCount()
        {
            return new Vector3(
                 MathF.Ceiling(Stride.X),
                 MathF.Ceiling(Stride.Y),
                 MathF.Ceiling(Stride.Z));
        }

        public void Setup()
        {
            Vector2 v = Vector2.Zero; //Todo figure out how this works

            Vector2 maxXY = new Vector2(Max.X, Max.Y) + v;
            Vector2 minXY = new Vector2(Min.X, Min.Y) + v;
            Vector2 size = maxXY - minXY + v;
            Stride.X = size.X / Step.X;
            Stride.Y = size.Y / Step.Y;
            Stride.Z = (Max.Z - Min.Z) / Step.Z;

            CalculatedMin.X = float.MaxValue;
            CalculatedMin.Y = float.MaxValue;
            CalculatedMin.Z = float.MaxValue;
            CalculatedMax.X = float.MinValue;
            CalculatedMax.Y = float.MinValue;
            CalculatedMax.Z = float.MinValue;

            if (Min.X < CalculatedMin.X) CalculatedMin.X = Min.X;
            if (Min.Y < CalculatedMin.Y) CalculatedMin.Y = Min.Y;
            if (Min.Z < CalculatedMin.Z) CalculatedMin.Z = Min.X;

            if (CalculatedMax.X < Min.X) CalculatedMax.X = Min.X;
            if (CalculatedMax.Y < Min.Y) CalculatedMax.Y = Min.Y;
            if (CalculatedMax.Z < Min.Z) CalculatedMax.Z = Min.Z;

            if (Max.X < CalculatedMin.X) CalculatedMin.X = Max.X;
            if (Max.Y < CalculatedMin.Y) CalculatedMin.Y = Max.Y;
            if (Max.Z < CalculatedMin.Z) CalculatedMin.Z = Max.X;

            if (CalculatedMax.X < Max.X) CalculatedMax.X = Max.X;
            if (CalculatedMax.Y < Max.Y) CalculatedMax.Y = Max.Y;
            if (CalculatedMax.Z < Max.Z) CalculatedMax.Z = Max.Z;
        }

        /// <summary>
        /// Checks if the given position is inside the bounding grid.
        /// </summary>
        /// <param name="positionX"></param>
        /// <param name="positionY"></param>
        /// <param name="positionZ"></param>
        /// <returns></returns>
        public bool IsInside(float positionX, float positionY, float positionZ)
        {
            return (positionX >= Min.X && positionX <= Max.X) &&
                   (positionY >= Min.Y && positionY <= Max.Y) &&
                   (positionZ >= Min.Z && positionZ <= Max.Z);
        }

        /// <summary>
        /// Gets the index of the world position in the voxel grid.
        /// Returns -1 if outside the grid.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int GetVoxelIndex(Vector3 worldPosition)
        {
            if (!IsInside(worldPosition.X, worldPosition.Y, worldPosition.Z))
                return -1;

            //World to voxel indices
            GetVoxelIndices(out uint x, out uint y, out uint z, worldPosition);

            var pos = GetVoxelPosition(x, y, z);

            //Get index for the x y z indices
            int voxelIndex = GetVoxelIndex(x, y, z);
            Console.WriteLine($"voxelIndex {voxelIndex} indices {x}_{y}_{z} wpos {worldPosition} gpos {pos}");

            //  GetVoxelIndices(out uint tX, out uint tY, out uint tZ, (uint)voxelIndex);

            //   Console.WriteLine($"voxelIndex {voxelIndex} {x}_{y}_{z} aabb {tX}_{tY}_{tZ}");

            return voxelIndex;
        }

        /// <summary>
        /// Gets the x, y, z indices converted from position world space.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="position"></param>
        public void GetVoxelIndices(out uint x, out uint y, out uint z, Vector3 worldPosition)
        {
            //Convert to relative coordinates
            float gridX = (worldPosition.X - Min.X);
            float gridY = (worldPosition.Y - Min.Y);
            float gridZ = (worldPosition.Z - Min.Z);
            //Get the index from the step amount
            x = (uint)(gridX / Step.X);
            y = (uint)(gridY / Step.Y);
            z = (uint)(gridZ / Step.Z);
        }

        /// <summary>
        /// Gets the x y z position in world coordinates from voxel indices.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Vector3 GetVoxelPosition(uint x, uint y, uint z)
        {
            return new Vector3(
                Min.X + x * Step.X,
                Min.Y + y * Step.Y,
                Min.Z + z * Step.Z);
        }

        public void GetVoxelIndices(out uint x, out uint y, out uint z, uint index)
        {
            y = (uint)(index / (Stride.X * Stride.Z));

            uint param = (uint)(Stride.X * Stride.Z);
            uint param2 = index - (index / param) * param;

            z = param2 / (uint)(int)Stride.X;

            x = (uint)(param2 - ((int)param2 / (int)(float)Stride.X) * (int)(float)Stride.X);
        }

        public int GetVoxelIndex(uint x, uint y, uint z)
        {
            return (int)(Stride.X * Stride.Z * y + Stride.X * z + x);
        }

        public int GetNearestLocalProbeIndex(Vector3 worldPosition, int voxelIndex)
        {
            float[] weights = GetVoxelTriLinearWeight(worldPosition, voxelIndex);
            int index = 0;

            //Get the nearest index of the 8 probes
            if (0.5f < weights[2]) index += 2;
            if (0.5f < weights[1]) index += 4;
            if (0.5f < weights[0]) index += 1;

            return index;
        }

        public float[] GetVoxelTriLinearWeight(Vector3 position, int voxelIndex)
        {
            //  GetVoxelAABBInfo(voxelIndex, out float[] values);
            GetVoxelIndices(out uint x, out uint y, out uint z, position);

            float[] values = new float[3];
            values[0] = Step.X * x + Min.X;
            values[1] = Step.Y * y + Min.Y;
            values[2] = Step.Z * z + Min.Z;

            float weight1 = (position.X - values[0]) / Step.X;
            float weight2 = (position.Y - values[1]) / Step.Y;
            float weight3 = (position.Z - values[2]) / Step.Z;

            return new float[3] { weight1, weight2, weight3 };
        }

        private void GetVoxelAABBInfo(int voxelIndex, out float[] values)
        {
            values = new float[3];
            GetVoxelIndices(out uint x, out uint y, out uint z, (uint)voxelIndex);

            values[0] = Step.X * x + Min.X;
            values[1] = Step.Y * y + Min.Y;
            values[2] = Step.Z * z + Min.Z;
        }
    }
}
