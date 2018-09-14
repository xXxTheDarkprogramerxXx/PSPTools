namespace pspsharp.util
{

	public class HashUtil
	{
		public static int crc32(sbyte[] data)
		{
			CRC32 crc32 = new CRC32();
			crc32.update(data);
			return (int) crc32.Value;
		}
	}

}