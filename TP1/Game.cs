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
		private bool shouldPause;

		public List<GameLevel> Levels { get; set; } = new List<GameLevel>();
		public int Level { get; set; }

		public GameObject Player { get; set; }
		private Vector2 PLAYER_SIZE = new Vector2(150, 20);
		private float PLAYER_VELOCITY = 8f;
		private int Lives = 3;

		public BallObject Ball { get; set; }
		private readonly float BALL_RADIUS = 12.5f;
		private Vector2 BALL_VELOCITY = new Vector2(8f, -12f);

		public ParticleGenerator ParticleGenerator { get; set; }
		public PostProcessor PostProcessor { get; set; }
		private float shakeTime;

		public TextRenderer TextRenderer { get; set; }

		public List<PowerUp> PowerUps { get; set; } = new List<PowerUp>();

		public Renderer2D Renderer { get; private set; }

		public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title, GameWindowFlags.FixedWindow)
		{
			GL.Viewport(0, 0, width, height);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			Init();
		}

		public void Init()
		{
			var bgm = ResourceManager.LoadSound("resources/bgm.mp3", "bgm", true);
			SoundEngine.Instance.PlaySound(bgm);

			ResourceManager.LoadSound("resources/bleep.wav", "bleepPaddle");
			ResourceManager.LoadSound("resources/powerup.wav", "powerup");
			ResourceManager.LoadSound("resources/solid.wav", "solid");
			ResourceManager.LoadSound("resources/bleep.mp3", "bleepBrick");

			ResourceManager.LoadShader("shaders/sprite.vert", "shaders/sprite.frag", "sprite");
			ResourceManager.LoadShader("shaders/particle.vert", "shaders/particle.frag", "particle");
			ResourceManager.LoadShader("shaders/postProcessing.vert", "shaders/postProcessing.frag", "postProcessing");

			var projection = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, -1, 1);
			var spriteshader = ResourceManager.GetShader("sprite");
			var particleshader = ResourceManager.GetShader("particle");
			spriteshader.SetInteger("image", 0, true);
			spriteshader.SetMatrix4("projection", projection, true);
			particleshader.SetInteger("sprite", 0, true);
			particleshader.SetMatrix4("projection", projection, true);
			Renderer = new Renderer2D(spriteshader);

			ResourceManager.LoadTexture("resources/ball.png", "ball");
			ResourceManager.LoadTexture("resources/paddle.png", "paddle");
			ResourceManager.LoadTexture("resources/block.png", "block", false);
			ResourceManager.LoadTexture("resources/block_solid.png", "block_solid", false);
			ResourceManager.LoadTexture("resources/background.jpg", "breakout_bg", false);

			ResourceManager.LoadTexture("resources/particle.png", "particle");

			ResourceManager.LoadTexture("resources/speed.png", "speed");
			ResourceManager.LoadTexture("resources/meteor.png", "comet");
			ResourceManager.LoadTexture("resources/width.png", "padIncrease");
			ResourceManager.LoadTexture("resources/shrink.png", "padDecrease");
			ResourceManager.LoadTexture("resources/multiply.png", "multiply");
			ResourceManager.LoadTexture("resources/sticky.png", "sticky");
			ResourceManager.LoadTexture("resources/confuse.png", "confuse");
			ResourceManager.LoadTexture("resources/skull.png", "chaos");

			GameLevel one = new GameLevel();
			one.Load("levels/one.lvl", Width, Height / 2);
			GameLevel two = new GameLevel();
			two.Load("levels/two.lvl", Width, Height / 2);
			GameLevel three = new GameLevel();
			three.Load("levels/three.lvl", Width, Height / 2);
			GameLevel four = new GameLevel();
			four.Load("levels/four.lvl", Width, Height / 2);
			Levels.Add(one);
			Levels.Add(two);
			Levels.Add(three);
			Levels.Add(four);
			Level = 2;

			const float offset = 64f;
			Vector2 playerStartPos = new Vector2((Width / 2) - (PLAYER_SIZE.X / 2), Height - PLAYER_SIZE.Y - offset);
			Player = new GameObject(playerStartPos, PLAYER_SIZE, ResourceManager.GetTex("paddle"));

			Vector2 ballStartPos = playerStartPos + new Vector2((Player.Size.X / 2f) - BALL_RADIUS, -BALL_RADIUS * 2f);
			Ball = new BallObject(ballStartPos, BALL_RADIUS, ResourceManager.GetTex("ball"), Color.Pink.ToVector3(), velocity: BALL_VELOCITY);

			ParticleGenerator = new ParticleGenerator(ResourceManager.GetShader("particle"), ResourceManager.GetTex("particle"), 500);

			PostProcessor = new PostProcessor(ResourceManager.GetShader("postProcessing"), Width, Height);

			TextRenderer = new TextRenderer(Width, Height);
			TextRenderer.Load("fonts/ocraext.TTF", 24);

			var point = PointToScreen(new Point(Width / 2, Height / 2));
			Mouse.SetPosition(point.X, point.Y);

			State = GameState.Menu;
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (State == GameState.Active)
			{
				base.OnUpdateFrame(e);
				ProcessEvents();

				if (Ball.Position.Y > Height)
				{
					Lives--;
					if (Lives == 0)
					{
						Reset();
						return;
					}
					ResetPlayer();
				}

				float x = Math.Max(Math.Min((float)(Player.Position.X + (PLAYER_VELOCITY * e.Time)), Width - Player.Size.X), 0);
				if (x <= 0 || x >= Width - Player.Size.X)
				{
					Player.Velocity = Vector2.Zero;
				}
				else
				{
					Player.Velocity = new Vector2((float)(PLAYER_VELOCITY * e.Time), 0);
				}
				Player.Position = new Vector2(x, Player.Position.Y);

				Ball.Move((float)e.Time, Width);
				if (Ball.Stuck)
					Ball.Position += Player.Velocity;

				ParticleGenerator.Update((float)e.Time, Ball, 2, new Vector2(Ball.Radius / 2f));

				UpdatePowerUps((float)e.Time);

				DoCollisions();

				if (shouldPause)
				{
					State = GameState.Paused;
					shouldPause = false;
					LogInfo();
				}

				if (shakeTime > 0)
				{
					shakeTime -= (float)e.Time;
					if (shakeTime <= 0)
						PostProcessor.Shake = false;
				}
			}
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit);

			if (State == GameState.Active || State == GameState.Paused || State == GameState.Menu)
			{
				PostProcessor.BeginRender();
				Renderer.DrawTexture(ResourceManager.GetTex("breakout_bg"), Vector2.Zero, new Vector2(Width, Height), 0, Vector3.One);
				Levels[Level].Draw(Renderer);
				Player.Draw(Renderer);
				ParticleGenerator.Draw();
				Ball.Draw(Renderer);
				PowerUps.ForEach(p =>
				{
					if (!p.Destroyed)
						p.Draw(Renderer);
				});
				PostProcessor.EndRender();
				PostProcessor.Render((float)e.Time);
			}

			if (State != GameState.Menu)
			{
				TextRenderer.RenderText($"Lives: {Lives}", 5, 5, 1);
			}
			else
			{
				TextRenderer.RenderText("Press ENTER to start", Width / 2 - 150, Height / 2f + 20f, 1);
				TextRenderer.RenderText("Press W or S to select level", Width / 2 - 165, Height / 2f + 50f, .8f);
			}

			Context.SwapBuffers();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			GL.Viewport(0, 0, Width, Height);
		}

		protected override void OnKeyDown(KeyboardKeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Key == Key.Q)
			{
				Close();
			}
			if (State == GameState.Active)
			{
				if (e.Key == Key.R)
				{
					Reset();
				}
				if (e.Key == Key.Space)
				{
					Ball.Stuck = false;
				}
			}
			if (State == GameState.Menu)
			{
				if (e.Key == Key.W)
				{
					Level = (Level + 1) % 4;
				}
				if (e.Key == Key.S)
				{
					if (Level > 0)
						Level--;
					else
						Level = 3;
				}
				if (e.Key == Key.Enter)
				{
					State = GameState.Active;
				}
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
			if (e.Button == MouseButton.Right)
			{
				if (State == GameState.Active)
				{
					State = GameState.Paused;
					LogInfo();
				}
				else if (State == GameState.Paused)
				{
					State = GameState.Active;
					shouldPause = true;
				}
			}
			base.OnMouseDown(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
		}

		private void DoCollisions()
		{
			foreach (GameObject box in Levels[Level].Bricks)
			{
				Collision col = Ball.CheckCollision(box);
				if (!box.Destroyed && col.Colliding)
				{
					if (!box.IsSolid)
					{
						box.Destroyed = true;
						SpawnPowerUps(box);
						SoundEngine.Instance.PlaySound(ResourceManager.GetSound("bleepBrick"));
					}
					else
					{
						PostProcessor.Shake = true;
						shakeTime = .05f;
						SoundEngine.Instance.PlaySound(ResourceManager.GetSound("solid"));
					}

					if (!Ball.Comet || box.IsSolid)
					{
						float pen;
						switch (col.Direction)
						{
							case Direction.Left:
							case Direction.Right:
								Ball.Velocity = new Vector2(-Ball.Velocity.X, Ball.Velocity.Y);
								pen = Ball.Radius - Math.Abs(col.Vector.X);
								if (col.Direction == Direction.Left)
									Ball.Position = new Vector2(Ball.Position.X + pen, Ball.Position.Y);
								else
									Ball.Position = new Vector2(Ball.Position.X - pen, Ball.Position.Y);
								break;
							default:
								Ball.Velocity = new Vector2(Ball.Velocity.X, -Ball.Velocity.Y);
								pen = Ball.Radius - Math.Abs(col.Vector.Y);
								if (col.Direction == Direction.Up)
									Ball.Position = new Vector2(Ball.Position.X, Ball.Position.Y - pen);
								else
									Ball.Position = new Vector2(Ball.Position.X, Ball.Position.Y + pen);
								break;
						}
						break;
					}
				}
			}

			Collision playerBallCol = Ball.CheckCollision(Player);
			if (!Ball.Stuck && playerBallCol.Colliding)
			{
				float paddleCenter = Player.Position.X + Player.Size.X / 2;
				float distance = Ball.Position.X + Ball.Radius - paddleCenter;
				float percentage = distance / Player.Size.X / 2;

				float velocityMag = Ball.Velocity.Length;
				var relativeVelocity = new Vector2(BALL_VELOCITY.X * percentage * 2, -Math.Abs(Ball.Velocity.Y));
				Ball.Velocity = relativeVelocity.Normalized() * velocityMag;
				Ball.Stuck = Ball.Sticky;
				SoundEngine.Instance.PlaySound(ResourceManager.GetSound("bleepPaddle"));
			}

			foreach (PowerUp power in PowerUps)
			{
				if (!power.Destroyed)
				{
					if (power.Position.Y >= Height)
						power.Destroyed = true;
					if (Player.CheckCollision(power).Colliding)
					{
						power.Destroyed = true;
						ActivatePowerUp(power);
						power.Active = true;
						SoundEngine.Instance.PlaySound(ResourceManager.GetSound("powerup"));

					}
				}
			}
		}

		private void ActivatePowerUp(PowerUp power)
		{
			switch (power.Type)
			{
				case "speed":
					Ball.Velocity *= 1.5f;
					break;
				case "sticky":
					Ball.Sticky = true;
					Player.Color = Color.PaleGreen.ToVector3();
					break;
				case "comet":
					Ball.Comet = true;
					Ball.Color = Color.DarkRed.ToVector3();
					break;
				case "padIncrease":
					Player.Size = new Vector2(Player.Size.X * 1.25f, Player.Size.Y);
					break;
				case "padDecrease":
					Player.Size = new Vector2(Player.Size.X * .75f, Player.Size.Y);
					break;
				case "confuse":
					PostProcessor.Confuse = true;
					break;
				case "chaos":
					PostProcessor.Chaos = true;
					break;
				case "multiply":
					break;
			}
		}

		private void SpawnPowerUps(GameObject brick)
		{
			var rand = new Random();
			string[] types =
			{
				"speed", "sticky", "comet", "padIncrease", "padDecrease", "confuse", "chaos", "multiply"
			};
			Dictionary<string, float> duration = new Dictionary<string, float>()
			{
				{"speed", 15},{"sticky", 10},{"comet", 3},{"padIncrease", 10},{"padDecrease", 5},{"confuse", 10},{"chaos", 3},{"multiply", .1f},
			};
			if (rand.NextDouble() > .75)
			{
				string type = types[rand.Next(0, types.Length)];
				PowerUps.Add(new PowerUp(type, Color.DeepPink.ToVector3(), duration[type], brick.Position, ResourceManager.GetTex(type)));
			}
		}

		private void UpdatePowerUps(float dt)
		{
			for (int i = 0; i < PowerUps.Count; i++)
			{
				PowerUps[i].Position += PowerUps[i].Velocity;
				if (PowerUps[i].Active)
				{
					PowerUps[i].Duration -= dt;
					if (PowerUps[i].Duration <= 0)
					{
						PowerUps[i].Active = false;
						switch (PowerUps[i].Type)
						{
							case "speed":
								Ball.Velocity /= 1.5f;
								break;
							case "sticky":
								Ball.Sticky = false;
								Player.Color = new Vector3(1);
								break;
							case "comet":
								Ball.Comet = false;
								Ball.Color = new Vector3(1);
								break;
							case "padIncrease":
								Player.Size = new Vector2(Player.Size.X / 1.25f, Player.Size.Y);
								break;
							case "padDecrease":
								Player.Size = new Vector2(Player.Size.X / .75f, Player.Size.Y);
								break;
							case "confuse":
								PostProcessor.Confuse = false;
								break;
							case "chaos":
								PostProcessor.Chaos = false;
								break;
							case "multiply":
								break;
						}
					}
				}
				if (!PowerUps[i].Active && PowerUps[i].Destroyed)
				{
					PowerUps.RemoveAt(i);
				}
			}
		}

		private void Reset()
		{
			State = GameState.Menu;
			Levels.Clear();
			UpdatePowerUps(100000f);
			PowerUps.Clear();
			Lives = 3;
			//SoundEngine.Instance.Reset();
			Init();
		}

		private void ResetPlayer()
		{
			Vector2 playerStartPos = new Vector2((Width / 2) - (PLAYER_SIZE.X / 2), Height - PLAYER_SIZE.Y - 64f);
			Player.Position = playerStartPos;
			Player.Size = PLAYER_SIZE;
			Player.Color = new Vector3(1);
			Ball.Reset(Player.Position + new Vector2((Player.Size.X / 2) - BALL_RADIUS, -BALL_RADIUS * 2f), BALL_VELOCITY);
			Ball.Size = new Vector2(BALL_RADIUS);
			PostProcessor.Chaos = false;
			PostProcessor.Confuse = false;
		}

		private void LogInfo()
		{
			Console.WriteLine("======================================");
			Console.WriteLine("DEBUG INFORMATION");
			Console.WriteLine("Paddle");
			Console.WriteLine("\t Position:" + Player.Position);
			Console.WriteLine("\t Velocity:" + Player.Velocity);
			Console.WriteLine("\t Size:" + Player.Size);
			Console.WriteLine("Ball");
			Console.WriteLine("\t Position:" + Ball.Position);
			Console.WriteLine("\t Velocity:" + Ball.Velocity);
			Console.WriteLine("\t Radius:" + Ball.Radius);
			Console.WriteLine("Bricks");
			for (int i = 0; i < Levels[Level].Bricks.Count; i++)
			{
				var brick = Levels[Level].Bricks[i];
				Console.WriteLine("\t Brick" + i);
				Console.WriteLine("\t\t Position" + brick.Position);
				Console.WriteLine("\t\t IsSolid" + brick.IsSolid);
				Console.WriteLine("\t\t Destroyed" + brick.Destroyed);
			}
			Console.WriteLine("======================================");
		}
	}
}
