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
//	import static pspsharp.media.codec.aac.AacDecoder.MAX_PREDICTORS;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// Single Channel Element - used for both SCE and LFE elements.
	/// </summary>
	public class SingleChannelElement
	{
		public IndividualChannelStream ics = new IndividualChannelStream();
		public TemporalNoiseShaping tns = new TemporalNoiseShaping();
		public Pulse pulse = new Pulse();
		public int[] bandType = new int[128]; ///< band types
		public int[] bandTypeRunEnd = new int[120]; ///< band type run end points
		public float[] sf = new float[120]; ///< scalefactors
		public float[] coeffs = new float[1024]; ///< coefficients for IMDCT
		public float[] saved = new float[1536]; ///< overlap
		public float[] retBuf = new float[2048]; ///< PCM output buffer
		public float[] ltpState = new float[3072]; ///< time signal for LTP
		public PredictorState[] predictorState = new PredictorState[MAX_PREDICTORS];
		public float[] ret;

		public SingleChannelElement()
		{
			for (int i = 0; i < predictorState.Length; i++)
			{
				predictorState[i] = new PredictorState();
			}
		}

		public virtual void copy(SingleChannelElement that)
		{
			ics.copy(that.ics);
			tns.copy(that.tns);
			pulse.copy(that.pulse);
			Utilities.copy(bandType, that.bandType);
			Utilities.copy(bandTypeRunEnd, that.bandTypeRunEnd);
			Utilities.copy(sf, that.sf);
			Utilities.copy(coeffs, that.coeffs);
			Utilities.copy(saved, that.saved);
			Utilities.copy(retBuf, that.retBuf);
			Utilities.copy(ltpState, that.ltpState);
			for (int i = 0; i < predictorState.Length; i++)
			{
				predictorState[i].copy(that.predictorState[i]);
			}
			ret = that.ret;
		}
	}

}