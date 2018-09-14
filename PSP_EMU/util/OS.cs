namespace pspsharp.util
{
	public class OS
	{
		public static readonly bool isWindows = System.getProperty("os.name").contains("Windows");
		public static readonly bool isLinux = System.getProperty("os.name").contains("Linux");
		public static readonly bool isMac = System.getProperty("os.name").contains("Mac");
		public static readonly bool is64Bit = System.getProperty("os.arch").Equals("amd64") || System.getProperty("os.arch").Equals("x86_64");
	}

}