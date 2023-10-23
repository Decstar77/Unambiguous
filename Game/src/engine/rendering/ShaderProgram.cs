using System.Diagnostics;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game {
    public class ShaderProgram {

        public int programId;
        public int vertexShaderId;
        public int fragmentShaderId;
        public Dictionary<string, int> uniformLocations = new Dictionary<string, int>();

        public ShaderProgram( string vertexShaderSrc, string fragmentShaderSrc ) {
            vertexShaderId = CreateShader( ShaderType.VertexShader, vertexShaderSrc );
            fragmentShaderId = CreateShader( ShaderType.FragmentShader, fragmentShaderSrc );

            programId = GL.CreateProgram();
            GL.AttachShader( programId, vertexShaderId );
            GL.AttachShader( programId, fragmentShaderId );
            GL.LinkProgram( programId );

            // Check errors
            GL.GetProgram( programId, GetProgramParameterName.LinkStatus, out int status );
            if ( status == 0 ) {
                string error = GL.GetProgramInfoLog( programId );
                Debug.WriteLine( error );
            }

            GL.DeleteShader( vertexShaderId );
            GL.DeleteShader( fragmentShaderId );
        }

        public void Bind() {
            GL.UseProgram( programId );
        }

        public void SetUniformInt( string name, int value ) {
            if ( !uniformLocations.ContainsKey( name ) ) {
                CacheUniformLocation( name );
            }
            GL.Uniform1( uniformLocations[name], value );
        }

        public void SetUniformVec4( string name, Vector4 v ) {
            if ( !uniformLocations.ContainsKey( name ) ) {
                CacheUniformLocation( name );
            }
            GL.Uniform4( uniformLocations[name], v );
        }

        public void SetUniformMat4( string name, ref Matrix4 v ) {
            if ( !uniformLocations.ContainsKey( name ) ) {
                CacheUniformLocation( name );
            }
            
            GL.UniformMatrix4( uniformLocations[name], false, ref v );
        }

        public void SetUniformTexture( string name, int textureSlot, int textureHandle ) {
            if ( !uniformLocations.ContainsKey( name ) ) {
                CacheUniformLocation( name );
            }
            GL.ActiveTexture( TextureUnit.Texture0 + textureSlot );
            GL.BindTexture( TextureTarget.Texture2D, textureHandle );
            GL.Uniform1( uniformLocations[name], textureSlot );
        }

        private void CacheUniformLocation( string name ) {
            int location = GL.GetUniformLocation( programId, name );
            Debug.Assert( location != -1, "Uniform does not exist!" );
            uniformLocations.Add( name, location );
        }

        private int CreateShader( ShaderType type, string src ) {
            int shaderId = GL.CreateShader( type );
            GL.ShaderSource( shaderId, src );
            GL.CompileShader( shaderId );

            // Check for errors
            int status;
            GL.GetShader( shaderId, ShaderParameter.CompileStatus, out status );
            if ( status == 0 ) {
                string infoLog = GL.GetShaderInfoLog( shaderId );
                Console.WriteLine( infoLog );
                throw new Exception( "Shader compilation failed!" );
            }

            return shaderId;
        }

    }
}
