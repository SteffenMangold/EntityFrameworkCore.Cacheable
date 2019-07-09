using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.CacheableTests.BusinessTestLogic
{
    public class Tag
    {
        public string TagId { get; set; }

        public List<PostTag> Posts { get; set; }
    }
}
