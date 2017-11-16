using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace ReadResult
{
    public class Pipeserver
    {
        public static string pipeName;
        public static MainWindow owner;
        private static NamedPipeServerStream pipeServer;

        private static readonly int BufferSize = 256;
        public static Invoker ownerInvoker;
        private static void ExecuteCommand(string sCommand)
        {
            Pipeserver.owner.ExecuteCommand(sCommand);
        }


        public static void createPipeServer()
        {
            Decoder decoder = Encoding.Default.GetDecoder();
            byte[] array = new byte[Pipeserver.BufferSize];
            char[] array2 = new char[Pipeserver.BufferSize];
            StringBuilder stringBuilder = new StringBuilder();
            Pipeserver.ownerInvoker.sDel = new Action<string>(Pipeserver.ExecuteCommand);
            Pipeserver.pipeName = "MultiReader";
            try
            {
                Pipeserver.pipeServer = new NamedPipeServerStream(Pipeserver.pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                while (true)
                {
                    Pipeserver.pipeServer.WaitForConnection();
                    int num;
                    do
                    {
                        stringBuilder.Length = 0;
                        do
                        {
                            num = Pipeserver.pipeServer.Read(array, 0, Pipeserver.BufferSize);
                            if (num > 0)
                            {
                                int charCount = decoder.GetCharCount(array, 0, num);
                                decoder.GetChars(array, 0, num, array2, 0, false);
                                stringBuilder.Append(array2, 0, charCount);
                            }
                        }
                        while (num > 0 && !Pipeserver.pipeServer.IsMessageComplete);
                        decoder.Reset();
                        if (num > 0)
                        {
                            string s = stringBuilder.ToString();
                            Pipeserver.ownerInvoker.Invoke(stringBuilder.ToString());
                        }
                    }
                    while (num != 0);
                    Pipeserver.pipeServer.Disconnect();
                }
            }
            catch (Exception)
            {
            }
        }

        internal static void Close()
        {
            if (Pipeserver.pipeServer == null)
            {
                return;
            }
            if (Pipeserver.pipeServer.IsConnected)
            {
                Pipeserver.pipeServer.Disconnect();
            }
            Pipeserver.pipeServer.Close();
        }
    }
}
