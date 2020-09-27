using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TP1
{
	public class GameLevel
	{
		private string FilePath;
		public List<GameObject> Bricks { get; set; } = new List<GameObject>();

		// ler os dados do arquivo de nível e transformá-lo em objetos do jogo
		public void Load(string filePath, int levelWidth, int levelHeight)
		{
			Bricks.Clear();
			List<uint> tileData = new List<uint>();
			FilePath = Path.Combine(AppContext.BaseDirectory, filePath);
			using StreamReader sr = new StreamReader(FilePath);
			string firstLine = sr.ReadLine();
			int maxBricksPerRow = firstLine.Replace('\r', ' ').Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
			string lvl = firstLine + '\n' + sr.ReadToEnd();
			foreach (var tile in lvl.Replace('\r', ' ').Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries))
			{
				tileData.Add(uint.Parse(tile));
			}

			if (tileData.Count > 0)
				Init(tileData, levelWidth, levelHeight, maxBricksPerRow);
		}

		// desenhar os blocos do jogo
		public void Draw(Renderer2D renderer)
		{
			foreach (var tile in Bricks)
			{
				if (!tile.Destroyed)
					tile.Draw(renderer);
			}
		}

		// retornar os tijolos à posição original
		public void Reload(int width, int height)
		{
			Load(FilePath, width, height);
		}

		// obter score atual
		public int GetScore()
		{
			return Bricks.FindAll(b => b.Destroyed).Count;
		}

		public bool IsCompleted() => Bricks.TrueForAll(b => (!b.IsSolid && b.Destroyed) || b.IsSolid);

		// transformar os dados lidos do arquivo em objetos do jogo
		// a altura e largura dos blocos é ajustada automaticamente 
		// de acordo com a quantidade de blocos na fase
		private void Init(List<uint> tileData, int levelWidth, int levelHeight, int maxBricksPerRow)
		{
			int width = maxBricksPerRow;
			int height = tileData.Count / maxBricksPerRow;

			float unitWidth = (float)levelWidth / width;
			float unitHeight = (float)levelHeight / height;

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (tileData[(y * width) + x] == 1)
					{
						GameObject obj = new GameObject(
							new Vector2(x * unitWidth, y * unitHeight),
							new Vector2(unitWidth, unitHeight),
							ResourceManager.GetTex("block_solid"))
						{
							Color = Color.LightSalmon.ToVector3(),
							IsSolid = true
						};
						Bricks.Add(obj);
					}
					else if (tileData[(y * width) + x] > 1)
					{
						var color = (tileData[(y * width) + x]) switch
						{
							2 => Color.BlanchedAlmond.ToVector3(),
							3 => Color.Salmon.ToVector3(),
							4 => Color.Pink.ToVector3(),
							5 => Color.HotPink.ToVector3(),
							_ => Vector3.One,
						};

						GameObject obj = new GameObject(
							new Vector2(x * unitWidth, y * unitHeight),
							new Vector2(unitWidth, unitHeight),
							ResourceManager.GetTex("block"))
						{
							Color = color,
						};
						Bricks.Add(obj);
					}
				}
			}
		}
	}
}
