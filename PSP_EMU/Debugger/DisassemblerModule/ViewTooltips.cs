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
namespace pspsharp.Debugger.DisassemblerModule
{
	/*
	 The contents of this file are subject to the terms of the Common Development
	 and Distribution License (the License). You may not use this file except in
	 compliance with the License. You can obtain a copy of the License at
	 http://www.netbeans.org/cddl.html or http://www.netbeans.org/cddl.txt. When
	 distributing Covered Code, include this CDDL Header Notice in each file and
	 include the License file at http://www.netbeans.org/cddl.txt. If applicable,
	 add the following below the CDDL Header, with the fields enclosed by brackets
	 [] replaced by your own identifying information: "Portions Copyrighted [year]
	 [name of copyright owner]" Copyright 2006 Sun Microsystems, all rights reserved.
	 */



	/// <summary>
	/// Displays pseudo-tooltips for tree and list views which don't have enough
	/// space.  This class is not NB specific, and can be used with any
	/// JTree or JList.
	/// 
	/// @author Tim Boudreau
	/// @modified Vimal
	/// </summary>
	public sealed class ViewTooltips : MouseAdapter, MouseMotionListener
	{
		/// <summary>
		/// The default instance, reference counted </summary>
		private static ViewTooltips INSTANCE = null;
		/// <summary>
		/// A reference count for number of comps listened to </summary>
		private int refcount = 0;
		/// <summary>
		/// The last known component we were invoked against, nulled on hide() </summary>
		private JComponent inner = null;
		/// <summary>
		/// The last row we were invoked against </summary>
		private int row = -1;
		/// <summary>
		/// An array of currently visible popups </summary>
		private Popup[] popups = new Popup[2];
		/// <summary>
		/// A component we'll reuse to paint into the popups </summary>
		private ImgComp painter = new ImgComp();
		/// <summary>
		/// Nobody should instantiate this </summary>
		private ViewTooltips()
		{
		}

		/// <summary>
		/// Register a child of a JScrollPane (only JList or JTree supported
		/// for now) which should show helper tooltips.  Should be called
		/// from the component's addNotify() method.
		/// </summary>
		public static void register(JComponent comp)
		{
			if (INSTANCE == null)
			{
				INSTANCE = new ViewTooltips();
			}
			INSTANCE.attachTo(comp);
		}

		/// <summary>
		/// Unregister a child of a JScrollPane (only JList or JTree supported
		/// for now) which should show helper tooltips. Should be called
		/// from the component's removeNotify() method.
		/// </summary>
		public static void unregister(JComponent comp)
		{
			System.Diagnostics.Debug.Assert(INSTANCE != null, "Unregister asymmetrically called");
			if (null != INSTANCE && INSTANCE.detachFrom(comp) == 0)
			{
				INSTANCE.hide();
				INSTANCE = null;
			}
		}

		/// <summary>
		/// Start listening to mouse motion on the passed component </summary>
		private void attachTo(JComponent comp)
		{
			System.Diagnostics.Debug.Assert(comp is JTree || comp is JList);
			comp.addMouseListener(this);
			comp.addMouseMotionListener(this);
			refcount++;
		}

		/// <summary>
		/// Stop listening to mouse motion on the passed component </summary>
		private int detachFrom(JComponent comp)
		{
			System.Diagnostics.Debug.Assert(comp is JTree || comp is JList);
			comp.removeMouseMotionListener(this);
			comp.removeMouseListener(this);
			return refcount--;
		}

		public override void mouseMoved(MouseEvent e)
		{
			Point p = e.Point;
			JComponent comp = (JComponent) e.Source;
			JScrollPane jsp = (JScrollPane) SwingUtilities.getAncestorOfClass(typeof(JScrollPane), comp);
			if (jsp != null)
			{
				p = SwingUtilities.convertPoint(comp, p, jsp);
				show(jsp, p);
			}
		}

