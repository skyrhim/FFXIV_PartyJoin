using System;
using System.IO;
using System.Text;

namespace PartyJoin
{
	internal static class PartyJoinConfig
	{
		private static string _filename;

		public static bool NotifyOnPartyJoin { get; set; }
		public static string DiscordWebhookUrl { get; set; } = string.Empty;
		public static string DiscordUserIds { get; set; } = string.Empty;
		public static bool LogPackets { get; set; }

		public static void Load(string filename)
		{
			_filename = filename;
			if (!File.Exists(filename))
				return;

			foreach (var line in File.ReadAllLines(filename, Encoding.UTF8))
			{
				if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
					continue;

				int separator = line.IndexOf('=');
				if (separator < 1)
					continue;

				string key = line.Substring(0, separator).Trim();
				string value = line.Substring(separator + 1).Trim();
				switch (key)
				{
					case "NotifyOnPartyJoin":
						NotifyOnPartyJoin = value.Equals("true", StringComparison.OrdinalIgnoreCase);
						break;
					case "DiscordWebhookUrl":
						DiscordWebhookUrl = value;
						break;
					case "DiscordUserIds":
						DiscordUserIds = value;
						break;
					case "LogPackets":
						LogPackets = value.Equals("true", StringComparison.OrdinalIgnoreCase);
						break;
				}
			}
		}

		public static void Save()
		{
			if (string.IsNullOrEmpty(_filename))
				return;

			Directory.CreateDirectory(Path.GetDirectoryName(_filename));
			File.WriteAllLines(_filename, new[]
			{
				"# PartyJoin configuration",
				$"NotifyOnPartyJoin={NotifyOnPartyJoin}",
				$"DiscordWebhookUrl={DiscordWebhookUrl}",
				$"DiscordUserIds={DiscordUserIds}",
				$"LogPackets={LogPackets}",
			}, Encoding.UTF8);
		}
	}
}
