using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Cacheable
{
    public static class Check
    {
        public static void NotNull(object obj, string name)
        {
            if (obj == null)
                throw new ArgumentNullException("name");
        }

        public static void NotEmpty(string obj, string name)
        {
            if (String.IsNullOrEmpty(obj))
                throw new ArgumentNullException("name");
        }
    }
}
