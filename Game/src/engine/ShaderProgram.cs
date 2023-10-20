using System.Diagnostics;
using System.Numerics;

using static OpenGL.GL;

namespace Game {
    public class ShaderProgram {

        public uint programId;
        public uint vertexShaderId;
        public uint fragmentShaderId;
        public Dictionary<string, int> uniformLocations = new Dictionary<string, int>();

        public ShaderProgram( string vertexShaderSrc, string fragmentShaderSrc ) {
            vertexShaderId = CreateShader( GL_VERTEX_SHADER, vertexShaderSrc );
            fragmentShaderId = CreateShader( GL_FRAGMENT_SHADER, fragmentShaderSrc );

            programId = glCreateProgram();
            glAttachShader( programId, vertexShaderId );
            glAttachShader( programId, fragmentShaderId );
            glLinkProgram( programId );

            unsafe {
                int status;
                glGetProgramiv( programId, GL_LINK_STATUS, &status );
                if ( status == GL_FALSE ) {
                    string infoLog = glGetProgramInfoLog( programId );
                    Console.WriteLine( infoLog );
                    throw new Exception( "Shader program linking failed!" );
                }
            }

            glDeleteShader( vertexShaderId );
            glDeleteShader( fragmentShaderId );
        }

        public void Use() {
            glUseProgram( programId );
        }

        public void SetUniformInt( string name, int value ) {
            if ( !uniformLocations.ContainsKey( name ) ) {
                CacheUniformLocation( name );
            }
            glUniform1i( uniformLocations[name], value );
        }

        public void SetUniformVec4( string name, Vector4 v ) {
            if ( !uniformLocations.ContainsKey( name ) ) {
                CacheUniformLocation( name );
            }
            glUniform4f( uniformLocations[name], v.X, v.Y, v.Z, v.W );
        }

        private void CacheUniformLocation( string name ) {
            int location = glGetUniformLocation( programId, name );
            Debug.Assert( location != -1, "Uniform does not exist!" );
            uniformLocations.Add( name, location );
        }

        private static uint CreateShader( int type, string src ) {
            uint shaderId = glCreateShader(type);
            glShaderSource( shaderId, src );
            glCompileShader( shaderId );

            unsafe {
                int status;
                glGetShaderiv( shaderId, GL_COMPILE_STATUS, &status );
                if ( status == GL_FALSE ) {
                    string infoLog = glGetShaderInfoLog( shaderId );
                    Console.WriteLine( infoLog );
                    throw new Exception( "Shader compilation failed!" );
                }
            }

            return shaderId;
        }

    }
}
