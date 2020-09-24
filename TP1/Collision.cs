using OpenTK;

namespace TP1
{
	public class Collision
	{
		public bool Colliding { get; set; }
		public Direction Direction { get; set; }
		public Vector2 Vector { get; set; }
		public Vector2 CollisionPoint { get; internal set; }
	}
}
