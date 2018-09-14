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
//	import static pspsharp.media.codec.aac.AacDecoder.MAX_ELEM_ID;
	using Utilities = pspsharp.util.Utilities;

	public class OutputConfiguration
	{
		public const int OC_NONE = 0; ///< Output unconfigured
		public const int OC_TRIAL_PCE = 1; ///< Output configuration under trial specified by an inband PCE
		public const int OC_TRIAL_FRAME = 2; ///< Output configuration under trial specified by a frame header
		public const int OC_GLOBAL_HDR = 3; ///< Output configuration set in a global header but not yet locked
		public const int OC_LOCKED = 4; ///< Output configuration locked in place

		public MPEG4AudioConfig m4ac = new MPEG4AudioConfig();
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public int[][] layoutMap = new int[MAX_ELEM_ID*4][3];
		public int[][] layoutMap = RectangularArrays.ReturnRectangularIntArray(MAX_ELEM_ID * 4, 3);
		public int layoutMapTags;
		public int channels;
		public int channelLayout;
		public int status;

		public virtual void copy(OutputConfiguration that)
		{
			m4ac.copy(that.m4ac);
			Utilities.copy(layoutMap, that.layoutMap);
			layoutMapTags = that.layoutMapTags;
			channels = that.channels;
			channelLayout = that.channelLayout;
			status = that.status;
		}
	}

}