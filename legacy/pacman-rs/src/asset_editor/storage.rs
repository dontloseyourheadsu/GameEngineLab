use serde::{Deserialize, Serialize};
use engine_core::storage::binary_storage::{deserialize_from_binary, serialize_to_binary};

#[derive(Clone, Serialize, Deserialize)]
pub struct AssetGroup {
    pub name: String,
    pub assets: Vec<AssetData>,
    pub is_done: bool,
}

#[derive(Clone, Serialize, Deserialize)]
pub struct AssetData {
    pub name: String,
    pub width: i32,
    pub height: i32,
    pub frames: Vec<Vec<u8>>, // Raw RGBA data
}

impl AssetGroup {
    pub const STORE_PATH: &'static str = "data/assets.bin";

    pub fn new_empty(name: String, definitions: &[super::models::AssetDef]) -> Self {
        let assets = definitions.iter().map(|def| {
            AssetData {
                name: def.name.to_string(),
                width: 256, // Default canvas size
                height: 256,
                frames: vec![vec![0; (256 * 256 * 4) as usize]; def.frames],
            }
        }).collect();

        Self {
            name,
            assets,
            is_done: false,
        }
    }

    pub fn check_is_done(&self) -> bool {
        for asset in &self.assets {
            for frame in &asset.frames {
                if is_frame_empty(frame) {
                    return false;
                }
            }
        }
        true
    }
}

pub fn is_frame_empty(data: &[u8]) -> bool {
    // Check if all pixels are technically blank/transparent
    // Raylib Color::BLANK is 0,0,0,0
    // We iterate by 4 (RGBA)
    for chunk in data.chunks(4) {
        if chunk.len() == 4 {
            // Check Alpha
            if chunk[3] != 0 {
                return false;
            }
        }
    }
    true
}

pub fn save_asset_groups(groups: &[AssetGroup]) {
    // Need to convert slice to Vec or pass reference to something Sized implementing Serialize
    // Vec<AssetGroup> implements Serialize. &[AssetGroup] (slice) implies [AssetGroup], unsized.
    let groups_vec = groups.to_vec();
    if let Ok(data) = serialize_to_binary(&groups_vec) {
        #[cfg(not(target_arch = "wasm32"))]
        {
            use engine_core::storage::desktop_storage::desktop_utils::save_file_to_disk;
            if let Err(e) = save_file_to_disk(AssetGroup::STORE_PATH, &data) {
                eprintln!("Failed to save asset groups: {}", e);
            }
        }

        #[cfg(target_arch = "wasm32")]
        {
            use engine_core::storage::wasm_storage::web_utils::save_file_to_local_storage;
            let key = "assets.bin";
            if let Err(_) = save_file_to_local_storage(key, &data) {}
        }
    }
}

pub fn load_stored_asset_groups() -> Vec<AssetGroup> {
    #[cfg(not(target_arch = "wasm32"))]
    {
        use engine_core::storage::desktop_storage::desktop_utils::load_file_from_disk;
        if let Ok(data) = load_file_from_disk(AssetGroup::STORE_PATH) {
            if let Ok(groups) = deserialize_from_binary::<Vec<AssetGroup>>(&data) {
                return groups;
            }
        }
    }

    #[cfg(target_arch = "wasm32")]
    {
        use engine_core::storage::wasm_storage::web_utils::load_file_from_local_storage;
        let key = "assets.bin";
        if let Ok(Some(data)) = load_file_from_local_storage(key) {
           if let Ok(groups) = deserialize_from_binary::<Vec<AssetGroup>>(&data) {
                return groups;
            }
        }
    }

    Vec::new()
}
