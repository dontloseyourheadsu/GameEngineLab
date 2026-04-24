use engine_core::storage::binary_storage::{deserialize_from_binary, serialize_to_binary};
use serde::{Deserialize, Serialize};

#[derive(Clone, Copy, Serialize, Deserialize)]
pub struct GameSettings {
    pub music_volume: f32, // 0.0 to 1.0
    pub sfx_volume: f32,   // 0.0 to 1.0
    pub scale: f32,        // 0.25 to 2.0
}

impl Default for GameSettings {
    fn default() -> Self {
        Self {
            music_volume: 0.5,
            sfx_volume: 0.5,
            scale: 1.0,
        }
    }
}

impl GameSettings {
    pub const SETTINGS_FILE_PATH: &'static str = "data/settings.bin";

    pub fn save(&self) {
        if let Ok(data) = serialize_to_binary(self) {
            #[cfg(not(target_arch = "wasm32"))]
            {
                use engine_core::storage::desktop_storage::desktop_utils::save_file_to_disk;
                if let Err(e) = save_file_to_disk(Self::SETTINGS_FILE_PATH, &data) {
                    eprintln!("Failed to save settings: {}", e);
                }
            }

            #[cfg(target_arch = "wasm32")]
            {
                use engine_core::storage::wasm_storage::web_utils::save_file_to_local_storage;
                let key = "settings.bin";
                if let Err(_) = save_file_to_local_storage(key, &data) {}
            }
        }
    }

    pub fn load() -> Self {
        #[cfg(not(target_arch = "wasm32"))]
        {
            use engine_core::storage::desktop_storage::desktop_utils::load_file_from_disk;
            if let Ok(data) = load_file_from_disk(Self::SETTINGS_FILE_PATH) {
                if let Ok(settings) = deserialize_from_binary(&data) {
                    return settings;
                }
            }
        }

        #[cfg(target_arch = "wasm32")]
        {
            use engine_core::storage::wasm_storage::web_utils::load_file_from_local_storage;
            let key = "settings.bin";
            if let Ok(Some(data)) = load_file_from_local_storage(key) {
                if let Ok(settings) = deserialize_from_binary(&data) {
                    return settings;
                }
            }
        }

        Self::default()
    }
}
