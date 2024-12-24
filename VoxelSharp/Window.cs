using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace VoxelSharp
{
    public class Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : GameWindow(gameWindowSettings, nativeWindowSettings)
    {
        private Shader? _chunkShader;

        private World.World _world = new(12, 16);



        // Now, we start initializing OpenGL.
        protected override void OnLoad()
        {
            base.OnLoad();

            _chunkShader = new Shader("Shaders/chunk.vert", "Shaders/chunk.frag");


            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);


            GL.Clear(ClearBufferMask.ColorBufferBit);

            _chunkShader.Use();
            
            _world.Render(_chunkShader);


            Shader.UnUse();

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