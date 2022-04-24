using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;

namespace CafeLibrary.Rendering
{
    public class ShaderRenderBase : BfresMaterialRender
    {
        /// <summary>
        /// Determines to enable SRGB or not when drawn to the final framebuffer.
        /// </summary>
        public virtual bool UseSRGB { get; } = true;

        /// <summary>
        /// Determines if the current material has a valid program.
        /// If the model fails to find a program in ReloadProgram, it will fail to load the shader.
        /// </summary>
        public virtual bool HasValidProgram { get; }

        /// <summary>
        /// Shader information from the decoded shader.
        /// This is used to store constants and source information.
        /// </summary>
        public ShaderInfo GLShaderInfo => GLShaders[ShaderIndex];

        /// <summary>
        /// Gets or sets a list of shaders used.
        /// </summary>
        public ShaderInfo[] GLShaders = new ShaderInfo[10];

        public int ShaderIndex { get; set; } = 0;

        /// <summary>
        /// A list of uniform blocks to store the current block data.
        /// Blocks are obtained using GetBlock() and added if one does not exist.
        /// </summary>
        public Dictionary<string, UniformBlock> UniformBlocks = new Dictionary<string, UniformBlock>();

        /// <summary>
        /// Gets or sets the state of the shader file.
        /// </summary>
        public ShaderState ShaderFileState { get; set; }

        public ShaderRenderBase(BfresRender render, BfresModelRender model) : base(render, model)
        {

        }

        public virtual bool UseRenderer(BfresLibrary.Material material, string archive, string model)
        {
            return false;
        }

        public enum ShaderState
        {
            /// <summary>
            /// The shader file is from a global source.
            /// </summary>
            Global,
            /// <summary>
            /// The shader file is embedded in a resource file.
            /// </summary>
            EmbeddedResource,
            /// <summary>
            /// The shader file is inside an archive file.
            /// </summary>
            EmbeddedArchive,
        }
    }
}
