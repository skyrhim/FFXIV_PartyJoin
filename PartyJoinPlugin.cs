using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace PartyJoin
{
	public sealed class PartyJoinPlugin : UserControl, IActPluginV1
	{
		private const ushort PartyJoinOpcode = 744;
		private const int PacketHeaderLength = 32;
		private const int MaxLoggedPayloadLength = 96;
		private const int MaxQueuedLogLines = 2000;
		private static readonly HttpClient HttpClient = new HttpClient();

		private readonly ConcurrentQueue<string> _packetLogQueue = new ConcurrentQueue<string>();
		private readonly TabControl _tabs = new TabControl();
		private readonly CheckBox _notifyEnabled = new CheckBox();
		private readonly TextBox _webhookUrl = new TextBox();
		private readonly TextBox _userIds = new TextBox();
		private readonly CheckBox _packetLoggingEnabled = new CheckBox();
		private readonly RichTextBox _packetLog = new RichTextBox();
		private readonly Label _status = new Label();
		private readonly Timer _logTimer = new Timer { Interval = 150 };

		private FFXIV_ACT_Plugin.FFXIV_ACT_Plugin _ffxivPlugin;
		private Label _actStatus;
		private bool _loadingSettings;
		private volatile bool _isPacketLoggingEnabled;

		public static string PluginName => "PartyJoin";

		public PartyJoinPlugin()
		{
			BuildUi();
			_logTimer.Tick += FlushPacketLog;
			_logTimer.Start();
		}

		public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
		{
			_actStatus = pluginStatusText;
			pluginScreenSpace.Text = PluginName;
			Dock = DockStyle.Fill;
			pluginScreenSpace.Controls.Add(this);

			string configPath = Path.Combine(
				ActGlobals.oFormActMain.AppDataFolder.FullName,
				"Config",
				"PartyJoin.config");
			PartyJoinConfig.Load(configPath);
			LoadSettings();

			var ffxivPluginData = ActGlobals.oFormActMain.ActPlugins.FirstOrDefault(plugin =>
				plugin.pluginFile.Name.StartsWith("FFXIV_ACT_Plugin", StringComparison.OrdinalIgnoreCase) &&
				plugin.lblPluginStatus.Text.StartsWith("FFXIV", StringComparison.OrdinalIgnoreCase));

			if (ffxivPluginData == null)
			{
				SetStatus("FFXIV ACT Plugin is not running.");
				return;
			}

			_ffxivPlugin = (FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)ffxivPluginData.pluginObj;
			_ffxivPlugin.DataSubscription.NetworkReceived += NetworkReceived;
			pluginStatusText.Text = "PartyJoin Started.";
			SetStatus("Connected to FFXIV ACT Plugin.");
		}

		public void DeInitPlugin()
		{
			if (_ffxivPlugin != null)
				_ffxivPlugin.DataSubscription.NetworkReceived -= NetworkReceived;

			_logTimer.Stop();
			PartyJoinConfig.Save();
			if (_actStatus != null)
				_actStatus.Text = "Closed";
		}

		private void BuildUi()
		{
			_tabs.Dock = DockStyle.Fill;
			var partyTab = new TabPage("Party notification");
			var packetTab = new TabPage("Packet log");
			_tabs.TabPages.Add(partyTab);
			_tabs.TabPages.Add(packetTab);
			Controls.Add(_tabs);

			var settings = new TableLayoutPanel
			{
				ColumnCount = 2,
				Dock = DockStyle.Top,
				AutoSize = true,
				Padding = new Padding(12),
			};
			settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
			settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			partyTab.Controls.Add(settings);

			_notifyEnabled.Text = "Notify when someone joins";
			_notifyEnabled.AutoSize = true;
			_notifyEnabled.CheckedChanged += SettingsChanged;
			settings.Controls.Add(_notifyEnabled, 0, 0);
			settings.SetColumnSpan(_notifyEnabled, 2);

			settings.Controls.Add(new Label { Text = "Discord webhook URL", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
			_webhookUrl.Dock = DockStyle.Fill;
			_webhookUrl.Leave += SettingsChanged;
			settings.Controls.Add(_webhookUrl, 1, 1);

			settings.Controls.Add(new Label { Text = "Discord user IDs", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
			_userIds.Dock = DockStyle.Fill;
			_userIds.Leave += SettingsChanged;
			settings.Controls.Add(_userIds, 1, 2);

			var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
			var saveButton = new Button { Text = "Save settings", AutoSize = true };
			saveButton.Click += (sender, args) => SaveSettings();
			actions.Controls.Add(saveButton);
			var testButton = new Button { Text = "Send test", AutoSize = true };
			testButton.Click += async (sender, args) =>
			{
				SaveSettings();
				await NotifyPartyJoinAsync("PartyJoin test notification");
			};
			actions.Controls.Add(testButton);
			settings.Controls.Add(actions, 1, 3);

			_status.AutoSize = true;
			_status.Padding = new Padding(12, 8, 12, 8);
			_status.Dock = DockStyle.Bottom;
			partyTab.Controls.Add(_status);

			var packetLayout = new TableLayoutPanel
			{
				ColumnCount = 1,
				RowCount = 2,
				Dock = DockStyle.Fill,
				Padding = new Padding(8),
			};
			packetLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			packetLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			packetTab.Controls.Add(packetLayout);

			var packetActions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
			_packetLoggingEnabled.Text = "Log incoming packets";
			_packetLoggingEnabled.AutoSize = true;
			_packetLoggingEnabled.CheckedChanged += (sender, args) =>
			{
				_isPacketLoggingEnabled = _packetLoggingEnabled.Checked;
				PartyJoinConfig.LogPackets = _isPacketLoggingEnabled;
				PartyJoinConfig.Save();
			};
			packetActions.Controls.Add(_packetLoggingEnabled);
			var clearButton = new Button { Text = "Clear", AutoSize = true };
			clearButton.Click += (sender, args) => _packetLog.Clear();
			packetActions.Controls.Add(clearButton);
			packetLayout.Controls.Add(packetActions, 0, 0);

			_packetLog.Dock = DockStyle.Fill;
			_packetLog.ReadOnly = true;
			_packetLog.WordWrap = false;
			_packetLog.Font = new Font(FontFamily.GenericMonospace, 9.0f);
			packetLayout.Controls.Add(_packetLog, 0, 1);
		}

		private void LoadSettings()
		{
			_loadingSettings = true;
			_notifyEnabled.Checked = PartyJoinConfig.NotifyOnPartyJoin;
			_webhookUrl.Text = PartyJoinConfig.DiscordWebhookUrl;
			_userIds.Text = PartyJoinConfig.DiscordUserIds;
			_packetLoggingEnabled.Checked = PartyJoinConfig.LogPackets;
			_isPacketLoggingEnabled = PartyJoinConfig.LogPackets;
			_loadingSettings = false;
		}

		private void SettingsChanged(object sender, EventArgs e)
		{
			if (!_loadingSettings)
				SaveSettings();
		}

		private void SaveSettings()
		{
			PartyJoinConfig.NotifyOnPartyJoin = _notifyEnabled.Checked;
			PartyJoinConfig.DiscordWebhookUrl = _webhookUrl.Text.Trim();
			PartyJoinConfig.DiscordUserIds = _userIds.Text.Trim();
			PartyJoinConfig.Save();
		}

		private void NetworkReceived(string connection, long epoch, byte[] message)
		{
			if (message == null || message.Length < PacketHeaderLength)
				return;

			ushort opcode = BitConverter.ToUInt16(message, 18);
			if (_isPacketLoggingEnabled)
				QueuePacketLog(connection, opcode, message);

			if (opcode != PartyJoinOpcode)
				return;

			const int nicknameOffset = 34;
			string nickname = string.Empty;
			if (message.Length > PacketHeaderLength + nicknameOffset)
			{
				int start = PacketHeaderLength + nicknameOffset;
				int end = Array.IndexOf(message, (byte)0, start);
				if (end < 0)
					end = message.Length;
				nickname = Encoding.UTF8.GetString(message, start, end - start);
			}

			QueueLogLine($"[{DateTime.Now:HH:mm:ss}] Party join opcode {opcode}: {nickname}");
			if (PartyJoinConfig.NotifyOnPartyJoin)
				_ = NotifyPartyJoinAsync($"Party join: {nickname}");
		}

		private void QueuePacketLog(string connection, ushort opcode, byte[] message)
		{
			int payloadLength = message.Length - PacketHeaderLength;
			int loggedLength = Math.Min(payloadLength, MaxLoggedPayloadLength);
			var payload = new StringBuilder(loggedLength * 3);
			for (int index = 0; index < loggedLength; index++)
				payload.Append(message[PacketHeaderLength + index].ToString("X2")).Append(' ');

			string suffix = loggedLength < payloadLength ? " ..." : string.Empty;
			QueueLogLine($"[{DateTime.Now:HH:mm:ss.fff}] {connection} opcode={opcode} bytes={payloadLength}: {payload}{suffix}");
		}

		private void QueueLogLine(string line)
		{
			_packetLogQueue.Enqueue(line);
			while (_packetLogQueue.Count > MaxQueuedLogLines)
				_packetLogQueue.TryDequeue(out _);
		}

		private void FlushPacketLog(object sender, EventArgs e)
		{
			if (_packetLog.IsDisposed || _packetLogQueue.IsEmpty)
				return;

			var batch = new StringBuilder();
			while (batch.Length < 16384 && _packetLogQueue.TryDequeue(out string line))
				batch.AppendLine(line);

			_packetLog.AppendText(batch.ToString());
			if (_packetLog.TextLength > 500000)
			{
				_packetLog.Select(0, _packetLog.TextLength - 250000);
				_packetLog.SelectedText = string.Empty;
			}
			_packetLog.SelectionStart = _packetLog.TextLength;
			_packetLog.ScrollToCaret();
		}

		private async Task NotifyPartyJoinAsync(string message)
		{
			string webhookUrl = PartyJoinConfig.DiscordWebhookUrl.Trim();
			if (string.IsNullOrEmpty(webhookUrl))
			{
				SetStatus("Discord webhook URL is empty.");
				return;
			}

			if (!webhookUrl.StartsWith("https://discord.com/api/webhooks/", StringComparison.OrdinalIgnoreCase))
			{
				SetStatus("Discord webhook URL is invalid.");
				return;
			}

			var mentions = PartyJoinConfig.DiscordUserIds
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(id => id.Trim())
				.Where(id => id.Length > 0)
				.Select(id => $"<@{id}>");
			string content = string.Join(" ", mentions);
			if (content.Length > 0)
				content += " ";
			content += message;

			try
			{
				using (var response = await HttpClient.PostAsync(
					webhookUrl,
					new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("content", content) })))
					response.EnsureSuccessStatusCode();

				SetStatus("Party notification sent.");
			}
			catch (Exception ex)
			{
				SetStatus($"Discord notification failed: {ex.Message}");
			}
		}

		private void SetStatus(string message)
		{
			if (IsDisposed || !IsHandleCreated)
				return;

			if (InvokeRequired)
				BeginInvoke(new Action(() => _status.Text = message));
			else
				_status.Text = message;
		}
	}
}
