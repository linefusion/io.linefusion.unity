using System.Threading;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Linefusion.Generator;

public static class Log
{
    static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
    static readonly ConcurrentQueue<byte[]> buffers = new ConcurrentQueue<byte[]>();

    public static void Write(string message)
    {
        Write(Encoding.UTF8.GetBytes(message + "\n"));
    }

    public static void Write(byte[] buffer)
    {
        buffers.Enqueue(buffer);

        Task.Run(async () =>
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                using var client = new UdpClient();
                while (buffers.TryDequeue(out var current))
                {
                    foreach (var chunk in current.Chunk(1000).Select(chunk => chunk.ToArray()))
                    {
                        await client.SendAsync(chunk, chunk.Length, "127.0.0.1", 9999);
                        await Task.Delay(5);
                    }
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        });
    }
}