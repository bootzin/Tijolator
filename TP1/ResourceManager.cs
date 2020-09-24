using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace TP1
{
	public static class ResourceManager
	{
		public static Dictionary<string, Shader> Shaders { get; set; } = new Dictionary<string, Shader>();
		public static Dictionary<string, Texture2D> Textures { get; set; } = new Dictionary<string, Texture2D>();
		public static Dictionary<string, Sound> Sounds { get; set; } = new Dictionary<string, Sound>();

		public static Shader GetShader(string name) => Shaders[name];
		public static Sound GetSound(string name) => Sounds[name];
		public static Texture2D GetTex(string name) => Textures[name];

		public static Shader LoadShader(string vShaderPath, string fShaderPath, string name)
		{
			if (!Shaders.ContainsKey(name))
				Shaders[name] = LoadShaderFromFile(vShaderPath, fShaderPath);
			return Shaders[name];
		}

		public static Texture2D LoadTexture(string texPath, string name, bool alpha = true)
		{
			if (!Textures.ContainsKey(name))
				Textures[name] = LoadTextureFromFile(texPath, alpha);
			return Textures[name];
		}

		public static Sound LoadSound(string filePath, string name, bool loop = false)
		{
			if (!Sounds.ContainsKey(name))
				Sounds[name] = LoadSoundFromFile(filePath, loop);
			return Sounds[name];
		}

		private static Sound LoadSoundFromFile(string filePath, bool loop)
		{
			using var audioFileReader = new AudioFileReader(filePath);
			var resampler = new WdlResamplingSampleProvider(audioFileReader, 44100);
			Sound snd = new Sound
			{
				WaveFormat = resampler.WaveFormat
			};
			List<float> wholeFile = new List<float>((int)(audioFileReader.Length / 4));
			float[] readBuffer = new float[resampler.WaveFormat.SampleRate * resampler.WaveFormat.Channels];
			int samplesRead;
			while ((samplesRead = resampler.Read(readBuffer, 0, readBuffer.Length)) > 0)
			{
				wholeFile.AddRange(readBuffer.Take(samplesRead));
			}
			snd.AudioData = wholeFile.ToArray();
			snd.Loop = loop;
			return snd;
		}

		private static Shader LoadShaderFromFile(string vertexShaderPath, string fragmentShaderPath)
		{
			using StreamReader vsr = new StreamReader(Path.Combine(AppContext.BaseDirectory, vertexShaderPath));
			using StreamReader fsr = new StreamReader(Path.Combine(AppContext.BaseDirectory, fragmentShaderPath));
			string vertexCode = vsr.ReadToEnd();
			string fragCode = fsr.ReadToEnd();
			return new Shader(vertexCode, fragCode);
		}

		private static Texture2D LoadTextureFromFile(string texPath, bool alpha)
		{
			Texture2D tex = new Texture2D();
			if (alpha)
			{
				tex.InternalFormat = PixelInternalFormat.Rgba;
				tex.ImageFormat = PixelFormat.Rgba;
			}

			StbImage.stbi_set_flip_vertically_on_load(0);
			var img = ImageResult.FromMemory(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, texPath)), alpha ? ColorComponents.RedGreenBlueAlpha : ColorComponents.RedGreenBlue);
			GCHandle pinnedArray = GCHandle.Alloc(img.Data, GCHandleType.Pinned);
			IntPtr pointer = pinnedArray.AddrOfPinnedObject();
			tex.Generate(img.Width, img.Height, pointer);
			pinnedArray.Free();
			return tex;
		}
	}
}
