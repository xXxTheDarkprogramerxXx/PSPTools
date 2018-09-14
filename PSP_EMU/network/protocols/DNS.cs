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
namespace pspsharp.network.protocols
{

	using Utilities = pspsharp.util.Utilities;

	public class DNS
	{
		// DNS packet format, see http://www.tcpipguide.com/free/t_DNSMessageHeaderandQuestionSectionFormat.htm
		public const int DNS_RESOURCE_RECORD_CLASS_IN = 1; // "Internet"
		public const int DNS_RESOURCE_RECORD_TYPE_A = 1; // Address (IPv4) record
		public const int DNS_RESPONSE_CODE_NO_ERROR = 0;
		public const int DNS_RESPONSE_CODE_NAME_ERROR = 3;
		public int identifier;
		public bool isResponseFlag;
		public int opcode;
		public bool authoritativeAnswer;
		public bool truncationFlag;
		public bool recursionDesired;
		public bool recursionAvailable;
		public int zero;
		public int responseCode;
		public int questionCount;
		public int answerRecordCount;
		public int authorityRecordCount;
		public int additionalRecordCount;
		public DNSRecord[] questions;
		public DNSAnswerRecord[] answerRecords;
		public DNSAnswerRecord[] authorityRecords;
		public DNSAnswerRecord[] additionalRecords;

		public class DNSRecord
		{
			// See format http://www.zytrax.com/books/dns/ch15/
			public string recordName;
			// List of types: https://en.wikipedia.org/wiki/List_of_DNS_record_types
			public int recordType;
			// Only record class: DNS_RESOURCE_RECORD_CLASS_IN
			public int recordClass;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
			public virtual void read(NetPacket packet)
			{
				recordName = packet.readDnsNameNotation();
				recordType = packet.read16();
				recordClass = packet.read16();
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
			public virtual NetPacket write(NetPacket packet)
			{
				packet.writeDnsNameNotation(recordName);
				packet.write16(recordType);
				packet.write16(recordClass);

				return packet;
			}

			public virtual int sizeOf()
			{
				if (string.ReferenceEquals(recordName, null) || recordName.Length == 0)
				{
					return 5;
				}
				return recordName.Length + 6;
			}

			public override string ToString()
			{
				return string.Format("recordName='{0}', recordType=0x{1:X}, recordClass=0x{2:X}", recordName, recordType, recordClass);
			}
		}

		public class DNSAnswerRecord : DNSRecord
		{
			// See format http://www.zytrax.com/books/dns/ch15/
			public int ttl;
			public int dataLength;
			public sbyte[] data;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(NetPacket packet) throws java.io.EOFException
			public override void read(NetPacket packet)
			{
				base.read(packet);
				ttl = packet.read32();
				dataLength = packet.read16();
				data = packet.readBytes(dataLength);
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public NetPacket write(NetPacket packet) throws java.io.EOFException
			public override NetPacket write(NetPacket packet)
			{
				base.write(packet);
				packet.write32(ttl);
				packet.write16(dataLength);
				packet.writeBytes(data, 0, dataLength);

				return packet;
			}

			public override int sizeOf()
			{
				return base.sizeOf() + 6 + dataLength;
			}

			public override string ToString()
			{
				return string.Format("{0}, ttl=0x{1:X}, dataLength=0x{2:X}, data={3}", base.ToString(), ttl, dataLength, Utilities.getMemoryDump(data, 0, dataLength));
			}
		}

		public DNS()
		{
		}

