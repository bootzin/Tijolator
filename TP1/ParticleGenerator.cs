using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace TP1
{
	public class ParticleGenerator
	{
		public Shader Shader { get; set; }
		public Texture2D Texture { get; set; }
		public int Amount { get; set; }
		public List<Particle> Particles { get; set; } = new List<Particle>();

		private int VAO;
		private int lastUsedParticle;

		public ParticleGenerator(Shader shader, Texture2D texture, int amount)
		{
			Amount = amount;
			Texture = texture;
			Shader = shader;

			Init();
		}

		// atualizar e refrescar as partículas, removendo as mais antigas e gerando novas
		// em posições aleatórias próximas do objeto original
		public void Update(float dt, GameObject obj, int newParticles, Vector2 offset)
		{
			// substituir as partículas antigas
			for (int i = 0; i < newParticles; i++)
			{
				int unusedParticle = FirstUnusedParticle();
				Particles[unusedParticle] = RespawnParticle(Particles[unusedParticle], obj, offset);
			}

			// atualizar posição, tempo de vida e alpha das partículas
			for (int i = 0; i < Amount; i++)
			{
				Particle p = Particles[i];
				p.Life -= dt;
				if (p.Life > 0)
				{
					p.Position -= p.Velocity * dt;
					p.Color = new Vector4(p.Color) { W = p.Color.W - (dt * 2.5f) };
				}
				Particles[i] = p;
			}
		}

		// desenhar cada uma das partículas utilizando shaders apropriados
		public void Draw()
		{
			// alterando a função de blend para gerar um efeito de "brilho" quando
			// múltiplas partículas se acumulam no mesmo lugar
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
			Shader.Use();
			foreach (Particle particle in Particles)
			{
				if (particle.Life > 0)
				{
					Shader.SetVector2f("offset", particle.Position.X, particle.Position.Y);
					Shader.SetVector4f("color", particle.Color);
					Texture.Bind();
					GL.BindVertexArray(VAO);
					GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
					GL.BindVertexArray(0);
				}
			}
			// retornando para a função de blend original
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		}

		// inicializar os arrays e buffers das partículas
		private void Init()
		{
			float[] particleQuad = {
				0.0f, 1.0f, 0.0f, 1.0f,
				1.0f, 0.0f, 1.0f, 0.0f,
				0.0f, 0.0f, 0.0f, 0.0f,

				0.0f, 1.0f, 0.0f, 1.0f,
				1.0f, 1.0f, 1.0f, 1.0f,
				1.0f, 0.0f, 1.0f, 0.0f
			};

			GL.GenVertexArrays(1, out VAO);
			int VBO = GL.GenBuffer();

			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * particleQuad.Length, particleQuad, BufferUsageHint.StaticDraw);

			GL.BindVertexArray(VAO);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);

			for (int i = 0; i < Amount; i++)
			{
				Particles.Add(new Particle(Vector2.Zero));
			}
		}

		// otimizar a função que obtém as partículas que serão removidas,
		// uma vez que é provável que as partículas estejam ordenadas por
		// tempo restante de vida
		private int FirstUnusedParticle()
		{
			for (int i = lastUsedParticle; i < Amount; i++)
			{
				if (Particles[i].Life <= 0)
				{
					lastUsedParticle = i;
					return i;
				}
			}

			for (int i = 0; i < Amount; i++)
			{
				if (Particles[i].Life <= 0)
				{
					lastUsedParticle = i;
					return i;
				}
			}

			return lastUsedParticle = 0;
		}

		// instanciar novas partículas com posições levemente aleatorizadas com base
		// na posição do objeto original
		private Particle RespawnParticle(Particle particle, GameObject obj, Vector2 offset)
		{
			var rand = Util.Random;
			float random = (rand.Next(1, 101) - 50) / 10f;
			float rColor = .1f + (rand.Next(1, 101) / 100f);
			particle.Position = new Vector2(obj.Position.X + random, obj.Position.Y + random) + offset;
			particle.Color = new Vector4(rColor) { W = 1f };
			particle.Life = 1f;
			particle.Velocity = obj.Velocity * .1f;
			return particle;
		}
	}
}
