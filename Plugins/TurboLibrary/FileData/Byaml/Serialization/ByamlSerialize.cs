using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Fasterflect;

namespace TurboLibrary
{
    public class ByamlSerialize
    {
        public static void Deserialize(object section, dynamic value)
        {
            if (value is IList)
            {
                var props = section.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                SetValues(props[0], props[0].PropertyType, section, value);
                return;
            }

            Dictionary<string, dynamic> bymlProperties;

            if (value is Dictionary<string, dynamic>) bymlProperties = (Dictionary<string, dynamic>)value;
            else throw new Exception("Not a dictionary");

            if (section is IByamlSerializable)
                ((IByamlSerializable)section).DeserializeByaml(bymlProperties);

            var properties = section.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var fields = section.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            for (int i = 0; i < fields.Length; i++)
            {
                //Only load properties with byaml attributes
                var byamlAttribute = fields[i].GetCustomAttribute<ByamlMember>();
                if (byamlAttribute == null)
                    continue;

                Type type = fields[i].FieldType;
                Type nullableType = Nullable.GetUnderlyingType(type);
                if (nullableType != null)
                    type = nullableType;

                //Set custom keys as property name if used
                string name = byamlAttribute.Key != null ? byamlAttribute.Key : fields[i].Name;

                //Skip properties that are not present
                if (!bymlProperties.ContainsKey(name))
                    continue;

                SetValues(fields[i], type, section, bymlProperties[name]);
            }

            for (int i = 0; i < properties.Length; i++)
            {
                //Get a property that stores the current dynamic dictionary
                var byamlPropertiesAttribute = properties[i].GetCustomAttribute<ByamlPropertyList>();
                if (byamlPropertiesAttribute != null)
                {
                    //Store the whole dynamic into the dictionary of the property
                    //All properties will be stored then saved back if none are serialized elsewhere in the class
                    properties[i].SetValue(section, value);
                    continue;
                }

                //Only load properties with byaml attributes
                var byamlAttribute = properties[i].GetCustomAttribute<ByamlMember>();
                if (byamlAttribute == null)
                    continue;

                Type type = properties[i].PropertyType;
                Type nullableType = Nullable.GetUnderlyingType(type);
                if (nullableType != null)
                    type = nullableType;

                //Set custom keys as property name if used
                string name = byamlAttribute.Key != null ? byamlAttribute.Key : properties[i].Name;

                //Skip properties that are not present
                if (!bymlProperties.ContainsKey(name))
                    continue;

                //Make sure the property has a setter and getter
                if (!properties[i].CanRead || !properties[i].CanWrite)
                {
                    throw new Exception(
                        $"Property {value}.{properties[i].Name} requires both a getter and setter to be used for dserialization.");
                }

                SetValues(properties[i], type, section, bymlProperties[name]);
            }
        }

        public static dynamic Serialize(object section)
        {
            return SetDictionary(section);
        }

        static dynamic SetDictionary(object section)
        {
            IDictionary<string, dynamic> bymlProperties = new Dictionary<string, dynamic>();

            if (section is IByamlSerializable)
                ((IByamlSerializable)section).SerializeByaml(bymlProperties);

            if (section is ByamlVector3F)
            {
                bymlProperties.Add("X", ((ByamlVector3F)section).X);
                bymlProperties.Add("Y", ((ByamlVector3F)section).Y);
                bymlProperties.Add("Z", ((ByamlVector3F)section).Z);
                return bymlProperties;
            }

            var properties = section.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var fields = section.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            for (int i = 0; i < fields.Length; i++)
            {
                //Only load properties with byaml attributes
                var byamlAttribute = fields[i].GetCustomAttribute<ByamlMember>();
                if (byamlAttribute == null)
                    continue;

                //Skip null optional values
                if (byamlAttribute.Optional && fields[i].GetValue(section) == null)
                    continue;

                //Set custom keys as property name if used
                string name = byamlAttribute.Key != null ? byamlAttribute.Key : fields[i].Name;
                bymlProperties.Add(name, SetBymlValue(fields[i].GetValue(section)));
            }

            for (int i = 0; i < properties.Length; i++)
            {
                //Only load properties with byaml attributes
                var byamlAttribute = properties[i].GetCustomAttribute<ByamlMember>();
                if (byamlAttribute == null)
                    continue;

                //Skip null optional values
                if (byamlAttribute.Optional && properties[i].GetValue(section) == null)
                    continue;

                //Skip empty lists
                if (typeof(IList).GetTypeInfo().IsAssignableFrom(properties[i].PropertyType) && ((IList)properties[i].GetValue(section)).Count == 0)
                    continue;

                //Set custom keys as property name if used
                string name = byamlAttribute.Key != null ? byamlAttribute.Key : properties[i].Name;

                //Make sure the property has a setter and getter
                if (!properties[i].CanRead || !properties[i].CanWrite)
                {
                    throw new Exception(
                        $"Property {section}.{properties[i].Name} requires both a getter and setter to be used for dserialization.");
                }

                bymlProperties.Add(name, SetBymlValue(properties[i].GetValue(section)));
            }

            return bymlProperties;
        }

