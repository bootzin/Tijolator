using OpenTK;

namespace TP1
{
	public class Difficulty
	{
		public string Name { get; set; }
		public double PowerUpChance { get; set; }
		public float BallUpSpeed { get; set; }
		public float PlayerSpeedMod { get; set; }

		public Difficulty(string name, float ballVelocity, double powerUpChance, float playerSpeedMod)
		{
			Name = name;
			BallUpSpeed = ballVelocity;
			PowerUpChance = powerUpChance;
			PlayerSpeedMod = playerSpeedMod;
		}
	}
}
