// Test file to check raylib-rs audio support
use raylib::prelude::*;

pub fn test_audio_formats() {
    let (mut rl, thread) = raylib::init().size(800, 600).title("Audio Test").build();

    rl.init_audio_device();

    println!("Testing audio format support:");

    // Test WAV support
    if let Ok(_sound) = rl.load_sound(&thread, "test.wav") {
        println!("✓ WAV format supported");
    } else {
        println!("✗ WAV format not supported or file missing");
    }

    // Test OGG support
    if let Ok(_sound) = rl.load_sound(&thread, "test.ogg") {
        println!("✓ OGG format supported");
    } else {
        println!("✗ OGG format not supported or file missing");
    }

    // Test MP3 support
    if let Ok(_sound) = rl.load_sound(&thread, "test.mp3") {
        println!("✓ MP3 format supported");
    } else {
        println!("✗ MP3 format not supported or file missing");
    }

    rl.close_audio_device();
}
