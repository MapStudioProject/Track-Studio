using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Syroot.NintenTools.MarioKart8.BinData;
using Syroot.NintenTools.MarioKart8.BinData.Parts;
using Newtonsoft.Json;
using System.Security.Permissions;

namespace PartsEditor
{
    public class PartObject
    {
        [JsonIgnore]
        public int PartIndex { get; set; }
        [JsonIgnore]
        public string PartName { get; set; }

        public PartObject() { }

        public PartObject(string filePath, string partName) {
            PartName = partName;
            UpdateGetters();
        }

        /// <summary>
        /// Updates the getters of PartParam properties of the part object.
        /// </summary>
        public void UpdateGetters() {
            UpdateGetters(this);
        }

        /// <summary>
        /// Updates the setters of PartParam properties of the part object.
        /// </summary>
        public void UpdateSetters() {
            UpdateSetters(this);
        }

        private int UpdateGetters(object value, int startIndex = 0)
        {
            var properties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            int propertyIndex = startIndex;
            for (int i = 0; i < properties.Length; i++)
            {
                var partAttribute = properties[i].GetCustomAttribute<PartParam>();
                if (partAttribute == null)
                    continue;

                if (partAttribute.StartIndex != -1)
                    propertyIndex = partAttribute.StartIndex;

                //Get the array value
                var subProperties = properties[i].GetType().GetProperties().ToArray();
                if (properties[i].PropertyType == typeof(Vector2F))
                {
                    var X = GetValue(partAttribute.Section, propertyIndex++);
                    var Y = GetValue(partAttribute.Section, propertyIndex++);
                    properties[i].SetValue(value, new Vector2F(X, Y));
                }
                else if (properties[i].PropertyType == typeof(Vector3F))
                {
                    var X = GetValue(partAttribute.Section, propertyIndex++);
                    var Y = GetValue(partAttribute.Section, propertyIndex++);
                    var Z = GetValue(partAttribute.Section, propertyIndex++);
                    properties[i].SetValue(value, new Vector3F(X, Y, Z));
                }
                else if (properties[i].PropertyType == typeof(Vector4F))
                {
                    var X = GetValue(partAttribute.Section, propertyIndex++);
                    var Y = GetValue(partAttribute.Section, propertyIndex++);
                    var Z = GetValue(partAttribute.Section, propertyIndex++);
                    var W = GetValue(partAttribute.Section, propertyIndex++);
                    properties[i].SetValue(value, new Vector4F(X, Y, Z, W));
                }
                else if (properties[i].PropertyType == typeof(Vector4B))
                {
                    var val = GetValue(partAttribute.Section, propertyIndex++);
                    var int32 = val.Int32;
                    if (MushResParts.BinFile.Format == BinFileFormat.MarioKart8Deluxe)
                    {
                        Vector4B newValue = new Vector4B(
                          (byte)(int32 & 0xFF),
                          (byte)(int32 >> 8 & 0xFF),
                          (byte)(int32 >> 16 & 0xFF),
                          (byte)(int32 >> 24));
                        properties[i].SetValue(value, newValue);
                    }
                    else
                    {
                        Vector4B newValue = new Vector4B(
                              (byte)(int32 >> 24),
                              (byte)(int32 >> 16 & 0xFF),
                              (byte)(int32 >> 8 & 0xFF),
                              (byte)(int32 & 0xFF));
                        properties[i].SetValue(value, newValue);
                    }
                }
                else if (properties[i].PropertyType == typeof(Vector2S))
                {
                    var val = GetValue(partAttribute.Section, propertyIndex++);
                    Vector2S newValue = new Vector2S(
                        (short)(val.Int32 & 0xFFFF),
                        (short)(val.Int32 >> 16));
                    properties[i].SetValue(value, newValue);
                }
                else if (properties[i].PropertyType == typeof(float))
                {
                    var datavalue = GetValue(partAttribute.Section, propertyIndex++);
                    properties[i].SetValue(value, datavalue.Single);
                }
                else if (properties[i].PropertyType == typeof(int))
                {
                    var datavalue = GetValue(partAttribute.Section, propertyIndex++);
                    properties[i].SetValue(value, datavalue.Int32);
                }
                else if (subProperties.Length > 0)
                {
                    object instance = Activator.CreateInstance(properties[i].PropertyType);
                    propertyIndex = UpdateGetters(instance, propertyIndex);
                    properties[i].SetValue(value, instance);
                }
            }
            return propertyIndex;
        }

