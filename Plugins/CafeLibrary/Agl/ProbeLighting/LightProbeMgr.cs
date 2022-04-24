using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace AGraphicsLibrary
{
    public partial class LightProbeMgr
    {
        /*
         * Game Struct
            struct Grid
            {
	            MinX, //0x24
	            MinY, //0x28
	            MinZ, //0x2C
	
	            MaxX, //0x3C
	            MaxY, //0x40
	            MaxZ, //0x44
	
	            StepX, //0x54
	            StepY, //0x58
	            StepZ, //0x5C

	            StrideX, //0x60
	            StrideY, //100
	            StrideZ, //0x68

                MinXYZ (adjusted in setup) //0x6C, 0x70, 0x74
                MaxXYZ (adjusted in setup) //0x78, 0x7C, 0x80
            }
        */

        public class ProbeInfo
        {
            public float[] shData;

            public Vector3 Position;

            public VolumeInfo[] Volumes = new VolumeInfo[0x10];

            public class VolumeInfo
            {
                public VoxelState VoxelState;

                public int VoxelIndex;

                public Vector3 VoxelIndices;

                public uint[] DataIndices = new uint[8];
            }
        }

        public static Vector4[] ConvertSH2RGB(float[] shData)
        {
            float[] weights = new float[3] { 1, 1, 1 };
            int dataIndex = 0;

            Vector4[] rgb = new Vector4[7];
            for (int channel = 0; channel < 3; channel++)
            {
                //Convert 8 coefficents for each RGB channel (9th one will be done last)
                var data = ConvertChannel(weights[channel], new float[9] {
                        shData[dataIndex++], shData[dataIndex++],shData[dataIndex++],
                        shData[dataIndex++], shData[dataIndex++],shData[dataIndex++],
                        shData[dataIndex++], shData[dataIndex++],shData[dataIndex++]});
                //2 vec4 per channel
                rgb[channel] = data[0];
                //This channel goes after the first 3
                rgb[3 + channel] = data[1];
            }

            float const_5 = 0.1364044f;
            //Last value of each 9 coefficents convert for the last vec4
            rgb[6] = new Vector4(
                weights[0] * shData[8]  * const_5,
                weights[1] * shData[19] * const_5,
                weights[2] * shData[26] * const_5,
                1.0f);

            return rgb;
        }

        static Vector4[] ConvertChannel(float weight, float[] data)
        {
            const float const_1 = 0.3253434f;
            const float const_2 = 0.2817569f;
            const float const_3 = 0.07875311f;

            const float const_4 = 0.2728088f;
            const float const_5 = 0.2362593f;

            float v1 = weight * data[3] * const_1;
            float v2 = weight * data[1] * const_1;
            float v3 = weight * data[2] * const_1;
            float v4 = weight * data[0] * const_2 - data[6] * const_3;

            float v21 = weight * data[4] * const_4;
            float v22 = weight * data[5] * const_4;
            float v23 = weight * data[6] * const_5;
            float v24 = weight * data[7] * const_4;

            return new Vector4[2]
            {
                new Vector4(v1, v2, v3, v4),
                new Vector4(v21, v22, v23, v24),
            };
        }

        public static bool GetInterpolatedSH(ProbeLighting probeLighting, Vector3 worldPosition, bool isTriLinear, ref float[] shData)
        {
            ProbeInfo info = new ProbeInfo();
            CafeLibrary.ProbeLightingDebugger.SetProbeInfo(info);

            info.Position = worldPosition;

            if (!probeLighting.RootGrid.IsInside(worldPosition.X, worldPosition.Y, worldPosition.Z))
                return false;

            int index = 0;
            foreach (ProbeVolume volume in probeLighting.Boxes) {
                if (volume.Grid.IsInside(worldPosition.X, worldPosition.Y, worldPosition.Z)) {
                    VoxelState state = VoxelState.Empty;

                    info.Volumes[index] = new ProbeInfo.VolumeInfo();
                    info.Volumes[index].VoxelIndex = volume.Grid.GetVoxelIndex(worldPosition);
                    volume.Grid.GetVoxelIndices(out uint x, out uint y, out uint z, worldPosition);
                    info.Volumes[index].VoxelIndices = new Vector3(x, y, z);
                    for (int i = 0; i < 8; i++)
                        info.Volumes[index].DataIndices[i] = volume.IndexBuffer.GetSHDataIndex(info.Volumes[index].VoxelIndex, i);

                    if (isTriLinear)
                        state = GetSHTriLinear(volume, worldPosition, ref shData);
                    else
                        state = GetSHNearest(volume, worldPosition, ref shData);

                    info.Volumes[index].VoxelState = state;
                    info.shData = shData;

                    index++;

                    //Found voxel hit, return true
                    if ((int)state > -1)
                        return true;

                    //Skip if there is a volume using an invisible state
                    if (state == VoxelState.Invisible) {
                        return false;
                    }
                }
            }

            return false;
        }

        static VoxelState GetSHTriLinear(ProbeVolume v, Vector3 worldPosition, ref float[] shData)
        {
            int voxelIndex = v.Grid.GetVoxelIndex(worldPosition);
            Console.WriteLine($"voxelIndex {voxelIndex}");
            if (voxelIndex < 0)
                return VoxelState.Empty;

            VoxelState voxelState = GetVoxelState(v, voxelIndex);
            if (voxelState != VoxelState.Valid)
                return voxelState;

            float[] weights = v.Grid.GetVoxelTriLinearWeight(worldPosition, voxelIndex);

            //Blend 4 regions
            var lerp1 = GetLerp(weights[0], v.GetSHData(voxelIndex, 0), v.GetSHData(voxelIndex, 1));
            var lerp2 = GetLerp(weights[0], v.GetSHData(voxelIndex, 4), v.GetSHData(voxelIndex, 5));
            var leftBlend = GetLerp(weights[1], lerp1, lerp2);

            //Blend 4 regions
            var lerp3 = GetLerp(weights[0], v.GetSHData(voxelIndex, 2), v.GetSHData(voxelIndex, 3));
            var lerp4 = GetLerp(weights[0], v.GetSHData(voxelIndex, 6), v.GetSHData(voxelIndex, 7));
            var rightBlend = GetLerp(weights[1], lerp3, lerp4);

            //Blend the 2 outputs into a final output
            var finalBlend = GetLerp(weights[2], leftBlend, rightBlend);
            shData = finalBlend;

            return VoxelState.Valid;
        }

        static float[] GetLerp(double weight, float[] blend1, float[] blend2)
        {
            float[] output = new float[27];
            for (int i = 0; i < 27; i++) {
                output[i] = Lerp(blend1[i], blend2[i], weight);
            }
            return output;
        }

        static float Lerp(float a, float b, double weight) {
            return (float)(a * (1 - weight) + b * weight);
        }

        static VoxelState GetSHNearest(ProbeVolume volume, Vector3 worldPosition, ref float[] shData)
        {
            int voxelIndex = volume.Grid.GetVoxelIndex(worldPosition);
            if (voxelIndex < 0)
                return VoxelState.Empty;

            VoxelState voxelState = GetVoxelState(volume, voxelIndex);
            if (voxelState != VoxelState.Valid)
                return voxelState;

            int nearestIndex = volume.Grid.GetNearestLocalProbeIndex(worldPosition, voxelIndex);
            shData = volume.GetSHData(voxelIndex, nearestIndex);
            return VoxelState.Valid;

            if (nearestIndex >= 0)
            {
                uint dataIndex = volume.IndexBuffer.GetSHDataIndex(voxelIndex, 0);
                bool isValid = volume.IndexBuffer.IsIndexValueValid(dataIndex);
                if (isValid)
                {
                    shData = volume.DataBuffer.GetSHData((int)dataIndex);
                    return VoxelState.Valid;
                }
            }
            return VoxelState.Empty;
        }

        /// <summary>
        /// Checks if there are 8 valid probes around the voxel index.
        /// </summary>
        static VoxelState GetVoxelState(ProbeVolume v, int voxelIndex)
        {
            const int numDivide = 8;
            for (int i = 0; i < numDivide; i++)
            {
                uint dataIndex = v.IndexBuffer.GetSHDataIndex(voxelIndex, i);
                bool isEmpty = v.IndexBuffer.IsIndexValueEmpty(dataIndex);
                if (isEmpty)
                    return VoxelState.Empty;

                bool isInvisible = v.IndexBuffer.IsIndexValueInvisible(dataIndex);
                if (isInvisible)
                    break;

                //Data passed through 8 times with valid values
                if (i == numDivide - 1)
                    return VoxelState.Valid;
            }
            return VoxelState.Invisible;
        }

        public enum VoxelState : uint
        {
            Valid = 0,
            Empty = 0xfffffffd,
            Invisible = 0xfffffffe,
        }
    }
}
