use std::net::{UdpSocket, SocketAddr};
use std::collections::VecDeque;
use std::time::{Duration, Instant};
use crate::online::p2p::packet::Packet;
use crate::online::p2p::crypto::{CryptoState, encrypt_rsa, decrypt_rsa, encrypt_aes, decrypt_aes};

const HEARTBEAT_INTERVAL: Duration = Duration::from_secs(1);
const TIMEOUT_DURATION: Duration = Duration::from_secs(5);

#[derive(Debug, PartialEq)]
pub enum ConnectionState {
    Disconnected,
    HandshakeSent,
    HandshakeReceived,
    KeysExchanged,
    Connected,
}

pub struct P2PNode {
    pub socket: UdpSocket,
    pub crypto: CryptoState,
    pub state: ConnectionState,
    pub peer_addr: Option<SocketAddr>,
    pub peer_public_key: Option<String>,
    last_heartbeat_sent: Instant,
    last_message_received: Instant,
    pub received_messages: VecDeque<Vec<u8>>,
}

impl P2PNode {
    pub fn new(port: u16) -> std::io::Result<Self> {
        let socket = UdpSocket::bind(format!("0.0.0.0:{}", port))?;
        socket.set_nonblocking(true)?;
        Ok(Self {
            socket,
            crypto: CryptoState::new(),
            state: ConnectionState::Disconnected,
            peer_addr: None,
            peer_public_key: None,
            last_heartbeat_sent: Instant::now(),
            last_message_received: Instant::now(),
            received_messages: VecDeque::new(),
        })
    }

    pub fn connect(&mut self, addr: SocketAddr) {
        self.peer_addr = Some(addr);
        self.state = ConnectionState::HandshakeSent;
        self.send_packet(Packet::Hello {
            public_key_pem: self.crypto.get_public_key_pem(),
        });
    }

    pub fn send_message(&mut self, data: Vec<u8>) {
        if self.state != ConnectionState::Connected {
            return;
        }
        if let Some(key) = &self.crypto.session_key {
            let (ciphertext, nonce) = encrypt_aes(key, &data);
            self.send_packet(Packet::Data { ciphertext, nonce });
        }
    }

    pub fn receive_message(&mut self) -> Option<Vec<u8>> {
        self.received_messages.pop_front()
    }

    pub fn update(&mut self) {
        // Read socket
        let mut buf = [0u8; 65535];
        while let Ok((amt, src)) = self.socket.recv_from(&mut buf) {
            self.last_message_received = Instant::now();
            
            // If we don't know the peer yet, or if this is a new connection from someone else?
            // For this simple P2P, we accept connection if we are disconnected.
            // Or if it matches our current peer.
            if self.peer_addr.is_none() {
                self.peer_addr = Some(src);
            }

            if Some(src) == self.peer_addr {
                if let Ok(packet) = bincode::deserialize::<Packet>(&buf[..amt]) {
                    self.handle_packet(packet);
                }
            }
        }

        // Heartbeat
        if self.state == ConnectionState::Connected {
            if self.last_heartbeat_sent.elapsed() > HEARTBEAT_INTERVAL {
                self.send_packet(Packet::Heartbeat);
                self.last_heartbeat_sent = Instant::now();
            }

            if self.last_message_received.elapsed() > TIMEOUT_DURATION {
                println!("Connection timed out");
                self.state = ConnectionState::Disconnected;
                self.peer_addr = None;
                self.peer_public_key = None;
                self.crypto.session_key = None;
            }
        }
    }

    fn handle_packet(&mut self, packet: Packet) {
        match packet {
            Packet::Hello { public_key_pem } => {
                self.peer_public_key = Some(public_key_pem.clone());
                // Respond with our key
                self.send_packet(Packet::HelloAck { 
                    public_key_pem: self.crypto.get_public_key_pem() 
                });
                
                // If we initiated (HandshakeSent), we wait for their HelloAck. 
                // But if they initiated, we are in Disconnected or receiving state.
                if self.state == ConnectionState::Disconnected || self.state == ConnectionState::HandshakeReceived {
                     self.state = ConnectionState::HandshakeReceived;
                }
            }
            Packet::HelloAck { public_key_pem } => {
                self.peer_public_key = Some(public_key_pem);
                // We have their key. Now we can generate session key and send it.
                // Only if we define a role (e.g. initiator generates key). 
                // Or simply, whoever receives the ACK generates the key.
                // To avoid both generating keys, we could use lexicographical order of IPs or just let the one who sent Hello (Active Open) generate it. 
                // If I am HandshakeSent, I initiated.
                if self.state == ConnectionState::HandshakeSent {
                    let session_key = CryptoState::generate_session_key();
                    self.crypto.set_session_key(session_key.clone());
                    
                    if let Some(peer_key) = &self.peer_public_key {
                        if let Some(encrypted_key) = encrypt_rsa(peer_key, &session_key) {
                            self.send_packet(Packet::SessionKey {
                                encrypted_key,
                                nonce: vec![], // Not used for RSA encryption in this scheme
                            });
                             self.state = ConnectionState::KeysExchanged;
                        }
                    }
                }
                 // If I received Ack but I didn't send Hello? (Maybe simultaneous open)
            }
            Packet::SessionKey { encrypted_key, .. } => {
                // Decrypt session key
                if let Some(key) = decrypt_rsa(&self.crypto.private_key, &encrypted_key) {
                    self.crypto.set_session_key(key);
                    self.send_packet(Packet::SessionKeyAck);
                    self.state = ConnectionState::Connected;
                    println!("Session established (Receiver)");
                }
            }
            Packet::SessionKeyAck => {
                self.state = ConnectionState::Connected;
                println!("Session established (Initiator)");
            }
            Packet::Data { ciphertext, nonce } => {
                if let Some(key) = &self.crypto.session_key {
                    if let Some(plaintext) = decrypt_aes(key, &ciphertext, &nonce) {
                        self.received_messages.push_back(plaintext);
                    }
                }
            }
            Packet::Heartbeat => {
                // Just updates last_message_received (handled in recv loop)
            }
            Packet::Disconnect => {
                self.state = ConnectionState::Disconnected;
                self.peer_addr = None;
                self.peer_public_key = None;
                self.crypto.session_key = None;
            }
        }
    }

    fn send_packet(&mut self, packet: Packet) {
        if let Some(addr) = self.peer_addr {
            if let Ok(encoded) = bincode::serialize(&packet) {
                let _ = self.socket.send_to(&encoded, addr);
            }
        }
    }
}
