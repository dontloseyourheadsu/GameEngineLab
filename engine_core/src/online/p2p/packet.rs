use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Debug, Clone)]
pub enum Packet {
    /// Initial handshake: Send public key
    Hello { public_key_pem: String },
    /// Acknowledge hello and send own public key
    HelloAck { public_key_pem: String },
    /// Send the symmetric session key (encrypted with receiver's public key)
    SessionKey {
        encrypted_key: Vec<u8>,
        nonce: Vec<u8>, // IV for the key decryption if needed, or if using hybrid
    },
    /// Acknowledge session key reception
    SessionKeyAck,
    /// Encrypted payload
    Data { ciphertext: Vec<u8>, nonce: Vec<u8> },
    /// Heartbeat to keep connection alive
    Heartbeat,
    /// Disconnect message
    Disconnect,
}
