using System;

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

/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace pspsharp.log
{

	using Category = org.apache.log4j.Category;
	using Level = org.apache.log4j.Level;
	//using Logger = org.apache.log4j.Logger;
	using Priority = org.apache.log4j.Priority;

	/// <summary>
	/// An OutputStream that flushes out to a Category.
	/// <para>
	/// 
	/// Note that no data is written out to the Category until the stream is flushed
	/// or closed.
	/// </para>
	/// <para>
	/// 
	/// Example:
	/// 
	/// <pre>
	/// // make sure everything sent to System.err is logged
	/// System.setErr(new PrintStream(new LoggingOutputStream(Category.getRoot(),
	/// 		Priority.WARN), true));
	/// 
	/// // make sure everything sent to System.out is also logged
	/// System.setOut(new PrintStream(new LoggingOutputStream(Category.getRoot(),
	/// 		Priority.INFO), true));
	/// </pre>
	/// 
	/// @author <a href="mailto://Jim.Moore@rocketmail.com">Jim Moore</a>
	/// </para>
	/// </summary>
	/// <seealso cref= Category </seealso>
	public class LoggingOutputStream : OutputStream
	{
		protected internal static readonly string LINE_SEPERATOR = System.getProperty("line.separator");

		/// <summary>
		/// Used to maintain the contract of <seealso cref="#close()"/>.
		/// </summary>
		protected internal bool hasBeenClosed = false;

		/// <summary>
		/// The internal buffer where data is stored.
		/// </summary>
		protected internal sbyte[] buf;

		/// <summary>
		/// The number of valid bytes in the buffer. This value is always in the
		/// range <tt>0</tt> through <tt>buf.Length</tt>; elements <tt>buf[0]</tt>
		/// through <tt>buf[count-1]</tt> contain valid byte data.
		/// </summary>
		protected internal int count;

		/// <summary>
		/// Remembers the size of the buffer for speed.
		/// </summary>
		private int bufLength;

		/// <summary>
		/// The default number of bytes in the buffer. =2048
		/// </summary>
		public const int DEFAULT_BUFFER_LENGTH = 2048;

		/// <summary>
		/// The category to write to.
		/// </summary>
		protected internal Category category;

		/// <summary>
		/// The priority to use when writing to the Category.
		/// </summary>
		protected internal Priority priority;

		/// <summary>
		/// Creates the LoggingOutputStream to flush to the given Category.
		/// </summary>
		/// <param name="cat">
		///            the Category to write to
		/// </param>
		/// <param name="priority">
		///            the Priority to use when writing to the Category
		/// </param>
		/// <exception cref="IllegalArgumentException">
		///                if cat == null or priority == null </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public LoggingOutputStream(org.apache.log4j.Logger cat, org.apache.log4j.Level priority) throws IllegalArgumentException
		public LoggingOutputStream(Logger cat, Level priority)
		{
			if (cat == null)
			{
				throw new System.ArgumentException("cat == null");
			}
			if (priority == null)
			{
				throw new System.ArgumentException("priority == null");
			}

			this.priority = priority;
			category = cat;
			bufLength = DEFAULT_BUFFER_LENGTH;
			buf = new sbyte[DEFAULT_BUFFER_LENGTH];
			count = 0;
		}

		/// <summary>
		/// Closes this output stream and releases any system resources associated
		/// with this stream. The general contract of <code>close</code> is that it
		/// closes the output stream. A closed stream cannot perform output
		/// operations and cannot be reopened.
		/// </summary>
		public override void close()
		{
			flush();
			hasBeenClosed = true;
		}

		/// <summary>
		/// Writes the specified byte to this output stream. The general contract for
		/// <code>write</code> is that one byte is written to the output stream. The
		/// byte to be written is the eight low-order bits of the argument
		/// <code>b</code>. The 24 high-order bits of <code>b</code> are ignored.
		/// </summary>
		/// <param name="b">
		///            the <code>byte</code> to write
		/// </param>
		/// <exception cref="IOException">
		///                if an I/O error occurs. In particular, an
		///                <code>IOException</code> may be thrown if the output
		///                stream has been closed. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(final int b) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public override void write(int b)
		{
			if (hasBeenClosed)
			{
				throw new IOException("The stream has been closed.");
			}

			// don't log nulls
			if (b == 0)
			{
				return;
			}

			if (b == '\r' || b == '\n')
			{
				flush();
				return;
			}

			// would this be writing past the buffer?
			if (count == bufLength)
			{
				// grow the buffer
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newBufLength = bufLength + DEFAULT_BUFFER_LENGTH;
				int newBufLength = bufLength + DEFAULT_BUFFER_LENGTH;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] newBuf = new byte[newBufLength];
				sbyte[] newBuf = new sbyte[newBufLength];

				Array.Copy(buf, 0, newBuf, 0, bufLength);

				buf = newBuf;
				bufLength = newBufLength;
			}

			buf[count] = (sbyte) b;
			count++;
		}

		/// <summary>
		/// Flushes this output stream and forces any buffered output bytes to be
		/// written out. The general contract of <code>flush</code> is that calling
		/// it is an indication that, if any bytes previously written have been
		/// buffered by the implementation of the output stream, such bytes should
		/// immediately be written to their intended destination.
		/// </summary>
		public override void flush()
		{
			if (count == 0)
			{
				return;
			}

			// don't print out blank lines; flushing from PrintStream puts out these
			if (count == LINE_SEPERATOR.Length)
			{
				if (((char) buf[0]) == LINE_SEPERATOR[0] && ((count == 1) || ((count == 2) && ((char) buf[1]) == LINE_SEPERATOR[1])))
				{ // <- Windows
					reset();
					return;
				}
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] theBytes = new byte[count];
			sbyte[] theBytes = new sbyte[count];

			Array.Copy(buf, 0, theBytes, 0, count);

			category.log(priority, StringHelper.NewString(theBytes));

			reset();
		}

		private void reset()
		{
			// not resetting the buffer -- assuming that if it grew that it
			// will likely grow similarly again
			count = 0;
		}
	}
}