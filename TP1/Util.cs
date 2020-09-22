using OpenTK;
using System.Drawing;

namespace TP1
{
	public static class Util
	{
		public static Vector3 ToVector3(this Color color)
		{
			return new Vector3(color.R/255, color.G/255, color.B/255);
		}
	}
}
