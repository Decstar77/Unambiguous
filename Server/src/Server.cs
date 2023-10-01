using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using Shared;

namespace Server {

    public class GamePeer {
        public Peer peer;
        public int playerNumber;

        public GamePeer( Peer p, int pn ) {
            peer = p;
            playerNumber = pn;
        }
    }

    public class GameSession {
        public Guid             id = Guid.NewGuid();
        public List<GamePeer>   peers = new List<GamePeer>();
    }

    public class Server {
        public static void Main( string[] args ) {
            if( Library.Initialize() == false ) {
                Console.WriteLine( "An error occurred while initializing ENet." );
                return;
            }

            Console.WriteLine( "ENet initialized." );

            Address address = new Address();
            address.Port = 27164;

            Host server = new Host();
            server.Create( address, 32 );

            Console.WriteLine( "Server created at {0}:{1}", address.GetIP(), address.Port );

            Peer? holdingPeer = null;
            List<GameSession> gameSessions = new List<GameSession>();

            while( !Console.KeyAvailable ) {
                bool polled = false;

                while( !polled ) {
                    Event netEvent;
                    if( server.CheckEvents( out netEvent ) <= 0 ) {
                        if( server.Service( 30, out netEvent ) <= 0 ) {
                            break;
                        }

                        polled = true;
                    }

                    switch( netEvent.Type ) {
                        case EventType.None:
                            break;

                        case EventType.Connect: {
                            Console.WriteLine( "Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP );
                            if( holdingPeer != null ) {
                                GameSession session = new GameSession();
                                session.peers.Add( new GamePeer( holdingPeer.Value, 1 ) );
                                session.peers.Add( new GamePeer( netEvent.Peer, 2 ) );
                                gameSessions.Add( session );

                                holdingPeer = null;

                                session.peers.ForEach( p => {
                                    byte[] data = new byte[2];
                                    data[0] = (byte)GamePacketType.MAP_START;
                                    data[1] = (byte)p.playerNumber;

                                    Packet packet = default(Packet);
                                    packet.Create( data, PacketFlags.Reliable );

                                    p.peer.Send( netEvent.ChannelID, ref packet );
                                } );

                                Console.WriteLine( $"Game session started {session.id}" );
                            }
                            else {
                                holdingPeer = netEvent.Peer;
                            }
                        }
                        break;

                        case EventType.Disconnect:
                            Console.WriteLine( "Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP );
                            break;

                        case EventType.Timeout:
                            Console.WriteLine( "Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP );
                            break;

                        case EventType.Receive: {
                            GameSession? session = gameSessions.FirstOrDefault(s => s.peers.Any(p => p.peer.ID == netEvent.Peer.ID));

                            byte[] buffer = new byte[netEvent.Packet.Length];
                            netEvent.Packet.CopyTo( buffer );

                            if( session != null ) {
                                session.peers.ForEach( p => {
                                    if( p.peer.ID != netEvent.Peer.ID ) {
                                        Packet packet = default(Packet);
                                        packet.Create( buffer );
                                        p.peer.Send( netEvent.ChannelID, ref packet );
                                    }
                                } );
                            }

                            netEvent.Packet.Dispose();
                        }
                        break;
                    }
                }
            }

            Console.WriteLine( "Server stopped" );

            server.Flush();
            server.Dispose();
            Library.Deinitialize();
        }
    }
}
