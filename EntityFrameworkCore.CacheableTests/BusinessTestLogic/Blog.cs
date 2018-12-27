using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.CacheableTests.BusinessTestLogic
{
    public class Blog
    {
        [Key]
        public int BlogId { get; set; }
        public string Url { get; set; }
        public int? Rating { get; set; }

        public List<Post> Posts { get; set; }

        public int OwnerId { get; set; }
        public Person Owner { get; set; }
    }
}
