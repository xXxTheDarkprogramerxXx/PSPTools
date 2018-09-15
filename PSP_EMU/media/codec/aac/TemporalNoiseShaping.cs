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
namespace pspsharp.media.codec.aac
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.media.codec.aac.AacDecoder.TNS_MAX_ORDER;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// Temporal Noise Shaping
	/// </summary>
	public class TemporalNoiseShaping
	{
		public bool present;
		public int[] nFilt = new int[8];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public int[][] Length = new int[8][4];
		public int[][] Length = RectangularArrays.ReturnRectangularIntArray(8, 4);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public bool[][] direction = new bool[8][4];
		public bool[][] direction = RectangularArrays.ReturnRectangularBoolArray(8, 4);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public int[][] order = new int[8][4];
		public int[][] order = RectangularArrays.ReturnRectangularIntArray(8, 4);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][][] coef = new float[8][4][TNS_MAX_ORDER];
		public float[][][] coef = RectangularArrays.ReturnRectangularFloatArray(8, 4, TNS_MAX_ORDER);

		public virtual void copy(TemporalNoiseShaping that)
		{
			present = that.present;
			Utilities.copy(nFilt, that.nFilt);
			Utilities.copy(Length, that.Length);
			Utilities.copy(direction, that.direction);
			Utilities.copy(order, that.order);
			Utilities.copy(coef, that.coef);
		}
	}

}