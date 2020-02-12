﻿using Unity.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Unity.Networking.Transport;
using NetworkConnection = Unity.Networking.Transport.NetworkConnection;
using Unity.Networking.Transport.Utilities;
//using UdpCNetworkDriver = Unity.Networking.Transport.BasicNetworkDriver<Unity.Networking.Transport.IPv4UDPSocket>;

public class NetworkController : MonoBehaviour
{
    protected const int BUFFER_SIZE = 250; //size of serialized Packet()

    protected float timeChange = 0f;

    #region serialization
    //protected byte[] reusableBuffer = new byte[REUSABLE_BUFFER_SIZE];
    protected byte[] Serialize(Packet packet)
    {
        //create an array of the calculated size
        byte[] arr = new byte[BUFFER_SIZE];
        //create a stream on top of the resuable buffer you have
        Stream stream = new MemoryStream(arr);
        //the binary formatter serializes the packet into the stream (in the resuable buffer)
        new BinaryFormatter().Serialize(stream, packet);
        //Debug.Log(stream.Position);
        //once the stream has been filled with the serialized thing, flush it
        stream.Flush();
        //close the stream now that you're done with it
        stream.Close();
        //return the new array
        return arr;
    }

    protected static Packet Deserialize(byte[] buffer)
    {
        //make sure the buffer isn't null for some dumb reason
        if (buffer == null) return null;
        //make sure that the buffer has nonzero length
        if (buffer.Length == 0) return null;
        //create a stream on top of the buffer passed in
        Stream stream = new MemoryStream(buffer);
        //and a binarry formatter to do the deserialziation
        BinaryFormatter formatter = new BinaryFormatter();
        //deserialize the contents of the buffer
        object o;
        try { o = formatter.Deserialize(stream); }
        catch (SerializationException e) { Debug.Log("Failed to deserialize"); throw e; }
        //close the stream
        stream.Close();
        //if the object you deserialized is a packet, great! return it
        if (o is Packet packet) return packet;
        //otherwise don't return anything
        return null;
    }
    #endregion serialization

    /// <summary>
    /// adds a packet to the queue of things to send
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="mDriver"></param>
    /// <param name="connection"></param>
    protected void Send(Packet packet, UdpNetworkDriver mDriver, NetworkConnection connection, NetworkPipeline pipeline)
    {
        if(packet.command != Packet.Command.Nothing)
        {
            Debug.Log($"Sending a packet with command {packet.command} to {connection}");
        }
        if (!connection.IsCreated) return;

        using (var writer = new DataStreamWriter(BUFFER_SIZE, Allocator.Temp)) //make the number large enough to contain entire byte array to be sent
        {
            writer.Write(Serialize(packet));
            mDriver.Send(pipeline, connection, writer);
        }
    }
}