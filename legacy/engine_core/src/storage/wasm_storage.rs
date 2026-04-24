#[cfg(target_arch = "wasm32")]
pub mod web_utils {
    use base64::prelude::{BASE64_STANDARD, Engine as _};
    use wasm_bindgen::prelude::*;

    const SUPPORTED_EXTENSIONS: &[&str] = &[".json", ".bin"];

    /// Stores a file content (as bytes) in the browser's LocalStorage encoded as Base64.
    /// Only allows saving files with supported extensions (.json, .bin).
    pub fn save_file_to_local_storage(key: &str, data: &[u8]) -> Result<(), JsValue> {
        if !SUPPORTED_EXTENSIONS.iter().any(|ext| key.ends_with(ext)) {
            return Err(JsValue::from_str(
                "Unsupported file extension. Only .json and .bin are allowed.",
            ));
        }

        let window = web_sys::window().ok_or_else(|| JsValue::from_str("No global window"))?;
        let storage = window
            .local_storage()?
            .ok_or_else(|| JsValue::from_str("No local storage"))?;

        // LocalStorage stores strings, so we encode binary data to Base64
        let encoded = BASE64_STANDARD.encode(data);
        storage.set_item(key, &encoded)?;
        Ok(())
    }

    /// Recovers a file content (as bytes) from the browser's LocalStorage.
    pub fn load_file_from_local_storage(key: &str) -> Result<Option<Vec<u8>>, JsValue> {
        let window = web_sys::window().ok_or_else(|| JsValue::from_str("No global window"))?;
        let storage = window
            .local_storage()?
            .ok_or_else(|| JsValue::from_str("No local storage"))?;

        if let Some(encoded) = storage.get_item(key)? {
            let decoded = BASE64_STANDARD
                .decode(encoded)
                .map_err(|e| JsValue::from_str(&format!("Base64 decode error: {}", e)))?;
            Ok(Some(decoded))
        } else {
            Ok(None)
        }
    }
}
