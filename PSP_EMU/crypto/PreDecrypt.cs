using System;
using System.Text;

/*
This file is part of pspsharp.

pspsharp is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

pspsharp is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with pspsharp.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace pspsharp.crypto
{


	using Logger = org.apache.log4j.Logger;
	using Document = org.w3c.dom.Document;
	using Element = org.w3c.dom.Element;
	using Node = org.w3c.dom.Node;
	using NodeList = org.w3c.dom.NodeList;
	using SAXException = org.xml.sax.SAXException;

	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// List of values that can't be decrypted on pspsharp due to missing keys.
	/// They have been decrypted on a real PSP and we just reuse the result from the PSP here.
	/// </summary>
	public class PreDecrypt
	{
		public static Logger log = CryptoEngine.log;
		private static PreDecryptInfo[] preDecrypts;

		private class PreDecryptInfo
		{
			internal sbyte[] input;
			internal sbyte[] output;
			internal int cmd;

			public PreDecryptInfo(sbyte[] input, sbyte[] output, int cmd)
			{
				this.input = input;
				this.output = output;
				this.cmd = cmd;
			}

			public virtual bool decrypt(sbyte[] @out, int outOffset, int outSize, sbyte[] @in, int inOffset, int inSize, int cmd)
			{
				if (this.cmd != cmd)
				{
					return false;
				}
				if (input.Length != inSize || output.Length != outSize)
				{
					return false;
				}

				for (int i = 0; i < inSize; i++)
				{
					if (input[i] != @in[inOffset + i])
					{
						return false;
					}
				}

				if (@out != null && outSize > 0)
				{
					Array.Copy(output, 0, @out, outOffset, outSize);
				}

				return true;
			}

			internal static bool Equals(sbyte[] a, sbyte[] b)
			{
				if (a == null)
				{
					return b == null;
				}
				if (b == null)
				{
					return false;
				}
				if (a.Length != b.Length)
				{
					return false;
				}

				for (int i = 0; i < a.Length; i++)
				{
					if (a[i] != b[i])
					{
						return false;
					}
				}

				return true;
			}

			public virtual bool Equals(PreDecryptInfo info)
			{
				if (cmd != info.cmd)
				{
					return false;
				}
				if (!Equals(input, info.input))
				{
					return false;
				}
				if (!Equals(output, info.output))
				{
					return false;
				}

				return true;
			}

			internal static void ToString(StringBuilder s, sbyte[] bytes, string name)
			{
				s.Append(string.Format(", {0}=[", name));
				if (bytes != null)
				{
					for (int i = 0; i < bytes.Length; i++)
					{
						if (i > 0)
						{
							s.Append(", ");
						}
						s.Append(string.Format("0x{0:X2}", bytes[i]));
					}
				}
				s.Append("]");
			}

			public override string ToString()
			{
				StringBuilder s = new StringBuilder();
				s.Append(string.Format("cmd=0x{0:X}", cmd));
				ToString(s, input, "Input");
				ToString(s, output, "Output");

				return s.ToString();
			}
		}

		public static bool preDecrypt(sbyte[] @out, int outOffset, int outSize, sbyte[] @in, int inOffset, int inSize, int cmd)
		{
			foreach (PreDecryptInfo preDecrypt in preDecrypts)
			{
				if (preDecrypt.decrypt(@out, outOffset, outSize, @in, inOffset, inSize, cmd))
				{
					return true;
				}
			}

			return false;
		}

		public static void init()
		{
			DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
			documentBuilderFactory.IgnoringElementContentWhitespace = true;
			documentBuilderFactory.IgnoringComments = true;
			documentBuilderFactory.Coalescing = true;
			try
			{
				DocumentBuilder documentBuilder = documentBuilderFactory.newDocumentBuilder();
				Document document = documentBuilder.parse(typeof(PreDecrypt).getResourceAsStream("PreDecrypt.xml"));
				Element configuration = document.DocumentElement;
				load(configuration);
			}
			catch (ParserConfigurationException e)
			{
				log.error(e);
			}
			catch (SAXException e)
			{
				log.error(e);
			}
			catch (IOException e)
			{
				log.error(e);
			}
		}

		private static string getContent(Node node)
		{
			if (node.hasChildNodes())
			{
				return getContent(node.ChildNodes);
			}

			return node.NodeValue;
		}

		private static string getContent(NodeList nodeList)
		{
			if (nodeList == null || nodeList.Length <= 0)
			{
				return null;
			}

			StringBuilder content = new StringBuilder();
			int n = nodeList.Length;
			for (int i = 0; i < n; i++)
			{
				Node node = nodeList.item(i);
				content.Append(getContent(node));
			}

			return content.ToString();
		}

		private static void load(Element configuration)
		{
			preDecrypts = new PreDecryptInfo[0];
			NodeList infos = configuration.getElementsByTagName("PreDecryptInfo");
			int n = infos.Length;
			for (int i = 0; i < n; i++)
			{
				Element info = (Element) infos.item(i);
				loadInfo(info);
			}
		}

		private static sbyte[] parseBytes(string s)
		{
			sbyte[] bytes = null;

			for (int i = 0; i < s.Length;)
			{
				i = s.IndexOf("0x", i, StringComparison.Ordinal);
				if (i < 0)
				{
					break;
				}
				i += 2;

				int value = Convert.ToInt32(s.Substring(i, 2), 16);
				bytes = Utilities.add(bytes, (sbyte) value);
			}

			return bytes;
		}

		private static void loadInfo(Element info)
		{
			int cmd = int.Parse(info.getAttribute("cmd"));
			string input = getContent(info.getElementsByTagName("Input"));
			string output = getContent(info.getElementsByTagName("Output"));

			addInfo(parseBytes(input), parseBytes(output), cmd);
		}

		private static void addInfo(sbyte[] input, sbyte[] output, int cmd)
		{
			PreDecryptInfo info = new PreDecryptInfo(input, output, cmd);
			for (int i = 0; i < preDecrypts.Length; i++)
			{
				if (info.Equals(preDecrypts[i]))
				{
					log.warn(string.Format("PreDecrypt.xml: duplicate entry {0}", info));
					return;
				}
			}

			PreDecryptInfo[] newPreDecrypts = new PreDecryptInfo[preDecrypts.Length + 1];
			Array.Copy(preDecrypts, 0, newPreDecrypts, 0, preDecrypts.Length);
			newPreDecrypts[preDecrypts.Length] = info;

			preDecrypts = newPreDecrypts;
		}
	}

}