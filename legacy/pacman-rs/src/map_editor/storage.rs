use serde::{Deserialize, Serialize};

use engine_core::storage::binary_storage::{deserialize_from_binary, serialize_to_binary};

use super::models::{
    EMPTY_CHAR, FOOD_CHAR, GHOST_CHAR, MAX_MAP_SIZE, PACMAN_CHAR, PILL_CHAR, WALL_CHAR,
};

#[derive(Clone, Serialize, Deserialize)]
pub struct MapGroup {
    pub name: String,
    pub width: usize,
    pub height: usize,
    pub data: Vec<String>,
    pub is_done: bool,
}

#[derive(Clone, Default)]
pub struct MapValidation {
    pub pacman_count: usize,
    pub ghost_count: usize,
    pub wall_count: usize,
    pub food_count: usize,
    pub pill_count: usize,
}

impl MapValidation {
    pub fn is_done(&self) -> bool {
        self.pacman_count == 1
            && self.ghost_count >= 1
            && self.wall_count >= 1
            && self.food_count >= 1
            && self.pill_count >= 1
    }

    pub fn missing_messages(&self) -> Vec<String> {
        let mut missing = Vec::new();
        if self.ghost_count == 0 {
            missing.push("Place at least 1 ghost.".to_string());
        }
        if self.pacman_count == 0 {
            missing.push("Place exactly 1 Pacman (missing).".to_string());
        } else if self.pacman_count > 1 {
            missing.push(format!(
                "Place exactly 1 Pacman (found {}).",
                self.pacman_count
            ));
        }
        if self.wall_count == 0 {
            missing.push("Place at least 1 wall.".to_string());
        }
        if self.food_count == 0 {
            missing.push("Place at least 1 food.".to_string());
        }
        if self.pill_count == 0 {
            missing.push("Place at least 1 pill.".to_string());
        }
        missing
    }
}

impl MapGroup {
    pub const STORE_PATH: &'static str = "data/maps.bin";

    pub fn new_empty(name: String, width: usize, height: usize) -> Self {
        let width = width.clamp(1, MAX_MAP_SIZE);
        let height = height.clamp(1, MAX_MAP_SIZE);
        let row = EMPTY_CHAR.to_string().repeat(width);
        let data = vec![row; height];
        Self {
            name,
            width,
            height,
            data,
            is_done: false,
        }
    }

    pub fn check_is_done(&self) -> bool {
        evaluate_map(&self.data).is_done()
    }
}

pub fn evaluate_map(data: &[String]) -> MapValidation {
    let mut validation = MapValidation::default();
    for row in data {
        for ch in row.chars() {
            match ch {
                PACMAN_CHAR => validation.pacman_count += 1,
                GHOST_CHAR => validation.ghost_count += 1,
                WALL_CHAR => validation.wall_count += 1,
                FOOD_CHAR => validation.food_count += 1,
                PILL_CHAR => validation.pill_count += 1,
                _ => {}
            }
        }
    }
    validation
}

pub fn save_map_groups(groups: &[MapGroup]) {
    let groups_vec = groups.to_vec();
    if let Ok(data) = serialize_to_binary(&groups_vec) {
        #[cfg(not(target_arch = "wasm32"))]
        {
            use engine_core::storage::desktop_storage::desktop_utils::save_file_to_disk;
            if let Err(e) = save_file_to_disk(MapGroup::STORE_PATH, &data) {
                eprintln!("Failed to save map groups: {}", e);
            }
        }

        #[cfg(target_arch = "wasm32")]
        {
            use engine_core::storage::wasm_storage::web_utils::save_file_to_local_storage;
            let key = "maps.bin";
            if let Err(e) = save_file_to_local_storage(key, &data) {
                eprintln!("Failed to save map groups: {:?}", e);
            }
        }
    }
}

pub fn load_stored_map_groups() -> Vec<MapGroup> {
    #[cfg(not(target_arch = "wasm32"))]
    {
        use engine_core::storage::desktop_storage::desktop_utils::load_file_from_disk;
        if let Ok(data) = load_file_from_disk(MapGroup::STORE_PATH) {
            if let Ok(groups) = deserialize_from_binary::<Vec<MapGroup>>(&data) {
                return groups;
            }
        }
    }

    #[cfg(target_arch = "wasm32")]
    {
        use engine_core::storage::wasm_storage::web_utils::load_file_from_local_storage;
        let key = "maps.bin";
        if let Ok(Some(data)) = load_file_from_local_storage(key) {
            if let Ok(groups) = deserialize_from_binary::<Vec<MapGroup>>(&data) {
                return groups;
            }
        }
    }

    Vec::new()
}
