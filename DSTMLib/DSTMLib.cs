﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIDSTM;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Serialization.Formatters;

namespace PADIDSTM
{
    public class DSTMLib
    {

        public static IMaster masterServ;
        public static string transactionTS;
        public static List<RemotePadInt> visitedPadInts;

        public static bool Init() {

            TcpChannel channel = new TcpChannel(0);
            ChannelServices.RegisterChannel(channel, true);
            masterServ = (IMaster)Activator.GetObject(
                                    typeof(IMaster),
                                "tcp://localhost:8086/RemoteMaster");


            return true;
            
        }

        public static bool TxBegin() {
            int tID = masterServ.getTransactionID();
            string timeStamp = masterServ.GetTS(tID);
            transactionTS = timeStamp;
            visitedPadInts = new List<RemotePadInt>();
            return true;
        }

        public static bool TxCommit() { return true; }

        public static bool TxAbort() {
            long tc = Convert.ToInt64(transactionTS.Split('#')[0]);
            foreach (RemotePadInt rpi in visitedPadInts)
            {
                rpi.abortTx(tc);
            }
            return true;
        
        }

        public static bool Status() { 
            masterServ.callStatusOnSlaves();
            return true;
        }

        public static bool Fail(string url)
        {
            ISlave slave = (ISlave)Activator.GetObject(
                typeof(ISlave),
                url);
            slave.fail();
            return true;
        }

        public static bool Freeze(string url) {
            ISlave slave = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);
            slave.freeze();
            return true;
        }

        public static bool Recover(string url)
        {
            ISlave slave = (ISlave)Activator.GetObject(
                  typeof(ISlave),
                  url);
            slave.recover();
            return true;  
        }

        public static RemotePadInt CreatePadInt(int uid) {
            string url = masterServ.GetLocationNewPadInt(uid);
            if (url == null)
                return null;
            ISlave slave = (ISlave)Activator.GetObject(
                                   typeof(ISlave),
                               url);

            RemotePadInt newPadInt = slave.create(uid);
            return newPadInt;
        }

        public static RemotePadInt AccessPadInt(int uid) {
            string url = masterServ.DiscoverPadInt(uid);
            if (url == null)
                return null;
            ISlave slave = (ISlave)Activator.GetObject(
                                  typeof(ISlave),
                              url);

            RemotePadInt pint = slave.access(uid);
            return pint;

        }
    
    }

}
