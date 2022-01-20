using System.Net.NetworkInformation;

namespace URLTester
{
    internal class ThreadedPing
    {
        /// <summary>Threshold for marking this ping as fast</summary>
        public static int ThresholdFast = 50;
        /// <summary>Threshold for marking this ping as slow</summary>
        public static int ThresholdWarning = 500;
        /// <summary>Threshold for marking this ping as problematic</summary>
        public static int ThresholdError = 1000;

        /// <summary>Last line that was written</summary>
        private static int lastLine = 0;

        /// <summary>Tells you, if a ping is currently running</summary>
        public static bool PingRunning { get; protected set; }


        /// <summary>The result table</summary>
        private static ResultTable results = new ResultTable(new string[0]);




        /// <summary>Print all the (not yet printed) lines</summary>
        public static void printCurrentLines()
        {
            while (lastLine < results.Length())
            {
                if (lastLine % 10 == 0)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    foreach (string address in results.Destinations)
                    {
                        WriteSingleHeading(address);
                    }
                    Console.WriteLine();
                }



                foreach (string address in results.Destinations)
                {
                    for (int i = 0; i < (address.Length - 7) / 2; i++)
                    {
                        Console.Write(" ");
                    }


                    if (results.GetRow(lastLine).ContainsKey(address))
                    {
                        WriteSingleInfo(results.GetElement(lastLine, address));
                    } else if (lastLine > 0 && results.GetRow(lastLine - 1).ContainsKey(address))
                    {
                        WriteSingleInfo(results.GetElement(lastLine - 1, address));
                    } else
                    {
                        Console.Write("        ");
                    }


                    for (int i = 0; i < (address.Length - 8) / 2; i++)
                    {
                        Console.Write(" ");
                    }


                    Console.Write("\t");
                }

                Console.WriteLine();

                lastLine++;
            }
        }


        /// <summary>
        /// Write a single information on the console
        /// </summary>
        /// <param name="reply">is the ping reply you like to note down</param>
        private static void WriteSingleInfo(PingReply? reply)
        {
            if (reply == null || reply.Status != IPStatus.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" error! ");
                return;
            }


            if (reply.RoundtripTime >= ThresholdError)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            } else if (reply.RoundtripTime >= ThresholdWarning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            } else if (reply.RoundtripTime < ThresholdFast)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            } else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }

            if (reply.RoundtripTime < 10000)
            {
                Console.Write(" ");
            }
            if (reply.RoundtripTime < 1000)
            {
                Console.Write(" ");
            }

            if (reply.RoundtripTime < 100)
            {
                Console.Write(" ");
            }
            
            if (reply.RoundtripTime < 10)
            {
                Console.Write(" ");
            }

            Console.Write(reply.RoundtripTime.ToString() + "ms");
        }

        /// <summary>
        /// Write a single headline to the console
        /// </summary>
        /// <param name="headline">is the string you like to note down</param>
        private static void WriteSingleHeading(string headline)
        {
            if (headline.Length < 8)
            {
                Console.Write(" ");
            }
            Console.Write(headline + "\t");
        }




        /// <summary>
        /// Ping a bunch of addresses with some parameters
        /// </summary>
        /// <param name="addresses">is the array of addresses you like to ping</param>
        /// <param name="iterations">is the number of iterations for this ping</param>
        /// <param name="speed">is the time (ms) you like to wait between the pings at minimum</param>
        public async static void PingAddresses(string[] addresses, int iterations, int speed)
        {
            if (PingRunning)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ping is already running");
                return;
            }


            results = new ResultTable(addresses);
            lastLine = 0;


            PingRunning = true;
            Task<PingReply>[] taskPool = new Task <PingReply>[addresses.Length];
            TimeSpan passed = TimeSpan.Zero;
            DateTime startTime = DateTime.Now;


            for (int j = 0; j < iterations; j++)
            {
                for (int i = 0; i < addresses.Length; i++)
                {
                    taskPool[i] = MakePing(addresses[i]);
                }
                startTime = DateTime.Now;

                for (int i = 0; i < addresses.Length; i++)
                {
                    await taskPool[i].WaitAsync(new TimeSpan(0, 0, 15));
                }

                passed = DateTime.Now - startTime;

                printCurrentLines();

                if (passed.TotalMilliseconds < speed)
                {
                    await Task.Delay(speed - (int)passed.TotalMilliseconds);
                }
                
            }


            PingRunning = false;
        }







        /// <summary>
        /// This performs a single ping to the desired address
        /// </summary>
        /// <param name="destination">is where you like to ping to</param>
        /// <returns>a ping reply (as soon as it is finished)</returns>
        public static Task<PingReply> MakePing(string destination)
        {
            TaskCompletionSource<PingReply> tcs = new TaskCompletionSource<PingReply>();
            Ping ping = new Ping();
            ping.PingCompleted += (obj, sender) =>
            {
                if (sender.Reply != null)
                {
                    tcs.SetResult(sender.Reply);
                }

                results.Write(destination, sender.Reply);
            };

            ping.SendAsync(destination, new object());
            return tcs.Task;
        }




        /// <summary>
        /// The table of the ping results
        /// </summary>
        private class ResultTable
        {
            /// <summary>The table itself</summary>
            private List<Dictionary<string, PingReply?>> table = new List<Dictionary<string, PingReply?>>();

            /// <summary>The list of destinations for this one</summary>
            private List<string> destinations = new List<string>();

            /// <summary>
            /// The list of destinations in this table
            /// </summary>
            public string[] Destinations
            {
                get { return destinations.ToArray(); }
            }

            /// <summary>
            /// Create a new result table
            /// </summary>
            /// <param name="destinations">is the list of possible destinations for this (will auto-update if you find new ones lateron)</param>
            public ResultTable (string[] destinations)
            {
                this.destinations = destinations.ToList();
            }


            /// <summary>
            /// Get a full row of the table
            /// </summary>
            /// <param name="position">is the position in table</param>
            /// <returns>the full row of answers (where some columns may be missing or null)</returns>
            public Dictionary<string, PingReply?> GetRow(int position)
            {
                return table[position];
            }

            /// <summary>
            /// Get the exact content of a cell in table
            /// </summary>
            /// <param name="position">is the row</param>
            /// <param name="column">is the column</param>
            /// <returns>either a valid ping reply value or null, if not set yet</returns>
            public PingReply? GetElement(int position, string column)
            {
                if (table.Count() > position && table[position].ContainsKey(column))
                {
                    return table[position][column];
                }
                return null;
            }

            /// <summary>
            /// Get the bottom row (which most likely has multiple entries missing at the moment)
            /// </summary>
            /// <returns></returns>
            public Dictionary<string, PingReply?> GetBottomRow()
            {
                return table.Last();
            }

            /// <summary>
            /// Get the lenght of this table
            /// </summary>
            public int Length()
            {
                return table.Count();
            }

            /// <summary>
            /// Write a new value into the table. A new row is automatically created, if the most recent row already has a value for this one
            /// </summary>
            /// <param name="column">define the column</param>
            /// <param name="reply">define the content for this cell</param>
            public void Write(string column, PingReply? reply)
            {
                if (!destinations.Contains(column))
                {
                    destinations.Add(column);
                }


                if (table.Count == 0 || table.Last().ContainsKey(column))
                {
                    Dictionary<string, PingReply?> newLine = new Dictionary<string, PingReply?>();
                    newLine.Add(column, reply);
                    table.Add(newLine);
                } else
                {
                    table.Last().Add(column, reply);
                }
            }

        }
    }
}
