# Unity Custom Track Generation Architecture

## Overview

The Unity Custom Track Generation tool is a modular procedural system designed to rapidly build customizable track layouts directly within the Unity editor.

The system currently supports:
- Spline-based track generation
- Parameter-driven loop generation
- Automatic snapping between track components
- Dynamic endcap management
- Shared global configuration with optional per-instance overrides

The architecture separates generation logic, configuration, geometry construction, and editor interaction into distinct, maintainable layers.

---

## Design Goals

- Rapid iteration inside the Unity editor  
- Modular system for adding new track generation tools
- Reusable shared generation pipeline  
- Consistent styling via global configuration  
- Flexible per-component customization  
- Clear separation of responsibilities  

---

## System Architecture

The system is composed of several types of classes:

- Editor Interaction
- Generation Orchestrator
- Generation Drivers 
- Data Containers
- GameObject Instantiators

---

## Architectural Components

### Editor Interaction

Handles all user-facing interaction within the Unity editor.

**Responsibilities:**
- Inspector UI controls  
- Generation triggers  
- Snapping behavior between track components  
- Connection point logic  

**Classes Included:**
- `ConnectionPoint`
- `TrackShapeEditor`
- `TrackGenerationInspectorButton`
- `TrackGenerationSettingsDrawer`

---

### Generation Orchestrator

Defines the shared generation workflow used by all track generators.

**Responsibilities:**
- Managing generation lifecycle  
- Clearing previously generated objects  
- Creating and maintaining root objects  
- Coordinating endcap generation  
- Applying shared configuration  
- Provides helper functions for child classes

**Classes Included:**
- `TrackGenerationOrchestrator`

This acts as the central pipeline through which all generation flows.

---

### Generation Drivers

Implements specific track generation behaviors.

#### TrackAlongSplineGenerator

Generates track geometry by sampling position and orientation along a spline.

**Responsibilities:**
- Sampling spline data  
- Driving ring-based geometry generation  
- Producing track segments along the spline path  

---

#### TrackLoopGenerator

Generates loop structures using parameter-driven geometry.

**Responsibilities:**
- Defining loop shape via parameters  
- Supporting radius, roll, direction, and loop percentage  
- Producing modular loop components  

---

### Data Containers

Handles procedural data construction and intermediate geometry representation.

**Responsibilities:**
- Generating vertex data  
- Storing intermediate mesh data  
- Defining geometry constraints  
- Supporting reusable data-driven workflows  

**Classes Included:**
- `MeshData`
- `TrackRingsData`
- `TrackRingVectorData`
- `TrackEndcapData`
- `LocalPointData`
- `TrackConstraintsData`

---

### GameObject Instantiators

Responsible for converting generated data into Unity GameObjects.

**Responsibilities:**
- Creating meshes from generated data  
- Applying materials  
- Assigning colliders  
- Producing final track components  

**Classes Included:**
- `TrackSegment`
- `TrackEndcap`

---

## Data Flow

The generation pipeline follows a consistent flow:

1. User interacts with the editor (dragging prefabs, pressing generate)  
2. Orchestrator initializes generation  
3. Selected generator defines geometry behavior  
4. Data container classes build and store required mesh data   
5. GameObject Instantiators create GameObjects  
6. Connection system enables snapping and interaction  

---

## Snapping & Connection System

The snapping system enables seamless modular track construction.

**Key behaviors:**
- Detect nearby compatible connection points  
- Align track components automatically  
- Prevent invalid or self-connections  
- Maintain relationships between connected pieces  

This system improves usability by eliminating manual alignment.

---

## Endcap Management

Endcaps are dynamically controlled based on connection state.

**Behavior:**
- Generated at exposed track ends  
- Removed when components connect  
- Restored when components separate  

This ensures visual consistency while reducing manual cleanup.

---

## Editor Workflow

Typical usage flow:

1. Drag a track prefab into the scene  
2. Configure settings in the Inspector  
3. Choose between global config or local overrides  
4. Generate the track  
5. Move and position components  
6. Snap components together  
7. Regenerate as needed  

---

## Extensibility

The system is designed to support future tools with minimal changes.

Planned additions include:
- Curves  
- Ramps  
- Intersections  
- Tunnels  

New generators can be added by inheriting from TrackGenerationOrchestrator and implementing custom geometry generation logic.

---

## Architectural Principles

- Separation of concerns across layers  
- Reusable generation pipeline  
- Data-driven geometry construction  
- Editor-first usability  
- Modular system expansion  

---

## Future Documentation

Future documentation may include:
- High-level UML diagram of current architecture  
- Detailed breakdown of generator implementations  
- Procedural mesh data flow visualization  