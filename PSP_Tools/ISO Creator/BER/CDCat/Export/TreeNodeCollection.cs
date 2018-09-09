using System;
using System.Collections.Generic;
using System.Text;

namespace BER.CDCat.Export {
	public class TreeNodeCollection : System.Collections.CollectionBase {
		public int Add( TreeNode node ) {
			return this.InnerList.Add( node );
		}

		public void AddRange( TreeNodeCollection collection ) {
			this.InnerList.AddRange( collection );
		}

		public void Remove( TreeNode node ) {
			this.InnerList.Remove( node );
		}

		public TreeNode this[int index] {
			get {
				return (TreeNode)this.InnerList[index];
			}
			set {
				this.InnerList[index] = value;
			}
		}

		public TreeNode[] ToArray() {
			return (TreeNode[])this.InnerList.ToArray( typeof( TreeNode ) );
		}
	}
}
