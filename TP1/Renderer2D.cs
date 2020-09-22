using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;

namespace TP1
{
	public sealed class Renderer2D : IDisposable
	{
		private readonly Shader Shader;
		private int quadVAO;
		private uint[] indices;

		public Renderer2D(Shader shader)
		{
			Shader = shader;
			InitRenderdata();
		}

		public void DrawTexture(Texture2D tex, Vector2 position, Vector2 size, float rotation, Color color)
		{
			Matrix4 model;
			model = Matrix4.CreateScale(size.X, size.Y, 1f) * Matrix4.CreateRotationZ((float)(Math.PI / 180) * rotation) * Matrix4.CreateTranslation(position.X, position.Y, 0f);
			//model *= Matrix4.CreateTranslation(-.5f * size.X, -.5f * size.Y, 0);
			//model *= ;
			//model *= Matrix4.CreateTranslation(.5f * size.X, .5f * size.Y, 0);
			//model *= ;

			Shader.Use();
			Shader.SetMatrix4("model", model, false);
			Shader.SetVector3f("spriteColor", color.ToVector3(), false);

			GL.ActiveTexture(TextureUnit.Texture0);
			tex.Bind();
			GL.BindVertexArray(quadVAO);
			//GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, indices);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			GL.BindVertexArray(0);
			//GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		private void InitRenderdata()
		{
			int VBO, EBO;
			float[] vertices = new float[] {
				// pos      // tex
				0.0f, 1.0f, 0.0f, 1.0f,
				1.0f, 0.0f, 1.0f, 0.0f,
				0.0f, 0.0f, 0.0f, 0.0f,

				0.0f, 1.0f, 0.0f, 1.0f,
				1.0f, 1.0f, 1.0f, 1.0f,
				1.0f, 0.0f, 1.0f, 0.0f
			};

			//float[] vertices = {
			//	0.0f, 1.0f, 0.0f, 1.0f, // top left
			//	1.0f, 0.0f, 1.0f, 0.0f, // bottom right
			//	0.0f, 0.0f, 0.0f, 0.0f, // bottom left
			//	1.0f, 1.0f, 1.0f, 1.0f	// top right
			//};

			//indices = new uint[]
			//{
			//	0, 1, 2,
			//	0, 1, 3
			//};

			GL.GenVertexArrays(1, out quadVAO);
			GL.GenBuffers(1, out VBO);
			GL.GenBuffers(1, out EBO);

			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

			//GL.BindBuffer(BufferTarget.ArrayBuffer, EBO);
			//GL.BufferData(BufferTarget.ArrayBuffer, sizeof(uint) * indices.Length, indices, BufferUsageHint.StaticDraw);

			GL.BindVertexArray(quadVAO);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
		}

		public void Dispose()
		{
			GL.DeleteVertexArray(quadVAO);
		}
	}
}
