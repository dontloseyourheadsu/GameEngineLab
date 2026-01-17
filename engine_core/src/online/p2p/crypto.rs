use aes_gcm::{
    Aes256Gcm, Key, Nonce,
    aead::{Aead, KeyInit},
};
use rand::RngCore;
use rand::rngs::OsRng;
use rsa::pkcs8::{DecodePublicKey, EncodePublicKey};
use rsa::{Pkcs1v15Encrypt, RsaPrivateKey, RsaPublicKey};

pub struct CryptoState {
    pub private_key: RsaPrivateKey,
    pub public_key: RsaPublicKey,
    pub session_key: Option<Key<Aes256Gcm>>,
}

impl CryptoState {
    pub fn new() -> Self {
        let mut rng = OsRng;
        let private_key = RsaPrivateKey::new(&mut rng, 2048).expect("failed to generate key");
        let public_key = private_key.to_public_key();
        Self {
            private_key,
            public_key,
            session_key: None,
        }
    }

    pub fn get_public_key_pem(&self) -> String {
        self.public_key
            .to_public_key_pem(rsa::pkcs8::LineEnding::LF)
            .unwrap()
    }

    pub fn set_session_key(&mut self, key: Vec<u8>) {
        if key.len() == 32 {
            self.session_key = Some(*Key::<Aes256Gcm>::from_slice(&key));
        }
    }

    pub fn generate_session_key() -> Vec<u8> {
        let mut key = [0u8; 32];
        OsRng.fill_bytes(&mut key);
        key.to_vec()
    }
}

pub fn encrypt_rsa(pub_key_pem: &str, data: &[u8]) -> Option<Vec<u8>> {
    let public_key = RsaPublicKey::from_public_key_pem(pub_key_pem).ok()?;
    let mut rng = OsRng;
    public_key.encrypt(&mut rng, Pkcs1v15Encrypt, data).ok()
}

pub fn decrypt_rsa(priv_key: &RsaPrivateKey, data: &[u8]) -> Option<Vec<u8>> {
    priv_key.decrypt(Pkcs1v15Encrypt, data).ok()
}

pub fn encrypt_aes(key: &Key<Aes256Gcm>, data: &[u8]) -> (Vec<u8>, Vec<u8>) {
    let cipher = Aes256Gcm::new(key);
    let mut nonce_bytes = [0u8; 12];
    OsRng.fill_bytes(&mut nonce_bytes);
    let nonce = Nonce::from_slice(&nonce_bytes);
    let ciphertext = cipher.encrypt(nonce, data).expect("encryption failure");
    (ciphertext, nonce_bytes.to_vec())
}

pub fn decrypt_aes(key: &Key<Aes256Gcm>, ciphertext: &[u8], nonce: &[u8]) -> Option<Vec<u8>> {
    let cipher = Aes256Gcm::new(key);
    let nonce = Nonce::from_slice(nonce);
    cipher.decrypt(nonce, ciphertext).ok()
}
