using System;

namespace TP1
{
	public static class Program
	{
		[STAThread]

		public static void Main()
		{
			using Game game = new Game(640, 480, "Jogo da bola e dos tijolo");
			game.Run(60);
		}
	}
}
