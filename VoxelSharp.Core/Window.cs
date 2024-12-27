﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelSharp.Core.Interfaces;
using VoxelSharp.Core.Renderer;
using VoxelSharp.Core.Renderer.Camera;
using VoxelSharp.Core.Structs;
using VoxelSharp.Core.World;

namespace VoxelSharp.Core
{
    public class Window : GameWindow, IWindow
    {
        private readonly Shader _chunkShader;

        private readonly World.World _world;

        private readonly FlyingCamera _camera;

        private readonly List<IUpdatable> _updatables;
        private readonly List<IRenderable> _renderables;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings,
            List<IUpdatable>? updatables = null, List<IRenderable>? renderables = null) :
            base(gameWindowSettings, nativeWindowSettings)
        {
            _chunkShader = new Shader("Shaders/chunk.vert", "Shaders/chunk.frag");


            _camera = new FlyingCamera((float)Size.X / Size.Y);
            _world = new World.World(2, 16);

            _updatables = updatables ?? [];
            _renderables = renderables ?? [];

            // create a test plane

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    _world.SetVoxel(new Position<int>(x, 0, z), new Voxel(Color.Red));
                }
            }
        }


        protected override void OnLoad()
        {
            base.OnLoad();


            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Disable(EnableCap.CullFace);

            GL.Clear(ClearBufferMask.DepthBufferBit);


            GL.Enable(EnableCap.Blend); // Enable blending for transparency
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); // Set blending function


            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        }

        private float _redValue = 0.0f;
        private double _elapsedTime = 0.0f; // Accumulate total time


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);


            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            GL.ClearColor(_redValue, 0.3f, 0.3f, 1.0f); // Change background colour

            _chunkShader.Use();

            _chunkShader.SetUniform("m_view", _camera.GetViewMatrix());
            _chunkShader.SetUniform("m_projection", _camera.GetProjectionMatrix());


            _world.Render(_chunkShader);

            Shader.UnUse();

            foreach (var renderable in _renderables)
            {
                renderable.Render();
            }

            SwapBuffers();
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            // Handle movement inputs
            if (KeyboardState.IsKeyDown(Keys.W)) _camera.MoveForward();
            if (KeyboardState.IsKeyDown(Keys.S)) _camera.MoveBackward();
            if (KeyboardState.IsKeyDown(Keys.A)) _camera.MoveLeft();
            if (KeyboardState.IsKeyDown(Keys.D)) _camera.MoveRight();
            if (KeyboardState.IsKeyDown(Keys.Space)) _camera.MoveUp();
            if (KeyboardState.IsKeyDown(Keys.LeftShift)) _camera.MoveDown();

            // Handle mouse inputs for camera rotation
            var (deltaX, deltaY) = MouseState.Delta;
            _camera.UpdateRotation(deltaX, -deltaY);

            // Update camera state
            _camera.Update((float)e.Time);

            foreach (var updatable in _updatables)
            {
                updatable.Update((float)e.Time);
            }


            _elapsedTime += e.Time; // Accumulate the total elapsed time
            _redValue = 0.25f * (MathF.Sin((float)(_elapsedTime * 0.5)) + 1); // Oscillate smoothly
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);

            // Update projection matrix for the new aspect ratio
            _camera.UpdateAspectRatio((float)Size.X / Size.Y);
        }


        public (int Width, int Height) ScreenSize => (Size.X, Size.Y);
    }
}