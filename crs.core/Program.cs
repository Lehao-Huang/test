using crs.core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crs.core
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.FirstOrDefault(m => m == "DEBUG") == null)
            {
                return;
            }

            await Task.Yield();
        }
    }
}
