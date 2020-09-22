using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace TP1
{
	public class Game : GameWindow
	{
		public GameState State { get; private set; }

		public List<GameLevel> Levels { get; set; } = new List<GameLevel>();
		public int Level { get; set; }

		public GameObject Player { get; set; }
		private Vector2 PLAYER_SIZE = new Vector2(100, 20);
		private float PLAYER_VELOCITY = 8f;

		public Renderer2D Renderer { get; private set; }

		public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
		{
			GL.Viewport(0, 0, width, height);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			var point = PointToScreen(new Point(Width / 2, Height / 2));
			Mouse.SetPosition(point.X, point.Y);

			State = GameState.Paused;
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
			ResourceManager.LoadTexture("resources/paddle.png", "paddle");
			ResourceManager.LoadTexture("resources/block.png", "block", false);
			ResourceManager.LoadTexture("resources/block_solid.png", "block_solid", false);
			ResourceManager.LoadTexture("resources/breakout_bg.png", "breakout_bg", false);

			GameLevel one = new GameLevel();
			one.Load("levels/one.lvl", Width, Height / 2, 15);
			Levels.Add(one);
			Level = 0;

			Vector2 playerStartPos = new Vector2(Width / 2 - PLAYER_SIZE.X / 2, Height - PLAYER_SIZE.Y);
			Player = new GameObject(playerStartPos, PLAYER_SIZE, ResourceManager.GetTex("paddle"));
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (State == GameState.Active)
			{
				base.OnUpdateFrame(e);
				ProcessEvents();

				float x = Math.Max(Math.Min((float)(Player.Position.X + (PLAYER_VELOCITY * e.Time)), Width - Player.Size.X), 0);
				Player.Position = new Vector2(x, Player.Position.Y);
			}
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			//Renderer.DrawTexture(ResourceManager.GetTex("ball"), new Vector2(200, 200), new Vector2(300, 400), 45, Color.Green);
			if (State == GameState.Active || State == GameState.Paused)
			{
				Renderer.DrawTexture(ResourceManager.GetTex("breakout_bg"), Vector2.Zero, new Vector2(Width, Height), 0, Vector3.One);
				Levels[Level].Draw(Renderer);
				Player.Draw(Renderer);
			}

			Context.SwapBuffers();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			GL.Viewport(0, 0, Width, Height);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			if (e.KeyChar == 'q')
			{
				Close();
			}
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);
			PLAYER_VELOCITY = (e.X - (Width / 2)) * 4;
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.Button == MouseButton.Left)
			{
				if (State == GameState.Active)
					State = GameState.Paused;
				else if (State == GameState.Paused)
					State = GameState.Active;
			}
			base.OnMouseDown(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

			State = GameState.Active;
		}
	}
}
