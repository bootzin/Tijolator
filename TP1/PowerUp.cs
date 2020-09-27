using OpenTK;

namespace TP1
{
	// classe para armazenar os dados de powerups
	public class PowerUp : GameObject
	{
		public string Type { get; set; }
		public float Duration { get; set; }
		public bool Active { get; set; }

		public PowerUp(string type, Vector3? color, float duration, Vector2 pos, Texture2D sprite) : base(pos, new Vector2(45, 45), sprite, color, new Vector2(0, 8))
		{
			Type = type;
			Duration = duration;
			Active = false;
		}
	}
}
