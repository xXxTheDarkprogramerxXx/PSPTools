using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ISO9660;
using ISO9660.PrimitiveTypes;

namespace IsoCreator.IsoWrappers {
	internal class DateWrapper {
		#region Fields

		private BinaryDateRecord m_binaryDateRecord;
		private AsciiDateRecord m_asciiDateRecord;
		private DateTime m_date;

		#endregion

		#region Properties

		public BinaryDateRecord BinaryDateRecord {
			get {
				return m_binaryDateRecord;
			}
			set {
				m_binaryDateRecord = value;
			}
		}

		public AsciiDateRecord AsciiDateRecord {
			get {
				return m_asciiDateRecord;
			}
			set {
				m_asciiDateRecord = value;
			}
		}

		public DateTime Date {
			get {
				return m_date;
			}
			set {
				m_date = value;
				this.SetAsciiDateRecord( value );
				this.SetBinaryDateRecord( value );
			}
		}

		public sbyte TimeZone {
			get {
				return m_asciiDateRecord.TimeZone;
			}
			set {
				m_asciiDateRecord.TimeZone = value;
			}
		}

		#endregion

		#region Constructor(s)

		public DateWrapper( DateTime date ) {
			this.Date = date;
		}

		public DateWrapper( DateTime date, sbyte timeZone ) {
			m_date = date;
			this.SetAsciiDateRecord( date, timeZone );
			this.SetBinaryDateRecord( date );
		}

		public DateWrapper( BinaryDateRecord dateRecord ) {
			m_binaryDateRecord = dateRecord;
			this.SetAsciiDateRecord( 1900+dateRecord.Year, dateRecord.Month, dateRecord.DayOfMonth, dateRecord.Hour, dateRecord.Minute, dateRecord.Second, 0, 8 );
			m_date = new DateTime( 1900+dateRecord.Year, dateRecord.Month, dateRecord.DayOfMonth, dateRecord.Hour, dateRecord.Minute, dateRecord.Second );
		}

		public DateWrapper( AsciiDateRecord dateRecord ) {
			m_asciiDateRecord = dateRecord;

			byte year = (byte)( Convert.ToInt32( IsoAlgorithm.ByteArrayToString( dateRecord.Year ) ) - 1900 );
			byte month = Convert.ToByte( IsoAlgorithm.ByteArrayToString( dateRecord.Month ) );
			byte dayOfMonth = Convert.ToByte( IsoAlgorithm.ByteArrayToString( dateRecord.DayOfMonth ) );
			byte hour = Convert.ToByte( IsoAlgorithm.ByteArrayToString( dateRecord.Hour ) );
			byte minute = Convert.ToByte( IsoAlgorithm.ByteArrayToString( dateRecord.Minute ) );
			byte second = Convert.ToByte( IsoAlgorithm.ByteArrayToString( dateRecord.Second ) );
			int millisecond = Convert.ToInt32( IsoAlgorithm.ByteArrayToString( dateRecord.HundredthsOfSecond ) ) * 10;

			this.SetBinaryDateRecord( year, month, dayOfMonth, hour, minute, second );
			m_date = new DateTime( 1900+year, month, dayOfMonth, hour, minute, second, millisecond );
		}

		#endregion

		#region Set Methods

		private void SetBinaryDateRecord( byte year, byte month, byte dayOfMonth, byte hour, byte minute, byte second ) {
			if ( m_binaryDateRecord == null ) {
				m_binaryDateRecord = new BinaryDateRecord();
			}

			m_binaryDateRecord.Year = year;
			m_binaryDateRecord.Month = month;
			m_binaryDateRecord.DayOfMonth = dayOfMonth;
			m_binaryDateRecord.Hour = hour;
			m_binaryDateRecord.Minute = minute;
			m_binaryDateRecord.Second = second;
		}

		private void SetBinaryDateRecord( DateTime date ) {
			this.SetBinaryDateRecord(
				(byte)( date.Year - 1900 ),
				(byte)date.Month,
				(byte)date.Day,
				(byte)date.Hour,
				(byte)date.Minute,
				(byte)date.Second
			);
		}

		private void SetAsciiDateRecord( int year, int month, int dayOfMonth, int hour, int minute, int second, int hundredthsOfSecond, sbyte timeZone ) {
			if ( m_asciiDateRecord == null ) {
				m_asciiDateRecord = new AsciiDateRecord();
			}

			string sYear = String.Format( "{0:D4}", year%10000 );
			string sMonth = String.Format( "{0:D2}", month );
			string sDay = String.Format( "{0:D2}", dayOfMonth );
			string sHour = String.Format( "{0:D2}", hour );
			string sMinute = String.Format( "{0:D2}", minute );
			string sSecond = String.Format( "{0:D2}", second );
			string sHundredths = String.Format( "{0:D2}", hundredthsOfSecond );

			m_asciiDateRecord.Year = IsoAlgorithm.StringToByteArray( sYear );
			m_asciiDateRecord.Month = IsoAlgorithm.StringToByteArray( sMonth );
			m_asciiDateRecord.DayOfMonth = IsoAlgorithm.StringToByteArray( sDay );
			m_asciiDateRecord.Hour = IsoAlgorithm.StringToByteArray( sHour );
			m_asciiDateRecord.Minute = IsoAlgorithm.StringToByteArray( sMinute );
			m_asciiDateRecord.Second = IsoAlgorithm.StringToByteArray( sSecond );
			m_asciiDateRecord.HundredthsOfSecond = IsoAlgorithm.StringToByteArray( sHundredths );
			m_asciiDateRecord.TimeZone = timeZone;
		}

		private void SetAsciiDateRecord( DateTime date, sbyte timeZone ) {
			this.SetAsciiDateRecord( date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond/10, timeZone );
		}

		private void SetAsciiDateRecord( DateTime date ) {
			this.SetAsciiDateRecord( date, 8 );
		}

		public void ResetAsciiDateRecord() {
			m_date = new DateTime( 0, 0, 0, 0, 0, 0, 0 );
			this.SetAsciiDateRecord( m_date );
			this.SetBinaryDateRecord( m_date );
		}

		#endregion

		#region I/O Methods

		public int WriteBinaryDateRecord( BinaryWriter writer ) {
			if ( m_binaryDateRecord == null ) {
				return 0;
			}

			writer.Write( new byte[6] { 
					m_binaryDateRecord.Year, 
					m_binaryDateRecord.Month, 
					m_binaryDateRecord.DayOfMonth, 
					m_binaryDateRecord.Hour, 
					m_binaryDateRecord.Minute, 
					m_binaryDateRecord.Second 
				} );

			return 6;
		}

		public int WriteAsciiDateRecord( BinaryWriter writer ) {
			if ( m_asciiDateRecord == null ) {
				return 0;
			}

			writer.Write( m_asciiDateRecord.Year );
			writer.Write( m_asciiDateRecord.Month );
			writer.Write( m_asciiDateRecord.DayOfMonth );
			writer.Write( m_asciiDateRecord.Hour );
			writer.Write( m_asciiDateRecord.Minute );
			writer.Write( m_asciiDateRecord.Second );
			writer.Write( m_asciiDateRecord.HundredthsOfSecond );
			writer.Write( m_asciiDateRecord.TimeZone );

			return 17;
		}

		#endregion
	}
}
