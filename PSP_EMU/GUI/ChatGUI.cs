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
namespace pspsharp.GUI
{


	using Modules = pspsharp.HLE.Modules;
	using sceUtility = pspsharp.HLE.modules.sceUtility;
	using INetworkAdapter = pspsharp.network.INetworkAdapter;

	using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// Network Chat User Interface
	/// </summary>
	public class ChatGUI : JFrame
	{

		private const long serialVersionUID = 5376146560681704272L;
		private Logger log = Emulator.log;
		private JScrollPane scrollPane;
		private JLabel chatMessagesLabel;
		private JTextField chatMessage;
		private JButton sendButton;
		private JLabel adhocIDLabel;
		private JLabel groupNameLabel;
		private JLabel membersLabel;
		private JLabel membersList;
		private IList<string> chatMessages = new LinkedList<string>();
		private IList<string> members = new LinkedList<string>();
		private const string chatMessageHeader = "<html>";
		private const string chatMessageFooter = "</html>";
		private const string membersHeader = "<html>";
		private const string membersFooter = "</html>";
		private Dictionary<string, Color> nickNameColors = new Dictionary<string, Color>();
		private int allColorsIndex = 0;
		// Always assign the GRAY color to me.
		private static readonly Color colorForMe = Color.GRAY;
		// The Nicknames will be assigned these colors (first come, first served)
		private static readonly Color[] allColors = new Color[]{Color.BLUE, Color.RED, Color.CYAN, Color.GREEN, Color.MAGENTA, Color.ORANGE, Color.PINK, Color.YELLOW, Color.BLACK};

		public ChatGUI()
		{
			initComponents();
			DefaultCloseOperation = DISPOSE_ON_CLOSE;
			nickNameColors[MyNickName] = colorForMe;
			updateMembersLabel();
		}

		private void initComponents()
		{
			scrollPane = new JScrollPane();
			chatMessagesLabel = new JLabel();
			chatMessage = new JTextField();
			sendButton = new JButton();
			adhocIDLabel = new JLabel();
			groupNameLabel = new JLabel();
			membersLabel = new JLabel();
			membersList = new JLabel();

			Title = "Chat";
			Resizable = true;

			sendButton.Text = "Send";
			sendButton.DefaultCapable = true;
			RootPane.DefaultButton = sendButton;
			sendButton.addActionListener(new ActionListenerAnonymousInnerClass(this));

			// Start displaying the chat message from the bottom
			chatMessagesLabel.VerticalAlignment = SwingConstants.BOTTOM;
			chatMessagesLabel.PreferredSize = new Dimension(500, 300);

			scrollPane.ViewportView = chatMessagesLabel;

			adhocIDLabel.Text = AdhocID;
			groupNameLabel.Text = GroupName;
			membersLabel.Text = "Members:";

			membersList.PreferredSize = new Dimension(100, chatMessagesLabel.PreferredSize.height);

			chatMessage.Editable = true;

			//
			// Layout:
			//
			// +-------------------------------------------+-----------------+
			// |                                           | adhocIDLabel    |
			// |                                           | groupNameLabel  |
			// | chatMessageLabel in scrollPane            | membersLabel    |
			// |                                           +-----------------+
			// |                                           | membersList     |
			// |                                           |                 |
			// |                                           |                 |
			// |                                           |                 |
			// |                                           |                 |
			// +-------------------------------------------+----+------------+
			// | chatMessage                                    | sendButton |
			// +------------------------------------------------+------------+
			//
			GroupLayout layout = new GroupLayout(ContentPane);
			ContentPane.Layout = layout;
			layout.HorizontalGroup = layout.createParallelGroup().addGroup(layout.createSequentialGroup().addComponent(scrollPane).addGroup(layout.createParallelGroup().addGroup(layout.createParallelGroup().addComponent(adhocIDLabel)).addGroup(layout.createParallelGroup().addComponent(groupNameLabel)).addComponent(membersLabel).addComponent(membersList))).addGroup(layout.createSequentialGroup().addComponent(chatMessage).addComponent(sendButton));
			layout.VerticalGroup = layout.createSequentialGroup().addGroup(layout.createParallelGroup().addComponent(scrollPane).addGroup(layout.createSequentialGroup().addGroup(layout.createParallelGroup().addComponent(adhocIDLabel)).addGroup(layout.createParallelGroup().addComponent(groupNameLabel)).addComponent(membersLabel).addComponent(membersList))).addGroup(layout.createParallelGroup().addComponent(chatMessage).addComponent(sendButton));
			pack();
		}

