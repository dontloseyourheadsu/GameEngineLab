use raylib::prelude::*;

pub struct AssetDef {
    pub name: &'static str,
    pub frames: usize,
}

pub struct AssetGroupSummary {
    pub name: String,
    pub thumbnails: Vec<Texture2D>,
    pub is_done: bool,
}

pub fn get_assets_definitions() -> Vec<AssetDef> {
    vec![
        AssetDef {
            name: "Pacman",
            frames: 4,
        },
        AssetDef {
            name: "Ghost",
            frames: 1,
        },
        AssetDef {
            name: "Wall",
            frames: 1,
        },
        AssetDef {
            name: "Food",
            frames: 1,
        },
        AssetDef {
            name: "Pill",
            frames: 1,
        },
    ]
}
