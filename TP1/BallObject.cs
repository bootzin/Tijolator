using OpenTK;

namespace TP1
{
	public class BallObject : GameObject
	{
		public float Radius { get; set; }
		public bool Stuck { get; set; } = true;
		public bool Sticky { get; set; }
		public bool Comet { get; set; }

		public BallObject(Vector2 pos, float radius, Texture2D sprite, Vector3? color = null, Vector2? velocity = null) : base(pos, new Vector2(radius * 2), sprite, color, velocity)
		{
			Radius = radius;
		}

		// redefinir os atributos da bola
		public void Reset(Vector2 position, Vector2 velocity)
		{
			Position = position;
			Velocity = velocity;
			Color = new Vector3(1);
			Comet = false;
			Sticky = false;
			Stuck = true;
		}

		// movimentar a bola, refletindo em caso de colisão
		// com as bordas laterais ou superior
		public Vector2 Move(float dt, int windowWidth)
		{
			if (!Stuck)
			{
				Position += Velocity * dt * 20;

				if (Position.X <= 0)
				{
					Velocity = new Vector2(-Velocity.X, Velocity.Y);
					Position = new Vector2(0, Position.Y);
				}
				else if (Position.X + Size.X >= windowWidth)
				{
					Velocity = new Vector2(-Velocity.X, Velocity.Y);
					Position = new Vector2(windowWidth - Size.X, Position.Y);
				}

				if (Position.Y <= 0)
				{
					Velocity = new Vector2(Velocity.X, -Velocity.Y);
					Position = new Vector2(Position.X, 0);
				}
			}

			return Position;
		}

		// método para checar a colisão, sobrescrevendo o
		// método padrão da classe de objeto.
		// esse método pega o ponto de colisão mais próximo de um retângulo com a bola
		// e utiliza ele para determinar se a colisão ocorre. Adicionalmente,
		// esse ponto é utilizado para determinar a direção de reflexão da bola
		public override Collision CheckCollision(GameObject other)
		{
			Collision col = new Collision();
			Vector2 ballCenter = Position + new Vector2(Radius);
			Vector2 aabbHalf = other.Size / 2;
			Vector2 aabbCenter = other.Position + aabbHalf;

			Vector2 diff = ballCenter - aabbCenter;
			Vector2 clamped = Vector2.Clamp(diff, -aabbHalf, aabbHalf);
			Vector2 closestPoint = aabbCenter + clamped;

			diff = closestPoint - ballCenter;
			col.Colliding = diff.Length < Radius;
			col.Vector = diff;
			if (col.Colliding)
			{
				col.CollisionPoint = closestPoint;
				col.Direction = Util.GetVectorDirection(diff.Normalized());
			}
			return col;
		}
	}
}
