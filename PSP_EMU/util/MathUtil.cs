namespace pspsharp.util
{
	public class MathUtil
	{
		public static int clamp(int v, int min, int max)
		{
			if (v < min)
			{
				return min;
			}
			if (v > max)
			{
				return max;
			}
			return v;
		}
	}

}