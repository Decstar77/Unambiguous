using FontStashSharp.Interfaces;

using static OpenGL.GL;

namespace Game {
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

                varying vec4 v_color;
                varying vec2 v_texCoords;

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

                varying vec4 v_color;
                varying vec2 v_texCoords;

                void main()
                {
	                gl_FragColor = v_color * texture2D(TextureSampler, v_texCoords);
                }
            ";

            fontProgram = new ShaderProgram( vertexShaderSource, fragmentShaderSource );

            int bufferStride = 0;
            unsafe { bufferStride = sizeof( VertexPositionColorTexture ); }
            fontBuffer = Engine.GLCreateVertexBuffer( bufferStride, bufferStride * 6, true, stride => {
                unsafe {
                    glEnableVertexAttribArray( 0 );
                    glVertexAttribPointer( 0, 3, GL_FLOAT, false, stride, NULL );

                    glEnableVertexAttribArray( 1 );
                    glVertexAttribPointer( 1, 4, GL_UNSIGNED_BYTE, true, stride, (void*) (sizeof( float ) * 3) );

                    glEnableVertexAttribArray( 2 );
                    glVertexAttribPointer( 2, 2, GL_FLOAT, false, stride, (void*) (sizeof( float ) * 4) );
                }
            } );
        }

        public void DrawQuad(
            object fontTexture,
            ref VertexPositionColorTexture topLeft,
            ref VertexPositionColorTexture topRight,
            ref VertexPositionColorTexture bottomLeft,
            ref VertexPositionColorTexture bottomRight
        ) {
            FontTexture texture = (FontTexture)fontTexture;

            fontProgram.Bind();
            fontProgram.SetUniformMat4( "MatrixTransform", ref Engine.screenProjection );
            fontProgram.SetUniformTexture( "TextureSampler", 0, texture.texture );

            VertexPositionColorTexture[] verts = new VertexPositionColorTexture[] {
                 topLeft,
                 bottomLeft,
                 bottomRight,
                 topLeft,
                 bottomRight,
                 topRight
            };

            Engine.GLEnableAlphaBlending();
            Engine.GLUpdateVertexBuffer( fontBuffer, verts );
            Engine.GLDrawVertexBuffer( fontBuffer );

        }
    }
}
