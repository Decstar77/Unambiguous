using OpenTK.Graphics.OpenGL4;

namespace Game
{

    public class VertexBuffer
    {
        public delegate void CreateVAttribs(int stride);

        public int vao;
        public int vbo;
        public int size;
        public int count;
        public int stride;
        public bool dyna;

        public VertexBuffer(int stride, int size, bool dyna, CreateVAttribs createVAttribs)
        {
            this.size = size;
            this.stride = stride;
            count = size / stride;
            this.dyna = dyna;

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, size, IntPtr.Zero, dyna ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw);

            createVAttribs( stride );
        }

        public void UpdateVertexBuffer<T>(T[] data) where T : unmanaged
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, size, data);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void DrawVertexBuffer()
        {
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, count);
            GL.BindVertexArray(0);
        }
    }
}
