﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PADIDSTM
{
    public interface IMaster
    {
        string[] DiscoverPadInt(int uid);
        string[] GetLocationNewPadInt(int uid);
        string GetTS();
        void RegisterNewPadInt(int uid, string serverURL);
        bool registerSlave(String url);
        void callStatusOnSlaves();
        void removeUID(List<int> UIDsToRemove);
        bool declareSlaveFailed(string serverUrlFailed);
        bool updateLoad(string slaveUrl, int load);
        int getLoad(string slaveUrl);
        List<int> recoverSlave();
    }
}
