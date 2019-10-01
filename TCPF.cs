﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace TCPF
{
    class TCPF
    {
        private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public void Start(IPEndPoint local, IPEndPoint remote)
        {
            _mainSocket.Bind(local);
            _mainSocket.Listen(10);

            while (true)
            {
                var source = _mainSocket.Accept();
                var destination = new TCPF();
                var state = new State(source, destination._mainSocket);
                destination.Connect(remote, source);
                source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
            }
        }
        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            var state = new State(_mainSocket, destination);
            _mainSocket.Connect(remoteEndpoint);
            _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
        }

        private static void OnDataReceive(IAsyncResult result)
        {
            var state = (State)result.AsyncState;

            IPEndPoint SLocalIPEndPoint = state.SourceSocket.LocalEndPoint as IPEndPoint;
            IPEndPoint SRemoteIPEndPoint = state.SourceSocket.RemoteEndPoint as IPEndPoint;

            IPEndPoint DLocalIPEndPoint = state.DestinationSocket.LocalEndPoint as IPEndPoint;
            IPEndPoint DRemoteIPEndPoint = state.DestinationSocket.RemoteEndPoint as IPEndPoint;
            
            try
            {
                var bytesRead = state.SourceSocket.EndReceive(result);
                if (bytesRead > 0)
                {
                    AppendAllBytes(GetExecutingDirectoryName()+"\\TCPF.log", state.Buffer).ConfigureAwait(false);

                    //Console.WriteLine("SourceSocket " + SLocalIPEndPoint.Address + ":" + SLocalIPEndPoint.Port + " <---> " + SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port);
                    //Console.WriteLine("DestinationSocket " + DLocalIPEndPoint.Address + ":" + DLocalIPEndPoint.Port + " <---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port);

                    Console.WriteLine(SRemoteIPEndPoint.Address + ":" + SRemoteIPEndPoint.Port + " ---> " + DRemoteIPEndPoint.Address + ":" + DRemoteIPEndPoint.Port);
                    Console.WriteLine("--------------------------------------------------");

                    Console.WriteLine(System.Text.Encoding.ASCII.GetString(state.Buffer));
                    Console.WriteLine("");

                    state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch
            {
                state.DestinationSocket.Close();
                state.SourceSocket.Close();
            }
        }

        private class State
        {
            public Socket SourceSocket { get; private set; }
            public Socket DestinationSocket { get; private set; }
            public byte[] Buffer { get; private set; }

            public State(Socket source, Socket destination)
            {
                SourceSocket = source;
                DestinationSocket = destination;
                Buffer = new byte[8192];
            }
        }

        public static async Task AppendAllBytes(string path, byte[] bytes)
        {
            using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None,bufferSize:4096, useAsync:true))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        public static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new FileInfo(location.AbsolutePath).Directory.FullName;
        }
        static void Main(string[] args)
        {
            Console.Clear();
            new TCPF().Start(
                new IPEndPoint(IPAddress.Parse(args[0]), int.Parse(args[1])),
                new IPEndPoint(IPAddress.Parse(args[2]), int.Parse(args[3])));
        }
    }
}