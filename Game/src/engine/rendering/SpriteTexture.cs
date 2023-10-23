using OpenTK.Graphics.OpenGL4;

namespace Game {
    public class SpriteTexture {
        public int texture;
        public int width;
        public int height;
        public int channels;

        public SpriteTexture( int w, int h, int c, byte[] data ) {
            width = w;
            height = h;
            channels = c;
            texture = GL.GenTexture();

            GL.BindTexture( TextureTarget.Texture2D, texture );
            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data );
            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );
            GL.BindTexture( TextureTarget.Texture2D, 0 );
        }

    }
}