        private int UpdateSetters(object value, int startIndex = 0)
        {
            var properties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            int propertyIndex = startIndex;
            for (int i = 0; i < properties.Length; i++)
            {
                var partAttribute = properties[i].GetCustomAttribute<PartParam>();
                if (partAttribute == null)
                    continue;

                if (partAttribute.StartIndex != -1)
                    propertyIndex = partAttribute.StartIndex;

                //Get the array value
                var subProperties = properties[i].GetType().GetProperties().ToArray();
                if (properties[i].PropertyType == typeof(Vector2F))
                {
                    Vector2F vec = (Vector2F)properties[i].GetValue(value);
                    SetValue(partAttribute.Section, propertyIndex++, (Dword)vec.X);
                    SetValue(partAttribute.Section, propertyIndex++, (Dword)vec.Y);
                }
                else if (properties[i].PropertyType == typeof(Vector3F))
                {
                    Vector3F vec = (Vector3F)properties[i].GetValue(value);
                    SetValue(partAttribute.Section, propertyIndex++, (Dword)vec.X);
                    SetValue(partAttribute.Section, propertyIndex++, (Dword)vec.Y);
                    SetValue(partAttribute.Section, propertyIndex++, (Dword)vec.Z);
                }
                else if (properties[i].PropertyType == typeof(Vector4F))
                {
                    Vector4F vec = (Vector4F)properties[i].GetValue(value);
                    SetValue(partAttribute.Section, propertyIndex++, (Dword)vec.X);
                    SetValue(partAttribute.Section, propertyIndex++, (Dword)vec.Y);
                    SetValue(partAttribute.Section, propertyIndex++, (Dword)vec.Z);
                    SetValue(partAttribute.Section, propertyIndex++, (Dword)vec.W);
                }
                else if (properties[i].PropertyType == typeof(Vector4B))
                {
                    Vector4B vec = (Vector4B)properties[i].GetValue(value);
                    byte[] bytes = new byte[4] { vec.X, vec.Y, vec.Z, vec.W };
                    if (MushResParts.BinFile.Format != BinFileFormat.MarioKart8Deluxe)
                        Array.Reverse(bytes);

                    int flags = BitConverter.ToInt32(bytes, 0); 
                    SetValue(partAttribute.Section, propertyIndex++, flags);
                }
                else if (properties[i].PropertyType == typeof(Vector2S))
                {
                    Vector2S vec = (Vector2S)properties[i].GetValue(value);

                    int flags =  (ushort)vec.X | (ushort)vec.Y << 16;
                    SetValue(partAttribute.Section, propertyIndex++, flags);
                }
                else if (properties[i].PropertyType == typeof(float)) 
                {
                    float val = (float)properties[i].GetValue(value);
                    SetValue(partAttribute.Section, propertyIndex++, val);
                }
                else if (properties[i].PropertyType == typeof(int))
                {
                    int val = (int)properties[i].GetValue(value);
                    SetValue(partAttribute.Section, propertyIndex++, val);
                }
                else if (subProperties.Length > 0)
                {
                    object instance = properties[i].GetValue(value);
                    propertyIndex = UpdateSetters(instance, propertyIndex);
                    properties[i].SetValue(value, instance);
                }
            }
            return propertyIndex;
        }

        internal virtual Dword GetValue(SectionIdentifier type, int index)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            //Get a list of all active indices for the current combination of parts
            int tireIndex = MushResParts.TireIndex;
            int bodyIndex = MushResParts.BodyIndex;
            int gliderIndex = MushResParts.GliderIndex;
            int driverIndex = MushResParts.DriverIndex;

            switch (type)
            {
                case SectionIdentifier.DriverTOM: return data[0][driverIndex][index];
                case SectionIdentifier.DriverEffect: return data[0][driverIndex][index];
                case SectionIdentifier.Tire: return data[0][tireIndex][index];
                case SectionIdentifier.TireBody: return data[tireIndex][bodyIndex][index];
                case SectionIdentifier.TireEffect: return data[0][tireIndex][index];
                case SectionIdentifier.TireRP: return data[0][tireIndex][index];
                case SectionIdentifier.Glider: return data[0][gliderIndex][index];
                case SectionIdentifier.GliderDriver: return data[gliderIndex][driverIndex][index];
                case SectionIdentifier.Body: return data[0][bodyIndex][index];
                case SectionIdentifier.BodyTire: return data[bodyIndex][tireIndex][index];
                case SectionIdentifier.BodyGlider: return data[bodyIndex][gliderIndex][index];
                case SectionIdentifier.BodyDriver: return data[bodyIndex][driverIndex][index];
                case SectionIdentifier.BodyEffect: return data[0][bodyIndex][index];
                default:
                    throw new Exception("Unsupported section!");
            }
        }

        internal virtual void SetValue(SectionIdentifier type, int index, Dword value)
        {
            Section section = MushResParts.BinFile.GetSectionByID((uint)type);
            var data = ((DwordSectionData)section.Data).Data;
            //Get a list of all active indices for the current combination of parts
            int tireIndex = MushResParts.TireIndex;
            int bodyIndex = MushResParts.BodyIndex;
            int gliderIndex = MushResParts.GliderIndex;
            int driverIndex = MushResParts.DriverIndex;

            switch (type)
            {
                case SectionIdentifier.DriverTOM: data[0][driverIndex][index] = value; break;
                case SectionIdentifier.DriverEffect: data[0][driverIndex][index] = value; break;
                case SectionIdentifier.Tire: data[0][tireIndex][index] = value; break;
                case SectionIdentifier.TireBody: data[tireIndex][bodyIndex][index] = value; break;
                case SectionIdentifier.TireEffect: data[0][tireIndex][index] = value; break;
                case SectionIdentifier.TireRP: data[0][tireIndex][index] = value; break;
                case SectionIdentifier.Glider: data[0][gliderIndex][index] = value; break;
                case SectionIdentifier.GliderDriver: data[gliderIndex][driverIndex][index] = value; break;
                case SectionIdentifier.Body: data[0][bodyIndex][index] = value; break;
                case SectionIdentifier.BodyTire: data[bodyIndex][tireIndex][index] = value; break;
                case SectionIdentifier.BodyGlider: data[bodyIndex][gliderIndex][index] = value; break;
                case SectionIdentifier.BodyDriver: data[bodyIndex][driverIndex][index] = value; break;
                case SectionIdentifier.BodyEffect: data[0][bodyIndex][index] = value; break;
                default:
                    throw new Exception("Unsupported section!");
            }
        }
    }
}
