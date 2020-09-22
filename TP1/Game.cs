using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace TP1
{
	public class Game : GameWindow
	{
		public GameState State { get; private set; }
		public Renderer2D Renderer { get; private set; }
		public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) 
		{
			GL.Viewport(0, 0, width, height);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.ConstantAlpha, BlendingFactor.OneMinusConstantAlpha);

			State = GameState.Active;
			Init();
		}

		public void Init()
		{
			ResourceManager.LoadShader("shaders/sprite.vert", "shaders/sprite.frag", "sprite");

			var projection = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, -1, 1);
			var spriteshader = ResourceManager.GetShader("sprite");
			spriteshader.SetInteger("image", 0, true);
			spriteshader.SetMatrix4("projection", projection, true);
			Renderer = new Renderer2D(spriteshader);
			ResourceManager.LoadTexture("resources/awesomeface.png", "ball");
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			ProcessEvents();
			base.OnUpdateFrame(e);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			//Renderer.DrawTexture(ResourceManager.GetTex("ball"), new Vector2(200, 200), new Vector2(512, 512), 0, Color.White);
			
			GL.Begin(PrimitiveType.Triangles);

			GL.Color3(Color.MidnightBlue);
			GL.Vertex2(-1.0f, 1.0f);
			GL.Color3(Color.SpringGreen);
			GL.Vertex2(0.0f, -1.0f);
			GL.Color3(Color.Ivory);
			GL.Vertex2(1.0f, 1.0f);
			GL.End();
			SwapBuffers();
			//Context.SwapBuffers();
			base.OnRenderFrame(e);
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);
			base.OnResize(e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (e.KeyChar == 'q')
			{
				Close();
			}
			base.OnKeyPress(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

			//Code goes here

			base.OnLoad(e);
		}
	}
}