		public override void mouseDragged(MouseEvent e)
		{
			hide();
		}

		public override void mouseEntered(MouseEvent e)
		{
			hide();
		}

		public override void mouseExited(MouseEvent e)
		{
			hide();
		}

		/// <summary>
		/// Shows the appropriate popups given the state of the scroll pane and
		/// its view. </summary>
		/// <param name="view"> The scroll pane owning the component the event happened on </param>
		/// <param name="pt"> The point at which the mouse event happened, in the coordinate
		///  space of the scroll pane. </param>
		internal void show(JScrollPane view, Point pt)
		{
			if (view.Viewport.View is JTree)
			{
				showJTree(view, pt);
			}
			else if (view.Viewport.View is JList)
			{
				showJList(view, pt);
			}
			else
			{
				System.Diagnostics.Debug.Assert(false, "Bad component type registered: " + view.Viewport.View);
			}
		}

		private void showJList(JScrollPane view, Point pt)
		{
			JList list = (JList) view.Viewport.View;
			Point p = SwingUtilities.convertPoint(view, pt.x, pt.y, list);
			int row = list.locationToIndex(p);
			if (row == -1)
			{
				hide();
				return;
			}
			Rectangle bds = list.getCellBounds(row, row);
			//GetCellBounds returns a width that is the
			//full component width;  we want only what
			//the renderer really needs.
			ListCellRenderer ren = list.CellRenderer;
			Dimension rendererSize = ren.getListCellRendererComponent(list, list.Model.getElementAt(row), row, false, false).PreferredSize;

			// fix for possible npe spotted by SCO
			// http://pspsharp.org/forum/viewtopic.php?p=3387#p3387
			if (bds == null)
			{
				hide();
				return;
			}

			bds.width = rendererSize.width;

			if (!bds.contains(p))
			{
				hide();
				return;
			}

			//bds.width = rendererSize.width;
			//if (bds == null || !bds.contains(p)) {
			//    hide();
			//    return;
			//}
			// end "fix for possible npe spotted by SCO"

			if (setCompAndRow(list, row))
			{
				Rectangle visible = getShowingRect(view);
				Rectangle[] rects = getRects(bds, visible);
				if (rects.Length > 0)
				{
					ensureOldPopupsHidden();
					painter.configure(list.Model.getElementAt(row), view, list, row);
					showPopups(rects, bds, visible, list, view);
				}
				else
				{
					hide();
				}
			}
		}

		private void showJTree(JScrollPane view, Point pt)
		{
			JTree tree = (JTree) view.Viewport.View;
			Point p = SwingUtilities.convertPoint(view, pt.x, pt.y, tree);
			int row = tree.getClosestRowForLocation(p.x, p.y);
			TreePath path = tree.getClosestPathForLocation(p.x, p.y);
			Rectangle bds = tree.getPathBounds(path);
			if (bds == null || !bds.contains(p))
			{
				hide();
				return;
			}
			if (setCompAndRow(tree, row))
			{
				Rectangle visible = getShowingRect(view);
				Rectangle[] rects = getRects(bds, visible);
				if (rects.Length > 0)
				{
					ensureOldPopupsHidden();
					painter.configure(path.LastPathComponent, view, tree, path, row);
					showPopups(rects, bds, visible, tree, view);
				}
				else
				{
					hide();
				}
			}
		}

		/// <summary>
		/// Set the currently shown component and row, returning true if they are
		/// not the same as the last known values.
		/// </summary>
		private bool setCompAndRow(JComponent inner, int row)
		{
			bool rowChanged = row != this.row;
			bool compChanged = inner != this.inner;
			this.inner = inner;
			this.row = row;
			return (rowChanged || compChanged);
		}

		/// <summary>
		/// Hide all popups and discard any references to the components the
		/// popups were showing for.
		/// </summary>
		internal void hide()
		{
			ensureOldPopupsHidden();
			if (painter != null)
			{
				painter.clear();
			}
			setHideComponent(null, null);
			inner = null;
			row = -1;
		}

