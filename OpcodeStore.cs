using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PartyJoin
{
	// Opcodes are resolved from: built-in defaults -> local cache -> remote opcodes.json.
	public static class OpcodeStore
	{
		public const string RemoteUrl = "https://raw.githubusercontent.com/skyrhim/FFXIV_PartyJoin/master/opcodes.json";
		public const string PartyJoinKey = "PartyJoin";

		private static readonly Dictionary<string, ushort> Defaults = new Dictionary<string, ushort>
		{
			{ PartyJoinKey, 810 },
		};

		private static readonly object Sync = new object();
		private static Dictionary<string, ushort> _current = new Dictionary<string, ushort>(Defaults);
		private static Version _version = new Version(0, 0);
		private static string _cachePath;

		public static event Action<string> Changed;

		public static ushort PartyJoin => Get(PartyJoinKey);

		public static ushort Get(string key)
		{
			lock (Sync)
				return _current.TryGetValue(key, out ushort value) ? value : (ushort)0;
		}

		public static void LoadCache(string cachePath)
		{
			_cachePath = cachePath;
			try
			{
				if (File.Exists(cachePath))
					Apply(File.ReadAllText(cachePath), false);
			}
			catch (Exception)
			{
			}
		}

		public static async Task<bool> UpdateFromRemoteAsync(HttpClient client)
		{
			try
			{
				string json = await client.GetStringAsync(RemoteUrl + "?t=" + DateTime.UtcNow.Ticks).ConfigureAwait(false);
				return Apply(json, true);
			}
			catch (Exception)
			{
				return false;
			}
		}

		private static bool Apply(string json, bool persist)
		{
			var serializer = new JavaScriptSerializer();
			var root = serializer.Deserialize<Dictionary<string, object>>(json);
			if (root == null || !root.TryGetValue("opcodes", out object rawOpcodes) ||
				!(rawOpcodes is Dictionary<string, object> opcodes))
				return false;

			if (!root.TryGetValue("version", out object rawVersion) || !Version.TryParse(Convert.ToString(rawVersion), out Version version))
				version = new Version(0, 0);
			var next = new Dictionary<string, ushort>(Defaults);
			foreach (var pair in opcodes)
				next[pair.Key] = Convert.ToUInt16(pair.Value);

			lock (Sync)
			{
				if (persist && version < _version)
					return false;
				bool same = version == _version && SameValues(_current, next);
				_current = next;
				_version = version;
				if (same)
					return false;
			}

			if (persist && _cachePath != null)
			{
				try
				{
					Directory.CreateDirectory(Path.GetDirectoryName(_cachePath));
					string temp = _cachePath + ".tmp";
					File.WriteAllText(temp, json);
					if (File.Exists(_cachePath))
						File.Delete(_cachePath);
					File.Move(temp, _cachePath);
				}
				catch (Exception)
				{
				}
			}

			if (persist)
				Changed?.Invoke("Opcodes updated to version " + version + " (PartyJoin=" + PartyJoin + ")");
			return true;
		}

		private static bool SameValues(Dictionary<string, ushort> a, Dictionary<string, ushort> b)
		{
			if (a.Count != b.Count)
				return false;
			foreach (var pair in a)
				if (!b.TryGetValue(pair.Key, out ushort value) || value != pair.Value)
					return false;
			return true;
		}
	}
}
