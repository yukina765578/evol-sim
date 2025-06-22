# Unity 2D Evolution Simulator

A comprehensive 2D evolution simulator built in Unity 6, featuring digital creatures that evolve through genetic algorithms in a dynamic environment with realistic physics and neural network-based decision making.

## 🎯 Project Overview

This evolution simulator creates digital creatures composed of simple components that evolve over generations based on environmental pressures and survival fitness. Watch as creatures develop increasingly sophisticated behaviors through natural selection, mutation, and genetic recombination.

### Core Concept

- **Creatures**: Built from circular motors (joints) connected by line segments
- **Movement**: Motors rotate based on neural network decisions, creating propulsion
- **Survival**: Creatures must find and consume food to survive and reproduce
- **Evolution**: Successful creatures pass their genetic traits to offspring
- **Visualization**: Real-time evolution tree showing lineage and genetic progression

## 🧬 Creature Design

### Anatomy

**Motors (Circles)**:

- Act as rotational joints with configurable turning speed
- Generate thrust through rotational motion
- Consume energy during operation
- Can have sensor capabilities for environmental detection

**Segments (Lines)**:

- Connect motors to form creature body structure
- Provide drag force for movement (like fish tail in water)
- Define creature shape and size
- Affect movement efficiency and stability

**Head Motor (Special)**:

- Specialized motor with food detection sensors
- Collision detection for food consumption
- Energy intake system
- Primary survival interface

### Neural Network Brain

Each creature possesses a neural network that processes:

- **Inputs**: Food proximity, energy levels, motor states, environmental sensors
- **Processing**: Hidden layers with configurable architecture
- **Outputs**: Motor rotation commands, movement decisions

## 🌍 Environment System

### World Features

- **Bounded Environment**: Creatures contained within defined world limits
- **Food Distribution**: Perlin noise-based food spawning for realistic patterns
- **Energy Dynamics**: Food provides energy, movement consumes energy
- **Environmental Pressure**: Limited resources create survival challenges

### Food System

- **Dynamic Spawning**: Food appears based on Perlin noise algorithms
- **Energy Values**: Different food types with varying nutritional content
- **Regeneration**: Food respawns to maintain ecosystem balance
- **Spatial Distribution**: Non-uniform food placement creates environmental niches

## 🧬 Genetic Algorithm

### DNA Structure

Creature genomes encode:

- **Motor Configuration**: Number, size, and placement of motors
- **Segment Properties**: Length, stiffness, and connection patterns
- **Brain Architecture**: Neural network weights, topology, and activation functions
- **Behavioral Traits**: Aggression, exploration, energy efficiency

### Evolution Mechanics

**Selection**: Tournament selection based on fitness metrics

- Survival time
- Food consumption efficiency
- Reproductive success
- Movement effectiveness

**Crossover**: Genetic recombination between successful parents

- Motor pattern inheritance
- Brain weight averaging and mixing
- Trait combination from both parents

**Mutation**: Random genetic variations

- Motor placement adjustments
- Neural network weight perturbations
- New behavioral trait emergence
- Structural modifications
