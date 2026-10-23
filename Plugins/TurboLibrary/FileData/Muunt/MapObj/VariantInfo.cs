using System;
using System.Collections.Generic;

namespace TurboLibrary
{
    /// <summary>
    /// Information about Map Object variants.
    /// </summary>
    public sealed class VariantInfo
    {
        public List<string> MatchLabels { get; set; }
        public string Icon { get; set; }
        public Dictionary<string, string> TextureMappings { get; set; }
        public Dictionary<string, int> MaterialAnimationFrames { get; set; }
        public Tuple<string, int> ParamModelVisibility { get; set; }

        public VariantInfo() { }

        //Additional constructors to make adding the most common variant types easier
        public VariantInfo(List<string> labels, Dictionary<string, string> textureMappings)
        {
            MatchLabels = labels;
            Icon = labels[0];
            TextureMappings = textureMappings;
        }
        public VariantInfo(string label, Dictionary<string, string> textureMappings) : this([label], textureMappings) {}
        
        
        public bool MatchesLabel(string label) => MatchLabels.Contains(label.Split()[0]);
    }
}
