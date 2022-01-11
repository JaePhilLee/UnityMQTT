using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.NetworkInformation; //MAC Address
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Internal;


public class MQTT : MonoBehaviour
{
    private MqttClient client;

    // 접속 정보
    public string brokerHostname = "127.0.0.1"; //Test Online Broker: broker.emqx.io
    public int brokerPort = 1883;
    public string userName = "test";
    public string password = "test";
    public TextAsset certificate;

    // 모든 토픽 구독 : #
    static string subTopic = "topic";

    // Start is called before the first frame update
    void Start()
    {
        if (brokerHostname != null && userName != null && password != null)
		{
			Debug.Log("connecting to " + brokerHostname + ":" + brokerPort);

			Connect();

			client.MqttMsgPublishReceived += ReceivedCallback;

            /*
            QOS Level
            0 : QOS_LEVEL_AT_MOST_ONCE
            1 : QOS_LEVEL_AT_LEAST_ONCE
            2 : QOS_LEVEL_EXACTLY_ONCE
            */
            string[] topics = { subTopic };
			byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
			client.Subscribe(new string[] { subTopic }, qosLevels);

            Publish(subTopic, "Hello World!!");
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Connect()
	{
		Debug.Log("about to connect on '" + brokerHostname + "'");

        // 인증서 유/무로 TLS인지 아닌지 확인.
        if ( certificate ) {
            X509Certificate cert = new X509Certificate();
            cert.Import(certificate.bytes);
            Debug.Log("Using the certificate '" + cert + "'");
            client = new MqttClient(brokerHostname, brokerPort, true, cert, null, MqttSslProtocols.TLSv1_0, MyRemoteCertificateValidationCallback);
        } else {
            client = new MqttClient(brokerHostname);
        }

        try
		{
            //MAC Address로 clientID 할당
		    string clientId = NetworkInterface.GetAllNetworkInterfaces()[0].GetPhysicalAddress().ToString();
		    Debug.Log("About to connect using '" + userName + "' / '" + password + "' [" + clientId + "]");

			client.Connect(clientId, userName, password);
		}
		catch (Exception e)
		{
			Debug.LogError("Connection error: " + e);
		}
	}

    //모든 인증서 허용
    public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		return true;
	}

    // 구독(Subscribe)한 토픽(들)에게서 메세지를 수신했을 때, 실행되는 콜백함수
    void ReceivedCallback(object sender, MqttMsgPublishEventArgs e)
	{
		string msg = System.Text.Encoding.UTF8.GetString(e.Message);
        Debug.Log ("Received message from " + e.Topic + " : " + msg);
	}

    // 지정된 토픽(_topic)에 메세지(msg)를 발행(Publish)
    private void Publish(string _topic, string msg)
	{
		client.Publish(
			_topic, System.Text.Encoding.UTF8.GetBytes(msg),
			MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
	}

    private void Publish(string _topic, string msg, byte qos)
	{
		client.Publish(
			_topic, System.Text.Encoding.UTF8.GetBytes(msg), qos, false);
	}
}
