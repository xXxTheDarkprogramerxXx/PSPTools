using System.Threading;

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
namespace pspsharp.memory.mmio.dmac
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContext.setLog4jMDC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.MMIO.normalizeAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.MMIOHandlerDdr.DDR_FLUSH_DMAC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_ATTRIBUTES_DST_INCREMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_ATTRIBUTES_DST_LENGTH_SHIFT_SHIFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_ATTRIBUTES_DST_STEP_SHIFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_ATTRIBUTES_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_ATTRIBUTES_SRC_INCREMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_ATTRIBUTES_SRC_LENGTH_SHIFT_SHIFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_ATTRIBUTES_SRC_STEP_SHIFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_ATTRIBUTES_TRIGGER_INTERRUPT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_ATTRIBUTES_UNKNOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.dmac.DmacProcessor.DMAC_STATUS_REQUIRES_DDR;

	using Logger = org.apache.log4j.Logger;

	using IAction = pspsharp.HLE.kernel.types.IAction;

	public class DmacThread : Thread
	{
		private static Logger log = MMIOHandlerDmac.log;
		private const int DMAC_MEMCPY_STEP2 = 0;
		private const int DMAC_MEMCPY_STEP16 = 1;
		private const int DMAC_MEMCPY_STEP8 = 2;
		private const int DMAC_MEMCPY_STEP4 = 3;
		private static readonly int[] dmacMemcpyStepLength = new int[8];
		private readonly Semaphore job = new Semaphore(0);
		private readonly Semaphore trigger = new Semaphore(0);
		private readonly DmacProcessor dmacProcessor;
		private volatile Memory memSrc;
		private volatile Memory memDst;
		private volatile int src;
		private volatile int dst;
		private volatile int next;
		private volatile int attributes;
		private volatile int status;
		private volatile IAction interruptAction;
		private volatile IAction completedAction;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private volatile bool exit_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private volatile bool abortJob_Renamed;

		public DmacThread(DmacProcessor dmacProcessor)
		{
			this.dmacProcessor = dmacProcessor;

			dmacMemcpyStepLength[DMAC_MEMCPY_STEP2] = 2;
			dmacMemcpyStepLength[DMAC_MEMCPY_STEP4] = 4;
			dmacMemcpyStepLength[DMAC_MEMCPY_STEP8] = 8;
			dmacMemcpyStepLength[DMAC_MEMCPY_STEP16] = 16;
		}

		public override void run()
		{
			setLog4jMDC();

			while (!exit_Renamed)
			{
				try
				{
					job.acquire();
					if (!exit_Renamed)
					{
						dmacMemcpy();
					}
				}
				catch (InterruptedException)
				{
					// Ignore exception
				}
			}
		}

		public virtual void exit()
		{
			exit_Renamed = true;
		}

		public virtual void execute(Memory memDst, Memory memSrc, int dst, int src, int next, int attributes, int status, IAction interruptAction, IAction completedAction)
		{
			abortJob_Renamed = false;
			this.memSrc = memSrc;
			this.memDst = memDst;
			this.dst = dst;
			this.src = src;
			this.next = next;
			this.attributes = attributes;
			this.status = status;
			this.interruptAction = interruptAction;
			this.completedAction = completedAction;

			job.release();
		}

		public virtual void abortJob()
		{
			abortJob_Renamed = true;

			trigger.release();
		}

		private void dmacMemcpy(int dst, int src, int dstLength, int srcLength, int dstStepLength, int srcStepLength, bool dstIncrement, bool srcIncrement)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("dmacMemcpy dst=0x%08X, src=0x%08X, dstLength=0x%X, srcLength=0x%X, dstStepLength=%d, srcStepLength=%d, dstIncrement=%b, srcIncrement=%b", dst, src, dstLength, srcLength, dstStepLength, srcStepLength, dstIncrement, srcIncrement));
				log.debug(string.Format("dmacMemcpy dst=0x%08X, src=0x%08X, dstLength=0x%X, srcLength=0x%X, dstStepLength=%d, srcStepLength=%d, dstIncrement=%b, srcIncrement=%b", dst, src, dstLength, srcLength, dstStepLength, srcStepLength, dstIncrement, srcIncrement));
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int srcStep4 = srcIncrement ? 4 : 0;
			int srcStep4 = srcIncrement ? 4 : 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int srcStep8 = srcIncrement ? 8 : 0;
			int srcStep8 = srcIncrement ? 8 : 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int srcStep12 = srcIncrement ? 12 : 0;
			int srcStep12 = srcIncrement ? 12 : 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int dstStep4 = dstIncrement ? 4 : 0;
			int dstStep4 = dstIncrement ? 4 : 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int dstStep8 = dstIncrement ? 8 : 0;
			int dstStep8 = dstIncrement ? 8 : 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int dstStep12 = dstIncrement ? 12 : 0;
			int dstStep12 = dstIncrement ? 12 : 0;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int stepLength = Math.min(srcStepLength, dstStepLength);
			int stepLength = System.Math.Min(srcStepLength, dstStepLength);

			while (dstLength > 0 && srcLength > 0)
			{
				switch (stepLength)
				{
					case 1:
						if (log.TraceEnabled)
						{
							log.trace(string.Format("memcpy dst=0x{0:X8}, src=0x{1:X8}, length=0x{2:X}", dst, src, 1));
						}
						memDst.write8(dst, (sbyte) memSrc.read8(src));
						break;
					case 2:
						if (log.TraceEnabled)
						{
							log.trace(string.Format("memcpy dst=0x{0:X8}, src=0x{1:X8}, length=0x{2:X}", dst, src, 2));
						}
						memDst.write16(dst, (short) memSrc.read16(src));
						break;
					case 4:
						if (log.TraceEnabled)
						{
							log.trace(string.Format("memcpy dst=0x{0:X8}, src=0x{1:X8}, length=0x{2:X}", dst, src, 4));
						}
						memDst.write32(dst, memSrc.read32(src));
						break;
					case 8:
						if (log.TraceEnabled)
						{
							log.trace(string.Format("memcpy dst=0x{0:X8}, src=0x{1:X8}, length=0x{2:X}", dst, src, 8));
						}
						memDst.write32(dst, memSrc.read32(src));
						memDst.write32(dst + dstStep4, memSrc.read32(src + srcStep4));
						break;
					case 16:
						if (log.TraceEnabled)
						{
							log.trace(string.Format("memcpy dst=0x{0:X8}, src=0x{1:X8}, length=0x{2:X}", dst, src, 16));
						}
						memDst.write32(dst, memSrc.read32(src));
						memDst.write32(dst + dstStep4, memSrc.read32(src + srcStep4));
						memDst.write32(dst + dstStep8, memSrc.read32(src + srcStep8));
						memDst.write32(dst + dstStep12, memSrc.read32(src + srcStep12));
						break;
				}
				dstLength -= stepLength;
				srcLength -= stepLength;

				if (dstIncrement)
				{
					dst += stepLength;
				}
				if (srcIncrement)
				{
					src += stepLength;
				}
			}
		}

		private bool dmacMemcpyStep()
		{
			if (abortJob_Renamed)
			{
				return false;
			}

			int srcStep = (attributes >> DMAC_ATTRIBUTES_SRC_STEP_SHIFT) & 0x7;
			int dstStep = (attributes >> DMAC_ATTRIBUTES_DST_STEP_SHIFT) & 0x7;
			int srcLengthShift = (attributes >> DMAC_ATTRIBUTES_SRC_LENGTH_SHIFT_SHIFT) & 0x7;
			int dstLengthShift = (attributes >> DMAC_ATTRIBUTES_DST_LENGTH_SHIFT_SHIFT) & 0x7;
			bool srcIncrement = (attributes & DMAC_ATTRIBUTES_SRC_INCREMENT) != 0;
			bool dstIncrement = (attributes & DMAC_ATTRIBUTES_DST_INCREMENT) != 0;
			int length = attributes & DMAC_ATTRIBUTES_LENGTH;

			int srcStepLength = dmacMemcpyStepLength[srcStep];
			if (srcStepLength == 0)
			{
				log.error(string.Format("dmacMemcpy with unknown srcStep={0:D}", srcStep));
				return false;
			}

			int dstStepLength = dmacMemcpyStepLength[dstStep];
			if (dstStepLength == 0)
			{
				log.error(string.Format("dmacMemcpy with unknown dstStep={0:D}", dstStep));
				return false;
			}

	//		if (srcStepLength != dstStepLength) {
	//			log.error(String.format("dmacMemcpy with different steps: srcStepLength=%d, dstSteplength=%d, dst=0x%08X, src=0x%08X, attr=0x%X, dstLength=0x%X(shift=%d), srcLength=0x%X(shift=%d)", srcStepLength, dstStepLength, dst, src, attributes, length << dstLengthShift, dstLengthShift, length << srcLengthShift, srcLengthShift));
	//			return false;
	//		}

			// TODO Not sure about the real meaning of this attribute flag...
			if ((attributes & DMAC_ATTRIBUTES_UNKNOWN) != 0)
			{
				// It seems to completely ignore the other attribute values
				srcIncrement = true;
				dstIncrement = true;
				srcStepLength = 1;
				dstStepLength = 1;
				srcLengthShift = 0;
				dstLengthShift = 0;
			}

			int srcLength = length << srcLengthShift;
			int dstLength = length << dstLengthShift;
			if (srcLength != dstLength)
			{
				log.error(string.Format("dmacMemcpy with different lengths: srcLength=0x{0:X}, dstLength=0x{1:X}", srcLength, dstLength));
				return false;
			}

			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("dmacMemcpy dst=0x%08X, src=0x%08X, attr=0x%08X, dstLength=0x%X(shift=%d), srcLength=0x%X(shift=%d), dstStepLength=0x%X(step=%d), srcStepLength=0x%X(step=%d), dstIncrement=%b, srcIncrement=%b, next=0x%08X, status=0x%X", dst, src, attributes, dstLength, dstLengthShift, srcLength, srcLengthShift, dstStepLength, dstStep, srcStepLength, srcStep, dstIncrement, srcIncrement, next, status));
				log.debug(string.Format("dmacMemcpy dst=0x%08X, src=0x%08X, attr=0x%08X, dstLength=0x%X(shift=%d), srcLength=0x%X(shift=%d), dstStepLength=0x%X(step=%d), srcStepLength=0x%X(step=%d), dstIncrement=%b, srcIncrement=%b, next=0x%08X, status=0x%X", dst, src, attributes, dstLength, dstLengthShift, srcLength, srcLengthShift, dstStepLength, dstStep, srcStepLength, srcStep, dstIncrement, srcIncrement, next, status));
			}

			// Update the DMAC registers
			dmacProcessor.Src = src;
			dmacProcessor.Dst = dst;
			dmacProcessor.Attributes = attributes;

			int normalizedSrc = normalizeAddress(src);
			int normalizedDst = normalizeAddress(dst);

			// Check for most common case which can be implemented through a simple memcpy
			if (srcIncrement && dstIncrement && memSrc == memDst)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("dmacMemcpy dst=0x{0:X8}, src=0x{1:X8}, length=0x{2:X}", normalizedDst, normalizedSrc, srcLength));
				}

				memSrc.memcpy(normalizedDst, normalizedSrc, srcLength);
			}
			else
			{
				dmacMemcpy(normalizedDst, normalizedSrc, dstLength, srcLength, dstStepLength, srcStepLength, dstIncrement, srcIncrement);
			}

			// Update the DMAC registers
			if (length > 0)
			{
				if (srcIncrement)
				{
					// Increment the src address
					dmacProcessor.Src = src + ((length - 1) << srcLengthShift);
				}
				if (dstIncrement)
				{
					// Increment the dst address
					dmacProcessor.Dst = dst + ((length - 1) << dstLengthShift);
				}
				// Clear the length field
				dmacProcessor.Attributes = attributes & unchecked((int)0xFFFFF000);
			}

			// Trigger an interrupt if requested in the attributes
			if ((attributes & DMAC_ATTRIBUTES_TRIGGER_INTERRUPT) != 0)
			{
				if (interruptAction != null)
				{
					interruptAction.execute();
				}
			}

			return true;
		}

		private void checkTrigger()
		{
			if (MMIOHandlerDdr.Instance.checkAndClearFlushDone(DDR_FLUSH_DMAC))
			{
				trigger.release();
			}
		}

		private bool waitForTrigger()
		{
			if (abortJob_Renamed)
			{
				return false;
			}

			if (trigger.tryAcquire())
			{
				return true;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("waitForTrigger starting"));
			}

			bool acquired = false;
			while (!acquired && !abortJob_Renamed)
			{
				try
				{
					trigger.acquire();
					acquired = true;
				}
				catch (InterruptedException)
				{
					// Ignore exception
				}
			}

			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("waitForTrigger done acquired=%b", acquired));
				log.debug(string.Format("waitForTrigger done acquired=%b", acquired));
			}

			return acquired;
		}

		private void dmacMemcpy()
		{
			bool waitForTrigger = false;

			if ((status & DMAC_STATUS_REQUIRES_DDR) != 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("dmacMemcpy requiring a call to sceDdrFlush(0x{0:X}), dst=0x{1:X8}, src=0x{2:X8}, attr=0x{3:X8}, next=0x{4:X8}, status=0x{5:X}", DDR_FLUSH_DMAC, dst, src, attributes, next, status));
				}
				trigger.drainPermits();
				waitForTrigger = true;
				checkTrigger();
				MMIOHandlerDdr.Instance.setFlushAction(DDR_FLUSH_DMAC, new DmacDdrFlushAction(this));

				if (!this.waitForTrigger())
				{
					return;
				}
			}

			if (dmacMemcpyStep())
			{
				while (next != 0 && !abortJob_Renamed)
				{
					src = memSrc.read32(next + 0);
					dst = memSrc.read32(next + 4);
					attributes = memSrc.read32(next + 12);

					if (!dmacMemcpyStep())
					{
						break;
					}

					if (waitForTrigger)
					{
						while (memSrc.read32(next + 8) == 0)
						{
							if (!this.waitForTrigger())
							{
								break;
							}
						}
					}

					next = memSrc.read32(next + 8);
					dmacProcessor.Next = next;
				}
			}

			if (completedAction != null)
			{
				completedAction.execute();
			}
		}

		public virtual void ddrFlushDone()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("dmacMemcpy sceDdrFlush(0x{0:X}) called", DDR_FLUSH_DMAC));
			}

			checkTrigger();
		}
	}

}