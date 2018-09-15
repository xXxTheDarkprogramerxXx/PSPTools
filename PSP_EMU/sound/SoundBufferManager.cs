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
namespace pspsharp.sound
{

	using sceAudio = pspsharp.HLE.modules.sceAudio;

	//using Logger = org.apache.log4j.Logger;
	using AL10 = org.lwjgl.openal.AL10;

	public class SoundBufferManager
	{
		private static Logger log = sceAudio.log;
		private static SoundBufferManager instance;
		private Stack<int> freeBuffers;
		private IList<ByteBuffer> freeDirectBuffers;

		public static SoundBufferManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new SoundBufferManager();
				}
    
				return instance;
			}
		}

		private SoundBufferManager()
		{
			freeBuffers = new Stack<int>();
			freeDirectBuffers = new LinkedList<ByteBuffer>();
		}

		public virtual int Buffer
		{
			get
			{
				if (freeBuffers.Count == 0)
				{
					int alBuffer = AL10.alGenBuffers();
					freeBuffers.Push(alBuffer);
				}
    
				return freeBuffers.Pop();
			}
		}

		public virtual void checkFreeBuffers(int alSource)
		{
			while (true)
			{
				int processedBuffers = AL10.alGetSourcei(alSource, AL10.AL_BUFFERS_PROCESSED);
				if (processedBuffers <= 0)
				{
					break;
				}
				int alBuffer = AL10.alSourceUnqueueBuffers(alSource);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("free buffer {0:D}", alBuffer));
				}
				freeBuffers.Push(alBuffer);
			}
		}

		public virtual ByteBuffer getDirectBuffer(int size)
		{
			for (int i = 0; i < freeDirectBuffers.Count; i++)
			{
				ByteBuffer directBuffer = freeDirectBuffers[i];
				if (directBuffer.capacity() >= size)
				{
					freeDirectBuffers.RemoveAt(i);
					return directBuffer;
				}
			}

			ByteBuffer directBuffer = ByteBuffer.allocateDirect(size);
			return directBuffer;
		}

		public virtual void releaseDirectBuffer(ByteBuffer directBuffer)
		{
			if (freeDirectBuffers.Count == 0)
			{
				freeDirectBuffers.Add(directBuffer);
			}
			else
			{
				freeDirectBuffers.Insert(0, directBuffer);
			}
		}
	}

}