        static dynamic SetBymlValue(object value)
        {
            Type type = value.GetType();
            Type nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null && nullableType.GetTypeInfo().IsEnum)
                type = nullableType;
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);

            if (value is IList<ByamlExt.Byaml.ByamlPathPoint>)
                return value;

            if (type == typeof(bool)) return value;
            else if (type == typeof(float)) return value;
            else if (type == typeof(int)) return (int)value;
            else if (type == typeof(uint)) return (uint)value;
            else if (type == typeof(string)) return value;
            else if (type == typeof(decimal)) return value;
            else if (type == typeof(ByamlExt.Byaml.ByamlPathPoint)) return value;
            else if (typeof(IList).GetTypeInfo().IsAssignableFrom(type))
            {
                List<dynamic> savedValues = new List<dynamic>();
                foreach (var val in ((IList)value))
                    savedValues.Add(SetBymlValue(val));
                return savedValues;
            }
            else if (IsTypeByamlObject(type))
                return SetDictionary(value);

            throw new Exception($"Type {type.Name} is not supported as BYAML data.");
        }

        static bool IsTypeByamlObject(Type type)
        {
            return type.GetTypeInfo().GetCustomAttribute<ByamlObject>(true) != null;
        }

        static void SetValues(object property, Type type, object section, dynamic value)
        {
            if (value is IList<ByamlExt.Byaml.ByamlPathPoint>)
            {
                SetValue(property, section, value);
            }
            else if (value is IList<dynamic>)
            {
                var list = (IList<dynamic>)value;
                var array = InstantiateType<IList>(type);

                Type elementType = type.GetTypeInfo().GetElementType();
                if (type.IsGenericType && elementType == null)
                    elementType = type.GetGenericArguments()[0];

                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j] is IDictionary<string, dynamic>)
                    {
                        var instance = CreateInstance(elementType);
                        Deserialize(instance, list[j]);
                        array.Add(instance);
                    }
                    else if (list[j] is IList<dynamic>)
                    {
                        var subList = list[j] as IList<dynamic>;

                        var instance = CreateInstance(elementType);
                        if (instance is IList)
                        {
                            for (int k = 0; k < subList.Count; k++)
                                ((IList)instance).Add(subList[k]);
                        }
                        array.Add(instance);
                    }
                    else
                        array.Add(list[j]);
                }
                SetValue(property, section, array);
            }
            else if (value is IDictionary<string, dynamic>)
            {
                if (type == typeof(ByamlVector3F))
                {
                    var values = (IDictionary<string, dynamic>)value;
                    var vec3 = new ByamlVector3F(values["X"], values["Y"], values["Z"]);
                    SetValue(property, section, vec3);
                }
                else
                {
                    var instance = CreateInstance(type);
                    Deserialize(instance, value);
                    SetValue(property, section, instance);
                }
            }
            else
                SetValue(property, section, value);
        }

        static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type, true);

           return type.CreateInstance();
        }

        static void SetValue(object property, object instance, object value)
        {
            if (property is PropertyInfo)
            {
                Type nullableType = Nullable.GetUnderlyingType(((PropertyInfo)property).PropertyType);
                if (nullableType != null && nullableType.GetTypeInfo().IsEnum)
                {
                    value = Enum.ToObject(nullableType, value);
                }
            }

            if (property is PropertyInfo)
                ((PropertyInfo)property).SetValue(instance, value);
            else if (property is FieldInfo)
                ((FieldInfo)property).SetValue(instance, value);
        }

        private static bool IsTypeList(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IList));
        }

        private static T InstantiateType<T>(Type type)
        {
            // Validate if the given type is compatible with the required one.
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(type))
            {
                throw new Exception($"Type {type.Name} cannot be used as BYAML object data.");
            }
            // Return a new instance.
            return (T)CreateInstance(type);
        }
    }
}
