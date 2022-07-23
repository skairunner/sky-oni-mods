using System.Collections.Generic;

namespace Accountant
{
    internal class Packet
    {
        public string msgtype;
    }

    internal class DataPacket : Packet
    {
        public List<LedgerEntry> inventory;

        public DataPacket()
        {
            msgtype = "inv";
        }
    }

    internal class LocPacket : Packet
    {
        public Dictionary<string, string> locs;

        public LocPacket()
        {
            msgtype = "loc";
        }
    }

    internal class RequestLocPacket : Packet
    {
        public List<string> itemnames;
    }

    internal class BacklogPacket : Packet
    {
        public List<string> backlog;

        public BacklogPacket()
        {
            msgtype = "backlog";
        }
    }
}