		public DNS(DNS dns)
		{
			identifier = dns.identifier;
			isResponseFlag = dns.isResponseFlag;
			opcode = dns.opcode;
			authoritativeAnswer = dns.authoritativeAnswer;
			truncationFlag = dns.truncationFlag;
			recursionDesired = dns.recursionDesired;
			recursionAvailable = dns.recursionAvailable;
			zero = dns.zero;
			responseCode = dns.responseCode;
			questionCount = dns.questionCount;
			answerRecordCount = dns.answerRecordCount;
			authorityRecordCount = dns.authorityRecordCount;
			additionalRecordCount = dns.additionalRecordCount;
			questions = dns.questions;
			answerRecords = dns.answerRecords;
			authorityRecords = dns.authorityRecords;
			additionalRecords = dns.additionalRecords;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private DNSRecord[] readRecords(NetPacket packet, int count) throws java.io.EOFException
		private DNSRecord[] readRecords(NetPacket packet, int count)
		{
			DNSRecord[] records = new DNSRecord[count];
			for (int i = 0; i < count; i++)
			{
				records[i] = new DNSRecord();
				records[i].read(packet);
			}

			return records;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private DNSAnswerRecord[] readAnswerRecords(NetPacket packet, int count) throws java.io.EOFException
		private DNSAnswerRecord[] readAnswerRecords(NetPacket packet, int count)
		{
			DNSAnswerRecord[] records = new DNSAnswerRecord[count];
			for (int i = 0; i < count; i++)
			{
				records[i] = new DNSAnswerRecord();
				records[i].read(packet);
			}

			return records;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
		public virtual void read(NetPacket packet)
		{
			identifier = packet.read16();
			isResponseFlag = packet.readBoolean();
			opcode = packet.readBits(4);
			authoritativeAnswer = packet.readBoolean();
			truncationFlag = packet.readBoolean();
			recursionDesired = packet.readBoolean();
			recursionAvailable = packet.readBoolean();
			zero = packet.readBits(3);
			responseCode = packet.readBits(4);
			questionCount = packet.read16();
			answerRecordCount = packet.read16();
			authorityRecordCount = packet.read16();
			additionalRecordCount = packet.read16();
			questions = readRecords(packet, questionCount);
			answerRecords = readAnswerRecords(packet, answerRecordCount);
			authorityRecords = readAnswerRecords(packet, authorityRecordCount);
			additionalRecords = readAnswerRecords(packet, additionalRecordCount);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeRecords(NetPacket packet, int recordsCount, DNSRecord records[]) throws java.io.EOFException
		private void writeRecords(NetPacket packet, int recordsCount, DNSRecord[] records)
		{
			for (int i = 0; i < recordsCount; i++)
			{
				records[i].write(packet);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
		public virtual NetPacket write(NetPacket packet)
		{
			packet.write16(identifier);
			packet.writeBoolean(isResponseFlag);
			packet.writeBits(opcode, 4);
			packet.writeBoolean(authoritativeAnswer);
			packet.writeBoolean(truncationFlag);
			packet.writeBoolean(recursionDesired);
			packet.writeBoolean(recursionAvailable);
			packet.writeBits(zero, 3);
			packet.writeBits(responseCode, 4);
			packet.write16(questionCount);
			packet.write16(answerRecordCount);
			packet.write16(authorityRecordCount);
			packet.write16(additionalRecordCount);
			writeRecords(packet, questionCount, questions);
			writeRecords(packet, answerRecordCount, answerRecords);
			writeRecords(packet, authorityRecordCount, authorityRecords);
			writeRecords(packet, additionalRecordCount, additionalRecords);

			return packet;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write() throws java.io.EOFException
		public virtual NetPacket write()
		{
			return write(new NetPacket(sizeOf()));
		}

		private int sizeOf(int recordsCount, DNSRecord[] records)
		{
			int size = 0;
			for (int i = 0; i < recordsCount; i++)
			{
				size += records[i].sizeOf();
			}

			return size;
		}

		public virtual int sizeOf()
		{
			int size = 12;
			size += sizeOf(questionCount, questions);
			size += sizeOf(answerRecordCount, answerRecords);
			size += sizeOf(authorityRecordCount, authorityRecords);
			size += sizeOf(additionalRecordCount, additionalRecords);

			return size;
		}

		private void ToString(StringBuilder s, string prefix, int recordsCount, DNSRecord[] records)
		{
			for (int i = 0; i < recordsCount; i++)
			{
				s.Append(string.Format(", {0}#{1:D}[{2}]", prefix, i, records[i]));
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: s.append(String.format("identifier=0x%04X, isResponseFlag=%b, opcode=0x%X, authoritativeAnswer=%b, truncationFlag=%b, recursionDesired=%b, recursionAvailable=%b, zero=0x%X, responseCode=0x%X, questionCount=%d, answerRecordCount=%d, authorityRecordCount=%d, additionalRecordCount=%d", identifier, isResponseFlag, opcode, authoritativeAnswer, truncationFlag, recursionDesired, recursionAvailable, zero, responseCode, questionCount, answerRecordCount, authorityRecordCount, additionalRecordCount));
			s.Append(string.Format("identifier=0x%04X, isResponseFlag=%b, opcode=0x%X, authoritativeAnswer=%b, truncationFlag=%b, recursionDesired=%b, recursionAvailable=%b, zero=0x%X, responseCode=0x%X, questionCount=%d, answerRecordCount=%d, authorityRecordCount=%d, additionalRecordCount=%d", identifier, isResponseFlag, opcode, authoritativeAnswer, truncationFlag, recursionDesired, recursionAvailable, zero, responseCode, questionCount, answerRecordCount, authorityRecordCount, additionalRecordCount));
			ToString(s, "question", questionCount, questions);
			ToString(s, "answerRecord", answerRecordCount, answerRecords);
			ToString(s, "authorityRecord", authorityRecordCount, authorityRecords);
			ToString(s, "additionalRecord", additionalRecordCount, additionalRecords);

			return s.ToString();
		}
	}

}