		private void ensureOldPopupsHidden()
		{
			for (int i = 0; i < popups.Length; i++)
			{
				if (popups[i] != null)
				{
					popups[i].hide();
					popups[i] = null;
				}
			}
		}

		/// <summary>
		/// Gets the sub-rectangle of a JScrollPane's area that
		/// is actually showing the view
		/// </summary>
		private Rectangle getShowingRect(JScrollPane pane)
		{
			Insets ins1 = pane.Viewport.Insets;
			Border inner = pane.ViewportBorder;
			Insets ins2;
			if (inner != null)
			{
				ins2 = inner.getBorderInsets(pane);
			}
			else
			{
				ins2 = new Insets(0,0,0,0);
			}
			Insets ins3 = new Insets(0,0,0,0);
			if (pane.Border != null)
			{
				ins3 = pane.Border.getBorderInsets(pane);
			}

			Rectangle r = pane.ViewportBorderBounds;
			r.translate(-r.x, -r.y);
			r.width -= ins1.left + ins1.right;
			r.width -= ins2.left + ins2.right;
			r.height -= ins1.top + ins1.bottom;
			r.height -= ins2.top + ins2.bottom;
			r.x -= ins2.left;
			r.x -= ins3.left;
			Point p = pane.Viewport.ViewPosition;
			r.translate(p.x, p.y);
			r = SwingUtilities.convertRectangle(pane.Viewport, r, pane);
			return r;
		}

		/// <summary>
		/// Fetches an array or rectangles representing the non-overlapping
		/// portions of a cell rect against the visible portion of the component.
		/// @bds The cell's bounds, in the coordinate space of the tree or list
		/// @vis The visible area of the tree or list, in the tree or list's coordinate space
		/// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static final java.awt.Rectangle[] getRects(final java.awt.Rectangle bds, final java.awt.Rectangle vis)
		private static Rectangle[] getRects(Rectangle bds, Rectangle vis)
		{
			Rectangle[] result;
			if (vis.contains(bds))
			{
				result = new Rectangle[0];
			}
			else
			{
				if (bds.x < vis.x && bds.x + bds.width > vis.x + vis.width)
				{
					Rectangle a = new Rectangle(bds.x, bds.y, vis.x - bds.x, bds.height);
					Rectangle b = new Rectangle(vis.x + vis.width, bds.y, (bds.x + bds.width) - (vis.x + vis.width), bds.height);
					result = new Rectangle[] {a, b};
				}
				else if (bds.x < vis.x)
				{
					result = new Rectangle[] {new Rectangle(bds.x, bds.y, vis.x - bds.x, bds.height)};
				}
				else if (bds.x + bds.width > vis.x + vis.width)
				{
					result = new Rectangle[] {new Rectangle(vis.x + vis.width, bds.y, (bds.x + bds.width) - (vis.x + vis.width), bds.height)};
				}
				else
				{
					result = new Rectangle[0];
				}
			}
			for (int i = 0; i < result.Length; i++)
			{
			}
			return result;
		}

		/// <summary>
		/// Show popups for each rectangle, using the now configured painter.
		/// </summary>
		private void showPopups(Rectangle[] rects, Rectangle bds, Rectangle visible, JComponent comp, JScrollPane view)
		{
			bool shown = false;
			for (int i = 0; i < rects.Length; i++)
			{
				Rectangle sect = rects[i];
				sect.translate(-bds.x, -bds.y);
				ImgComp part = painter.getPartial(sect, bds.x + rects[i].x < visible.x);
				Point pos = new Point(bds.x + rects[i].x, bds.y + rects[i].y);
				SwingUtilities.convertPointToScreen(pos, comp);
				if (comp is JList)
				{
					//XXX off by one somewhere, only with JLists - where?
					pos.y--;
				}
				if (pos.x > 0)
				{ //Mac OS will reposition off-screen popups to x=0,
					//so don't try to show them
					popups[i] = PopupFactory.getPopup(view, part, pos.x, pos.y);
					popups[i].show();
					shown = true;
				}
			}
			if (shown)
			{
				setHideComponent(comp, view);
			}
			else
			{
				setHideComponent(null, null); //clear references
			}
		}

