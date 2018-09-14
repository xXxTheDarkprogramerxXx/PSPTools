namespace pspsharp.util
{

	public class ByteUtil
	{
		public static ByteBuffer toByteBuffer(sbyte[] data)
		{
			return ByteBuffer.wrap(data).order(ByteOrder.LITTLE_ENDIAN);
		}

		public static sbyte[] toByteArray(ByteBuffer data)
		{
			sbyte[] @out = new sbyte[data.limit()];
			for (int n = 0; n < @out.Length; n++)
			{
				@out[n] = data.get(n);
			}
			return @out;
		}

		public static sbyte[] toByteArray(params int[] @in)
		{
			sbyte[] @out = new sbyte[@in.Length];
			for (int n = 0; n < @in.Length; n++)
			{
				@out[n] = (sbyte) @in[n];
			}
			return @out;
		}

		public static sbyte[] readBytes(ByteBuffer buffer, int offset, int len)
		{
			sbyte[] @out = new sbyte[len];
			int oldPos = buffer.position();
			try
			{
				buffer.position(offset);
				buffer.get(@out);
			}
			finally
			{
				buffer.position(oldPos);
			}
			return @out;
		}
	}

}