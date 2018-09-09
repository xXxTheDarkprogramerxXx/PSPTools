using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace BER.CDCat.Export {
	public class TreeNode {
		#region Fields

		private TreeNodeCollection m_dirCol = new TreeNodeCollection();
		private TreeNodeCollection m_fileCol = new TreeNodeCollection();

		private string m_name = "";
		private string m_shortName = "";
		private UInt32 m_length;
		private DateTime m_creationTime;
		private bool m_isDirectory;
		private string m_fullName = "";

		#endregion

		#region Constructors

		#endregion

		#region Properties

		public TreeNodeCollection Files {
			get {
				return m_fileCol;
			}
		}

		public TreeNodeCollection Directories {
			get {
				return m_dirCol;
			}
		}

		public string Name {
			get {
				return m_name;
			}
			set {
				m_name = value;
			}
		}

		public string ShortName {
			get {
				return m_shortName;
			}
			set {
				m_shortName = value;
			}
		}

		public UInt32 Length {
			get {
				return m_length;
			}
			set {
				m_length = value;
			}
		}

		public DateTime CreationTime {
			get {
				return m_creationTime;
			}
			set {
				m_creationTime = value;
			}
		}

		public bool IsDirectory {
			get {
				return m_isDirectory;
			}
			set {
				m_isDirectory = value;
			}
		}

		public string FullName {
			get {
				return m_fullName;
			}
			set {
				m_fullName = value;
			}
		}

		#endregion

		#region Useful Methods

		public TreeNode[] GetFiles() {
			return m_fileCol.ToArray();
		}

		public TreeNode[] GetDirectories() {
			return m_dirCol.ToArray();
		}

		public TreeNode[] GetAllChildren() {
			TreeNodeCollection result = new TreeNodeCollection();
			result.AddRange( m_dirCol );
			result.AddRange( m_fileCol );
			return result.ToArray();
		}

		/// <summary>
		/// Debug purposes: generates a rudimentary xml.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			string result = "<node name=\"" + this.Name + "\" dir=\"true\">";
			TreeNode[] dirs = this.GetDirectories();
			for ( int i=0; i<dirs.Length; i++ ) {
				result += dirs[i].ToString();
			}
			TreeNode[] files = this.GetFiles();
			for ( int i=0; i<files.Length; i++ ) {
				result += "<node name=\"" + files[i].Name + "\" dir=\"false\"/>";
			}
			result += "</node>";
			return result;
		}

		#endregion
	}
}
