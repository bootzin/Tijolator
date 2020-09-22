using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace TP1
{
	public class Shader : IDisposable
	{
		public int ID { get; set; }

		private bool disposed;

		public Shader(string vShader, string fShader)
		{
			Compile(vShader, fShader);
		}

		public Shader Use()
		{
			GL.UseProgram(ID);
			return this;
		}

		public void Compile(string vShader, string fShader)
		{
			int sVertex, sFragment;

			//Vertex
			sVertex = GL.CreateShader(ShaderType.VertexShader);
			GL.ShaderSource(sVertex, vShader);
			GL.CompileShader(sVertex);
			CheckCompileErrors(sVertex, "VERTEX");

			//Fragment
			sFragment = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(sFragment, fShader);
			GL.CompileShader(sFragment);
			CheckCompileErrors(sFragment, "FRAGMENT");

			this.ID = GL.CreateProgram();
			GL.AttachShader(ID, sVertex);
			GL.AttachShader(ID, sFragment);
			GL.LinkProgram(ID);
			CheckCompileErrors(ID, "PROGRAM");
			GL.DetachShader(ID, sVertex);
			GL.DetachShader(ID, sFragment);
			GL.DeleteShader(sVertex);
			GL.DeleteShader(sFragment);
		}

		public void SetFloat(string name, float value, bool useShader)
		{
			if (useShader)
				this.Use();
			GL.Uniform1(GL.GetUniformLocation(ID, name), value);
		}

		public void SetInteger(string name, int value, bool useShader)
		{
			if (useShader)
				this.Use();
			GL.Uniform1(GL.GetUniformLocation(ID, name), value);
		}

		public void SetVector2f(string name, float x, float y, bool useShader)
		{
			if (useShader)
				this.Use();
			GL.Uniform2(GL.GetUniformLocation(ID, name), x, y);
		}

		public void SetVector3f(string name, Vector3 vector, bool useShader)
		{
			if (useShader)
				Use();
			GL.Uniform3(GL.GetUniformLocation(ID, name), vector);
		}

		public void SetMatrix4(string name, Matrix4 matrix, bool useShader)
		{
			if (useShader)
				Use();
			GL.UniformMatrix4(GL.GetUniformLocation(ID, name), false, ref matrix);
		}

		private void CheckCompileErrors(int obj, string type)
		{
			int success;
			if (type !="PROGRAM")
			{
				GL.GetShader(obj, ShaderParameter.CompileStatus, out success);
				if (success == 0)
				{
					string error = GL.GetShaderInfoLog(obj);
					Console.WriteLine($"ERROR::SHADER: Compile time error! Type: {type}");
					Console.WriteLine(error);
					Console.WriteLine("-- ---------------------------------------------- --\n");
				}
			}
			else
			{
				GL.GetProgram(obj, GetProgramParameterName.LinkStatus, out success);
				if (success == 0)
				{
					string error = GL.GetProgramInfoLog(obj);
					Console.WriteLine($"ERROR::SHADER: Link time error! Type: {type}");
					Console.WriteLine(error);
					Console.WriteLine("-- ---------------------------------------------- --\n");
				}
			}
		}

		~Shader() => Dispose(false);

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					GL.DeleteProgram(ID);
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