		private class ActionListenerAnonymousInnerClass : ActionListener
		{
			private readonly ChatGUI outerInstance;

			public ActionListenerAnonymousInnerClass(ChatGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void actionPerformed(ActionEvent arg0)
			{
				outerInstance.onSend();
			}
		}

		private void onSend()
		{
			string message = chatMessage.Text;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Sending chat message '{0}'", message));
			}

			// Send the chat message to the network adapter
			INetworkAdapter networkAdapter = Modules.sceNetModule.NetworkAdapter;
			if (networkAdapter != null)
			{
				networkAdapter.sendChatMessage(message);
				chatMessage.Text = "";

				// Add my own chat to the messages
				addChatMessage(MyNickName, message, true);
			}
		}

		private void addChatLine(string line)
		{
			chatMessages.Add(line);

			StringBuilder formattedText = new StringBuilder();
			formattedText.Append(chatMessageHeader);
			foreach (string chatMessage in chatMessages)
			{
				formattedText.Append(string.Format("<br>{0}</br>\n", chatMessage));
			}
			formattedText.Append(chatMessageFooter);

			chatMessagesLabel.Text = formattedText.ToString();
		}

		private Color NewColor
		{
			get
			{
				if (allColorsIndex >= allColors.Length)
				{
					allColorsIndex = 0;
				}
				return allColors[allColorsIndex++];
			}
		}

		private Color getNickNameColor(string nickName)
		{
			Color color = nickNameColors[nickName];
			if (color == null)
			{
				// No color yet assigned to the nickName, assign a new one
				color = NewColor;
				nickNameColors[nickName] = color;
			}

			return color;
		}

		public virtual void addChatMessage(string nickName, string message)
		{
			addChatMessage(nickName, message, false);
		}

		private string getFormattedNickName(string nickName, bool isMe)
		{
			Color nickNameColor = getNickNameColor(nickName);
			string nickNameSuffix = isMe ? " (me)" : "";
			return string.Format("<font color='#{0:X6}'>{1}{2}</font>", nickNameColor.RGB & 0x00FFFFFF, nickName, nickNameSuffix);
		}

		private void addChatMessage(string nickName, string message, bool isMe)
		{
			string line;

			if (string.ReferenceEquals(nickName, null))
			{
				line = message;
			}
			else
			{
				line = string.Format("{0} - {1}", getFormattedNickName(nickName, isMe), message);
			}

			addChatLine(line);
		}

		public override void dispose()
		{
			Emulator.MainGUI.endWindowDialog();
			base.dispose();
		}

		private void updateMembersLabel()
		{
			if (membersList == null)
			{
				return;
			}

			StringBuilder label = new StringBuilder();
			label.Append(membersHeader);

			// Always put myself in front of the list
			label.Append(string.Format("<br>{0}</br>", getFormattedNickName(MyNickName, true)));

			foreach (string member in members)
			{
				label.Append(string.Format("<br>{0}</br>", getFormattedNickName(member, false)));
			}
			label.Append(membersFooter);

			membersList.Text = label.ToString();
		}

		public virtual void updateMembers(IList<string> members)
		{
			this.members.Clear();
			((List<string>)this.members).AddRange(members);
			updateMembersLabel();
		}

		private static string MyNickName
		{
			get
			{
				return sceUtility.SystemParamNickname;
			}
		}
		private static string AdhocID
		{
			get
			{
				return Modules.sceNetAdhocctlModule.hleNetAdhocctlGetAdhocID();
			}
		}
		private static string GroupName
		{
			get
			{
				return Modules.sceNetAdhocctlModule.hleNetAdhocctlGetGroupName();
			}
		}
	}

}