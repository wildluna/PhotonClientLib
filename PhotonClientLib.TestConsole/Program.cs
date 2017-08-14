using PhotonGlobalLib.Operation;
using System;
using System.Threading;
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
            int loopCount = 1000;
            int taskCount = 5;
            int msgCount = 100;

            for (int k = 0; k < loopCount; k++)
            {
                PhotonUtil.Instance.CreateAndConnect("Test", "localhost:4530");
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
     
                Task[] works = new Task[taskCount];

                for (int i = 0; i < taskCount; i++)
                {
                    int maxDelay = (i % 2) == 0?0:1000;
                    works[i] = await Task.Factory.StartNew(async (Id) =>
                    {
                        for (int j = 0; j < msgCount; j++)
                        {
                            int delay = rnd.Next(0, maxDelay);
                            string message = string.Format("Thread: {0}, Times: {1} Delay: {2}", Id, j, delay);
                            var response = await PhotonUtil.Instance.RequestDelayAsync(delay, message);
                            Console.WriteLine("Send Message {0}, Response: {1}", message, response.Message);
                        }
                        Console.WriteLine("Thread{0} Done.", Id);
                    }, i);
                }
                Task.WaitAll(works);
                Console.WriteLine("All Work Done. Loop: {0}", k);
                PhotonUtil.Instance.GetClient("Test").Close();
            } 
        }
    }
}
