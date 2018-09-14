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
namespace pspsharp.format.rco.anim
{
	using FloatType = pspsharp.format.rco.type.FloatType;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;

	public class DelayAnim : BaseAnim
	{
		[ObjectField(order : 1)]
		public FloatType time;

		protected internal override long doPlay(VSMXBaseObject @object)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("DelayAnim {0:D}", time.IntValue));
			}

			return time.IntValue;
		}
	}

}