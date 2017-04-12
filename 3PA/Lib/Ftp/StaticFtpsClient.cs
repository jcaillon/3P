#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (StaticFtpsClient.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System.Collections.Generic;

namespace _3PA.Lib.Ftp {

    public sealed partial class FtpsClient {

        private static FtpsClients _instance;

        public static FtpsClients Instance {
            get { return _instance ?? (_instance = new FtpsClients()); }
        }

        public class FtpsClients {

            private Dictionary<string, FtpsClient> _ftpClients = new Dictionary<string, FtpsClient>();

            private void DisconnectFtp() {
                foreach (var ftpsClient in _ftpClients) {
                    ftpsClient.Value.Close();
                }
                _ftpClients.Clear();
            }

            public FtpsClient Get(string id) {
                if (!_ftpClients.ContainsKey(id)) {
                    Set(id, new FtpsClient());
                }
                return _ftpClients[id];
            }

            public void Set(string id, FtpsClient client) {
                if (_ftpClients.ContainsKey(id)) {
                    _ftpClients[id] = client;
                } else {
                    if (_ftpClients.Count == 0) {
                        // dispose of the ftp on shutdown
                        Plug.OnShutDown += DisconnectFtp;
                    }
                    _ftpClients.Add(id, client);
                }
            }

        }

    }
}
