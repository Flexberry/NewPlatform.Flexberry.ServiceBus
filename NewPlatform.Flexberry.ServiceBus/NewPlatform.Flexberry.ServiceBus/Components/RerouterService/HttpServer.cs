namespace NewPlatform.Flexberry.ServiceBus.Components.Rerouter
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// Многопоточный http-сервер, принимающий сообщения и передающий его на обработку.
    /// </summary>
    public class HttpServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly Thread listenerThread;
        private readonly Thread[] workers;
        private readonly ManualResetEvent stop, ready;
        private Queue<HttpListenerContext> queue;

        /// <summary>
        /// Создаёт многопоточный http-сервер, принимающий сообщения и передающий его на обработку.
        /// </summary>
        /// <param name="maxThreads">Размер пула потоков.</param>
        public HttpServer(int maxThreads)
        {
            workers = new Thread[maxThreads];
            queue = new Queue<HttpListenerContext>();
            stop = new ManualResetEvent(false);
            ready = new ManualResetEvent(false);
            listener = new HttpListener();
            listenerThread = new Thread(HandleRequests);
        }

        /// <summary>
        /// Запускает сервер.
        /// </summary>
        /// <param name="port">Порт, на котором будет запущен сервер.</param>
        public void Start(int port)
        {
            listener.Prefixes.Add(String.Format(@"http://+:{0}/", port));
            listener.Start();
            listenerThread.Start();

            for (int i = 0; i < workers.Length; i++)
            {
                workers[i] = new Thread(Worker);
                workers[i].Start();
            }
        }

        /// <summary>
        /// Освобождение ресурсов.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Остановка сервера.
        /// </summary>
        public void Stop()
        {
            stop.Set();
            listenerThread.Join();
            foreach (Thread worker in workers)
                worker.Join();
            listener.Stop();
        }

        /// <summary>
        /// Перехватывает запросы.
        /// </summary>
        private void HandleRequests()
        {
            while (listener.IsListening)
            {
                var context = listener.BeginGetContext(ContextReady, null);

                if (0 == WaitHandle.WaitAny(new[] { stop, context.AsyncWaitHandle }))
                    return;
            }
        }

        /// <summary>
        /// Callback-функция для метода HttpListener.BeginGetContext. Добавляет контекст соединения в очередь для обработки.
        /// </summary>
        /// <param name="ar">Результат выполнения HttpListener.BeginGetContext.</param>
        private void ContextReady(IAsyncResult ar)
        {
            try
            {
                lock (queue)
                {
                    queue.Enqueue(listener.EndGetContext(ar));
                    ready.Set();
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Внутренний обработчик запросов. Получает контекст из очереди и передаёт внешним обработчикам.
        /// </summary>
        private void Worker()
        {
            var wait = new[] { ready, stop };
            while (WaitHandle.WaitAny(wait) == 0)
            {
                HttpListenerContext context;
                lock (queue)
                {
                    if (queue.Count > 0)
                        context = queue.Dequeue();
                    else
                    {
                        ready.Reset();
                        continue;
                    }
                }

                if (ProcessRequest != null)
                    ProcessRequest(context);
            }
        }

        /// <summary>
        /// Событие обработки запроса. На него должны подписываться внешние обработчики.
        /// </summary>
        public event Action<HttpListenerContext> ProcessRequest;
    }
}
