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
