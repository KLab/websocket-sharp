using System;
using System.Linq;
using System.Text;
using WebSocketSharp;

namespace EchoClient
{
  class MainClass
  {
    public static void Main (string[] args)
    {
      if (args.Length != 1) {
        Console.WriteLine ("Usage: EchoClient.exe ws://echo.websocket.org");
        return;
      }

      var byteData = Encoding.UTF8.GetBytes("DummyByteData");
      var stringData = "DummyStringData";
      var stringDataByte = Encoding.UTF8.GetBytes(stringData);

      bool largeData = false;
      if (largeData)
      {
        byteData = Encoding.UTF8.GetBytes(new string('a', 1 << 16));
        stringData = new string('b', 1 << 16);
        stringDataByte = Encoding.UTF8.GetBytes(stringData);
      }

      WebSocket ws = new WebSocket(args[0]);
      ws.OnOpen = (Object sender, EventArgs e) => {
        Console.WriteLine ("Connected");
        Console.Write("-> ");
      };
      ws.OnMessage = (Object sender, MessageEventArgs e) => {
        if (e.Type == Opcode.BINARY)
        {
          if (!e.RawData.SequenceEqual(byteData))
          {
            throw new Exception("Incorrect binary data");
          }
          Console.WriteLine ("Read BINARY");
        }
        else if (e.Type == Opcode.TEXT)
        {
          if (!e.RawData.SequenceEqual(stringDataByte))
          {
            throw new Exception("Incorrect string data");
          }
          Console.WriteLine ("Read TEXT");
        }
      };
      ws.OnError = (Object sender, ErrorEventArgs e) => {
        Console.WriteLine ("ERROR: " + e.Message);
        Console.Write("-> ");
        throw new Exception("OnError", e.Exception);
      };
      ws.OnClose = (Object sender, CloseEventArgs e) => {
        Console.WriteLine ("Closed " + e.Code + e.Reason + e.WasClean);
      };
 
      ws.Connect ();

      for(int i = 0; i < 10000; i++) {
        if (i % 2 == 0) ws.Send(byteData);
        else ws.Send(stringData);
        System.Threading.Thread.Sleep(10);
      }
    }
  }
}
