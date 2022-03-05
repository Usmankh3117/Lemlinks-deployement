using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LimLink_API.Model
{
    public class HistoryLinkRequestModel
    {
        public string UserId { get; set; }
        public List<string> Links { get; set; }
    }
}
