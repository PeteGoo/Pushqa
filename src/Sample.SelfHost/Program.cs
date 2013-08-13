﻿using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:14844/"))
            {
                Console.WriteLine("Server running at http://localhost:14844/");
                Console.ReadLine();
            }
        }
    }
}