		private static PopupFactory PopupFactory
		{
			get
			{
		//        if ((Utilities.getOperatingSystem() & Utilities.OS_MAC) != 0 ) {
		//
		//            // See ide/applemenu/src/org/netbeans/modules/applemenu/ApplePopupFactory
		//            // We have a custom PopupFactory that will consistently use
		//            // lightweight popups on Mac OS, since HW popups get a drop
		//            // shadow.  By default, popups returned when a heavyweight popup
		//            // is needed (SDI mode) are no-op popups, since some hacks
		//            // are necessary to make it really work.
		//
		//            // To enable heavyweight popups which have no drop shadow
		//            // *most* of the time on mac os, run with
		//            // -J-Dnb.explorer.hw.completions=true
		//
		//            // To enable heavyweight popups which have no drop shadow
		//            // *ever* on mac os, you need to put the cocoa classes on the
		//            // classpath - modify netbeans.conf to add
		//            // System/Library/Java on the bootclasspath.  *Then*
		//            // run with the above line switch and
		//            // -J-Dnb.explorer.hw.cocoahack=true
		//
		//            PopupFactory result = (PopupFactory) Lookup.getDefault().lookup (
		//                    PopupFactory.class);
		//            return result == null ? PopupFactory.getSharedInstance() : result;
		//        } else {
					return PopupFactory.SharedInstance;
		//        }
			}
		}

		private Hider hider = null;
		/// <summary>
		/// Set a component (JList or JTree) which should be listened to, such that if
		/// a model, selection or scroll event occurs, all currently open popups
		/// should be hidden.
		/// </summary>
		private void setHideComponent(JComponent comp, JScrollPane parent)
		{
			if (hider != null)
			{
				if (hider.isListeningTo(comp))
				{
					return;
				}
			}
			if (hider != null)
			{
				hider.detach();
			}
			if (comp != null)
			{
				hider = new Hider(comp, parent);
			}
			else
			{
				hider = null;
			}
		}

		/// <summary>
		/// A JComponent which creates a BufferedImage of a cell renderer and can
		/// produce clones of itself that display subrectangles of that cell
		/// renderer.
		/// </summary>
		private sealed class ImgComp : JComponent
		{
			internal const long serialVersionUID = 7193267359698796218L;
			internal BufferedImage img;
			internal Dimension d = null;

			internal Color bg = Color.WHITE;
			internal JScrollPane comp = null;

			internal object node = null;

			internal AffineTransform at = AffineTransform.getTranslateInstance(0d, 0d);
			internal bool isRight = false;

			internal ImgComp()
			{
			}

			/// <summary>
			/// Create a clone with a specified backing image
			/// </summary>
			internal ImgComp(BufferedImage img, Rectangle off, bool right)
			{
				this.img = img;
				at = AffineTransform.getTranslateInstance(-off.x, 0);
				d = new Dimension(off.width, off.height);
				isRight = right;
			}

			public ImgComp getPartial(Rectangle bds, bool right)
			{
				System.Diagnostics.Debug.Assert(img != null);
				return new ImgComp(img, bds, right);
			}

