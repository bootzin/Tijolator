using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace TP1
{
	public static class ResourceManager
	{
		public static Dictionary<string, Shader> Shaders { get; set; } = new Dictionary<string, Shader>();
		public static Dictionary<string, Texture2D> Textures { get; set; } = new Dictionary<string, Texture2D>();

		public static Shader LoadShader(string vShaderPath, string fShaderPath, string name)
		{
			Shaders[name] = LoadShaderFromFile(vShaderPath, fShaderPath);
			return Shaders[name];
		}

		public static Shader GetShader(string name)
		{
			return Shaders[name];
		}

		public static Texture2D GetTex(string name)
		{
			return Textures[name];
		}

		public static Texture2D LoadTexture(string texPath, string name, bool alpha = true)
		{
			Textures[name] = LoadTextureFromFile(texPath, alpha);
			return Textures[name];
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
			var img = ImageResult.FromMemory(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, texPath)), ColorComponents.RedGreenBlueAlpha);
			GCHandle pinnedArray = GCHandle.Alloc(img.Data, GCHandleType.Pinned);
			IntPtr pointer = pinnedArray.AddrOfPinnedObject();
			tex.Generate(img.Width, img.Height, pointer);
			pinnedArray.Free();
			return tex;
		}
	}
}
