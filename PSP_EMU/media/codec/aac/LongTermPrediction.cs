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
//	import static pspsharp.media.codec.aac.AacDecoder.MAX_LTP_LONG_SFB;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// Long Term Prediction
	/// </summary>
	public class LongTermPrediction
	{
		public bool present;
		public int lag;
		public float coef;
		public bool[] used = new bool[MAX_LTP_LONG_SFB];

		public virtual void copy(LongTermPrediction that)
		{
			present = that.present;
			lag = that.lag;
			coef = that.coef;
			Utilities.copy(used, that.used);
		}
	}

}