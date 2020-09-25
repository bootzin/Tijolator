using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace TP1
{
	public class PostProcessor
	{
		public Shader PostProcessingShader { get; set; }
		public Texture2D Texture { get; set; } = new Texture2D();
		public int Width { get; set; }
		public int Height { get; set; }
		public bool Confuse { get; set; } = false;
		public bool Chaos { get; set; } = false;
		public bool Shake { get; set; } = false;

		private int VAO;
		private readonly int MSFBO;
		private readonly int FBO;

		public PostProcessor(Shader postProcessingShader, int width, int height)
		{
			PostProcessingShader = postProcessingShader;
			Width = width;
			Height = height;

			MSFBO = GL.GenFramebuffer();
			FBO = GL.GenFramebuffer();
			int RBO = GL.GenRenderbuffer();

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, MSFBO);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
			GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, GL.GetInteger(GetPName.MaxSamples), RenderbufferStorage.Rgb8, width, height);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, RBO);
			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
				Console.WriteLine("ERROR::POSTPROCESSOR: Failed to initialize MSFBO!");

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
			Texture.Generate(Width, Height, IntPtr.Zero);
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture.ID, 0);
			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
				Console.WriteLine("ERROR::POSTPROCESSOR: Failed to initialize FBO!");
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			InitRenderData();

			PostProcessingShader.SetInteger("scene", 0, true);
			const float offset = 1f / 300f;
			float[,] offsets = {
				{ -offset,  offset  },  // top-left
				{ 0f,    offset  },  // top-center
				{ offset,  offset  },  // top-right
				{ -offset,  0f    },  // center-left
				{ 0f,    0f    },  // center-center
				{ offset,  0f    },  // center - right
				{ -offset, -offset  },  // bottom-left
				{ 0f,   -offset  },  // bottom-center
				{ offset, -offset  }   // bottom-right    
			};
			PostProcessingShader.SetVector2fv("offsets", 9, ref offsets[0, 0], true);

			int[] edgeKernel = {
				-1, -1, -1,
				-1,  8, -1,
				-1, -1, -1
			};
			PostProcessingShader.SetVector1iv("edge_kernel", 9, edgeKernel, true);

			float[] blurKernel = {
				1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f,
				2.0f / 16.0f, 4.0f / 16.0f, 2.0f / 16.0f,
				1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f
			};
			PostProcessingShader.SetVector1fv("blur_kernel", 9, blurKernel, true);
		}

		public void BeginRender()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, MSFBO);
			GL.ClearColor(Color4.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit);
		}

		public void EndRender()
		{
			GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, MSFBO);
			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);
			GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		}

		public void Render(float time)
		{
			PostProcessingShader.Use();
			PostProcessingShader.SetFloat("time", time);
			PostProcessingShader.SetInteger("chaos", Chaos ? 1 : 0);
			PostProcessingShader.SetInteger("confuse", Confuse ? 1 : 0);
			PostProcessingShader.SetInteger("shake", Shake ? 1 : 0);

			GL.ActiveTexture(TextureUnit.Texture0);
			Texture.Bind();
			GL.BindVertexArray(VAO);
			GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
			GL.BindVertexArray(0);
		}

		private void InitRenderData()
		{
			int VBO;
			float[] vertices = new float[] {
				// pos        // tex
				-1.0f, -1.0f, 0.0f, 0.0f,
				 1.0f,  1.0f, 1.0f, 1.0f,
				-1.0f,  1.0f, 0.0f, 1.0f,

				-1.0f, -1.0f, 0.0f, 0.0f,
				 1.0f, -1.0f, 1.0f, 0.0f,
				 1.0f,  1.0f, 1.0f, 1.0f
			};

			VAO = GL.GenVertexArray();
			VBO = GL.GenBuffer();

			GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
			GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);

			GL.BindVertexArray(VAO);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindVertexArray(0);
		}
	}
}
