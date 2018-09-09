using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace IsoCreator.DirectoryTree {
	/// <summary>
	/// This class represents a collection of folder elements (files and folders) which can be sorted by name.
	/// </summary>
	internal class FolderElementList : CollectionBase {

		#region Comparer

		public class DirEntryComparer : IComparer {
			#region IComparer Members

			public int Compare( object x, object y ) {
				string nameX = ( (IsoFolderElement)x ).LongName;
				string nameY = ( (IsoFolderElement)y ).LongName;

				return String.Compare( nameX, nameY, false );
			}

			#endregion
		}

		#endregion

		public void Add( IsoFolderElement value ) {
			this.InnerList.Add( value );
		}

		public void Sort() {
			DirEntryComparer dirEntryComparer = new DirEntryComparer();
			this.InnerList.Sort( dirEntryComparer );
		}

		public IsoFolderElement this[int index] {
			get {
				return (IsoFolderElement)this.InnerList[index];
			}
			set {
				this.InnerList[index] = value;
			}
		}
	}
}
