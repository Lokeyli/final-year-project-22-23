using UnityEngine;
using UnityEngine.Assertions;

using Unity.Collections;
using System.Collections.Generic;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

// TcpListener: 
// https://learn.microsoft.com/zh-cn/dotnet/api/system.net.sockets.tcplistener?view=net-7.0

public class TCPServerBehaviour : MonoBehaviour
{
    public TcpListener server;
    private Socket handler;
    private TcpClient client;
    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;
    List<int> blendshapes_i = new List<int>();
    List<int> blendshapesWeights = new List<int>();
    int blendShapeCount;

    void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
    }


    void Start()
    {
        // Create the socket server.
        IPAddress ip = IPAddress.Any;
        IPEndPoint ipEndPoint = new IPEndPoint(ip, 8080);
        server = new TcpListener(ipEndPoint);
        server.Start();
        Debug.Log("HTTP Server created.");


        // Put all the AU blendshapes' index into a list
        Regex FACSRegex = new Regex(".*AU.*");
        blendShapeCount = skinnedMesh.blendShapeCount;
        for (int i = 0; i < blendShapeCount; i++)
        {
            string blendShapeName = skinnedMesh.GetBlendShapeName(i);
            if (FACSRegex.IsMatch(blendShapeName))
            {
                blendshapes_i.Add(i);
            }
        }
    }

    public void OnDestroy()
    {

    }

    void Update()
    {
        if (client == null && server.Pending())
        {
            ConnectClient();
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
            ReadClient();
        }
    }
    private void ConnectClient()
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

    private void ReadClient()
    {
        if (client.Available == 0)
        {
            return;
        }
        NetworkStream stream = client.GetStream();
        Debug.Log(client.Available);
        Byte[] bytes = new Byte[client.Available];
        stream.Read(bytes, 0, bytes.Length);
        String data = Encoding.UTF8.GetString(bytes);
        int[] array = JsonConvert.DeserializeObject<int[]>(data);
        // blendshapesWeights = array.ToList();
        if (array.Length != 0)
        {
            for (int i = 0; i < blendshapes_i.Count; i++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendshapes_i[i], array[i]);
            }
        }
    }
}