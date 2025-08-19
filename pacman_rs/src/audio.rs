use raylib::prelude::*;

pub struct GameAudio {
    pub main_music: Option<Sound>,
    pub death_music: Option<Sound>,
    pub current_music: MusicType,
}

#[derive(PartialEq)]
pub enum MusicType {
    Main,
    Death,
    None,
}

impl GameAudio {
    pub fn new() -> Self {
        GameAudio {
            main_music: None,
            death_music: None,
            current_music: MusicType::None,
        }
    }
    
    pub fn load_audio(&mut self, rl: &mut RaylibHandle, thread: &RaylibThread) -> Result<(), String> {
        let main_music = rl.load_sound("Resources/Venari-Strigas.wav")
            .map_err(|e| format!("Failed to load main music: {:?}", e))?;
        let death_music = rl.load_sound("Resources/Once-We-were.wav")
            .map_err(|e| format!("Failed to load death music: {:?}", e))?;
            
        self.main_music = Some(main_music);
        self.death_music = Some(death_music);
        
        Ok(())
    }
    
    pub fn play_main_music(&mut self, audio: &RaylibAudio) {
        if self.current_music != MusicType::Main {
            if let Some(ref music) = self.death_music {
                audio.stop_sound(music);
            }
            
            if let Some(ref music) = self.main_music {
                audio.play_sound(music);
                self.current_music = MusicType::Main;
            }
        }
    }
    
    pub fn play_death_music(&mut self, audio: &RaylibAudio) {
        if self.current_music != MusicType::Death {
            if let Some(ref music) = self.main_music {
                audio.stop_sound(music);
            }
            
            if let Some(ref music) = self.death_music {
                audio.play_sound(music);
                self.current_music = MusicType::Death;
            }
        }
    }
    
    pub fn stop_all_music(&mut self, audio: &RaylibAudio) {
        if let Some(ref music) = self.main_music {
            audio.stop_sound(music);
        }
        if let Some(ref music) = self.death_music {
            audio.stop_sound(music);
        }
        self.current_music = MusicType::None;
    }
    
    pub fn update(&mut self, _audio: &RaylibAudio) {
        // For sounds we don't need to update like music streams
    }
}
