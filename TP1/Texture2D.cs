using OpenTK.Graphics.OpenGL;
using System;

namespace TP1
{
	// classe para armazenar as informações de textura
	public class Texture2D
	{
		public int ID { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public PixelInternalFormat InternalFormat { get; set; }
		public PixelFormat ImageFormat { get; set; }
		public TextureWrapMode WrapS { get; set; }
		public TextureWrapMode WrapT { get; set; }
		public TextureMinFilter FilterMin { get; set; }
		public TextureMagFilter FilterMax { get; set; }

		public Texture2D()
		{
			Width = 0;
			Height = 0;
			InternalFormat = PixelInternalFormat.Rgb;
			ImageFormat = PixelFormat.Rgb;
			WrapS = TextureWrapMode.Repeat;
			WrapT = TextureWrapMode.Repeat;
			FilterMin = TextureMinFilter.Linear;
			FilterMax = TextureMagFilter.Linear;
			ID = GL.GenTexture();
		}

		// gerar e carregar dados da textura no OpenGL
		public void Generate(int width, int height, IntPtr data)
		{
			Width = width;
			Height = height;
			GL.BindTexture(TextureTarget.Texture2D, ID);
			GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Width, Height, 0, ImageFormat, PixelType.UnsignedByte, data);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapS);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapT);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)FilterMin);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)FilterMax);
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		// determinar essa textura como a textura ativa no openGl
		public void Bind()
		{
			GL.BindTexture(TextureTarget.Texture2D, ID);
		}
	}
}
