using NAudio.Wave;

namespace TP1
{
	public class Sound
	{
		public float[] AudioData { get; set; }
		public WaveFormat WaveFormat { get; set; }
		public bool Loop { get; set; }
	}
}
