using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelSharp
{
  
    public class Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : GameWindow(gameWindowSettings, nativeWindowSettings)
    {

        float[] _vertices =
        {
            0.5f, 0.5f, 0.0f, // top right
            0.5f, -0.5f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, // bottom left
            -0.5f, 0.5f, 0.0f // top left
        };

        uint[] indices =
        {
            // note that we start from 0!
            0, 1, 3, // first triangle
            1, 2, 3 // second triangle
        };

     
        private int _vertexBufferObject;

        private int _vertexArrayObject;
        
        private Shader? _shader;


        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");


            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices,
                BufferUsageHint.StaticDraw);
            
            


            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);


            GL.VertexAttribPointer(_shader.GetAttribLocation("aPosition"), 3, VertexAttribPointerType.Float, false,
                3 * sizeof(float), 0);

            GL.EnableVertexAttribArray(0);

            _shader.Use();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);


            GL.Clear(ClearBufferMask.ColorBufferBit);


            _shader.Use();

            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);


            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);


            GL.Viewport(0, 0, Size.X, Size.Y);
        }
    }
}