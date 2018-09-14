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
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// coupling parameters
	/// </summary>
	public class ChannelCoupling
	{
		public int couplingPoint; ///< The point during decoding at which coupling is applied.
		public int numCoupled; ///< number of target elements
		public int[] type = new int[8]; ///< Type of channel element to be coupled - SCE or CPE.
		public int[] idSelect = new int[8]; ///< element id
		public int[] chSelect = new int[8]; /**< [0] shared list of gains; [1] list of gains for right channel;
	                                         *   [2] list of gains for left channel; [3] lists of gains for both channels
	                                         */
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] gain = new float[16][120];
		public float[][] gain = RectangularArrays.ReturnRectangularFloatArray(16, 120);

		public virtual void copy(ChannelCoupling that)
		{
			couplingPoint = that.couplingPoint;
			numCoupled = that.numCoupled;
			Utilities.copy(type, that.type);
			Utilities.copy(idSelect, that.idSelect);
			Utilities.copy(chSelect, that.chSelect);
			Utilities.copy(gain, that.gain);
		}
	}

}