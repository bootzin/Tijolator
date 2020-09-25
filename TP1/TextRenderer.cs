using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpFont;
using System;
using System.Collections.Generic;
using System.IO;

namespace TP1
{
	public class TextRenderer
	{
		public Shader TextShader { get; set; }
		public Dictionary<char, Character> Characters { get; set; } = new Dictionary<char, Character>();

		private readonly int VAO;
		private readonly int VBO;

		public TextRenderer(int width, int height)
		{
			TextShader = ResourceManager.LoadShader("shaders/text.vert", "shaders/text.frag", "text");
			TextShader.Use();
			TextShader.SetMatrix4("projection", Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1));
			TextShader.SetInteger("text", 0);

			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();
			GL.BindVertexArray(VAO);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 24, IntPtr.Zero, BufferUsageHint.DynamicDraw);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		public void Load(string font, uint fontSize)
		{
			Characters.Clear();
			Library ft = new Library();
			Face face = ft.NewFace(Path.Combine(AppContext.BaseDirectory, font), 0);
			face.SetPixelSizes(0, fontSize);
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
			for (byte c = 0; c < 128; c++)
			{
				face.LoadChar(c, LoadFlags.Render, LoadTarget.Normal);
				int tex = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, tex);
				GL.TexImage2D(
					TextureTarget.Texture2D,
					0,
					PixelInternalFormat.R8,
					face.Glyph.Bitmap.Width,
					face.Glyph.Bitmap.Rows,
					0,
					PixelFormat.Red,
					PixelType.UnsignedByte,
					face.Glyph.Bitmap.Buffer);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);

				Character character = new Character()
				{
					TextureID = tex,
					Size = new Vector2(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
					Bearing = new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
					Advance = (int)face.Glyph.Advance.X,
				};
				Characters.Add((char)c, character);
			}
			GL.BindTexture(TextureTarget.Texture2D, 0);
			face.Dispose();
			ft.Dispose();
		}

		public void RenderText(string text, float x, float y, float scale) => RenderText(text, x, y, scale, Vector3.One);

		public void RenderText(string text, float x, float y, float scale, Vector3 color)
		{
			TextShader.Use();
			TextShader.SetVector3f("textColor", color);
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindVertexArray(VAO);

			foreach (char c in text)
			{
				Character ch = Characters[c];

				float xpos = x + (ch.Bearing.X * scale);
				float ypos = y + ((Characters['X'].Bearing.Y - ch.Bearing.Y) * scale);

				float w = ch.Size.X * scale;
				float h = ch.Size.Y * scale;

				float[,] vertices = {
					{ xpos,     ypos + h,   0.0f, 1.0f },
					{ xpos + w, ypos,       1.0f, 0.0f },
					{ xpos,     ypos,       0.0f, 0.0f },

					{ xpos,     ypos + h,   0.0f, 1.0f },
					{ xpos + w, ypos + h,   1.0f, 1.0f },
					{ xpos + w, ypos,       1.0f, 0.0f }
				};

				GL.BindTexture(TextureTarget.Texture2D, ch.TextureID);
				GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
				GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * vertices.Length, vertices);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
				x += ch.Advance * scale;
			}
			GL.BindVertexArray(0);
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		public class Character
		{
			public int TextureID { get; set; }
			public Vector2 Size { get; set; }
			public Vector2 Bearing { get; set; }
			public int Advance { get; set; }
		}
	}
}
