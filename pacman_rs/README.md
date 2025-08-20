# Pacman RS

A Rust implementation of the classic Pacman game using raylib-rs.

This project is a complete recreation of the original C# Pacman project, featuring maze navigation, ghost AI, power pills, and classic arcade gameplay mechanics.

## Demo

Screenshots will be added here once the game is tested.

## Features

### Core Gameplay

- **Classic Pacman mechanics**: Navigate the maze, collect pills, avoid ghosts
- **Power pills**: Temporarily turn ghosts blue and vulnerable
- **Multiple ghost types**: Red, Pink, Blue, and Orange ghosts with different AI behaviors
- **Lives system**: 3 lives with respawn mechanics
- **Scoring system**: Points for pills (1), power pills (10), and ghosts (100)

### Game Mechanics

- **Smooth movement**: Interpolated character movement for fluid animation
- **Screen wrapping**: Horizontal tunnel connections like classic Pacman
- **Ghost AI**:
  - Chase mode: Ghosts actively pursue Pacman
  - Scared mode: Ghosts flee when power pill is consumed
  - Line-of-sight detection: Ghosts can see Pacman across open corridors
- **Collision detection**: Accurate pixel-perfect collision system
- **Death and respawn**: Animation effects and proper game state management

### Visual Features

- **Sprite-based graphics**: Custom textures for Pacman and ghosts
- **Animated characters**: Running animations and directional sprites
- **Retro aesthetics**: Pixel-perfect rendering with classic arcade feel
- **Custom font**: Retro gaming font for UI elements
- **Visual feedback**: Power mode indicators, death animations

### Audio Support

- **Background music**: Looping game music
- **Sound effects**: Death sounds and power-up audio
- **Dynamic audio**: Music changes during different game states

## Controls

### Movement

- **Arrow Keys**: Move Pacman (Up, Down, Left, Right)
- **Smart movement**: Queue next direction when hitting walls

### Game Control

- **R**: Restart game (when game over or level complete)
- **ESC**: Exit game

## Technical Implementation

### Game Architecture

- **Entity-Component System**: Clean separation between Pacman, Ghosts, and Game logic
- **State Management**: Proper game state handling (playing, game over, level complete)
- **Frame-based Animation**: 60 FPS with smooth interpolation
- **Efficient Collision**: Grid-based collision detection with pixel-perfect accuracy

### AI Implementation

- **Ghost Behavior States**: Normal chase, scared flee, respawn
- **Pathfinding**: Basic line-of-sight and distance-based movement
- **Dynamic Difficulty**: Ghosts become more aggressive over time
- **Unique Personalities**: Each ghost type has distinct behavior patterns

### Rendering System

- **Texture Management**: Efficient loading and caching of game sprites
- **Smooth Animation**: Interpolated movement between grid positions
- **Layer Rendering**: Proper draw order for game elements
- **UI Overlay**: Score, lives, and game state information

## Map Layout

The game features a 15x15 grid maze with:

- **Walls (w)**: Blue barrier blocks
- **Pills (#)**: Small yellow dots worth 1 point
- **Power Pills (")**: Large yellow dots worth 10 points that make ghosts vulnerable
- **Paths**: Open spaces for movement
- **Spawn Points**: Designated areas for Pacman and ghosts

## Ghost Types

1. **Red Ghost**: Aggressive chaser, directly pursues Pacman
2. **Pink Ghost**: Ambush tactics, tries to cut off Pacman's path
3. **Blue Ghost**: Patrol behavior, guards specific map areas
4. **Orange Ghost**: Random movement with occasional aggressive bursts

## Game States

- **Playing**: Normal gameplay with movement and collision
- **Power Mode**: Ghosts turn blue and flee from Pacman
- **Death**: Pacman death animation and life reduction
- **Game Over**: All lives lost, show final score
- **Level Complete**: All pills collected, victory screen

## Resources

The `Resources/` folder contains:

- **Character sprites**: `madoka-running.gif`, `madoka-standing.gif`
- **Ghost sprites**: `red-slime-move.gif`, `pink-slime-move.gif`, etc.
- **Audio files**: `Venari-Strigas.wav`, `Once-We-were.wav`
- **Fonts**: `PressStart2P-Regular.ttf`

## Original C# Implementation

This Rust version maintains all the functionality of the original C# Windows Forms Pacman game while adding:

- **Cross-platform compatibility** through raylib
- **Better performance** with efficient Rust implementation
- **Smoother animations** with proper frame interpolation
- **Enhanced graphics** with sprite-based rendering

## Build and Run Commands

### Prerequisites:

```fish
# Install Rust if not already installed
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
source ~/.cargo/env

# Install system dependencies for raylib
sudo apt update
sudo apt install build-essential cmake libx11-dev libxrandr-dev libxinerama-dev libxcursor-dev libxi-dev libasound2-dev mesa-common-dev
```

### Build and Run:

```fish
# Navigate to project directory
cd /home/dontloseyourheadsu/Documents/GitHub/LaboratorioDeVideojuegos/pacman_rs

# Run in development mode
cargo run

# Build optimized release version
cargo build --release

# Run optimized release version
cargo run --release
```

### Additional Commands:

```fish
# Check compilation without building
cargo check

# Clean build artifacts
cargo clean

# Update dependencies
cargo update
```

The Rust implementation provides the complete Pacman experience with smooth gameplay, intelligent ghost AI, and all the classic mechanics that make the game timeless!
