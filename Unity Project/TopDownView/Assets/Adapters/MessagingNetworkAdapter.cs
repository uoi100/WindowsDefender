using UnityEngine;
using System.Collections;
using System;

public class MessagingNetworkAdapter {
    public class Message
    {
        
        private string[] args;
        private int type;
        public string[] Args { get { return args; } }
        public int Type { get { return type; } }
        public Message(string[] parts)
        {
            type = Convert.ToInt32(parts[0]);
            args = new string[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
                args[i - 1] = parts[i];
        }
    }

    public delegate void send_msg_delegate(string msg);

    private send_msg_delegate send;
    private System.Collections.Generic.Queue<Message> queue;

    public MessagingNetworkAdapter() : this(null)
    {
        NetworkMock mock = new NetworkMock(this);
        this.send = mock.Send;
    }

    public MessagingNetworkAdapter(send_msg_delegate send)
    {
        this.send = send;
        queue = new System.Collections.Generic.Queue<Message>();
    }

    public void InputRecvData(string msg)
    {
        string[] parts = msg.Split('|');
        queue.Enqueue(new Message(parts));
    }

    public bool MessageRecvReady()
    {
        return queue.Count > 0;
    }

    public Message Recv()
    {
        return queue.Dequeue();
    }

    public void Send(int type, params string[] args)
    {
        string msg = type + "|" + string.Join("|", args);
        send(msg);
    }

}
