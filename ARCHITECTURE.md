# Unity 2D Evolution Simulator - Architecture

## Project Structure Overview

This document outlines the complete architecture for the Unity 2D Evolution Simulator, following Unity best practices with a feature-based organization approach.

## Unity Assets Folder Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   ├── Creature/
│   ├── Environment/
│   ├── Evolution/
│   └── Visualization/
├── Prefabs/
│   ├── Creatures/
│   ├── Environment/
│   └── UI/
├── Materials/
├── Settings/
├── Scenes/
└── Data/
```

## Core Architecture Principles

### Component-Based Design

- **300-Word Rule**: Each script file limited to 300 words maximum
- **Single Responsibility**: Each component handles one specific task
- **Modular Systems**: Independent systems communicating through events
- **Data-Oriented Design**: ScriptableObjects for configuration and runtime optimization

### Performance Considerations

- Object pooling for creatures and food items
- Efficient collision detection for large populations
- Optimized neural network processing
- Frame-rate independent simulation updates

## System Architecture

### 1. Core System (`Scripts/Core/`)

**Purpose**: Central simulation management and control

**Components**:

- `SimulationManager`: Master controller for entire simulation
- `GameLoop`: Fixed update timing, simulation speed control
- `EventSystem`: Communication hub between all systems
- `PerformanceMonitor`: FPS tracking, population limits, optimization
- `TimeController`: Simulation speed, pause/resume functionality

**Responsibilities**:

- Initialize and coordinate all other systems
- Manage simulation state (running, paused, evolving)
- Handle performance optimization and population scaling
- Provide global event communication

### 2. Creature System (`Scripts/Creature/`)

**Purpose**: Individual creature behavior and structure

**Components**:

- `Motor`: Circle component with rotation mechanics, sensor radius, energy consumption
- `Segment`: Line component connecting motors, physics simulation
- `CreatureBody`: Container managing motor network, creature assembly
- `CreatureBrain`: Neural network controller, decision making system
- `CreatureGenome`: DNA data structure, genetic trait storage
- `SensorMotor`: Specialized head motor with food detection capabilities

**Creature Architecture**:

- **Motors**: Circular joints that rotate based on brain output
- **Segments**: Lines connecting motors, creating creature body structure
- **Head Motor**: Special motor with collision detection for food consumption
- **Brain**: Neural network processing sensor inputs, controlling motor rotations
- **Genome**: Genetic blueprint defining motor count, connections, brain weights

### 3. Environment System (`Scripts/Environment/`)

**Purpose**: World simulation and resource management

**Components**:

- `WorldManager`: Environment boundaries, physics settings
- `FoodSpawner`: Perlin noise-based food generation, distribution patterns
- `FoodItem`: Individual food behavior, energy values, respawn mechanics
- `Boundaries`: World limits, creature containment
- `EnvironmentSettings`: Configurable world parameters

**Environment Features**:

- Perlin noise food spawning for realistic distribution
- Dynamic food regeneration based on consumption
- World boundaries preventing creature escape
- Environmental pressures affecting survival

### 4. Evolution System (`Scripts/Evolution/`)

**Purpose**: Genetic algorithms and population evolution

**Components**:

- `GeneticAlgorithm`: Selection, crossover, mutation operations
- `PopulationController`: Creature spawning, generation management
- `FitnessCalculator`: Survival metrics, reproduction criteria
- `LineageTracker`: Family tree tracking, ancestry recording
- `GenerationManager`: Evolution cycle control, advancement logic

**Evolution Process**:

- Fitness evaluation based on survival time, food consumption
- Tournament selection for breeding pairs
- Genetic crossover combining parent traits
- Mutation introducing random variations
- Population replacement with offspring

### 5. Visualization System (`Scripts/Visualization/`)

**Purpose**: Data display and debugging tools

**Components**:

- `EvolutionTreeRenderer`: Family tree visualization, lineage display
- `StatsPanel`: Population statistics, generation metrics
- `CreatureDebugger`: Individual creature inspection, brain visualization
- `CameraController`: View management, creature following
- `UIManager`: Interface coordination, panel management

**Visualization Features**:

- Real-time evolution tree rendering
- Population statistics graphs
- Individual creature neural network display
- Generation progression tracking

## Data Architecture

### Settings (`Settings/`)

**ScriptableObject Configurations**:

- `SimulationSettings`: Population size, world parameters, evolution rates
- `CreatureSettings`: Motor count ranges, brain architecture, mutation rates
- `EnvironmentSettings`: Food spawn rates, world size, energy systems
- `EvolutionSettings`: Selection pressure, crossover rates, fitness weights

### Data Storage (`Data/`)

**Runtime Data Management**:

- `EvolutionHistory`: Generation records, fitness progression
- `SaveSystem`: Simulation state persistence, creature genome storage
- `StatisticsCollector`: Performance metrics, evolution data

## Communication Architecture

### Event-Driven Design

- `CreatureEvents`: Birth, death, reproduction, food consumption
- `EvolutionEvents`: Generation start/end, fitness evaluation, selection
- `UIEvents`: User interactions, display updates, debugging commands
- `SimulationEvents`: Pause/resume, speed changes, population updates

### Data Flow

1. **Input**: User settings, random genetic variations
2. **Processing**: Neural networks, genetic algorithms, physics simulation
3. **Output**: Creature behaviors, evolution statistics, visualization data

## Performance Architecture

### Optimization Strategies

- **Object Pooling**: Reuse creature and food GameObjects
- **Spatial Partitioning**: Efficient collision detection for large populations
- **LOD System**: Reduce visual complexity for distant creatures
- **Frame Distribution**: Spread expensive operations across multiple frames

### Scalability Targets

- **Small Scale**: 100-300 creatures for detailed observation
- **Medium Scale**: 500-800 creatures for balanced performance
- **Large Scale**: 1000+ creatures for long-term evolution studies

## Extension Points

### Future Feature Integration

- **Advanced Brains**: Recurrent neural networks, memory systems
- **Complex Environments**: Multi-biome worlds, environmental hazards
- **Social Behaviors**: Group dynamics, communication, cooperation
- **Advanced Genetics**: Gene expression, regulatory networks, epigenetics

This architecture provides a solid foundation for building a comprehensive evolution simulator while maintaining code organization, performance, and extensibility.
