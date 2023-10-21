using System;

using static OpenGL.GL;

namespace Game {

    public class VertexBuffer {
        public delegate void CreateVAttribs( int stride );

        public uint     vao;
        public uint     vbo;
        public int      size;
        public int      count;
        public int      stride;
        public bool     dynamic;

        public VertexBuffer( int stride, int size, bool dyanmic, CreateVAttribs createVAttribs ) {
            this.size = size;
            this.stride = stride;
            this.count = size / stride;
            this.dynamic = dyanmic;

            this.vao = glGenVertexArray();
            this.vbo = glGenBuffer();

            glBindVertexArray( vao );
            glBindBuffer( GL_ARRAY_BUFFER, vbo );

            unsafe {
                glBufferData( GL_ARRAY_BUFFER, size, NULL, dyanmic ? GL_DYNAMIC_DRAW : GL_STATIC_DRAW );
                createVAttribs.Invoke( stride );
            }
        }

        public void UpdateVertexBuffer<T>( T[] data ) where T : unmanaged {
            glBindBuffer( GL_ARRAY_BUFFER, vbo );
            unsafe {
                fixed ( T* ptr = &data[0] ) {
                    glBufferSubData( GL_ARRAY_BUFFER, 0, size, ptr );
                }
            }
            glBindBuffer( GL_ARRAY_BUFFER, 0 );
        }

        public void DrawVertexBuffer() {
            glBindVertexArray( vao );
            glDrawArrays( GL_TRIANGLES, 0, count );
            glBindVertexArray( 0 );
        }
    }
}
