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
