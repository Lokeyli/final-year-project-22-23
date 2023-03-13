using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

// TcpListener: 
// https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.tcplistener?view=net-7.0

public class TCPServerBehaviour : MonoBehaviour
{
    public TcpListener server;
    private Socket handler;
    private TcpClient client;
    void Start()
    {
        IPAddress ip = IPAddress.Any;
        IPEndPoint ipEndPoint = new IPEndPoint(ip, 8080);
        server = new TcpListener(ipEndPoint);
        server.Start();
        // listener.Blocking = false;
        Debug.Log("HTTP Server created.");
    }

    public void OnDestroy()
    {

    }

    void Update()
    {
        if (client == null && server.Pending())
        {
            ConnectKalidoKit();
        }
        else if (client != null)
        {
            // Check if the connection still exists
            if (client.Client.Poll(100, SelectMode.SelectRead) && client.Available == 0)
            {
                Debug.Log("Client disconnected");
                client = null;
                return;
            }
            ReadKalidoKit();
        }
    }
    private void ConnectKalidoKit()
    {
        // Simply assume the first connection is from kilidokit 
        // and is a GET method.

        // https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
        client = server.AcceptTcpClient();
        NetworkStream stream = client.GetStream();
        while (client.Available < 3)
        {
            // Wait for the client to send the GET method
        }

        Byte[] bytes = new Byte[client.Available];
        stream.Read(bytes, 0, bytes.Length);
        String data = Encoding.UTF8.GetString(bytes);
        const string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
        Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
            + "Connection: Upgrade" + eol
            + "Upgrade: websocket" + eol
            + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                System.Security.Cryptography.SHA1.Create().ComputeHash(
                    Encoding.UTF8.GetBytes(
                        new System.Text.RegularExpressions.Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                    )
                )
            ) + eol
            + eol);
        stream.Write(response, 0, response.Length);
        Debug.Log("Accepted a connection");
    }

    private void ReadKalidoKit()
    {
        if (client.Available == 0)
        {
            return;
        }
        NetworkStream stream = client.GetStream();
        Byte[] bytes = new Byte[client.Available];
        stream.Read(bytes, 0, bytes.Length);
        String data = Encoding.UTF8.GetString(bytes);
        Debug.Log(data);
        // string receiveString = "";
        // byte[] receiveBytes = new byte[4096];
        // int nBytes;
        // nBytes = handler.Receive(receiveBytes, receiveBytes.Length, 0); //0 for None
        // receiveString += Encoding.ASCII.GetString(receiveBytes, 0, nBytes);
        // Debug.Log(receiveString);
        // byte[] msg = Encoding.UTF8.GetBytes("Server: Echo > " + receiveString);
        // handler.Send(msg, 0, msg.Length, SocketFlags.None);
    }
}