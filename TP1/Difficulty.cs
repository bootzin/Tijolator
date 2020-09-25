using System.Collections.Generic;

namespace TP1
{
	public class Difficulty
	{
		public string Name { get; set; }
		public double PowerUpChance { get; set; }
		public float BallUpSpeed { get; set; }
		public float PlayerSpeedMod { get; set; }
		public int AmountOfBalls { get; set; }
		public int LifeAmount { get; set; }
		public Dictionary<string, float> PowerUpDuration { get; set; }
		public List<string> PowerUpTypes { get; set; }

		public Difficulty(string name, float ballVelocity, double powerUpChance, float playerSpeedMod, int ballAmnt, int lifeAmount)
		{
			Name = name;
			BallUpSpeed = ballVelocity;
			PowerUpChance = powerUpChance;
			PlayerSpeedMod = playerSpeedMod;
			AmountOfBalls = ballAmnt;
			LifeAmount = lifeAmount;
		}
	}
}
