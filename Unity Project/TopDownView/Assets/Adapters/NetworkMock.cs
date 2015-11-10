using UnityEngine;
using System.Collections;

public class NetworkMock {
    private MessagingNetworkAdapter syncObj;

    public NetworkMock(MessagingNetworkAdapter syncObj)
    {
        this.syncObj = syncObj;
    }
     
    public void Send(string msg)
    {
        Debug.Log("send: " + msg);
    }

    public void Recv(string msg)
    {
        Debug.Log("recv: " + msg);
        syncObj.InputRecvData(msg);
    }
}
