using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using LiteNetLib;
using MLAPI.Puncher.Shared;
namespace MLAPI.Puncher.LiteNetLib;

/// <summary>
/// A LiteNetLib based UDPTransport implementation that works with LiteNetLib instances which have already started.
/// It works with unconnected messages.
/// </summary>
public class LiteNetLibUdpTransport : IDisposable, IUDPTransport
{
    private NetManager _manager;
    private EventBasedNetListener _listener;
    private Queue<(byte[], IPEndPoint)> _receiveQueue = new Queue<(byte[], IPEndPoint)>();
    private Stopwatch _receiveStopwatch = new Stopwatch();
    private bool _isClosed;

    public LiteNetLibUdpTransport(NetManager manager, EventBasedNetListener listener)
    {
        _manager = manager;
        _manager.UnconnectedMessagesEnabled = true;
        _listener = listener;
        _listener.NetworkReceiveUnconnectedEvent += ReceiveUnconnectedEvent;
    }

    /// <inheritdoc />
    public int SendTo(byte[] buffer, int offset, int length, int timeoutMs, IPEndPoint endpoint)
    {
        if (_manager.SendUnconnectedMessage(buffer, offset, length, endpoint))
            return length;

        return 0;
    }

    /// <inheritdoc />
    public int ReceiveFrom(byte[] buffer, int offset, int length, int timeoutMs, out IPEndPoint endpoint)
    {
        var timeout = timeoutMs == -1 ? long.MaxValue : timeoutMs;

        _receiveStopwatch.Restart();
        while (_receiveStopwatch.ElapsedMilliseconds < timeout && !_isClosed)
        {
            if (_receiveQueue.TryDequeue(out var result))
            {
                endpoint = result.Item2;
                var bytesReceived = Math.Min(buffer.Length, result.Item1.Length);
                Buffer.BlockCopy(result.Item1, 0, buffer, 0, bytesReceived);
                return result.Item1.Length;
            }

            Thread.Sleep(_manager.UpdateTime);
            if (_manager.IsInManualMode && !_isClosed)
                _manager.PollEvents();
        }

        endpoint = new IPEndPoint(IPAddress.None, 0);
        return 0;
    }

    /// <inheritdoc />
    public void Bind(IPEndPoint endpoint)
    {
        if (!_manager.IsRunning)
            _manager.Start(endpoint.Address, IPAddress.IPv6None, endpoint.Port);
    }

    /// <inheritdoc />
    public void Close()
    {
        Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _listener.NetworkReceiveUnconnectedEvent -= ReceiveUnconnectedEvent;
        _isClosed = true;
    }

    private void ReceiveUnconnectedEvent(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messagetype)
    {
        _receiveQueue.Enqueue((reader.GetRemainingBytes(), remoteEndPoint));
    }
}