			/// <summary>
			/// Configures a tree cell renderer and sets up sizing and the
			/// backing image from it 
			/// </summary>
			public bool configure(object nd, JScrollPane tv, JTree tree, TreePath path, int row)
			{
				LastRendereredObject = nd;
				LastRenderedScrollPane = tv;
				Component renderer = null;
				bg = tree.Background;
				bool sel = tree.SelectionEmpty ? false : tree.SelectionModel.isPathSelected(path);
				bool exp = tree.isExpanded(path);
				bool leaf = !exp && tree.Model.isLeaf(nd);
				bool lead = path.Equals(tree.SelectionModel.LeadSelectionPath);
				renderer = tree.CellRenderer.getTreeCellRendererComponent(tree, nd, sel, exp, leaf, row, lead);
				if (renderer != null)
				{
					Component = renderer;
				}
				return true;
			}

			/// <summary>
			/// Configures a list cell renderer and sets up sizing and the
			/// backing image from it 
			/// </summary>
			public bool configure(object nd, JScrollPane tv, JList list, int row)
			{
				LastRendereredObject = nd;
				LastRenderedScrollPane = tv;
				Component renderer = null;
				bg = list.Background;
				bool sel = list.SelectionEmpty ? false : list.SelectionModel.isSelectedIndex(row);
				renderer = list.CellRenderer.getListCellRendererComponent(list, nd, row, sel, false);
				if (renderer != null)
				{
					Component = renderer;
				}
				return true;
			}

			internal bool setLastRenderedScrollPane(JScrollPane comp)
			{
				bool result = comp != this.comp;
				this.comp = comp;
				return result;
			}

			internal bool setLastRendereredObject(object nd)
			{
				bool result = node != nd;
				if (result)
				{
					node = nd;
				}
				return result;
			}

			internal void clear()
			{
				comp = null;
				node = null;
			}

			/// <summary>
			/// Set the cell renderer we will proxy.
			/// </summary>
			public Component Component
			{
				set
				{
					Dimension d = value.PreferredSize;
					BufferedImage nue = new BufferedImage(d.width, d.height + 2, BufferedImage.TYPE_INT_ARGB_PRE);
					SwingUtilities.paintComponent(nue.Graphics, value, this, 0, 0, d.width, d.height + 2);
					Image = nue;
				}
			}

			public override Rectangle Bounds
			{
				get
				{
					Dimension dd = PreferredSize;
					return new Rectangle(0, 0, dd.width, dd.height);
				}
			}

			internal BufferedImage Image
			{
				set
				{
					this.img = value;
					d = null;
				}
			}

			public override Dimension PreferredSize
			{
				get
				{
					if (d == null)
					{
						d = new Dimension(img.Width, img.Height);
					}
					return d;
				}
			}

			public override Dimension Size
			{
				get
				{
					return PreferredSize;
				}
			}

			public override void paint(Graphics g)
			{
				g.Color = bg;
				g.fillRect(0, 0, d.width, d.height);
				Graphics2D g2d = (Graphics2D) g;
				g2d.drawRenderedImage(img, at);
				g.Color = Color.GRAY;
				g.drawLine(0, 0, d.width, 0);
				g.drawLine(0, d.height - 1, d.width, d.height - 1);
				if (isRight)
				{
					g.drawLine(0, 0, 0, d.height - 1);
				}
				else
				{
					g.drawLine(d.width - 1, 0, d.width - 1, d.height - 1);
				}
			}

			public override void firePropertyChange(string s, object a, object b)
			{
			}
			public override void invalidate()
			{
			}
			public override void validate()
			{
			}
			public override void revalidate()
			{
			}
		}

		/// <summary>
		/// A listener that listens to just about everything in the known universe
		/// and hides all currently displayed popups if anything happens.
		/// </summary>
		private sealed class Hider : ChangeListener, PropertyChangeListener, TreeModelListener, TreeSelectionListener, HierarchyListener, HierarchyBoundsListener, ListSelectionListener, ListDataListener, ComponentListener
		{
			internal readonly JTree tree;

			internal JScrollPane pane;
			internal readonly JList list;

