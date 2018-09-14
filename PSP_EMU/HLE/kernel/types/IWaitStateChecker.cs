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
namespace pspsharp.HLE.kernel.types
{
	public interface IWaitStateChecker
	{
		/// <summary>
		/// Checks if a thread has to return to its wait state after the execution
		/// of a callback.
		/// </summary>
		/// <param name="thread"> the thread </param>
		/// <param name="wait">   the wait state that has to be checked </param>
		/// <returns>       true if the thread has to return to the wait state
		///               false if the thread has not to return to the wait state but
		///               continue in the READY state. In that case, the return values have
		///               to be set in the CpuState of the thread (at least $v0). </returns>
		bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait);
	}

}