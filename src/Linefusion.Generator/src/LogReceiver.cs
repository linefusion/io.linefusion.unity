using System;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

namespace Linefusion.Generator;

public class LogReceiver
{
    public static LogReceiver Instance { get; } = new LogReceiver();

    private event EventHandler<string>? log;
    public event EventHandler<string>? Log
    {
        add => log += value; remove => log -= value;
    }

    private event EventHandler<Exception>? error;
    public event EventHandler<Exception>? Error
    {
        add => error += value; remove => error -= value;
    }

    private UdpClient? client;
    private Task? task;
    private CancellationTokenSource? token;

    public void Start(int port = 9999)
    {
        this.Cancel();
        
        client = new UdpClient(new IPEndPoint(IPAddress.Any, port));

        token = new CancellationTokenSource();
        task = Task.Run(() =>
        {
            while (!token.Token.IsCancellationRequested)
            {
                try
                {
                    var receiver = client.ReceiveAsync();
                    if (receiver.Wait(100, token.Token))
                    {
                        if (!(task?.IsCanceled ?? true)) 
                        {
                            log?.Invoke(this, Encoding.UTF8.GetString(receiver.Result.Buffer));
                        }
                    }
                }
                catch (Exception ex)
                {
                    error?.Invoke(this, ex);
                }
            }
        });
    }

    public void Cancel()
    {
        token?.Cancel();
        task?.Wait();
    }

    public async Task CancelAsync()
    {
        token?.Cancel();
        if (task != null)
        {
            await task;
        }
    }
}