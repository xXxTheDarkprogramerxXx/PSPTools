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
namespace pspsharp.HLE
{

	/// <summary>
	/// Annotation for the TPointer type, giving indications on the length
	/// of the buffer and if the buffer is used as input and/or input.
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false]
	public class BufferInfo : System.Attribute
	{
		public const Usage defaultUsage = Usage.unknown;
		public const LengthInfo defaultLengthInfo = LengthInfo.unknown;
		public const int defaultLength = -1;
		public const int defaultMaxDumpLength = -1;

		public enum LengthInfo
		{
			unknown,
			nextParameter,
			nextNextParameter,
			previousParameter,
			variableLength,
			fixedLength,
			returnValue
		}
		public enum Usage
		{
			unknown,
			@in,
			@out,
			inout
		}

		public LengthInfo lengthInfo;

		public int length;

		public Usage usage;

		public int maxDumpLength;

		public BufferInfo(,  ,  ,  ,  ,  ,  ,  ,  ,  ,  ,  ,  ,  ,  ,  ,  , public LengthInfo lengthInfo = LengthInfo.unknown, public int length = defaultLength, public Usage usage = Usage.unknown, public int maxDumpLength = defaultMaxDumpLength)
		{
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this. = ;
			this.lengthInfo = lengthInfo;
			this.length = length;
			this.usage = usage;
			this.maxDumpLength = maxDumpLength;
		}
	}

}