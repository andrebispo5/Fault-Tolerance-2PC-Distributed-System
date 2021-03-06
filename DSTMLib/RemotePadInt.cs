﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;

namespace PADIDSTM
{
    public class RemotePadInt : MarshalByRefObject
    {
        public int uid;
        public int value;
        public string url;
        private long wts;
        List<long> rts = new List<long>();
        List<TVersion> tentativeVersions = new List<TVersion>();
        public bool isCommited;
        public long creatorTID;


        public List<long> Rts
        {
            get { return rts; }
            set { rts = value; }
        }

        public long Wts
        {
            get { return wts; }
            set { wts = value; }
        }


        public RemotePadInt(int uid, string url)
        {
            this.uid = uid;
            this.url = url;
            this.wts = 0;
            this.isCommited = false;
        }

        public int Read(string ts)
        {
            long tc = Convert.ToInt64(ts.Split('#')[0]);
            int tieBreaker = Convert.ToInt32(ts.Split('#')[1]);
            ISlave server = (ISlave)Activator.GetObject(
                                    typeof(ISlave),
                                url);
            //JUST FOR INITIALIZING. WILL ALWAYS BE OVERRIDED OR TRANSACTION WILL ABORT
            int value;
            if (tc > this.wts)
            {
                TVersion dSelect = getMax(tc);
                if (dSelect == null)
                {
                    rts.Add(tc);
                    server.checkStatus();
                    return 0;
                }
                if (dSelect.commited || dSelect.writeTS == tc)
                {
                    server.checkStatus();//to block
                    value = dSelect.versionVal;
                    rts.Add(tc);
                }
                else
                {
                    //WAITING FOR THE DSELECT TO COMMIT
                    Thread.Sleep(4000);
                    if (dSelect.writeTS == this.wts)
                    {
                        server.checkStatus();//to block
                        value = dSelect.versionVal;
                        rts.Add(tc);
                    }
                    else
                        throw new TxException("Timeout when waiting for a write commit.");

                }
            }
            else
                throw new TxException("Reading canceled because value is outdated");

            return value;
        }

        public bool Write(int value, string ts)
        {
            Console.WriteLine("uid:" + uid + " ts: " + ts);
            string[] txID = ts.Split('#');

            long tc = Convert.ToInt64(txID[0]);
            //NOT USED FOR CHECKPOINT IMPLEMENTATION
            int tieBreaker = Convert.ToInt32(ts.Split('#')[1]);
            ISlave server = (ISlave)Activator.GetObject(
                                    typeof(ISlave),
                                url);
            long maxD = Int64.MinValue;
            if (rts.Count > 0)
                maxD = rts.Max();
            if (tc >= maxD && tc > wts)
            {
                server.checkStatus();
                tentativeVersions.Add(new TVersion(tc, value));
                this.value = value;
                return true;
            }
            else
                return false;
        }

        private TVersion getMax(long ts)
        {
            TVersion max = null;
            long maxLong = 0;
            foreach (TVersion v in tentativeVersions)
            {
                if (v.writeTS <= ts && v.writeTS >= maxLong)
                {
                    maxLong = v.writeTS;
                    max = v;
                }

            }
            return max;
        }

        public void abortTx(long txID)
        {
            List<TVersion> toRemove = new List<TVersion>();
            foreach (TVersion tv in tentativeVersions)
            {
                if (tv.writeTS == txID)
                    toRemove.Add(tv);
            }
            foreach (TVersion tv in toRemove)
            {
                tentativeVersions.Remove(tv);
            }
        }

        public void commitTx(long txID)
        {
            foreach (TVersion tv in tentativeVersions)
            {
                if (tv.writeTS == txID)
                {
                    wts = txID;
                    value = tv.versionVal;
                    tv.commited = true;
                }
            }
        }

        public void commitPadInt(long txID) {
            this.isCommited = true;
        }
    }
}
