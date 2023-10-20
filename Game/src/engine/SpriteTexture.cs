using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static OpenGL.GL;

namespace Game {
    public class SpriteTexture {
        public uint texture;
        public int width;
        public int height;
        public int channels;

        public SpriteTexture( int w, int h, int c, ReadOnlySpan<byte> data ) {
            width = w;
            height = h;
            channels = c;
            texture = glGenTexture();
            glBindTexture( GL_TEXTURE_2D, texture );
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE );
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE );
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR );
            glTexParameteri( GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR );
            unsafe {
                fixed ( byte* d = &data[0] ) {
                    glTexImage2D( GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, d );
                }
            }
        }

        public void Bind( int slot ) {
            glActiveTexture( GL_TEXTURE0 + slot );
            glBindTexture( GL_TEXTURE_2D, texture );
        }
    }
}
