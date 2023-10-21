using FontStashSharp.Interfaces;
using System.Drawing;

using static OpenGL.GL;

namespace Game {
    public class FontTexture : IDisposable {
        public uint texture = 0;
        public int width = 0;
        public int height = 0;
        public int channels = 0;

        public FontTexture( int width, int height ) {
            Logger.Log( $"Creating font texture {width}x{height}" );

            this.width = width;
            this.height = height;
            this.channels = 4;

            texture = glGenTexture();
            glBindTexture( GL_TEXTURE_2D, texture );
            unsafe { glTexImage2D( GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, null ); }
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR );
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR );
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE );
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE );

            glGenerateMipmap( GL_TEXTURE_2D );

            glBindTexture( GL_TEXTURE_2D, 0 );
        }

        public void SetTextureData( Rectangle bounds, byte[] data ) {
            glBindTexture( GL_TEXTURE_2D, (uint)texture );
            unsafe {
                fixed ( byte* ptr = data ) {
                    glTexSubImage2D( GL_TEXTURE_2D, 0, bounds.X, bounds.Y, bounds.Width, bounds.Height, GL_RGBA, GL_UNSIGNED_BYTE, ptr );
                }
            }
            glBindTexture( GL_TEXTURE_2D, 0 );
        }

        public void Dispose() {
            glDeleteTexture( texture );
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