			public Hider(JComponent comp, JScrollPane pane)
			{
				if (comp is JTree)
				{
					tree = (JTree) comp;
					list = null;
				}
				else
				{
					list = (JList) comp;
					tree = null;
				}
				System.Diagnostics.Debug.Assert(tree != null || list != null);
				this.pane = pane;
				attach();
			}

			internal bool isListeningTo(JComponent comp)
			{
				return !detached && (comp == list || comp == tree);
			}

			internal void attach()
			{
				if (tree != null)
				{
					tree.Model.addTreeModelListener(this);
					tree.SelectionModel.addTreeSelectionListener(this);
					tree.addHierarchyBoundsListener(this);
					tree.addHierarchyListener(this);
					tree.addComponentListener(this);
				}
				else
				{
					list.SelectionModel.addListSelectionListener(this);
					list.Model.addListDataListener(this);
					list.addHierarchyBoundsListener(this);
					list.addHierarchyListener(this);
					list.addComponentListener(this);
				}
				if (null != pane.HorizontalScrollBar)
				{
					pane.HorizontalScrollBar.Model.addChangeListener(this);
				}
				if (null != pane.VerticalScrollBar)
				{
					pane.VerticalScrollBar.Model.addChangeListener(this);
				}
				KeyboardFocusManager.CurrentKeyboardFocusManager.addPropertyChangeListener(this);
			}

			internal bool detached = false;
			internal void detach()
			{
				KeyboardFocusManager.CurrentKeyboardFocusManager.removePropertyChangeListener(this);
				if (tree != null)
				{
					tree.SelectionModel.removeTreeSelectionListener(this);
					tree.Model.removeTreeModelListener(this);
					tree.removeHierarchyBoundsListener(this);
					tree.removeHierarchyListener(this);
					tree.removeComponentListener(this);
				}
				else
				{
					list.SelectionModel.removeListSelectionListener(this);
					list.Model.removeListDataListener(this);
					list.removeHierarchyBoundsListener(this);
					list.removeHierarchyListener(this);
					list.removeComponentListener(this);
				}
				if (null != pane.HorizontalScrollBar)
				{
					pane.HorizontalScrollBar.Model.removeChangeListener(this);
				}
				if (null != pane.VerticalScrollBar)
				{
					pane.VerticalScrollBar.Model.removeChangeListener(this);
				}
				detached = true;
			}

			internal void change()
			{
				if (ViewTooltips.INSTANCE != null)
				{
					ViewTooltips.INSTANCE.hide();
				}
				detach();
			}

			public override void propertyChange(PropertyChangeEvent evt)
			{
				change();
			}
			public override void treeNodesChanged(TreeModelEvent e)
			{
				change();
			}

			public override void treeNodesInserted(TreeModelEvent e)
			{
				change();
			}

			public override void treeNodesRemoved(TreeModelEvent e)
			{
				change();
			}

			public override void treeStructureChanged(TreeModelEvent e)
			{
				change();
			}

			public override void hierarchyChanged(HierarchyEvent e)
			{
				change();
			}

			public override void valueChanged(TreeSelectionEvent e)
			{
				change();
			}

			public override void ancestorMoved(HierarchyEvent e)
			{
				change();
			}

			public override void ancestorResized(HierarchyEvent e)
			{
				change();
			}

			public override void stateChanged(ChangeEvent e)
			{
				change();
			}

			public override void valueChanged(ListSelectionEvent e)
			{
				change();
			}

			public override void intervalAdded(ListDataEvent e)
			{
				change();
			}

			public override void intervalRemoved(ListDataEvent e)
			{
				change();
			}

			public override void contentsChanged(ListDataEvent e)
			{
				change();
			}

			public override void componentResized(ComponentEvent e)
			{
				change();
			}

			public override void componentMoved(ComponentEvent e)
			{
				change();
			}

			public override void componentShown(ComponentEvent e)
			{
				change();
			}

			public override void componentHidden(ComponentEvent e)
			{
				change();
			}
		}
	}
}