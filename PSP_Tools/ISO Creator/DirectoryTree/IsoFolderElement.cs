using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BER.CDCat.Export;

namespace IsoCreator.DirectoryTree {

	/// <summary>
	/// Base class for all folder elements (files and subfolders).
	/// </summary>
	internal abstract class IsoFolderElement {

		#region Fields

		private DateTime m_date;

		private string m_shortIdent;	/* The shortIdent is used for the DOS short-ascii-name. I haven't given too much 
										 * effort into making it right. It isn't of much use these days.
										 */


		private string m_identifier;	// The original name. (unicode n stuff)

		#endregion

		#region Constructors

		public IsoFolderElement( FileSystemInfo folderElement, bool isRoot, string childNumber ) {
			m_date = folderElement.CreationTime;
			m_identifier = folderElement.Name;

			// If you need to use the short name, then you may want to change the naming method.
			if ( isRoot ) {
				m_shortIdent = ".";
				m_identifier = ".";
			} else {
				if ( m_identifier.Length > 8 ) {
					m_shortIdent = m_identifier.Substring( 0, 8 - childNumber.Length ).ToUpper().Replace( ' ', '_' ).Replace( '.', '_' );
					m_shortIdent += childNumber;
				} else {
					m_shortIdent = m_identifier.ToUpper().Replace( ' ', '_' ).Replace( '.', '_' );
				}
			}

			if ( m_identifier.Length > IsoAlgorithm.FileNameMaxLength ) {
				m_identifier = m_identifier.Substring( 0, IsoAlgorithm.FileNameMaxLength - childNumber.Length ) + childNumber;
			}

		}

		public IsoFolderElement( TreeNode folderElement, bool isRoot, string childNumber ) {
			m_date = folderElement.CreationTime;
			m_identifier = folderElement.Name;

			if ( isRoot ) {
				m_shortIdent = ".";
				m_identifier = ".";
			} else {
				if ( m_identifier.Length > 8 ) {
					m_shortIdent = m_identifier.Substring( 0, 8 - childNumber.Length ).ToUpper().Replace( ' ', '_' ).Replace( '.', '_' );
					m_shortIdent += childNumber;
				} else {
					m_shortIdent = m_identifier.ToUpper().Replace( ' ', '_' ).Replace( '.', '_' );
				}
			}

			if ( m_identifier.Length > IsoAlgorithm.FileNameMaxLength ) {
				m_identifier = m_identifier.Substring( 0, IsoAlgorithm.FileNameMaxLength - childNumber.Length ) + childNumber;
			}
		}

		#endregion

		#region Abstract properties

		public abstract UInt32 Extent1 {
			get;
			set;
		}

		public abstract UInt32 Extent2 {
			get;
			set;
		}

		public abstract UInt32 Size1 {
			get;
		}

		public abstract UInt32 Size2 {
			get;
		}

		public abstract bool IsDirectory {
			get;
		}

		public DateTime Date {
			get {
				return m_date;
			}
		}

		public string ShortName {
			get {
				return m_shortIdent;
			}
			set {
				m_shortIdent = value;
			}
		}

		public string LongName {
			get {
				return m_identifier;
			}
		}

		#endregion

		#region I/O Methods

		#endregion
	}
}
