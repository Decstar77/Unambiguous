using FontStashSharp.Interfaces;
using OpenTK.Graphics.OpenGL4;

namespace Game
{
    public class FontRenderer : IFontStashRenderer2 {
        public ITexture2DManager TextureManager { get; }
        public ShaderProgram fontProgram = null;
        public VertexBuffer  fontBuffer = null;

        public FontRenderer() {
            Logger.Log( "Creating FontRenderer" );
            TextureManager = new FontTextureCallbacks();

            string vertexShaderSource= @"
                #version 330 core
                layout (location = 0) in vec3 a_position;
                layout (location = 1) in vec4 a_color;
                layout (location = 2) in vec2 a_texCoords0;

                uniform mat4 MatrixTransform;

                out vec4 v_color;
                out vec2 v_texCoords;

                void main()
                {
	                v_color = a_color;
	                v_texCoords = a_texCoords0;
	                gl_Position = MatrixTransform * vec4(a_position, 1.0);
                }
            ";

            string fragmentShaderSource = @"
                #version 330 core
                uniform sampler2D TextureSampler;

                in vec4 v_color;
                in vec2 v_texCoords;

                out vec4 FragColor;
                void main()
                {
	                FragColor = v_color * texture2D(TextureSampler, v_texCoords);
                }
            ";

            fontProgram = new ShaderProgram( vertexShaderSource, fragmentShaderSource );

            int bufferStride = 0;
            unsafe { bufferStride = sizeof( VertexPositionColorTexture ); }
            fontBuffer = new VertexBuffer( bufferStride, bufferStride * 6, true, stride => {
                GL.EnableVertexAttribArray( 0 );
                GL.VertexAttribPointer( 0, 3, VertexAttribPointerType.Float, false, stride, 0 );

                GL.EnableVertexAttribArray( 1 );
                GL.VertexAttribPointer( 1, 4, VertexAttribPointerType.UnsignedByte, true, stride, sizeof( float ) * 3 );

                GL.EnableVertexAttribArray( 2 );
                GL.VertexAttribPointer( 2, 2, VertexAttribPointerType.Float, false, stride, sizeof( float ) * 4 );
            } );
        }

        public void DrawQuad(
            object fontTexture,
            ref VertexPositionColorTexture topLeft,
            ref VertexPositionColorTexture topRight,
            ref VertexPositionColorTexture bottomLeft,
            ref VertexPositionColorTexture bottomRight
        ) {
            // Unpack and pack
            FontTexture texture = (FontTexture)fontTexture;
            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[] {
                 topLeft,
                 bottomLeft,
                 bottomRight,
                 topLeft,
                 bottomRight,
                 topRight
            };

            Engine.GLEnablePreMultipliedAlphaBlending();

            fontProgram.Bind();
            fontProgram.SetUniformMat4( "MatrixTransform", ref Engine.screenProjection );
            fontProgram.SetUniformTexture( "TextureSampler", 0, texture.texture );
            fontBuffer.UpdateVertexBuffer( verts );
            fontBuffer.DrawVertexBuffer();
        }
    }
}
