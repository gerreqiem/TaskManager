using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManagerApp.Services
{
    public class OAuthListener : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly string _callbackPath;
        private readonly Action<string> _callback;
        private CancellationTokenSource _cts;

        public OAuthListener(int port, string callbackPath, Action<string> callback)
        {
            _callbackPath = callbackPath;
            _callback = callback;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{port}/");
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => ListenAsync(_cts.Token));
        }

        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            _listener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (HttpListenerException) { break; }
                catch (Exception) {}
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                if (context.Request.Url.AbsolutePath == _callbackPath)
                {
                    var code = context.Request.QueryString["code"];
                    if (!string.IsNullOrEmpty(code))
                    {
                        _callback.Invoke(code);
                        SendResponse(context.Response, 200, "<html><script>window.close();</script><body>Авторизация успешна, окно можно закрыть</body></html>");
                        return;
                    }
                }
                SendResponse(context.Response, 404, "Not Found");
            }
            catch (Exception) {}
        }

        private void SendResponse(HttpListenerResponse response, int statusCode, string message)
        {
            try
            {
                response.StatusCode = statusCode;
                var buffer = System.Text.Encoding.UTF8.GetBytes(message);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
            }
            catch {}
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _listener?.Close();
        }
    }
}