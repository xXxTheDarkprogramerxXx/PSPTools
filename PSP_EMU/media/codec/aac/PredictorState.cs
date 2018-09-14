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
	/// <summary>
	/// Predictor State
	/// </summary>
	public class PredictorState
	{
		public float cor0;
		public float cor1;
		public float var0;
		public float var1;
		public float r0;
		public float r1;

		public virtual void copy(PredictorState that)
		{
			cor0 = that.cor0;
			cor1 = that.cor1;
			var0 = that.var0;
			var1 = that.var1;
			r0 = that.r0;
			r1 = that.r1;
		}
	}

}