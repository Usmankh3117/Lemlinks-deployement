using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LimLink_API.Model
{
    public class GetHistoryModel
    {
        public string UserId { get; set; }
    }

    public class DeleteHistoryModel
    {
        public int Id { get; set; }
    }

    public class SaveUserLinkModel
    {
        public string UserId { get; set; }
        public int Id { get; set; }
    }

    public class LinkShareModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public int LinkId { get; set; }
        public int Type { get; set; }

    }

    public class DeductCreditModel
    {
        public string UserId { get; set; }
        public long Credit { get; set; }
    }
}
