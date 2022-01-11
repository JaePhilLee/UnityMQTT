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
    static string subTopic = "23800";

    // Start is called before the first frame update
    void Start()
    {
        if (brokerHostname != null && userName != null && password != null)
		{
			Debug.Log("connecting to " + brokerHostname + ":" + brokerPort);

			Connect();

			client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

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
		    string clientId = NetworkInterface.GetAllNetworkInterfaces()[0].GetPhysicalAddress().ToString();
		    Debug.Log("About to connect using '" + userName + "' / '" + password + "' [" + clientId + "]");

			client.Connect(clientId, userName, password);
		}
		catch (Exception e)
		{
			Debug.LogError("Connection error: " + e);
		}
	}

    public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		return true;
	}

    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
	{
		string msg = System.Text.Encoding.UTF8.GetString(e.Message);
        Debug.Log ("Received message from " + e.Topic + " : " + msg);
	}

    private void Publish(string _topic, string msg)
	{
		client.Publish(
			_topic, System.Text.Encoding.UTF8.GetBytes(msg),
			MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
	}
}
