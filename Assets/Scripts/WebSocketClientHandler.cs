using System;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Codecs.Http;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using UnityEngine;
using TaskCompletionSource = DotNetty.Common.Concurrency.TaskCompletionSource;

namespace ConsoleApp1
{
    public class WebSocketClientHandler : SimpleChannelInboundHandler<object>
    {
        readonly WebSocketClientHandshaker handshaker;
        readonly TaskCompletionSource completionSource;

        public WebSocketClientHandler(WebSocketClientHandshaker handshaker)
        {
            this.handshaker = handshaker;
            this.completionSource = new TaskCompletionSource();
        }

        public Task HandshakeCompletion => this.completionSource.Task;

        public override void ChannelActive(IChannelHandlerContext ctx) =>
            this.handshaker.HandshakeAsync(ctx.Channel).LinkOutcome(this.completionSource);

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Debug.Log("WebSocket Client disconnected!");
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, object msg)
        {
            IChannel ch = ctx.Channel;
            if (!this.handshaker.IsHandshakeComplete)
            {
                try
                {
                    Debug.Log(msg);
                    this.handshaker.FinishHandshake(ch, (IFullHttpResponse) msg);
                    Debug.Log("WebSocket Client connected!");
                    this.completionSource.TryComplete();
                }
                catch (WebSocketHandshakeException e)
                {
                    Debug.Log(e);
                    Debug.Log("WebSocket Client failed to connect");
                    this.completionSource.TrySetException(e);
                }

                return;
            }


            if (msg is IFullHttpResponse response)
            {
                throw new InvalidOperationException(
                    $"Unexpected FullHttpResponse (getStatus={response.Status}, content={response.Content.ToString(Encoding.UTF8)})");
            }

            if (msg is TextWebSocketFrame textFrame)
            {
                Debug.Log($"WebSocket Client received message: {textFrame.Text()}");
            }
            else if (msg is PongWebSocketFrame)
            {
                Debug.Log("WebSocket Client received pong");
            }
            else if (msg is CloseWebSocketFrame)
            {
                Debug.Log("WebSocket Client received closing");
                ch.CloseAsync();
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
        {
            Debug.Log("Exception: " + exception);
            this.completionSource.TrySetException(exception);
            ctx.CloseAsync();
        }
    }
}