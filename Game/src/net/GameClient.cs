using ENet;

namespace Game {

    public class GameClient {
        bool enetInitialized = false;
        Host client = new Host();
        Peer? server = null;

        public bool ConnectToServer( string strAddress, int port ) {
            if ( enetInitialized == false && Library.Initialize() == false ) {
                Console.WriteLine( "An error occurred while initializing ENet." );
                return false;
            }

            enetInitialized = true;

            client.Create();

            Address address = new Address();
            address.SetHost( strAddress );
            address.Port = (ushort)port;

            Peer peer = client.Connect(address);
            Event netEvent;
            if ( client.Service( 1000, out netEvent ) > 0 && netEvent.Type == EventType.Connect ) {
                Console.WriteLine( "Connected to server at {0}:{1}", strAddress, port );
                server = peer;
                return true;
            }
            else {
                Console.WriteLine( "Could not connect to server at {0}:{1}", strAddress, port );
                client.Flush();
                client.Dispose();
                return false;
            }
        }

        public void NetoworkDisconnectFromServer() {
            if ( server.HasValue && client.IsSet ) {
                server.Value.Disconnect( 0 );
                Event netEvent;
                bool disconnected = false;
                while ( !disconnected && client.Service( 1000, out netEvent ) > 0 ) {
                    switch ( netEvent.Type ) {
                        case EventType.Disconnect: {
                            Console.WriteLine( "Disconnected from server" );
                            disconnected = true;
                        }
                        break;
                        case EventType.Timeout: {
                            Console.WriteLine( "Connection timeout" );
                            disconnected = true;
                        }
                        break;
                    }
                }

                server = null;
            }

            if ( client.IsSet ) {
                client.Dispose();
            }
        }

        public bool NetworkPoll( byte[] packetData ) {
            if ( client.IsSet && server.HasValue ) {
                Event netEvent;
                if ( client.Service( 0, out netEvent ) >= 0 ) {
                    switch ( netEvent.Type ) {
                        case EventType.Connect: {
                            Console.WriteLine( "Client connected to server - ID: " + netEvent.Peer.ID );
                        }
                        break;
                        case EventType.Disconnect: {
                            Console.WriteLine( "Client disconnected from server - ID: " + netEvent.Peer.ID );
                            server = null;
                            client.Flush();
                            client.Dispose();
                        }
                        break;
                        case EventType.Timeout: {
                            Console.WriteLine( "Client connection timeout - ID: " + netEvent.Peer.ID );
                            server = null;
                            client.Flush();
                            client.Dispose();
                        }
                        break;
                        case EventType.Receive: {
                            //Console.WriteLine("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                            netEvent.Packet.CopyTo( packetData );
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool NetworkIsConnected() {
            return server.HasValue;
        }

        public void NetworkSendPacket( byte[] packetData, bool reliable ) {
            if ( server.HasValue ) {
                Packet netPacket = default;
                netPacket.Create( packetData, reliable ? PacketFlags.Reliable : PacketFlags.None );
                server.Value.Send( 0, ref netPacket );
            }
        }

        public int NetworkGetPing() {
            if ( server.HasValue ) {
                return (int)server.Value.RoundTripTime;
            }

            return 0;
        }
    }
}
