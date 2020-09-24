using OpenTK;
using System;
using System.Drawing;

namespace TP1
{
	public static class Util
	{
		public static Random Random { get; } = new Random();
		public static Vector3 ToVector3(this Color color)
		{
			return new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
		}

		public static Vector4 ToVector4(this Color color, float w)
		{
			return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, w);
		}

		internal static Direction GetVectorDirection(Vector2 diff)
		{
			Vector2[] compass =
			{
				new Vector2(0, 1),
				new Vector2(1, 0),
				new Vector2(0, -1),
				new Vector2(-1, 0)
			};

			float max = 0;
			uint best = 5;

			for (uint i = 0; i < compass.Length; i++)
			{
				float dot = Vector2.Dot(compass[i], diff);
				if (dot > max)
				{
					max = dot;
					best = i;
				}
			}

			return (Direction)best;
		}
	}
}
