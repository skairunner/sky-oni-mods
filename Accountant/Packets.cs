using System.Collections.Generic;

namespace Accountant
{
    class Packet
    {
        public string msgtype;
    }

    class DataPacket : Packet
    {
        public DataPacket()
        {
            msgtype = "inv";
        }

        public List<LedgerEntry> inventory;
    }

    class LocPacket : Packet
    {
        public LocPacket()
        {
            msgtype = "loc";
        }

        public Dictionary<string, string> locs;
    }

    class RequestLocPacket : Packet
    {
        public List<string> itemnames;
    }

    class BacklogPacket : Packet
    {
        public BacklogPacket()
        {
            msgtype = "backlog";
        }

        public List<string> backlog;
    }
}