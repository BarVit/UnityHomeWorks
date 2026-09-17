using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace FlappyBridge
{
    [InitializeOnLoad]
    public static class BridgeServer
    {
        public const int Port = 17890;

        private const int RequestTimeoutMilliseconds = 15000;

        private static readonly Queue<BridgeRequest> Pending = new Queue<BridgeRequest>();

        private static HttpListener _listener;
        private static Thread _thread;

        static BridgeServer()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            EditorApplication.quitting += Stop;
            EditorApplication.update += DrainQueue;

            Start();
        }

        private static void Start()
        {
            if (_listener != null)
                return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://127.0.0.1:" + Port + "/");
                _listener.Start();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Bridge] could not listen on port " + Port + ": " + exception.Message);
                _listener = null;

                return;
            }

            _thread = new Thread(Listen);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private static void Stop()
        {
            EditorApplication.update -= DrainQueue;

            FailPending("the editor is reloading, retry");

            if (_listener == null)
                return;

            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch (Exception)
            {
            }

            _listener = null;
            _thread = null;
        }

        private static void Listen()
        {
            while (true)
            {
                HttpListener listener = _listener;

                if (listener == null || listener.IsListening == false)
                    return;

                try
                {
                    Serve(listener.GetContext());
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        private static void Serve(HttpListenerContext context)
        {
            BridgeRequest request = new BridgeRequest(context.Request.Url);

            lock (Pending)
                Pending.Enqueue(request);

            string body;

            if (request.Completed.Wait(RequestTimeoutMilliseconds))
                body = request.Response;
            else
                body = Json.Error("the editor did not answer within " + RequestTimeoutMilliseconds + " ms");

            byte[] bytes = Encoding.UTF8.GetBytes(body);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.Close();
        }

        private static void DrainQueue()
        {
            while (true)
            {
                BridgeRequest request;

                lock (Pending)
                {
                    if (Pending.Count == 0)
                        return;

                    request = Pending.Dequeue();
                }

                try
                {
                    request.Response = BridgeRouter.Route(request.Url);
                }
                catch (Exception exception)
                {
                    request.Response = Json.Error(exception.GetType().Name + ": " + exception.Message);
                }

                request.Completed.Set();
            }
        }

        private static void FailPending(string reason)
        {
            lock (Pending)
            {
                while (Pending.Count > 0)
                {
                    BridgeRequest request = Pending.Dequeue();
                    request.Response = Json.Error(reason);
                    request.Completed.Set();
                }
            }
        }
    }
}
