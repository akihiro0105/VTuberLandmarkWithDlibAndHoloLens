using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using HoloLensModule.Network;
using HoloLensModule.Environment;
using UnityEngine.UI;

public class ReceiveMotion : MonoBehaviour
{

    [SerializeField] private Text proxy;

    private UDPListenerManager udpListener;
    // Use this for initialization
    void Start ()
	{
        StartCoroutine(receiveUDP());
	}

    private IEnumerator receiveUDP()
    {
        var json = new JsonMessage();
        var eye = new Vector2(0, 0);
        var mouse = new Vector2(0, 0);
        udpListener = new UDPListenerManager(8080);
        udpListener.ListenerMessageEvent += (ms, address) =>
        {
            if (address!=SystemInfomation.IPAddress)
            {
                json = JsonUtility.FromJson<JsonMessage>(ms);
            }
        };
        while (true)
        {
            eye = Vector2.Lerp(eye, new Vector2(json.eye, 0), 0.5f);
            mouse = Vector2.Lerp(mouse, new Vector2(json.mouse, 0), 0.5f);
            proxy.text = "Eye : " + eye.x + " Mouse : " + mouse.x;
            yield return null;
        }
    }

    void OnDestroy()
    {
        udpListener.DisConnectListener();
    }

    [Serializable]
    public class JsonMessage
    {
        public float eye;
        public float mouse;
    }
}
