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
namespace pspsharp.format.rco.@object
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.round;


	using RCOEntry = pspsharp.format.RCO.RCOEntry;
	using AbstractAnimAction = pspsharp.format.rco.anim.AbstractAnimAction;
	using EventType = pspsharp.format.rco.type.EventType;
	using FloatType = pspsharp.format.rco.type.FloatType;
	using IntType = pspsharp.format.rco.type.IntType;
	using VSMXArray = pspsharp.format.rco.vsmx.interpreter.VSMXArray;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using VSMXNumber = pspsharp.format.rco.vsmx.interpreter.VSMXNumber;
	using BaseNativeObject = pspsharp.format.rco.vsmx.objects.BaseNativeObject;
	using Resource = pspsharp.format.rco.vsmx.objects.Resource;

	public class BasePositionObject : BaseObject, IDisplay
	{
		[ObjectField(order : 101)]
		public FloatType posX;
		[ObjectField(order : 102)]
		public FloatType posY;
		[ObjectField(order : 103)]
		public FloatType posZ;
		[ObjectField(order : 104)]
		public FloatType redScale;
		[ObjectField(order : 105)]
		public FloatType greenScale;
		[ObjectField(order : 106)]
		public FloatType blueScale;
		[ObjectField(order : 107)]
		public FloatType alphaScale;
		[ObjectField(order : 108)]
		public FloatType width;
		[ObjectField(order : 109)]
		public FloatType height;
		[ObjectField(order : 110)]
		public FloatType depth;
		[ObjectField(order : 111)]
		public FloatType scaleWidth;
		[ObjectField(order : 112)]
		public FloatType scaleHeight;
		[ObjectField(order : 113)]
		public FloatType scaleDepth;
		[ObjectField(order : 114)]
		public IntType iconOffset;
		[ObjectField(order : 115)]
		public EventType onInit;

		public float rotateX;
		public float rotateY;
		public float rotateAngle;
		public float animX;
		public float animY;
		public float animZ;

		private class AnimRotateAction : AbstractAnimAction
		{
			private readonly BasePositionObject outerInstance;

			internal float angle;

			public AnimRotateAction(BasePositionObject outerInstance, float x, float y, float angle, int duration) : base(duration)
			{
				this.outerInstance = outerInstance;
				outerInstance.rotateX = x;
				outerInstance.rotateY = y;
				this.angle = angle;
			}

			protected internal override void anim(float step)
			{
				outerInstance.rotateAngle = angle * step;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("AnimRotateAction to angle={0:F}", outerInstance.rotateAngle));
				}

				outerInstance.onDisplayUpdated();
			}
		}

		private class AnimPosAction : AbstractAnimAction
		{
			private readonly BasePositionObject outerInstance;

			internal float x;
			internal float y;
			internal float z;
			internal float startX;
			internal float startY;
			internal float startZ;

			public AnimPosAction(BasePositionObject outerInstance, float x, float y, float z, int duration) : base(duration)
			{
				this.outerInstance = outerInstance;
				this.x = x;
				this.y = y;
				this.z = z;

				startX = outerInstance.posX.FloatValue;
				startY = outerInstance.posY.FloatValue;
				startZ = outerInstance.posZ.FloatValue;
			}

			protected internal override void anim(float step)
			{
				outerInstance.posX.FloatValue = interpolate(startX, x, step);
				outerInstance.posY.FloatValue = interpolate(startY, y, step);
				outerInstance.posZ.FloatValue = interpolate(startZ, z, step);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("AnimPosAction from ({0:F},{1:F},{2:F}) to ({3:F},{4:F},{5:F})", startX, startY, startZ, outerInstance.posX.FloatValue, outerInstance.posY.FloatValue, outerInstance.posZ.FloatValue));
				}

				outerInstance.onDisplayUpdated();
			}
		}

		private class AnimScaleAction : AbstractAnimAction
		{
			private readonly BasePositionObject outerInstance;

			internal float width;
			internal float height;
			internal float depth;
			internal float startWidth;
			internal float startHeight;
			internal float startDepth;

			public AnimScaleAction(BasePositionObject outerInstance, float width, float height, float depth, int duration) : base(duration)
			{
				this.outerInstance = outerInstance;
				this.width = width;
				this.height = height;
				this.depth = depth;

				startWidth = outerInstance.scaleWidth.FloatValue;
				startHeight = outerInstance.scaleHeight.FloatValue;
				startDepth = outerInstance.scaleDepth.FloatValue;
			}

			protected internal override void anim(float step)
			{
				outerInstance.scaleWidth.FloatValue = interpolate(startWidth, width, step);
				outerInstance.scaleHeight.FloatValue = interpolate(startHeight, height, step);
				outerInstance.scaleDepth.FloatValue = interpolate(startDepth, depth, step);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("AnimScaleAction scaling from ({0:F},{1:F},{2:F}) to ({3:F},{4:F},{5:F})", startWidth, startHeight, startDepth, outerInstance.scaleWidth.FloatValue, outerInstance.scaleHeight.FloatValue, outerInstance.scaleDepth.FloatValue));
				}

				outerInstance.onDisplayUpdated();
			}
		}

		private class AnimColorAction : AbstractAnimAction
		{
			private readonly BasePositionObject outerInstance;

			internal float red;
			internal float green;
			internal float blue;
			internal float alpha;
			internal float startRed;
			internal float startGreen;
			internal float startBlue;
			internal float startAlpha;

			public AnimColorAction(BasePositionObject outerInstance, float red, float green, float blue, float alpha, int duration) : base(duration)
			{
				this.outerInstance = outerInstance;
				this.red = red;
				this.green = green;
				this.blue = blue;
				this.alpha = alpha;

				startRed = outerInstance.redScale.FloatValue;
				startGreen = outerInstance.greenScale.FloatValue;
				startBlue = outerInstance.blueScale.FloatValue;
				startAlpha = outerInstance.alphaScale.FloatValue;
			}

			protected internal override void anim(float step)
			{
				outerInstance.redScale.FloatValue = interpolate(startRed, red, step);
				outerInstance.greenScale.FloatValue = interpolate(startGreen, green, step);
				outerInstance.blueScale.FloatValue = interpolate(startBlue, blue, step);
				outerInstance.alphaScale.FloatValue = interpolate(startAlpha, alpha, step);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("AnimColorAction scaling from ({0:F},{1:F},{2:F},{3:F}) to ({4:F},{5:F},{6:F},{7:F})", startRed, startGreen, startBlue, startAlpha, red, green, blue, alpha));
				}

				outerInstance.onDisplayUpdated();
			}
		}

		public virtual int Width
		{
			get
			{
				float w = width.FloatValue;
				if (w == 0f)
				{
					BufferedImage image = Image;
					if (image != null)
					{
						w = (float) image.Width;
					}
				}
    
				return System.Math.Round(w * scaleWidth.FloatValue);
			}
		}

		public virtual int Height
		{
			get
			{
				float h = height.FloatValue;
				if (h == 0f)
				{
					BufferedImage image = Image;
					if (image != null)
					{
						h = (float) image.Height;
					}
				}
    
				return System.Math.Round(h * scaleHeight.FloatValue);
			}
		}

		public virtual BufferedImage Image
		{
			get
			{
				BufferedImage image = null;
    
				if (object.hasPropertyValue(Resource.textureName))
				{
					VSMXBaseObject textureObject = object.getPropertyValue(Resource.textureName);
					if (textureObject is VSMXNativeObject)
					{
						BaseNativeObject texture = ((VSMXNativeObject) textureObject).Object;
						if (texture is ImageObject)
						{
							image = ((ImageObject) texture).Image;
						}
					}
				}
    
				return image;
			}
		}

		public virtual int X
		{
			get
			{
				int parentX = 0;
				if (Parent is BasePositionObject)
				{
					parentX = ((BasePositionObject) Parent).X;
				}
    
				return parentX + posX.IntValue + round(animX);
			}
		}

		public virtual int Y
		{
			get
			{
				int parentY = 0;
				if (Parent is BasePositionObject)
				{
					parentY = ((BasePositionObject) Parent).Y;
				}
    
				return parentY + posY.IntValue + round(animY);
			}
		}

		public virtual float Alpha
		{
			get
			{
				return alphaScale.FloatValue;
			}
		}

		public virtual BufferedImage AnimImage
		{
			get
			{
				BufferedImage image = Image;
				if (image == null)
				{
					return image;
				}
    
				if (image.ColorModel is IndexColorModel)
				{
					// Cannot rescale colors on an indexed image
				}
				else
				{
					float[] scales = new float[] {redScale.FloatValue, greenScale.FloatValue, blueScale.FloatValue, alphaScale.FloatValue};
					float[] offsets = new float[] {0f, 0f, 0f, 0f};
					RescaleOp colorRescale = new RescaleOp(scales, offsets, null);
					image = colorRescale.filter(image, null);
				}
    
				if (rotateAngle != 0f)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Rotating image at ({0:F},{1:F}) by {2:F}", rotateX, rotateY, rotateAngle));
					}
					AffineTransform rotation = new AffineTransform();
					rotation.rotate(-rotateAngle, rotateX + image.Width / 2, rotateY + image.Height / 2);
					AffineTransformOp op = new AffineTransformOp(rotation, AffineTransformOp.TYPE_BILINEAR);
					image = op.filter(image, null);
				}
    
				return image;
			}
		}

		public virtual void setPos(VSMXBaseObject @object, VSMXBaseObject posX, VSMXBaseObject posY)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("setPos({0}, {1})", posX, posY));
			}
			this.posX.FloatValue = posX.FloatValue;
			this.posY.FloatValue = posY.FloatValue;
		}

		public virtual void setPos(VSMXBaseObject @object, VSMXBaseObject posX, VSMXBaseObject posY, VSMXBaseObject posZ)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("setPos({0}, {1}, {2})", posX, posY, posZ));
			}
			this.posX.FloatValue = posX.FloatValue;
			this.posY.FloatValue = posY.FloatValue;
			this.posZ.FloatValue = posZ.FloatValue;
		}

		public virtual VSMXBaseObject getPos(VSMXBaseObject @object)
		{
			VSMXInterpreter interpreter = @object.Interpreter;
			VSMXArray pos = new VSMXArray(interpreter, 3);
			pos.setPropertyValue(0, new VSMXNumber(interpreter, posX.FloatValue));
			pos.setPropertyValue(1, new VSMXNumber(interpreter, posY.FloatValue));
			pos.setPropertyValue(2, new VSMXNumber(interpreter, posZ.FloatValue));

			if (log.DebugEnabled)
			{
				log.debug(string.Format("getPos() returning {0}", pos));
			}

			return pos;
		}

		public virtual void setRotate(VSMXBaseObject @object, VSMXBaseObject x, VSMXBaseObject y, VSMXBaseObject rotationRads)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("setRotate({0}, {1}, {2})", x, y, rotationRads));
			}

			rotateX = x.FloatValue;
			rotateY = y.FloatValue;
			rotateAngle = rotationRads.FloatValue;
		}

		public virtual VSMXBaseObject getColor(VSMXBaseObject @object)
		{
			VSMXInterpreter interpreter = @object.Interpreter;
			VSMXArray color = new VSMXArray(interpreter, 4);
			color.setPropertyValue(0, new VSMXNumber(interpreter, redScale.FloatValue));
			color.setPropertyValue(1, new VSMXNumber(interpreter, greenScale.FloatValue));
			color.setPropertyValue(2, new VSMXNumber(interpreter, blueScale.FloatValue));
			color.setPropertyValue(3, new VSMXNumber(interpreter, alphaScale.FloatValue));

			if (log.DebugEnabled)
			{
				log.debug(string.Format("getColor() returning {0}", color));
			}

			return color;
		}

		public virtual void setColor(VSMXBaseObject @object, VSMXBaseObject red, VSMXBaseObject green, VSMXBaseObject blue, VSMXBaseObject alpha)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("setColor({0}, {1}, {2}, {3})", red, green, blue, alpha));
			}

			redScale.FloatValue = red.FloatValue;
			greenScale.FloatValue = green.FloatValue;
			blueScale.FloatValue = blue.FloatValue;
			alphaScale.FloatValue = alpha.FloatValue;

			onDisplayUpdated();
		}

		public virtual void animColor(VSMXBaseObject @object, VSMXBaseObject red, VSMXBaseObject green, VSMXBaseObject blue, VSMXBaseObject alpha, VSMXBaseObject duration)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("animColor({0}, {1}, {2}, {3}, {4})", red, green, blue, alpha, duration));
			}

			AnimColorAction action = new AnimColorAction(this, red.FloatValue, green.FloatValue, blue.FloatValue, alpha.FloatValue, duration.IntValue);
			Scheduler.addAction(action);
		}

		public virtual void setScale(VSMXBaseObject @object, VSMXBaseObject width, VSMXBaseObject height, VSMXBaseObject depth)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("setScale({0}, {1}, {2})", width, height, depth));
			}

			// TODO To be implemented
			onDisplayUpdated();
		}

		public virtual void animScale(VSMXBaseObject @object, VSMXBaseObject width, VSMXBaseObject height, VSMXBaseObject depth, VSMXBaseObject duration)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("animScale({0}, {1}, {2}, {3})", width, height, depth, duration));
			}

			AnimScaleAction action = new AnimScaleAction(this, width.FloatValue, height.FloatValue, depth.FloatValue, duration.IntValue);
			Scheduler.addAction(action);
		}

		public virtual void animPos(VSMXBaseObject @object, VSMXBaseObject x, VSMXBaseObject y, VSMXBaseObject z, VSMXBaseObject duration)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("animPos from ({0},{1},{2}) to ({3}, {4}, {5}), duration={6}", posX, posY, posZ, x, y, z, duration));
			}

			AnimPosAction action = new AnimPosAction(this, x.FloatValue, y.FloatValue, z.FloatValue, duration.IntValue);
			Scheduler.addAction(action);
		}

		public virtual void animRotate(VSMXBaseObject @object, VSMXBaseObject x, VSMXBaseObject y, VSMXBaseObject rotationRads, VSMXBaseObject duration)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("animRotate({0}, {1}, {2}, {3})", x, y, rotationRads, duration));
			}

			AnimRotateAction action = new AnimRotateAction(this, x.FloatValue, y.FloatValue, rotationRads.FloatValue, duration.IntValue);
			Scheduler.addAction(action);
		}

		public virtual void setFocus()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("setFocus()"));
			}
			if (display != null)
			{
				display.Focus = object;
			}
			if (controller != null)
			{
				controller.Focus = this;
			}
		}

		public virtual VSMXBaseObject Focus
		{
			set
			{
				setFocus();
			}
		}

		public virtual void focusOut()
		{
		}

		public virtual void setSize(VSMXBaseObject @object, VSMXBaseObject width, VSMXBaseObject height, VSMXBaseObject depth)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("setSize({0}, {1}, {2})", width, height, depth));
			}

			this.width.FloatValue = width.FloatValue;
			this.height.FloatValue = height.FloatValue;
			this.depth.FloatValue = depth.FloatValue;

			onDisplayUpdated();
		}

		public virtual void onUp()
		{
		}

		public virtual void onDown()
		{
		}

		public virtual void onLeft()
		{
		}

		public virtual void onRight()
		{
		}

		public virtual void onPush()
		{
		}

		public override VSMXBaseObject createVSMXObject(VSMXInterpreter interpreter, VSMXBaseObject parent, RCOEntry entry)
		{
			VSMXBaseObject @object = base.createVSMXObject(interpreter, parent, entry);

			BufferedImage image = Image;
			if (image != null)
			{
				@object.setPropertyValue(Resource.textureName, new VSMXNativeObject(interpreter, new ImageObject(image)));
			}

			return @object;
		}
	}

}