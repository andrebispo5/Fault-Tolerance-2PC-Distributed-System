﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PADIDSTM;

namespace DummyClient
{
    public class Program
    {
        static void Main()
        {
            bool res;
            Console.WriteLine("STARTED");
            DSTMLib.Init();
            Console.WriteLine("INITIALIZED");
            res = DSTMLib.TxBegin();
            Console.WriteLine("AFTER BEGIN");
            PadInt pi_a = DSTMLib.CreatePadInt(0);
            Console.WriteLine("AFTER createPadInt");
            //PadInt pi_b = DSTMLib.CreatePadInt(1);
            res = DSTMLib.TxCommit();

            res = DSTMLib.TxBegin();
            pi_a = DSTMLib.AccessPadInt(0);
            //pi_b = DSTMLib.AccessPadInt(1);
            pi_a.Write(36);
            //pi_b.Write(37);
            Console.WriteLine("a = " + pi_a.Read());
            //Console.WriteLine("b = " + pi_b.Read());
            DSTMLib.Status();
            // The following 3 lines assume we have 2 servers: one at port 2001 and another at port 2002
            res = DSTMLib.Freeze("tcp://localhost:2001/RemoteSlave");
            pi_a.Write(40);
            //pi_b.Write(41);
            res = DSTMLib.Status();
            res = DSTMLib.Recover("tcp://localhost:2001/RemoteSlave");
            res = DSTMLib.Status();

            res = DSTMLib.TxCommit();  
        }
    }
}
