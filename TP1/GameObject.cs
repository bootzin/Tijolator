using OpenTK;

namespace TP1
{
	public class GameObject
	{
		public Vector2 Position { get; set; }
		public Vector2 Size { get; set; }
		public Vector2 Velocity { get; set; }
		public Vector3 Color { get; set; }
		public float Rotation { get; set; }
		public bool IsSolid { get; set; }
		public bool Destroyed { get; set; }
		public Texture2D Sprite { get; set; }

		public GameObject(Vector2 pos, Vector2 size, Texture2D sprite, Vector3? color = null, Vector2? velocity = null)
		{
			Position = pos;
			Size = size;
			Sprite = sprite;
			Color = color ?? Vector3.One;
			Velocity = velocity ?? Vector2.Zero;
		}

		public void Draw(Renderer2D renderer)
		{
			renderer.DrawTexture(Sprite, Position, Size, Rotation, Color);
		}

		public virtual Collision CheckCollision(GameObject other)
		{
			Collision col = new Collision();
			bool collisionX = Position.X + Size.X >= other.Position.X && other.Position.X + other.Size.X >= Position.X;
			bool collisionY = Position.Y + Size.Y >= other.Position.Y && other.Position.Y + other.Size.Y >= Position.Y;
			col.Colliding = collisionX && collisionY;
			return col;
		}
	}
}
