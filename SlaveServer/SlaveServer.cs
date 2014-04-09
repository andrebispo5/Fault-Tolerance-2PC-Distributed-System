﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PADIDSTM;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;
using System.Collections;

namespace SlaveServer
{
    public class SlaveServer
    {

        private static string url;
        private static IMaster masterServ;
        
        static void Main()
        {
            bool res = login();
            Console.Read();
        }

        public static bool login()
        {

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 2001;
            TcpChannel channel = new TcpChannel(props, null, provider);

            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(RemoteSlave),
                "RemoteSlave",
                WellKnownObjectMode.Singleton);

            var channelData = (ChannelDataStore)channel.ChannelData;
            var port = new System.Uri(channelData.ChannelUris[0]).Port;
            url = "tcp://localhost:" + port + "/RemoteSlave";

            masterServ = (IMaster)Activator.GetObject(
                                    typeof(IMaster),
                                "tcp://localhost:8086/RemoteMaster");
            RemoteSlave.masterServ = masterServ;
            bool ret = masterServ.registerSlave(url);
            RemoteSlave.url = url;
            return ret;
        }
    }

    public class RemoteSlave : MarshalByRefObject, ISlave
    {
        private Dictionary<int, RemotePadInt> padIntObjects = new Dictionary<int, RemotePadInt>();
        private bool freezed=false;
        private bool failed = false;
        public static string url;
        public static IMaster masterServ;

        
        public RemotePadInt access(int uid)
        {
            while (freezed || failed) { };
            RemotePadInt req = padIntObjects[uid];
            return req;
            
        }

        public void WritePadInt(int uid, int value)
        {
            while (freezed || failed) { };
            padIntObjects[uid].value = value;
            
        }

        public int ReadPadInt(int uid)
        {
            while (freezed || failed) { };
            return 0;
        }

        public RemotePadInt create(int uid)
        {
            while (freezed || failed) { };
            RemotePadInt newPadInt = new RemotePadInt(uid, url);
            padIntObjects.Add(uid, newPadInt);
            masterServ.RegisterNewPadInt(uid, url);
            return newPadInt;
        }


        public void freeze() 
        {
            freezed = true;
        }

        public void fail()
        {
            failed = true;
        }

        public void recover() {
            freezed = false;
            failed = false;
        }
        public void status()
        {
            Console.WriteLine("\r\n");
            Console.WriteLine("STATUS ON SERVER {0}", url);
            if (freezed)
                Console.WriteLine("SERVER IS FREEZED");
            else if (failed)
                Console.WriteLine("SERVER IS FAILED");
            else
                Console.WriteLine("SERVER IS OK");
            Console.WriteLine("------------STORED OBJECTS------------");
            foreach (KeyValuePair<int, RemotePadInt> entry in padIntObjects)
            {
                Console.WriteLine("Object with id:{0} has value {1}", entry.Key, entry.Value.value);
            }
        }

    }

}
