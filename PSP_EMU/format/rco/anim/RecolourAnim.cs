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
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using BaseNativeObject = pspsharp.format.rco.vsmx.objects.BaseNativeObject;

	public class RecolourAnim : BaseAnim
	{
		[ObjectField(order : 1)]
		public ObjectType @ref;
		[ObjectField(order : 2)]
		public FloatType duration;
		[ObjectField(order : 3)]
		public IntType accelMode;
		[ObjectField(order : 4)]
		public FloatType red;
		[ObjectField(order : 5)]
		public FloatType green;
		[ObjectField(order : 6)]
		public FloatType blue;
		[ObjectField(order : 7)]
		public FloatType alpha;

		private class RecolourAnimAction : AbstractAnimAction
		{
			private readonly RecolourAnim outerInstance;

			internal BasePositionObject positionObject;
			internal float startRed;
			internal float startGreen;
			internal float startBlue;
			internal float startAlpha;

			public RecolourAnimAction(RecolourAnim outerInstance, int duration, BasePositionObject positionObject) : base(duration)
			{
				this.outerInstance = outerInstance;
				this.positionObject = positionObject;
				startRed = positionObject.redScale.FloatValue;
				startGreen = positionObject.greenScale.FloatValue;
				startBlue = positionObject.blueScale.FloatValue;
				startAlpha = positionObject.alphaScale.FloatValue;
			}

			protected internal override void anim(float step)
			{
				positionObject.redScale.FloatValue = interpolate(startRed, outerInstance.red.FloatValue, step);
				positionObject.greenScale.FloatValue = interpolate(startGreen, outerInstance.green.FloatValue, step);
				positionObject.blueScale.FloatValue = interpolate(startBlue, outerInstance.blue.FloatValue, step);
				positionObject.alphaScale.FloatValue = interpolate(startAlpha, outerInstance.alpha.FloatValue, step);

				positionObject.onDisplayUpdated();

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("RecolourAnim '{0}' from ({1:F},{2:F},{3:F},{4:F}) to ({5:F},{6:F},{7:F},{8:F})", positionObject.Name, startRed, startGreen, startBlue, startAlpha, positionObject.redScale.FloatValue, positionObject.greenScale.FloatValue, positionObject.blueScale.FloatValue, positionObject.alphaScale.FloatValue));
				}
			}
		}

		protected internal override long doPlayReference(BasePositionObject @object)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("RecolourAnim play {0} on {1}", ToString(), @object));
			}

			Scheduler.addAction(new RecolourAnimAction(this, duration.IntValue, @object));

			return 0;
		}

		protected internal override long doPlay(VSMXBaseObject @object)
		{
			if (@object is VSMXNativeObject)
			{
				VSMXNativeObject nativeObject = (VSMXNativeObject) @object;
				BaseNativeObject baseNativeObject = nativeObject.Object;
				if (baseNativeObject is BasePositionObject)
				{
					return doPlayReference((BasePositionObject) baseNativeObject);
				}
			}
			return base.doPlay(@object);
		}
	}

}