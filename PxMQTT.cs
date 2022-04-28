/*
    [2022-04-20] 이재필
    MQTT 사용 예제로서, Editor, Android, iOS 테스트 완료
    
    # Example Code
    // PxMQTT.Connect("broker.emqx.io", 1883, "User", "1234");
    // PxMQTT.Publish("topic", "Hello World");
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.NetworkInformation; //MAC Address
// Nuget Install : MQTTNet, MQTTNet Extensions ManageClient
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;

public class PxMQTT
{
    private static MqttFactory mqttFactory = new MqttFactory();
    private static IManagedMqttClient client;

    // 접속 정보
    private static string _host = null; //Test Online Broker: broker.emqx.io
    private static int _port = 1883;
    private static string _id = "test";
    private static string _password = "test";

    // Subscribe Callback
    private static List<string> topics = new List<string>();
    public delegate void CallbackEventHandler(string topic, string msg);
    public static event CallbackEventHandler Callback; 

    public static async void Connect(string host, int port, string id, string password)
	{
        if ( _host != null && _host != host ) {
            // Host가 변경 됨.
            Disconnect();
        }

        _host = host;
        _port = port;
        _id = id;
        _password = password;

        try {
            ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder()
                 .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                 .WithClientOptions(new MqttClientOptionsBuilder()
                     .WithClientId(Guid.NewGuid().ToString().Substring(0, 13))
                     .WithTcpServer(host, port)
                     .WithCredentials(id, password)
                     .Build()
                 )
                 .Build();

            Debug.Log( "Builded" );

            client = mqttFactory.CreateManagedMqttClient();

            await client.StartAsync(options);
        } catch (Exception e) {
            Debug.Log( e );
        }
	}

    public static async void Disconnect() 
    {
        foreach(string topic in topics) {
            await client.UnsubscribeAsync(topic);
        }
        await client.StopAsync();

        Callback = null;

        topics.Clear();
    }

    //모든 인증서 허용
    public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		return true;
	}

    // 구독(Subscribe)한 토픽(들)에게서 메세지를 수신했을 때, 실행되는 콜백함수
    private static void ReceivedCallback(MqttApplicationMessageReceivedEventArgs e)
	{
		string msg = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        // Debug.Log ("Received message from " + e.ApplicationMessage.Topic + " : " + msg);

        Callback( e.ApplicationMessage.Topic, msg );
	}

    public static async void Subscribe(string topic, int qos = 0)
    {
        client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(ReceivedCallback);

        try {
            switch( qos ) {
                case 0:
                default:
                    await client.SubscribeAsync(new MqttTopicFilter { Topic = topic, QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce });
                break;
                case 1:
                    await client.SubscribeAsync(new MqttTopicFilter { Topic = topic, QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce });
                break;
                case 2:
                    await client.SubscribeAsync(new MqttTopicFilter { Topic = topic, QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce });
                break;
            }
        } catch ( Exception e ) {
            Debug.Log( e );
        }

        topics.Add(topic);
    }

    public static async void Publish(string topic, string msg, int qos = 0)
	{
        try {
            switch( qos ) {
                case 0:
                default:
                    await client.PublishAsync(builder => builder.WithTopic(topic).WithPayload(msg).WithAtMostOnceQoS());
                break;
                case 1:
                    await client.PublishAsync(builder => builder.WithTopic(topic).WithPayload(msg).WithAtLeastOnceQoS());
                break;
                case 2:
                    await client.PublishAsync(builder => builder.WithTopic(topic).WithPayload(msg).WithExactlyOnceQoS());
                break;
            }
        } catch ( Exception e ) {
            Debug.Log( e );
        }
	}
}
