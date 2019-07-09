using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.CacheableTests.BusinessTestLogic
{
    public class PersonPhoto
    {
        public int PersonPhotoId { get; set; }
        public string Caption { get; set; }
        public byte[] Photo { get; set; }

        public Person Person { get; set; }
    }
}
