using System;

namespace TP1
{
	public class Program
	{
		[STAThread]

		static void Main(string[] args)
		{
			using Game game = new Game(800, 600, "Jogo da bola e dos tijolo");
			game.Run(60);
		}
	}
}
