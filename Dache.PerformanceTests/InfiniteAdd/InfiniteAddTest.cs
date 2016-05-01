﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dache.Client;
using Dache.Client.Exceptions;

namespace Dache.PerformanceTests.InfiniteAdd
{
    internal static class InfiniteAddTest
    {
        public static void Run()
        {
            var cacheClient = new CacheClient();

            // 502 chars = ~1 kb
            string value = "asdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasasdfasdfasas";

            int itemsToAdd = 1000;

            Console.WriteLine("***** BEGIN INFINITE ADD " + itemsToAdd + " 1 KB STRING OBJECTS TEST (WILL NEVER END) *****");
            Console.WriteLine();

            cacheClient.HostDisconnected += (sender, e) => { Console.WriteLine("*** Host disconnected"); };
            cacheClient.HostReconnected += (sender, e) => { Console.WriteLine("*** Host reconnected"); };

            // Add items
            var cts = new CancellationTokenSource();
            Task.Factory.StartNew(() => {
                int i = 0;
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        cacheClient.AddOrUpdate(string.Intern("test" + i), value);
                    }
                    catch (NoCacheHostsAvailableException)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    i = ++i % itemsToAdd;

                    if (i == 0) Thread.Sleep(1);
                }
            }, cts.Token);

            var key = Console.ReadKey();
            // Graceful shutdown option
            if (key.KeyChar == 'q')
            {
                cts.Cancel();
                cacheClient.Shutdown();
            }
        }
    }
}
