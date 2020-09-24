using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TP1
{
	public class Game : GameWindow
	{
		public GameState State { get; private set; }
		private bool shouldPause, showInfo;
		private string[] lines = new string[] { };

		public List<GameLevel> Levels { get; set; } = new List<GameLevel>();
		public int Level { get; set; }

		public GameObject Player { get; set; }
		private Vector2 PLAYER_SIZE = new Vector2(150, 20);
		private float PLAYER_VELOCITY = 8f;
		private int Lives = 3;
		public Difficulty Difficulty { get; set; }
		public List<Difficulty> Difficulties { get; set; } = new List<Difficulty>();

		public BallObject Ball { get; set; }
		private readonly float BALL_RADIUS = 12.5f;
		private const float BALL_X_SPEED = 8;
		private Vector2 BALL_VELOCITY = new Vector2(BALL_X_SPEED, -12f);

		public ParticleGenerator ParticleGenerator { get; set; }
		public PostProcessor PostProcessor { get; set; }
		private float shakeTime;

		public TextRenderer TextRenderer { get; set; }
		public Texture2D Overlay { get; set; }

		public List<PowerUp> PowerUps { get; set; } = new List<PowerUp>();

		public Renderer2D Renderer { get; private set; }

		public Game(int width, int height, string title) : base(width, height, GraphicsMode.Default, title, GameWindowFlags.FixedWindow)
		{
			GL.Viewport(0, 0, width, height);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			Difficulties.Add(new Difficulty("easy", -12f, .65, 4));
			Difficulties.Add(new Difficulty("medium", -14f, .45, 3));
			Difficulties.Add(new Difficulty("hard", -16f, .15, 2));

			Difficulty = Difficulties[0];

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

			const float offset = 84f;
			Vector2 playerStartPos = new Vector2((Width / 2) - (PLAYER_SIZE.X / 2), Height - PLAYER_SIZE.Y - offset);
			Player = new GameObject(playerStartPos, PLAYER_SIZE, ResourceManager.GetTex("paddle"));

			Vector2 ballStartPos = playerStartPos + new Vector2((Player.Size.X / 2f) - BALL_RADIUS, -BALL_RADIUS * 2f);
			BALL_VELOCITY = new Vector2(GetRandomBallSpeed(), Difficulty.BallUpSpeed);
			Ball = new BallObject(ballStartPos, BALL_RADIUS, ResourceManager.GetTex("ball"), Color.Pink.ToVector3(), velocity: BALL_VELOCITY);

			ParticleGenerator = new ParticleGenerator(ResourceManager.GetShader("particle"), ResourceManager.GetTex("particle"), 500);

			PostProcessor = new PostProcessor(ResourceManager.GetShader("postProcessing"), Width, Height);

			TextRenderer = new TextRenderer(Width, Height);
			TextRenderer.Load("fonts/ocraext.TTF", 24);
			Overlay = new Texture2D();
			Overlay.Generate(Width, Height, IntPtr.Zero);

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

			if (State == GameState.Active || State == GameState.Paused)
			{
				TextRenderer.RenderText($"Vidas: {Lives}", 5, 5, 1);
				TextRenderer.RenderText("Efeitos:", 15, Height - 75, .95f);
				for (int i = 0; i < PowerUps.Count; i++)
				{
					if (PowerUps[i].Active)
						Renderer.DrawTexture(PowerUps[i].Sprite, new Vector2(30 + (i * PowerUps[i].Size.X), Height - (1.1f * PowerUps[i].Size.Y)), PowerUps[i].Size / 1.25f, 0, Color.DeepPink.ToVector3());
				}
			}
			if (State == GameState.Menu)
			{
				TextRenderer.RenderText("Pressione ENTER para iniciar", (Width / 2f) - 200, (Height / 2f) + 20f, 1);
				TextRenderer.RenderText("Pressione W ou S para selecionar a fase", (Width / 2f) - 225f, (Height / 2f) + 50f, .8f);
				TextRenderer.RenderText("Pressione I para INFO", (Width / 2f) - 95f, (Height / 2f) + 80f, .6f);
				if (showInfo)
				{
					Renderer.DrawTexture(Overlay, Vector2.Zero, new Vector2(Width, Height), 0, Color.DarkSlateGray.ToVector4(.8f));
					TextRenderer.RenderText("INFO", Width / 2 - 75, 16, 2);
					TextRenderer.RenderText("Bem vindo ao TIJOLATOR!", 15, 16 * 5, .8f);
					TextRenderer.RenderText("Para mudar a dificuldade, pressione [1], [2] ou [3]", 15, 16 * 7.5f, .8f);
					TextRenderer.RenderText("O mouse movimenta a barra de acordo com sua posicao", 15, 16 * 9.5f, .8f);
					TextRenderer.RenderText("relativa ao centro da tela, sendo que quanto mais", 15, 16 * 11, .8f);
					TextRenderer.RenderText("perto da borda, mais velocidade naquela direcao a", 15, 16 * 12.5f, .8f);
					TextRenderer.RenderText("barra tem", 15, 16 * 14, .8f);
					TextRenderer.RenderText("O botao da esquerda pausa, e o da direita imprime ", 15, 16 * 17, .8f);
					TextRenderer.RenderText("informacoes tecnicas.", 15, 16 * 18.5f, .8f);
					TextRenderer.RenderText("Quando a bola grudar na barra,", 15, 16 * 20.5f, .8f);
					TextRenderer.RenderText("aperte espaco para solta-la", 15, 16 * 22, .8f);
					TextRenderer.RenderText("Por fim:", 15, 16 * 25, .8f);
					TextRenderer.RenderText("LET'S TIJOLATE!", Width / 2 - 170, 16 * 32, 1.5f);
				}
			}
			if (State == GameState.Win)
			{
				TextRenderer.RenderText("Venceste!!!", (Width / 2f) - 75f, (Height / 2f) + 280f, 1f, new Vector3(0f, 1f, 0f));
			}
			if (State == GameState.Active && Levels[Level].IsCompleted())
			{
				Levels[Level].Reload(Width, Height / 2);
				PostProcessor.Chaos = true;
				State = GameState.Win;
			}

			if (shouldPause)
			{
				State = GameState.Paused;
				shouldPause = false;
				LogInfo();
			}
			if (State == GameState.Paused)
			{
				Renderer.DrawTexture(Overlay, Vector2.Zero, new Vector2(Width, Height), 0, Color.DarkSlateGray.ToVector4(.4f));
				TextRenderer.RenderText("PAUSE", Width / 2f - 75, Height / 2f, 2);
				for (int line = 0; line < lines.Length; line++)
				{
					var height = line * TextRenderer.Characters['T'].Size.Y;
					bool secondColumn = height > Width - 15;
					TextRenderer.RenderText(lines[line], 5 + (secondColumn ? Width / 2 : 0), 5 + (secondColumn ? height - Height + 75 : height), .55f, Color.Yellow.ToVector3());
				}
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
			if (e.Key == Key.R)
			{
				Reset();
			}
			if (State == GameState.Active && e.Key == Key.Space)
			{
				Ball.Stuck = false;
			}
			if (State == GameState.Menu)
			{
				if (e.Key == Key.W)
				{
					if (showInfo)
						showInfo = false;
					else
						Level = (Level + 1) % 4;
				}
				if (e.Key == Key.S)
				{
					if (showInfo)
						showInfo = false;
					else
						if (Level > 0)
							Level--;
						else
							Level = 3;
				}
				if (e.Key == Key.Enter)
				{
					if (showInfo)
						showInfo = false;
					else
						State = GameState.Active;
				}
				if (e.Key == Key.I)
				{
					showInfo = !showInfo;
				}
			}
			if (e.Key == Key.L)
			{
				Levels[Level].Reload(Width, Height / 2);
				PostProcessor.Chaos = true;
				State = GameState.Win;
			}
			if (e.Key == Key.Number1)
			{
				Difficulty = Difficulties[0];
			}
			if (e.Key == Key.Number2)
			{
				Difficulty = Difficulties[1];
			}
			if (e.Key == Key.Number3)
			{
				Difficulty = Difficulties[2];
			}
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);
			PLAYER_VELOCITY = (e.X - (Width / 2)) * Difficulty.PlayerSpeedMod;
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.Button == MouseButton.Left)
			{
				if (State == GameState.Active)
					State = GameState.Paused;
				else if (State == GameState.Paused)
				{
					State = GameState.Active;
					lines = new string[] { };
				}
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
				float paddleCenter = Player.Position.X + (Player.Size.X / 2);
				float distance = Ball.Position.X + Ball.Radius - paddleCenter;
				float percentage = distance / (Player.Size.X / 2);

				float velocityMag = Ball.Velocity.Length;
				var relativeVelocity = new Vector2(BALL_X_SPEED * percentage * 2, -Math.Abs(Ball.Velocity.Y));
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
			var rand = Util.Random;
			string[] types =
			{
				"speed", "sticky", "comet", "padIncrease", "padDecrease", "confuse", "chaos", "multiply"
			};
			Dictionary<string, float> duration = new Dictionary<string, float>()
			{
				{"speed", 15},{"sticky", 10},{"comet", 7},{"padIncrease", 10},{"padDecrease", 5},{"confuse", 8},{"chaos", 3},{"multiply", 0.00001f},
			};
			if (rand.NextDouble() > Difficulty.PowerUpChance)
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
								Ball.Sticky = PowerUps.Any(p => p.Active && p.Type == "sticky");
								Player.Color = new Vector3(1);
								break;
							case "comet":
								Ball.Comet = PowerUps.Any(p => p.Active && p.Type == "comet");
								Ball.Color = new Vector3(1);
								break;
							case "padIncrease":
								Player.Size = new Vector2(Player.Size.X / 1.25f, Player.Size.Y);
								break;
							case "padDecrease":
								Player.Size = new Vector2(Player.Size.X / .75f, Player.Size.Y);
								break;
							case "confuse":
								PostProcessor.Confuse = PowerUps.Any(p => p.Active && p.Type == "confuse");
								break;
							case "chaos":
								PostProcessor.Chaos = PowerUps.Any(p => p.Active && p.Type == "chaos");
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
			PostProcessor.Chaos = false;
			PostProcessor.Confuse = false;
			Init();
		}

		private void ResetPlayer()
		{
			UpdatePowerUps(100000);
			PowerUps.Clear();
			Player.Position = new Vector2((Width / 2) - (PLAYER_SIZE.X / 2), Height - PLAYER_SIZE.Y - 84f);
			Player.Size = PLAYER_SIZE;
			Player.Color = new Vector3(1);
			BALL_VELOCITY = new Vector2(GetRandomBallSpeed(), Difficulty.BallUpSpeed);
			Ball.Reset(Player.Position + new Vector2((Player.Size.X / 2) - BALL_RADIUS, -BALL_RADIUS * 2f), BALL_VELOCITY);
			Ball.Size = new Vector2(2f * BALL_RADIUS);
		}

		private void LogInfo()
		{
			string text = ("======================================");
			text += '\n' + ("DEBUG INFORMATION");
			text += '\n' + ("Difficulty");
			text += '\n' + ("     Name: " + Difficulty.Name);
			text += '\n' + ("Paddle");
			text += '\n' + ("     Position: " + Player.Position);
			text += '\n' + ("     Velocity: " + Player.Velocity);
			text += '\n' + ("     Size: " + Player.Size);
			text += '\n' + ("Ball");
			text += '\n' + ("     Position: " + Ball.Position);
			text += '\n' + ("     Velocity: " + Ball.Velocity);
			text += '\n' + ("     Radius: " + Ball.Radius);
			text += '\n' + ("     Comet: " + Ball.Comet);
			text += '\n' + ("     Sticky: " + Ball.Sticky);
			text += '\n' + ("PostProcessing");
			text += '\n' + ("     Chaos: " + PostProcessor.Chaos);
			text += '\n' + ("     Confuse: " + PostProcessor.Confuse);
			text += '\n' + ("     Shake: " + PostProcessor.Shake);
			text += '\n' + ("PowerUps");
			foreach (var p in PowerUps)
			{
				text += '\n' + ("     Type: " + p.Type);
				text += '\n' + ("         Active: " + p.Active);
				text += '\n' + ("         Duration: " + p.Duration);
			}
			text += '\n' + ("Bricks");
			for (int i = 0; i < Levels[Level].Bricks.Count; i++)
			{
				var brick = Levels[Level].Bricks[i];
				if (!brick.Destroyed)
				{
					text += '\n' + ("     Brick" + i);
					text += '\n' + ("         Position" + brick.Position);
					text += '\n' + ("         IsSolid" + brick.IsSolid);
				}
			}
			text += '\n' + ("======================================");
			Console.WriteLine(text);

			lines = text.Split('\n');
		}

		private float GetRandomBallSpeed()
		{
			if (Util.Random.NextDouble() > .5)
			{
				return 1 * BALL_X_SPEED;
			}
			return -1 * BALL_X_SPEED;
		}
	}
}
