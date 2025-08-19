# Particle Simulator RS

A unified Rust implementation combining two particle simulation systems using raylib-rs:

1. **Particle Life Simulation** - Particles with attraction/repulsion forces based on color
2. **Particle Emission System** - Physics-based particle emitters with environmental influences

This project merges the functionality of both `ParticleLife` and `ParticlesTwo` C# projects into a single, comprehensive particle simulator.

## Features

### Particle Life Simulation

- **Multi-colored particles**: Yellow, Red, and Green particles with different interaction rules
- **Attraction/Repulsion forces**: Particles attract or repel based on color combinations
- **Boundary collision**: Particles bounce off screen edges
- **Friction system**: Gradual deceleration for realistic movement

### Particle Emission System

- **Dynamic emitters**: Click to create particle emitters at mouse position
- **Physics influences**: Gravity and wind forces affect particle movement
- **Sprite-based rendering**: Fire and water textures for visual variety
- **Particle lifetime**: Particles fade and respawn naturally
- **Alpha transparency**: Realistic visual effects

### Unified Features

- **Multiple simulation modes**:
  - Life Simulation only
  - Particle Emission only
  - Mixed mode (both systems running simultaneously)
- **Interactive controls**: Keyboard shortcuts for all features
- **Real-time switching**: Change modes and settings on the fly
- **Performance optimized**: 60 FPS with hundreds of particles

## Demo

The simulator demonstrates complex particle interactions and physics:

- Life particles form clusters and patterns based on attraction rules
- Emitted particles create realistic fire/water effects with environmental physics
- Mixed mode shows both systems interacting in the same space

## Controls

### Simulation Modes

- **1**: Life Simulation mode only
- **2**: Particle Emission mode only
- **3**: Mixed mode (both systems)

### Interaction

- **Left Click**: Add particle emitter at mouse position
- **R**: Reset life simulation particles
- **C**: Clear all emitters and emitted particles

### Particle Types

- **F**: Switch to fire textures
- **W**: Switch to water textures

### Interface

- **H**: Toggle help display
- **ESC**: Exit application

## Technical Implementation

### Particle Life System

- Implementation of attraction/repulsion forces between particle groups
- Configurable force rules with distance-based calculations
- Boundary collision detection with velocity reversal
- Friction-based movement damping

### Particle Emission System

- Emitter-based particle generation with configurable parameters
- Physics influences (gravity, wind) with mass-based calculations
- Texture-based rendering with alpha transparency
- Automatic particle recycling for performance

### Unified Architecture

- Modular design allowing both systems to coexist
- Shared particle base class with type-specific behaviors
- Efficient rendering pipeline handling multiple particle types
- Real-time mode switching without performance penalties

## Resources

The `Resources/` folder contains particle sprites:

- `sfirei.png`, `sFIREII.png`, `sFIREIII.png`: Fire particle textures
- `WAT I.png`, `WAT II.png`, `WAT iii.png`: Water particle textures

## Original C# Projects

This Rust implementation unifies two separate C# projects:

- **ParticleLife**: Color-based particle interaction simulation
- **ParticlesTwo**: Physics-based particle emission system

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
cd /home/dontloseyourheadsu/Documents/GitHub/LaboratorioDeVideojuegos/particle_simulator_rs

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
