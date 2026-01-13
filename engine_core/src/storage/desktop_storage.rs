#[cfg(not(target_arch = "wasm32"))]
pub mod desktop_utils {
    use std::fs;
    use std::path::Path;

    const SUPPORTED_EXTENSIONS: &[&str] = &[".json", ".bin"];

    /// Stores a file content (as bytes) to the disk.
    /// Only allows saving files with supported extensions (.json, .bin).
    pub fn save_file_to_disk(path: &str, data: &[u8]) -> std::io::Result<()> {
        if !SUPPORTED_EXTENSIONS.iter().any(|ext| path.ends_with(ext)) {
            return Err(std::io::Error::new(
                std::io::ErrorKind::InvalidInput,
                "Unsupported file extension. Only .json and .bin are allowed.",
            ));
        }

        let p = Path::new(path);
        if let Some(parent) = p.parent() {
            fs::create_dir_all(parent)?;
        }
        fs::write(path, data)
    }

    /// Recovers a file content (as bytes) from the disk.
    pub fn load_file_from_disk(path: &str) -> std::io::Result<Vec<u8>> {
        fs::read(path)
    }
}
