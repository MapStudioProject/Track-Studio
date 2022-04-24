using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using GLFrameworkEngine;

namespace CafeLibrary.Rendering
{
    public class PostEffectsQuad
    {
        public static ShaderProgram DefaultShaderProgram { get; private set; }

        static VertexBufferObject vao;

        static int Length;

        public static void Initialize(GLContext control)
        {
            if (DefaultShaderProgram != null)
                return;

            if (DefaultShaderProgram == null)
            {
                string vert = @"#version 330
                        layout (location = 0) in vec2 aPos;
                        layout (location = 1) in vec2 aTexCoords;

                        out vec2 TexCoords;

                        void main()
                        {
                            gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0); 
                            TexCoords = aTexCoords;
                        }";
                string frag = @"#version 330 core
                    out vec4 FragColor;
  
                    in vec2 TexCoords;

                    uniform sampler2D scene;
                    uniform sampler2D bloomBlur;
                    uniform float exposure;

                    void main()
                    {             
                        const float gamma = 2.2;
                        vec3 hdrColor = texture(scene, TexCoords).rgb;      
                        vec3 bloomColor = texture(bloomBlur, TexCoords).rgb * 0.7f;
                        hdrColor += bloomColor; // additive blending
                        // tone mapping
                     //   vec3 result = vec3(1.0) - exp(-hdrColor * exposure);
                        FragColor = vec4(hdrColor, 1.0);
                    }";

                
                DefaultShaderProgram = new ShaderProgram(
                    new FragmentShader(frag), new VertexShader(vert));

                int buffer = GL.GenBuffer();
                vao = new VertexBufferObject(buffer);
                vao.AddAttribute(0, 2, VertexAttribPointerType.Float, false, 16, 0);
                vao.AddAttribute(1, 2, VertexAttribPointerType.Float, false, 16, 8);
                vao.Initialize();

                Vector2[] positions = new Vector2[4]
                {
                    new Vector2(-1.0f, 1.0f),
                    new Vector2(-1.0f, -1.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(1.0f, -1.0f),
                };

                Vector2[] texCoords = new Vector2[4]
                {
                    new Vector2( 0.0f, 1.0f),
                    new Vector2(0.0f, 0.0f),
                    new Vector2(1.0f, 1.0f),
                    new Vector2(1.0f, 0.0f),
                };

                List<float> list = new List<float>();
                for (int i = 0; i < 4; i++)
                {
                    list.Add(positions[i].X);
                    list.Add(positions[i].Y);
                    list.Add(texCoords[i].X);
                    list.Add(texCoords[i].Y);
                }

                Length = 4;

                float[] data = list.ToArray();
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.StaticDraw);
            }
            else
            {
                vao.Initialize();
                DefaultShaderProgram.Link();
            }
        }

        public static void Draw(GLContext control, GLTexture screen, GLTexture brightnessTexture)
        {
            Initialize(control);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            control.CurrentShader = DefaultShaderProgram;

            DefaultShaderProgram.SetFloat("exposure", 1.0f);

            GL.ActiveTexture(TextureUnit.Texture23);
            screen.Bind();
            DefaultShaderProgram.SetInt("scene", 23);

            GL.ActiveTexture(TextureUnit.Texture24);
            brightnessTexture.Bind();
            DefaultShaderProgram.SetInt("bloomBlur", 24);

            vao.Enable(DefaultShaderProgram);
            vao.Use();
            GL.DrawArrays(PrimitiveType.QuadStrip, 0, Length);
        }
    }
}
