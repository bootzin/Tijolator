using System;

namespace TP1
{
	public static class Program
	{
		[STAThread]

		public static void Main()
		{
			// Inicializar uma janela do jogo com dimensões 612x648 e rodar o loop principal a 30 fps
			using Game game = new Game(612, 648, "TIJOLATOR!");
			game.Run(30);
		}
	}
}
