﻿/*
 * Copyright (c) 2021 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 */

using Snowplow.Tracker.Logging;
using Snowplow.Tracker.Models.Events;
using System;
using System.Linq;

namespace Snowplow.Demo.Console
{
    public class Program
    {

        /// <summary>
        /// Runs the Console Demo application
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            string collectorHostname = "<invalid>";
            int port = 80;

            int count = 100;

            switch (args.Count())
            {
                case 2:
                    count = Int32.Parse(args[1]);
                    goto case 1;
                case 1:
                    collectorHostname = args[0];
                    break;
                default:
                    System.Console.WriteLine("Invalid arguments. Usage: <app> <collector-hostname> [number of events to send]");
                    return;
            }

            // help the user out a bit with ports
            if (Uri.IsWellFormedUriString(collectorHostname, UriKind.Absolute))
            {
                Uri tmp;
                if (Uri.TryCreate(collectorHostname, UriKind.Absolute, out tmp))
                {
                    if (tmp.Scheme == "http" || tmp.Scheme == "https")
                    {
                        collectorHostname = tmp.Host;
                        port = tmp.Port;
                    }
                }
            }

            System.Console.WriteLine("Demo app started - sending " + count + " events to " + collectorHostname + " port " + port);

            var logger = new ConsoleLogger();

            Tracker.Tracker.Instance.Start(collectorHostname, "snowplow-demo-app.db", l: logger, endpointPort: port);

            for (int i = 0; i < count; i++)
            {
                Tracker.Tracker.Instance.Track(new PageView().SetPageUrl("http://helloworld.com/sample/sample.php").Build());
            }

            Tracker.Tracker.Instance.Flush();
            Tracker.Tracker.Instance.Stop();

            System.Console.WriteLine("Demo app finished");
            System.Console.Out.Flush();
        }
    }
}
