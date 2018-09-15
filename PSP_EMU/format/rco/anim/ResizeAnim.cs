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
	using BasePositionObject = pspsharp.format.rco.@object.BasePositionObject;
	using FloatType = pspsharp.format.rco.type.FloatType;
	using IntType = pspsharp.format.rco.type.IntType;
	using ObjectType = pspsharp.format.rco.type.ObjectType;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;

	public class ResizeAnim : BaseAnim
	{
		[ObjectField(order : 1)]
		public ObjectType @ref;
		[ObjectField(order : 2)]
		public FloatType duration;
		[ObjectField(order : 3)]
		public IntType accelMode;
		[ObjectField(order : 4)]
		public FloatType width;
		[ObjectField(order : 5)]
		public FloatType height;
		[ObjectField(order : 6)]
		public FloatType depth;

		private class ResizeAnimAction : AbstractAnimAction
		{
			private readonly ResizeAnim outerInstance;

			internal BasePositionObject positionObject;
			internal float startWidth;
			internal float startHeight;
			internal float startDepth;

			public ResizeAnimAction(ResizeAnim outerInstance, int duration, BasePositionObject positionObject) : base(duration)
			{
				this.outerInstance = outerInstance;
				this.positionObject = positionObject;

				startWidth = positionObject.scaleWidth.FloatValue;
				startHeight = positionObject.scaleHeight.FloatValue;
				startDepth = positionObject.scaleDepth.FloatValue;
			}

			protected internal override void anim(float step)
			{
				positionObject.scaleWidth.FloatValue = startWidth + outerInstance.width.FloatValue * step;
				positionObject.scaleHeight.FloatValue = startHeight + outerInstance.height.FloatValue * step;
				positionObject.scaleDepth.FloatValue = startDepth + outerInstance.depth.FloatValue * step;

				positionObject.onDisplayUpdated();

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("ResizeAnim '{0}' from ({1:F},{2:F},{3:F}) to ({4:F},{5:F},{6:F})", positionObject.Name, startWidth, startHeight, startDepth, positionObject.scaleWidth.FloatValue, positionObject.scaleHeight.FloatValue, positionObject.scaleDepth.FloatValue));
				}
			}
		}

		protected internal override long doPlayReference(BasePositionObject @object)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("ResizeAnim play {0} on {1}", ToString(), @object));
			}

			Scheduler.addAction(new ResizeAnimAction(this, duration.IntValue, @object));

			return 0;
		}

		protected internal override long doPlay(VSMXBaseObject @object)
		{
			return doPlayReference(@ref);
		}
	}

}