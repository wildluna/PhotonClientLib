﻿using System;
using System.Threading.Tasks;
using TestClientLib;

namespace PhotonLib.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Start();
            Console.ReadKey();
        }

        static async void Start()
        {
            int loopCount = 100;
            int taskCount = 100;
            int msgCount = 100;

            for (int k = 0; k < loopCount; k++)
            {
                PhotonUtil.Instance.CreateAndConnect("Test", "localhost:4530");
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
     
                Task[] works = new Task[taskCount];

                for (int i = 0; i < taskCount; i++)
                {
                    int maxDelay = (i%2) == 0?0:500;
                    works[i] = await Task.Factory.StartNew(async (Id) =>
                    {
                        for (int j = 0; j < msgCount; j++)
                        {
                            int delay = rnd.Next(0, maxDelay);
                            string message = string.Format("Thread: {0,-5}, Times: {1,-5} Delay: {2,-5}", Id, j, delay);
                            var response = await PhotonUtil.Instance.RequestDelayAsync(delay, message);
                            Console.WriteLine("Send    : {0}\r\nResponse: {0}", message, response.Message);
                        }
                        Console.WriteLine("Thread{0} Done.", Id);
                    }, i);
                }
                Task.WaitAll(works);
                Console.WriteLine("All Work Done. Loop: {0}", k);
                PhotonUtil.Instance.RemoveClient("Test");
            }
            Console.WriteLine("Final Done!!");
        }
    }
}
