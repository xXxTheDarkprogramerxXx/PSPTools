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
//	import static pspsharp.media.codec.aac.AacDecoder.MAX_CHANNELS;

	/// <summary>
	/// Dynamic Range Control - decoded from the bitstream but not processed further.
	/// </summary>
	public class DynamicRangeControl
	{
		internal int pceInstanceTag; ///< Indicates with which program the DRC info is associated.
		internal int[] dynRngSgn = new int[17]; ///< DRC sign information; 0 - positive, 1 - negative
		internal int[] dynRngCtl = new int[17]; ///< DRC magnitude information
		internal int[] excludeMask = new int[MAX_CHANNELS]; ///< Channels to be excluded from DRC processing.
		internal int bandIncr; ///< Number of DRC bands greater than 1 having DRC info.
		internal int interpolationScheme; ///< Indicates the interpolation scheme used in the SBR QMF domain.
		internal int[] bandTop = new int[17]; ///< Indicates the top of the i-th DRC band in units of 4 spectral lines.
		internal int progRefLevel; /**< A reference level for the long-term program audio level for all
	                                                   *   channels combined.
	                                                   */
	}

}