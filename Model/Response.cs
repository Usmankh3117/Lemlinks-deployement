using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LimLink_API.Model
{
    public class Response
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }

    public class GetHistoryResponse
    {
        public string Date { get; set; }
        public List<AllHistoryLinks> historyLinks { get; set; }
    }

    public class AllHistoryLinks
    {
        public int Id { get; set; }
        public string Link { get; set; }
        public bool isSaved { get; set; }
        public int SaveLinkId { get; set; }
    }


    public class SharedlinkResponse
    {
        public string SharedUserName { get; set; }
        public List<AllSharedLinks> sharedLinks { get; set; }
    }

    public class AllSharedLinks
    {
        public int Id { get; set; }
        public string Link { get; set; }
    }
}
