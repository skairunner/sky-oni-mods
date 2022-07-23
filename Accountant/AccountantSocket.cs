using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;
using static SkyLib.Logger;

namespace Accountant
{
    public class AccountantData
    {
        public static AccountantData Instance;
        public Deque<string> history; // previous inventory states to be sent to a new client.
        public Dictionary<string, string> name_mapping;

        public AccountantData()
        {
            Instance = this;
            name_mapping = new Dictionary<string, string>();
            history = new Deque<string>(150);
        }

        public static Deque<string> History => Instance.history;

        public static Dictionary<string, string> NameMapping => Instance.name_mapping;
    }

    internal class LedgerEntry
    {
        public float in_use;
        public string itemname;
        public float timestamp;
        public float total;

        public LedgerEntry(string itemname, float total, float in_use, float timestamp = 0f)
        {
            this.itemname = itemname;
            this.total = total;
            this.in_use = in_use;
            this.timestamp = timestamp;
        }
    }

    internal class AccountantSocket : WebSocketBehavior
    {
        public static List<LedgerEntry> DumpInventory()
        {
            var output = new List<LedgerEntry>();
            try
            {
                var inv = WorldInventory.Instance;
                HashSet<Tag> tags = inv.GetDiscovered();
                output.Capacity = tags.Count;
                var clock = GameClock.Instance.GetTime();
                foreach (var tag in tags)
                {
                    var name = tag.Name;
                    if (!AccountantData.NameMapping.ContainsKey(name))
                        AccountantData.NameMapping.Add(name, tag.ProperName());

                    var total = inv.GetTotalAmount(tag);
                    var inuse = MaterialNeeds.Instance.GetAmount(tag);
                    output.Add(new LedgerEntry(tag.Name, total, inuse, clock));
                }

                return output;
            }
            catch
            {
                LogLine("Tried to send inventory contents when it is unloaded.");
                return output;
            }
        }

        public static string GetInventoryString()
        {
            var msg = new DataPacket();
            msg.inventory = DumpInventory();
            var dumped = DumpJson(msg);

            Deque<string> history = AccountantData.History;
            if (history.Count == 150) history.RemoveBack();

            history.Add(dumped);

            return dumped;
        }

        public void BroadcastInventory()
        {
            Sessions.BroadcastAsync(GetInventoryString(), null);
        }

        public void BroadcastBacklog()
        {
            var packet = new BacklogPacket
            {
                backlog = AccountantData.Instance.history.ToList()
            };
            Sessions.BroadcastAsync(DumpJson(packet), null);
        }

        public void BroadcastLocstrings(List<string> itemnames)
        {
            var locstrings = new Dictionary<string, string>();
            foreach (var name in itemnames) locstrings[name] = AccountantData.NameMapping[name];

            var packet = new LocPacket
            {
                locs = locstrings
            };
            Sessions.BroadcastAsync(DumpJson(packet), null);
        }

        public static string DumpJson(object o)
        {
            var ser = JsonSerializer.Create();
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            ser.Serialize(writer, o);

            return sb.ToString();
        }

        protected override void OnOpen()
        {
            LogLine("New WS connection.");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var packet = JsonConvert.DeserializeObject<Packet>(e.Data);
            switch (packet.msgtype)
            {
                case "init":
                    BroadcastBacklog();
                    break;
                case "requestloc":
                    BroadcastLocstrings(JsonConvert.DeserializeObject<RequestLocPacket>(e.Data).itemnames);
                    break;
                default:
                    LogLine($"Unknown message type: {packet.msgtype}");
                    break;
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            LogLine($"Client disconnected. Reason: {e.Reason}");
        }
    }
}