using System;
using System.Collections.Generic;
using System.Text;

namespace BER.CDCat.Export {
	public class FinishEventArgs : System.EventArgs {
		private string m_message;

		public FinishEventArgs( string message ) {
			m_message = message;
		}

		public string Message {
			get {
				return m_message;
			}
			set {
				m_message = value;
			}
		}
	}

	public class ProgressEventArgs : System.EventArgs {
		private int m_current = -1;
		private int m_maximum = -1;
		private string m_action = null;

		public ProgressEventArgs( int current ) {
			m_current = current;
		}

		public ProgressEventArgs( int current, int maximum ) {
			m_current = current;
			m_maximum = maximum;
		}

		public ProgressEventArgs( string action, int current, int maximum ) {
			m_action = action;
			m_current = current;
			m_maximum = maximum;
		}

		public int Current {
			get {
				return m_current;
			}
			set {
				m_current = value;
			}
		}

		public int Maximum {
			get {
				return m_maximum;
			}
			set {
				m_maximum = value;
			}
		}

		public string Action {
			get {
				return m_action;
			}
			set {
				m_action = value;
			}
		}
	}

	public class AbortEventArgs : System.EventArgs {
		private string m_message;

		public AbortEventArgs( string message ) {
			m_message = message;
		}

		public string Message {
			get {
				return m_message;
			}
			set {
				m_message = value;
			}
		}
	}

	public delegate void FinishDelegate( object sender, FinishEventArgs e );
	public delegate void ProgressDelegate( object sender, ProgressEventArgs e );
	public delegate void AbortDelegate( object sender, AbortEventArgs e );

	public interface IExportPlugin {

		#region Events

		event ProgressDelegate Progress;
		event FinishDelegate Finished;
		event AbortDelegate Abort;

		#endregion

		#region Properties

		string ID {
			get;
		}

		string Description {
			get;
		}

		string Extension {
			get;
		}

		BER.CDCat.Export.TreeNode Volume {
			get;
			set;
		}

		string FileName {
			get;
			set;
		}

		#endregion

		#region Methods

		void DoExport( TreeNode volume, string volumeDescription );

		void DoExport();

		#endregion
	}
}
