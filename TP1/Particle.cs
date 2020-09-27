using OpenTK;

namespace TP1
{
	// struct para armazenar dados de partículas
	public struct Particle
	{
		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; set; }
		public Vector4 Color { get; set; }
		public float Life { get; set; }

		public Particle(Vector2? position = null, Vector2? velocity = null, Vector4? color = null, float life = 0)
		{
			Position = position ?? Vector2.Zero;
			Velocity = velocity ?? Vector2.Zero;
			Color = color ?? Vector4.One;
			Life = life;
		}
	}
}
