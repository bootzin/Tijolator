using System;

namespace TP1
{
	public static class Program
	{
		[STAThread]

		public static void Main()
		{
			using Game game = new Game(680, 720, "TIJOLATOR!");
			game.Run(30);
		}
	}
}
