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

		public Renderer2D(Shader shader)
		{
			Shader = shader;
			InitRenderdata();
		}

		public void DrawTexture(
			Texture2D tex,
			System.Numerics.Vector2 position,
			System.Numerics.Vector2 size,
			float rotation,
			System.Numerics.Vector3 color)
		{
			DrawTexture(
				tex,
				new Vector2(position.X, position.Y),
				new Vector2(size.X, size.Y),
				rotation,
				new Vector3(color.X, color.Y, color.Z));
		}

		public void DrawTexture(Texture2D tex, Vector2 position, Vector2 size, float rotation, Vector3 color) => DrawTexture(tex, position, size, rotation, new Vector4(color, 1f));

		public void DrawTexture(Texture2D tex, Vector2 position, Vector2 size, float rotation, Color color) => DrawTexture(tex, position, size, rotation, color.ToVector3());

		// desenhar as texturas na posição, escala, rotação e cor desejados
		public void DrawTexture(Texture2D tex, Vector2 position, Vector2 size, float rotation, Vector4 color)
		{
			// aplicar a multiplicação das matrizes (em ordem reversa)
			// escala
			Matrix4 model = Matrix4.CreateScale(size.X, size.Y, 1f);
			// translação para que a rotação ocorra em torno do centro da textura
			// necessária pois a textura possui o ponto (0,0) no canto superior esquerdo
			model *= Matrix4.CreateTranslation(-.5f * size.X, -.5f * size.Y, 0);
			// rotação
			model *= Matrix4.CreateRotationZ((float)(Math.PI / 180) * rotation);
			// translação para retornar à posição original
			model *= Matrix4.CreateTranslation(.5f * size.X, .5f * size.Y, 0);
			// translação para se chegar na posição desejada
			model *= Matrix4.CreateTranslation(position.X, position.Y, 0f);

			// enviar dados ao shader
			Shader.Use();
			Shader.SetMatrix4("model", model, false);
			Shader.SetVector4f("spriteColor", color, false);

			// desenhar utilizando a textura desejada, bem como o array de vértices de quadrados
			GL.ActiveTexture(TextureUnit.Texture0);
			tex.Bind();
			GL.BindVertexArray(quadVAO);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			GL.BindVertexArray(0);
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		private void InitRenderdata()
		{
			// vértices dos triângulos que compõe o quadrado que representa a imagem a ser desenhada
			// os dois primeiros valores representam o X e o Y do objeto, enquanto os dois últimos
			// representam as coordenadas da textura do objeto
			float[] vertices = new float[] {
				// pos      // tex
				0.0f, 1.0f, 0.0f, 1.0f,
				1.0f, 0.0f, 1.0f, 0.0f,
				0.0f, 0.0f, 0.0f, 0.0f,

				0.0f, 1.0f, 0.0f, 1.0f,
				1.0f, 1.0f, 1.0f, 1.0f,
				1.0f, 0.0f, 1.0f, 0.0f
			};

			// gerar array e buffer
			GL.GenVertexArrays(1, out quadVAO);
			int VBO = GL.GenBuffer();

			// gravar dados dos vértices no buffer
			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

			// enviar dados aos shaders
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
