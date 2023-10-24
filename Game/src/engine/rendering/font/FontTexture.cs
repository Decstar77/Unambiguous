using FontStashSharp.Interfaces;
using System.Drawing;

using OpenTK.Graphics.OpenGL4;

namespace Game {
    public class FontTexture : IDisposable {
        public int texture = 0;
        public int width = 0;
        public int height = 0;
        public int channels = 0;

        public FontTexture( int width, int height ) {
            Logger.Log( $"Creating font texture {width}x{height}" );

            this.width = width;
            this.height = height;
            this.channels = 4;

            texture = GL.GenTexture();
            GL.BindTexture( TextureTarget.Texture2D, texture );
            GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, System.IntPtr.Zero );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge );
            GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge );
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture( TextureTarget.Texture2D, 0 );
        }

        public void SetTextureData( Rectangle bounds, byte[] data ) {
            GL.BindTexture( TextureTarget.Texture2D, texture );
            GL.TexSubImage2D( TextureTarget.Texture2D, 0, bounds.X, bounds.Y, bounds.Width, bounds.Height, PixelFormat.Rgba, PixelType.UnsignedByte, data );
            GL.BindTexture( TextureTarget.Texture2D, 0 );
        }

        public void Dispose() {
            if ( texture != 0 ) {
                GL.DeleteTexture( texture );
                texture = 0;
            }
        }
    }

    public class FontTextureCallbacks : ITexture2DManager {
        public object CreateTexture( int width, int height ) {
            return new FontTexture( width, height );
        }

        public Point GetTextureSize( object texture ) {
            FontTexture t = ( FontTexture)texture;
            return new Point( t.width, t.height );
        }

        public void SetTextureData( object texture, Rectangle bounds, byte[] data ) {
            FontTexture t = ( FontTexture)texture;
            t.SetTextureData( bounds, data );
        }
    }
}
