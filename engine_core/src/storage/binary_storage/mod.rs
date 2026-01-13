use serde::{Serialize, de::DeserializeOwned};
use std::fs::File;
use std::io::{Read, Write};
use std::path::Path;

/// Serialize a generic type to a compact binary format.
pub fn serialize_to_binary<T: Serialize>(data: &T) -> bincode::Result<Vec<u8>> {
    bincode::serialize(data)
}

/// Deserialize a generic type from a compact binary format.
pub fn deserialize_from_binary<T: DeserializeOwned>(data: &[u8]) -> bincode::Result<T> {
    bincode::deserialize(data)
}

/// Specific utility to convert a serde_json::Value to binary.
pub fn json_to_binary(json: &serde_json::Value) -> bincode::Result<Vec<u8>> {
    bincode::serialize(json)
}

/// Specific utility to convert binary back to serde_json::Value.
pub fn binary_to_json(data: &[u8]) -> bincode::Result<serde_json::Value> {
    bincode::deserialize(data)
}

/// Save binary data to a file on disk.
pub fn save_binary_to_file(path: &Path, data: &[u8]) -> std::io::Result<()> {
    if let Some(parent) = path.parent() {
        std::fs::create_dir_all(parent)?;
    }
    let mut file = File::create(path)?;
    file.write_all(data)?;
    Ok(())
}

/// Read binary data from a file on disk.
pub fn load_binary_from_file(path: &Path) -> std::io::Result<Vec<u8>> {
    let mut file = File::open(path)?;
    let mut buffer = Vec::new();
    file.read_to_end(&mut buffer)?;
    Ok(buffer)
}
