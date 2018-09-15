using System;
using System.Collections.Generic;
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
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.THREAD_CALLBACK_USER_DEFINED;


	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceKernelCallbackInfo = pspsharp.HLE.kernel.types.SceKernelCallbackInfo;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceNpAuthRequestParameter = pspsharp.HLE.kernel.types.SceNpAuthRequestParameter;
	using SceNpTicket = pspsharp.HLE.kernel.types.SceNpTicket;
	using TicketParam = pspsharp.HLE.kernel.types.SceNpTicket.TicketParam;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;

	public class sceNpAuth : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceNpAuth");
		public static bool useDummyTicket = false;

		public const int STATUS_ACCOUNT_SUSPENDED = 0x80;
		public const int STATUS_ACCOUNT_CHAT_RESTRICTED = 0x100;
		public const int STATUS_ACCOUNT_PARENTAL_CONTROL_ENABLED = 0x200;

		private bool initialized;
		private int npMemSize; // Memory allocated by the NP utility.
		private int npMaxMemSize; // Maximum memory used by the NP utility.
		private int npFreeMemSize; // Free memory available to use by the NP utility.
		private SceKernelCallbackInfo npAuthCreateTicketCallback;
		private string serviceId;
		private sbyte[] ticketBytes = new sbyte[10000];
		private int ticketBytesLength;

		public override void start()
		{
			initialized = false;
			base.start();
		}

		protected internal virtual void checkInitialized()
		{
			if (!initialized)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_NPAUTH_NOT_INIT);
			}
		}

		public static void addTicketParam(SceNpTicket ticket, int type, string value, int Length)
		{
			sbyte[] stringBytes = value.GetBytes(Charset.forName("ASCII"));
			sbyte[] bytes = new sbyte[Length];
			Array.Copy(stringBytes, 0, bytes, 0, System.Math.Min(Length, stringBytes.Length));
			ticket.parameters.Add(new SceNpTicket.TicketParam(type, bytes));
		}

		public static void addTicketParam(SceNpTicket ticket, string value, int Length)
		{
			addTicketParam(ticket, SceNpTicket.TicketParam.PARAM_TYPE_STRING_ASCII, value, Length);
		}

		public static void addTicketParam(SceNpTicket ticket, int value)
		{
			sbyte[] bytes = new sbyte[4];
			Utilities.writeUnaligned32(bytes, 0, Utilities.endianSwap32(value));
			ticket.parameters.Add(new SceNpTicket.TicketParam(SceNpTicket.TicketParam.PARAM_TYPE_INT, bytes));
		}

		public static void addTicketDateParam(SceNpTicket ticket, long time)
		{
			sbyte[] bytes = new sbyte[8];
			Utilities.writeUnaligned32(bytes, 0, Utilities.endianSwap32((int)(time >> 32)));
			Utilities.writeUnaligned32(bytes, 4, Utilities.endianSwap32((int) time));
			ticket.parameters.Add(new SceNpTicket.TicketParam(SceNpTicket.TicketParam.PARAM_TYPE_DATE, bytes));
		}

		public static void addTicketLongParam(SceNpTicket ticket, long value)
		{
			sbyte[] bytes = new sbyte[8];
			Utilities.writeUnaligned32(bytes, 0, Utilities.endianSwap32((int)(value >> 32)));
			Utilities.writeUnaligned32(bytes, 4, Utilities.endianSwap32((int) value));
			ticket.parameters.Add(new SceNpTicket.TicketParam(SceNpTicket.TicketParam.PARAM_TYPE_LONG, bytes));
		}

		public static void addTicketParam(SceNpTicket ticket)
		{
			ticket.parameters.Add(new SceNpTicket.TicketParam(SceNpTicket.TicketParam.PARAM_TYPE_NULL, new sbyte[0]));
		}

		private static string encodeURLParam(string value)
		{
			try
			{
				return URLEncoder.encode(value, "UTF-8");
			}
			catch (UnsupportedEncodingException)
			{
				return value;
			}
		}

		private static void addURLParam(StringBuilder @params, string name, string value)
		{
			if (@params.Length > 0)
			{
				@params.Append("&");
			}
			@params.Append(name);
			@params.Append("=");
			@params.Append(encodeURLParam(value));
		}

		private static void addURLParam(StringBuilder @params, string name, int addr, int Length)
		{
			StringBuilder value = new StringBuilder();
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, Length, 1);
			for (int i = 0; i < Length; i++)
			{
				int c = memoryReader.readNext();
				value.Append((char) c);
			}

			addURLParam(@params, name, value.ToString());
		}

		/// <summary>
		/// Initialization.
		/// </summary>
		/// <param name="poolSize"> </param>
		/// <param name="stackSize"> </param>
		/// <param name="threadPriority">
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA1DE86F8, version = 150, checkInsideInterrupt = true) public int sceNpAuthInit(int poolSize, int stackSize, int threadPriority)
		[HLEFunction(nid : 0xA1DE86F8, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpAuthInit(int poolSize, int stackSize, int threadPriority)
		{
			npMemSize = poolSize;
			npMaxMemSize = poolSize / 2; // Dummy
			npFreeMemSize = poolSize - 16; // Dummy.

			initialized = true;

			return 0;
		}

		/// <summary>
		/// Termination.
		/// 
		/// @return
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4EC1F667, version = 150, checkInsideInterrupt = true) public int sceNpAuthTerm()
		[HLEFunction(nid : 0x4EC1F667, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpAuthTerm()
		{
			initialized = false;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF4531ADC, version = 150, checkInsideInterrupt = true) public int sceNpAuthGetMemoryStat(pspsharp.HLE.TPointer32 memStatAddr)
		[HLEFunction(nid : 0xF4531ADC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpAuthGetMemoryStat(TPointer32 memStatAddr)
		{
			checkInitialized();

			memStatAddr.setValue(0, npMemSize);
			memStatAddr.setValue(4, npMaxMemSize);
			memStatAddr.setValue(8, npFreeMemSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCD86A656, version = 150) public int sceNpAuthCreateStartRequest(pspsharp.HLE.TPointer paramAddr)
		[HLEFunction(nid : 0xCD86A656, version : 150)]
		public virtual int sceNpAuthCreateStartRequest(TPointer paramAddr)
		{
			SceNpAuthRequestParameter param = new SceNpAuthRequestParameter();
			param.read(paramAddr);
			if (log.InfoEnabled)
			{
				log.info(string.Format("sceNpAuthCreateStartRequest param: {0}", param));
			}

			serviceId = param.serviceId;

			if (!useDummyTicket)
			{
				string loginId = JOptionPane.showInputDialog("Enter your PSN Sign-In ID (Email Address)");
				if (!string.ReferenceEquals(loginId, null))
				{
					string password = JOptionPane.showInputDialog("Enter your PSN Password");
					if (!string.ReferenceEquals(password, null))
					{
						StringBuilder @params = new StringBuilder();
						addURLParam(@params, "serviceid", serviceId);
						addURLParam(@params, "loginid", loginId);
						addURLParam(@params, "password", password);
						if (param.cookie != 0)
						{
							addURLParam(@params, "cookie", param.cookie, param.cookieSize);
						}
						if (param.entitlementIdAddr != 0)
						{
							addURLParam(@params, "entitlementid", param.entitlementId);
							addURLParam(@params, "consumedcount", Convert.ToString(param.consumedCount));
						}

						HttpURLConnection connection = null;
						ticketBytesLength = 0;
						try
						{
							connection = (HttpURLConnection) (new URL("https://auth.np.ac.playstation.net/nav/auth")).openConnection();
							connection.setRequestProperty("X-I-5-Version", "2.1");
							connection.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");
							connection.setRequestProperty("X-Platform-Version", "PSP 06.60");
							connection.setRequestProperty("Content-Length", Convert.ToString(@params.Length));
							connection.setRequestProperty("User-Agent", "Lediatio Lunto Ritna");
							connection.RequestMethod = "POST";
							connection.DoOutput = true;
							System.IO.Stream os = connection.OutputStream;
							os.WriteByte(@params.ToString().GetBytes());
							os.Close();
							connection.connect();
							int responseCode = connection.ResponseCode;
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("Response code: {0:D}", responseCode));
								foreach (KeyValuePair<string, IList<string>> entry in connection.HeaderFields.entrySet())
								{
									Console.WriteLine(string.Format("{0}: {1}", entry.Key, entry.Value));
								}
							}

							if (responseCode == 200)
							{
								System.IO.Stream @in = connection.InputStream;
								while (true)
								{
									int Length = @in.Read(ticketBytes, ticketBytesLength, ticketBytes.Length - ticketBytesLength);
									if (Length < 0)
									{
										break;
									}
									ticketBytesLength += Length;
								}
								@in.Close();

								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("Received ticket: {0}", Utilities.getMemoryDump(ticketBytes, 0, ticketBytesLength)));
								}
							}
						}
						catch (MalformedURLException e)
						{
							Console.WriteLine(e);
						}
						catch (IOException e)
						{
							Console.WriteLine(e);
						}
						finally
						{
							if (connection != null)
							{
								connection.disconnect();
							}
						}
					}
				}
			}

			if (param.ticketCallback != 0)
			{
				int ticketLength = ticketBytesLength > 0 ? ticketBytesLength : 248;
				npAuthCreateTicketCallback = Modules.ThreadManForUserModule.hleKernelCreateCallback("sceNpAuthCreateStartRequest", param.ticketCallback, param.callbackArgument);
				if (Modules.ThreadManForUserModule.hleKernelRegisterCallback(THREAD_CALLBACK_USER_DEFINED, npAuthCreateTicketCallback.Uid))
				{
					Modules.ThreadManForUserModule.hleKernelNotifyCallback(THREAD_CALLBACK_USER_DEFINED, npAuthCreateTicketCallback.Uid, ticketLength);
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3F1C1F70, version = 150) public int sceNpAuthGetTicket(int id, pspsharp.HLE.TPointer buffer, int Length)
		[HLEFunction(nid : 0x3F1C1F70, version : 150)]
		public virtual int sceNpAuthGetTicket(int id, TPointer buffer, int Length)
		{
			int result;

			if (useDummyTicket)
			{
				SceNpTicket ticket = new SceNpTicket();
				ticket.version = 0x00000121;
				ticket.size = 0xF0;
				addTicketParam(ticket, "XXXXXXXXXXXXXXXXXXXX", 20);
				addTicketParam(ticket, 0);
				long now = DateTimeHelper.CurrentUnixTimeMillis();
				addTicketDateParam(ticket, now);
				addTicketDateParam(ticket, now + 10 * 60 * 1000); // now + 10 minutes
				addTicketLongParam(ticket, 0L);
				addTicketParam(ticket, SceNpTicket.TicketParam.PARAM_TYPE_STRING, "DummyOnlineID", 32);
				addTicketParam(ticket, "gb", 4);
				addTicketParam(ticket, SceNpTicket.TicketParam.PARAM_TYPE_STRING, "XX", 4);
				addTicketParam(ticket, serviceId, 24);
				int status = 0;
				if (Modules.sceNpModule.parentalControl == sceNp.PARENTAL_CONTROL_ENABLED)
				{
					status |= STATUS_ACCOUNT_PARENTAL_CONTROL_ENABLED;
				}
				status |= (Modules.sceNpModule.UserAge & 0x7F) << 24;
				addTicketParam(ticket, status);
				addTicketParam(ticket);
				addTicketParam(ticket);
				ticket.unknownBytes = new sbyte[72];
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceNpAuthGetTicket returning dummy ticket: {0}", ticket));
				}
				ticket.write(buffer);
				result = ticket.@sizeof();
			}
			else if (ticketBytesLength > 0)
			{
				result = System.Math.Min(ticketBytesLength, Length);
				IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(buffer.Address, result, 1);
				for (int i = 0; i < result; i++)
				{
					memoryWriter.writeNext(ticketBytes[i] & 0xFF);
				}

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceNpAuthGetTicket returning real ticket: {0}", Utilities.getMemoryDump(buffer.Address, result)));
				}
			}
			else
			{
				buffer.clear(Length);

				result = Length;

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceNpAuthGetTicket returning empty ticket"));
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6900F084, version = 150) public int sceNpAuthGetEntitlementById(pspsharp.HLE.TPointer ticketBuffer, int ticketLength, int unknown1, int unknown2)
		[HLEFunction(nid : 0x6900F084, version : 150)]
		public virtual int sceNpAuthGetEntitlementById(TPointer ticketBuffer, int ticketLength, int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x72BB0467, version = 150) public int sceNpAuthDestroyRequest(int id)
		[HLEFunction(nid : 0x72BB0467, version : 150)]
		public virtual int sceNpAuthDestroyRequest(int id)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD99455DD, version = 150) public int sceNpAuthAbortRequest(int id)
		[HLEFunction(nid : 0xD99455DD, version : 150)]
		public virtual int sceNpAuthAbortRequest(int id)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5A3CB57A, version = 150) public int sceNpAuthGetTicketParam(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer ticketBuffer, int ticketLength, int paramNumber, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=256, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x5A3CB57A, version : 150)]
		public virtual int sceNpAuthGetTicketParam(TPointer ticketBuffer, int ticketLength, int paramNumber, TPointer buffer)
		{
			// This clear is always done, even when an error is returned
			buffer.clear(256);

			if (paramNumber < 0 || paramNumber >= SceNpTicket.NUMBER_PARAMETERS)
			{
				return SceKernelErrors.ERROR_NP_MANAGER_INVALID_ARGUMENT;
			}

			if (ticketBuffer.getValue32() == 0)
			{
				// This is an empty ticket, do no analyze it
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceNpAuthGetTicketParam returning empty param from empty ticket"));
				}
			}
			else
			{
				SceNpTicket ticket = new SceNpTicket();
				ticket.read(ticketBuffer);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceNpAuthGetTicketParam ticket: {0}", ticket));
				}

				SceNpTicket.TicketParam ticketParam = ticket.parameters[paramNumber];
				ticketParam.writeForPSP(buffer);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x75FB0AE3, version = 150) public int sceNpAuthGetEntitlementIdList()
		[HLEFunction(nid : 0x75FB0AE3, version : 150)]
		public virtual int sceNpAuthGetEntitlementIdList()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x61BB18B3, version = 150) public int sceNpAuth_61BB18B3()
		[HLEFunction(nid : 0x61BB18B3, version : 150)]
		public virtual int sceNpAuth_61BB18B3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB714FBDD, version = 150) public int sceNpAuth_B714FBDD()
		[HLEFunction(nid : 0xB714FBDD, version : 150)]
		public virtual int sceNpAuth_B714FBDD()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCE85B3B8, version = 150) public int sceNpAuth_CE85B3B8()
		[HLEFunction(nid : 0xCE85B3B8, version : 150)]
		public virtual int sceNpAuth_CE85B3B8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDAD65284, version = 150) public int sceNpAuth_DAD65284()
		[HLEFunction(nid : 0xDAD65284, version : 150)]
		public virtual int sceNpAuth_DAD65284()
		{
			return 0;
		}
	}

}