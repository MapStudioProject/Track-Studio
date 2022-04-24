using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core;

namespace TurboLibrary
{
    public class RouteChange : SpatialObject, ICourseReferencable
    {
        public List<Obj> RouteChangeObjects { get; set; }

        [ByamlMember]
        private List<int> RouteChange_Obj { get; set; }

        [ByamlMember]
        [BindGUI("Route Change ID")]
        public int RouteChangeID { get; set; }

        [ByamlMember]
        [BindGUI("Start Only")]
        public bool StartOnly { get; set; } = false;

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Allows references of course data objects to be resolved to provide real instances instead of the raw values
        /// in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void DeserializeReferences(CourseDefinition courseDefinition)
        {
            RouteChangeObjects = new List<Obj>();
            foreach (var id in RouteChange_Obj)
                RouteChangeObjects.Add(courseDefinition.Objs[id]);
        }

        /// <summary>
        /// Allows references between course objects to be serialized into raw values stored in the BYAML.
        /// </summary>
        /// <param name="courseDefinition">The <see cref="CourseDefinition"/> providing the objects.</param>
        public void SerializeReferences(CourseDefinition courseDefinition)
        {
            RouteChange_Obj = new List<int>();
            for (int i = 0; i < RouteChangeObjects.Count; i++)
                RouteChange_Obj.Add(courseDefinition.Objs.IndexOf(RouteChangeObjects[i]));
        }
    }
}
