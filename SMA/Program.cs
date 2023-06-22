// See https://aka.ms/new-console-template for more information
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Text;
using SMA;


int port = 9522;
string address = "239.12.255.254";





var bytes = (new byte[]
{
    83,77,65, // SMA
    0,0,4,2,160,0,0,0,
    1,2,76,
    0,16, // SMA NET 2
    96,105, // Protocol Id
    1,245,
    179,184,167,180, // SerialNumber
    73,32,69,234, // Time

    // Obis Block (2 bytes measured type/channel & 2 bytes type (counter = 8, actual = 4, version = 0)
    // actual has 4 byte of data
    // counter has 8 byte of data
    0,1,4,0, 
    0,0,0,74,

    0,1,8,0,
    0,0,0,0,129,48,15,168,

    0,2,4,0,
    0,0,0,0,

    0,2,8,0,
    0,0,0,1,127,246,107,104,

    0,3,4,0,
    0,0,81,230,

    0,3,8,0,
    0,0,0,0,69,211,137,48,

    0,4,4,0,
    0,0,0,0,

    0,4,8,0,
    0,0,0,0,180,122,107,176,

    0,9,4,0,
    0,0,81,230,

    0,9,8,0,
    0,0,0,0,199,206,254,16,

    0,10,4,0,
    0,0,0,0,

    0,10,8,0,
    0,0,0,1,184,204,44,152,

    0,13,4,0,0,0,0,4,0,14,4,0,0,0,195,87,0,21,4,0,0,0,53,205,0,21,8,0,0,0,0,0,98,221,230,216,0,22,4,0,0,0,0,0,0,22,8,0,0,0,0,0,221,242,234,128,0,23,4,0,0,0,0,155,0,23,8,0,0,0,0,0,19,224,36,96,0,24,4,0,0,0,0,0,0,24,8,0,0,0,0,0,121,195,252,152,0,29,4,0,0,0,53,206,0,29,8,0,0,0,0,0,135,217,66,152,0,30,4,0,0,0,0,0,0,30,8,0,0,0,0,0,244,99,68,24,0,31,4,0,0,0,22,109,0,32,4,0,0,3,174,139,0,33,4,0,0,0,3,232,0,41,4,0,0,0,0,0,0,41,8,0,0,0,0,0,124,143,74,64,0,42,4,0,0,0,105,71,0,42,8,0,0,0,0,0,164,3,178,120,0,43,4,0,0,0,74,218,0,43,8,0,0,0,0,0,30,77,157,216,0,44,4,0,0,0,0,0,0,44,8,0,0,0,0,0,53,143,79,160,0,49,4,0,0,0,0,0,0,49,8,0,0,0,0,0,135,231,9,120,0,50,4,0,0,0,129,45,0,50,8,0,0,0,0,0,171,25,51,8,0,51,4,0,0,0,53,164,0,52,4,0,0,3,180,13,0,53,4,0,0,0,3,47,0,61,4,0,0,0,51,196,0,61,8,0,0,0,0,0,27,60,229,104,0,62,4,0,0,0,0,0,0,62,8,0,0,0,0,0,119,121,216,24,0,63,4,0,0,0,6,113,0,63,8,0,0,0,0,0,24,25,112,64,0,64,4,0,0,0,0,0,0,64,8,0,0,0,0,0,9,154,197,240,0,69,4,0,0,0,52,42,0,69,8,0,0,0,0,0,33,202,28,232,0,70,4,0,0,0,0,0,0,70,8,0,0,0,0,0,123,47,0,160,0,71,4,0,0,0,22,69,0,72,4,0,0,3,162,104,0,73,4,0,0,0,3,224,144,0,0,0,2,12,5,82,0,0,0,0
}).AsSpan();




IPAddress mcastAddress = IPAddress.Parse(address);

UdpClient udpClient = new(9522, AddressFamily.InterNetwork);
udpClient.JoinMulticastGroup(mcastAddress);
IPEndPoint endpoint = new (IPAddress.Any, 0);


int counter = 0;
while (true)
{
    var file = $"/Users/mbux/Downloads/{DateTime.Now:yyyy-MM-dd}.txt";
    counter++;
    var receiveBytes = udpClient.Receive(ref endpoint);
    if (receiveBytes.Length != 608) continue;
    var telegram = new EnergyMeterTelegram(receiveBytes);


    DataPoint? consume = null;
    DataPoint? sell = null;
    DataPoint? l1 = null;
    DataPoint? l2 = null;
    DataPoint? l3 = null;


    foreach (var item in telegram.DataPoints)
    {
        switch (item.Key)
        {
            case ObisIdentifier i when i == DataPoint.PowerConsume:
                consume = item.Value;
                break;
            case ObisIdentifier i when i == DataPoint.PowerSell:
                sell = item.Value;
                break;
            case ObisIdentifier i when i == DataPoint.L1Voltage:
                l1 = item.Value;
                break;
            case ObisIdentifier i when i == DataPoint.L2Voltage:
                l2 = item.Value;
                break;
            case ObisIdentifier i when i == DataPoint.L3Voltage:
                l3 = item.Value;
                break;
        };


        //data += $"{item.Value.Value:0.00};";
        //consoleData += $"{dp.Identifier} : {item.Value.Value,8:0.00} {item.Value.ValueSuffix} \t";
    }
    var data = $"{DateTime.Now};{consume?.Value:0.00};{sell?.Value:0.00};{l1?.Value:0.00};{l2?.Value:0.00};{l3?.Value:0.00}\n";
    var consoleData = $"{DateTime.Now}\t{telegram.SerialNumber}\t{consume,8:0.00}\t{sell,8:0.00}\t{l1,8:0.00}\t{l2,8:0.00)}\t{l3,8:0.00}\n";



    Console.Write(consoleData);

    if (!File.Exists(file)) {
        File.WriteAllText(file, "Time;Consume;Sell;L1V;L2V;L3V\n");
    }

    File.AppendAllText(file, data);
}

udpClient.DropMulticastGroup(mcastAddress);

//int counter = 0;
//while(counter < 20)
//{
//    IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
//    var receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
//    string returnData = Encoding.UTF8.GetString(receiveBytes);
//    Console.WriteLine(returnData);
//    counter++;
//}

udpClient.Close();



