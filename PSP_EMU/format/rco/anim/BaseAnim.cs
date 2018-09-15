using System.Collections.Generic;

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

	using IAction = pspsharp.HLE.kernel.types.IAction;
	using BaseObject = pspsharp.format.rco.@object.BaseObject;
	using BasePositionObject = pspsharp.format.rco.@object.BasePositionObject;
	using ObjectType = pspsharp.format.rco.type.ObjectType;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using BaseNativeObject = pspsharp.format.rco.vsmx.objects.BaseNativeObject;
	using Resource = pspsharp.format.rco.vsmx.objects.Resource;
	using Scheduler = pspsharp.scheduler.Scheduler;

	public class BaseAnim : BaseObject
	{
		private class PlayAnimAction : IAction
		{
			internal BaseAnim[] children;
			internal int index;
			internal int Length;
			internal VSMXBaseObject @object;

			public PlayAnimAction(BaseAnim[] children, int index, int Length, VSMXBaseObject @object)
			{
				this.children = children;
				this.index = index;
				this.Length = Length;
				this.@object = @object;
			}

			public virtual void execute()
			{
				while (index < Length)
				{
					long delay = children[index++].doPlay(@object);
					if (delay > 0)
					{
						Scheduler.addAction(Scheduler.Now + delay * 1000, this);
						return;
					}
				}
			}
		}

		protected internal virtual long doPlayReference(BasePositionObject @object)
		{
			return 0;
		}

		protected internal virtual long doPlayReference(BaseNativeObject @object)
		{
			if (@object is BasePositionObject)
			{
				return doPlayReference((BasePositionObject) @object);
			}

			return 0;
		}

		private long doPlayReference(VSMXBaseObject @object)
		{
			if (@object is VSMXNativeObject)
			{
				return doPlayReference(((VSMXNativeObject) @object).Object);
			}

			return 0;
		}

		protected internal virtual long doPlayReference(ObjectType @ref)
		{
			BasePositionObject positionObject = @ref.PositionObject;
			if (positionObject == null)
			{
				return 0;
			}

			VSMXBaseObject @object = positionObject.Object;
			long delay = doPlayReference(@object);
			if (@object.hasPropertyValue(Resource.childrenName))
			{
				VSMXBaseObject children = @object.getPropertyValue(Resource.childrenName);
				IList<string> names = children.PropertyNames;
				if (names != null)
				{
					foreach (string name in names)
					{
						VSMXBaseObject child = children.getPropertyValue(name);
						delay = System.Math.Max(delay, doPlayReference(child));
					}
				}
			}

			return delay;
		}

		protected internal virtual long doPlay(VSMXBaseObject @object)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("BaseAnim play on {0}", @object));
			}

			return 0;
		}

		public virtual void play(VSMXBaseObject thisObject, VSMXBaseObject @object)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("BaseAnim.play {0}, {1}", thisObject, @object));
			}

			if (thisObject.hasPropertyValue(Resource.childrenName))
			{
				VSMXBaseObject children = thisObject.getPropertyValue(Resource.childrenName);
				IList<string> names = children.PropertyNames;
				BaseAnim[] baseAnims = new BaseAnim[names.Count + 1];
				int numberBaseAnims = 0;
				baseAnims[numberBaseAnims++] = this;
				foreach (string name in names)
				{
					VSMXBaseObject child = children.getPropertyValue(name);
					if (child is VSMXNativeObject)
					{
						BaseNativeObject baseNativeObject = ((VSMXNativeObject) child).Object;
						if (baseNativeObject is BaseAnim)
						{
							baseAnims[numberBaseAnims++] = (BaseAnim) baseNativeObject;
						}
					}
				}

				if (numberBaseAnims > 0)
				{
					Scheduler.addAction(new PlayAnimAction(baseAnims, 0, numberBaseAnims, @object));
				}
			}
		}
	}

}