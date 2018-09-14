using System.Text;

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
namespace pspsharp.format.rco.type
{

	using BaseObject = pspsharp.format.rco.@object.BaseObject;
	using BasePositionObject = pspsharp.format.rco.@object.BasePositionObject;

	public class BaseReferenceType : BaseType
	{
		protected internal const int REFERENCE_TYPE_NONE = 0xFFFF;
		protected internal const int REFERENCE_TYPE_EVENT = 0x400;
		protected internal const int REFERENCE_TYPE_TEXT = 0x401;
		protected internal const int REFERENCE_TYPE_IMAGE = 0x402;
		protected internal const int REFERENCE_TYPE_MODEL = 0x403;
		protected internal const int REFERENCE_TYPE_FONT = 0x405;
		protected internal const int REFERENCE_TYPE_OBJECT = 0x407;
		protected internal const int REFERENCE_TYPE_ANIM = 0x408;
		protected internal const int REFERENCE_TYPE_POSITION_OBJECT = 0x409;
		public int referenceType;
		public int unknownShort;
		protected internal string @event;
		protected internal BaseObject @object;
		protected internal BufferedImage image;

		public override int size()
		{
			return 8;
		}

		public override void read(RCOContext context)
		{
			referenceType = read16(context);
			unknownShort = read16(context);

			base.read(context);
		}

		public override void init(RCOContext context)
		{
			switch (referenceType)
			{
				case REFERENCE_TYPE_NONE:
					break;
				case REFERENCE_TYPE_EVENT:
					@event = context.events[value];
					break;
				case REFERENCE_TYPE_OBJECT:
				case REFERENCE_TYPE_POSITION_OBJECT:
				case REFERENCE_TYPE_ANIM:
					@object = context.objects[value];
					break;
				case REFERENCE_TYPE_IMAGE:
					image = context.images[value];
					break;
				default:
					log.warn(string.Format("BaseReferenceType: unknown referenceType 0x{0:X}({1})", referenceType, getReferenceTypeString(referenceType)));
					break;
			}
			base.init(context);
		}

		private static string getReferenceTypeString(int referenceType)
		{
			switch (referenceType)
			{
				case REFERENCE_TYPE_NONE:
					return "NONE";
				case REFERENCE_TYPE_EVENT:
					return "EVENT";
				case REFERENCE_TYPE_TEXT:
					return "TEXT";
				case REFERENCE_TYPE_IMAGE:
					return "IMAGE";
				case REFERENCE_TYPE_MODEL:
					return "MODEL";
				case REFERENCE_TYPE_FONT:
					return "FONT";
				case REFERENCE_TYPE_OBJECT:
					return "OBJECT";
				case REFERENCE_TYPE_ANIM:
					return "ANIM";
				case REFERENCE_TYPE_POSITION_OBJECT:
					return "POSITION_OBJECT";
			}

			return "UNKNOWN";
		}

		public virtual string Event
		{
			get
			{
				return @event;
			}
		}

		public virtual BaseObject Object
		{
			get
			{
				return @object;
			}
		}

		public virtual BasePositionObject PositionObject
		{
			get
			{
				if (@object is BasePositionObject)
				{
					return (BasePositionObject) @object;
				}
    
				return null;
			}
		}

		public virtual BufferedImage Image
		{
			get
			{
				return image;
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.Append(string.Format("referenceType=0x{0:X}({1}), short1=0x{2:X}, value=0x{3:X}", referenceType, getReferenceTypeString(referenceType), unknownShort, value));
			if (!string.ReferenceEquals(@event, null))
			{
				s.Append(string.Format(", event='{0}'", @event));
			}
			if (@object != null)
			{
				s.Append(string.Format(", object='{0}'", @object.Name));
			}
			if (image != null)
			{
				s.Append(string.Format(", image={0:D}x{1:D}", image.Width, image.Height));
			}

			return s.ToString();
		}
	}

}