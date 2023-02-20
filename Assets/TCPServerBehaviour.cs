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
    private Socket listener;
    private Socket handler;
    void Start ()
    {   
        IPAddress ip = IPAddress.Any;             
        IPEndPoint ipEndPoint = new IPEndPoint(ip, 8080);
        listener = new(
            ipEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);
        listener.Bind(ipEndPoint);
        listener.Blocking = false;
        Debug.Log("Server created.");
        listener.Listen(100);
    }

    public void OnDestroy()
    {
        
    }

    void Update ()
    {   
        try{
            if (handler == null){
                handler = listener.Accept();
                Debug.Log("Accepted a connection");
                byte[] msg = Encoding.UTF8.GetBytes("Server: Connected\n");
                byte[] bytes = new byte[256];
                handler.Send(msg, 0, msg.Length, SocketFlags.None);
            }
            else {
                // Check if the connection still exists
                if(handler.Poll(100, SelectMode.SelectRead) && handler.Available == 0){
                    Debug.Log("Client disconnected");
                    handler = null;
                    return;
                }
                handler.Blocking = false;
                string receiveString = "";
                byte[] receiveBytes = new byte[4096];
                int nBytes;
                nBytes = handler.Receive(receiveBytes, receiveBytes.Length, 0); //0 for None
                receiveString += Encoding.ASCII.GetString(receiveBytes, 0, nBytes);
                Debug.Log(receiveString);
                byte[] msg = Encoding.UTF8.GetBytes("Server: Echo > " + receiveString);
                handler.Send(msg, 0, msg.Length, SocketFlags.None);
            }
        }
        catch (SocketException e){
            if (e.ErrorCode!=10035){
                throw e;
            };
            // Resource temporarily unavailable.
            // This error is returned from operations on nonblocking sockets that cannot be completed immediately, 
            // for example recv when no data is queued to be read from the socket. 
            // It is a nonfatal error, and the operation should be retried later. 
            // It is normal for WSAEWOULDBLOCK to be reported as the result from 
            // calling connect on a nonblocking SOCK_STREAM socket, 
            // since some time must elapse for the connection to be established.
        }   
    }
}