# Unity Custom Track Generation

## Rapid Custom Track Prototyping

![Track Construction Demonstration](Documentation/Images/GIFs/TrackSnappingDemo.gif)

Design and connect modular track pieces in seconds -> no manual alignment, no cleanup, no friction

---

## Description

This Unity tool enables developers to rapidly prototype and build game-ready track systems with full control over geometry, materials, and structure.

Track components are created procedurally using spline traversal and vector-based calculations to drive custom mesh generation with user-defined geometry. A centralized ScriptableObject stores default track geometry parameters to ensure consistent styling across all components, while still allowing customization of individual track components as needed.

To streamline the workflow, track pieces automatically snap together in the editor when placed near valid connection points. Endcaps are dynamically removed upon connection and restored when components are separated, eliminating the need for manual cleanup.

---

## Features

- **Automatic snapping between track components**  
- **Dynamic deletion and restoration of track endcaps**  
- **Track generation along user-defined spline components**  
- **Customizable track loops**  
- **Global configuration via ScriptableObject for consistency and modularity**  
- **Per-component customization for flexible design**  
- **Procedural mesh generation for parameter-defined track geometry**  

---

## What's Next?

In the near future, additional tools will be implemented, including:

- Curves  
- Ramps  
- Intersections  
- Tunnels  
- ... and more  

---

## License

**© 2026 James McAdams — All Rights Reserved.**  
This project is **proprietary** and **not open source**.

You may view this code for evaluation or portfolio-review purposes, but you may **not** copy, modify, use, or distribute any part of it without explicit written permission.

---

## Creator

**James McAdams (2026)**

---

## Documentation

### How would a developer use this tool?

---

## Prefabs

![Prefabs Location](Documentation/Images/Screenshots/PrefabsHighlightScreenshot.png)

Navigate to the **Prefabs** folder and drag-and-drop track components directly into the scene.

![Prefab Options](Documentation/Images/Screenshots/PrefabsScreenshot.png)

The folder currently includes two prefabs:
- **TrackAlongSpline**
- **TrackLoop**

Both can be placed directly into the scene and configured through the Inspector.

---

## Track Along Spline

![Track Along Spline Demonstration](Documentation/Images/GIFs/TrackAlongSplineDemo.gif)

Define track layouts using spline control points, then procedurally generate fully customizable track geometry along the path.

![Track Along Spline Settings](Documentation/Images/Screenshots/TrackAlongSplineSettingScreenshot.png)

When *Use Config* is disabled, track and rail geometry can be fully customized per component. Mesh resolution and materials can also be adjusted to match the style of any project.

---

## Track Loop

![Track Loop Demonstration](Documentation/Images/GIFs/TrackLoopDemo.gif)

Quickly generate loop structures and seamlessly integrate them into existing track systems with fully customizable loop geometry.

![Track Loop Settings](Documentation/Images/Screenshots/TrackLoopScreenshot.png)

Adjust loop parameters including:
- Radius  
- Gap between rising and falling segments  
- Loop percentage  
- Track roll  
- Direction  

---

## Global Settings Configuration

![Config Location](Documentation/Images/Screenshots/ConfigHighlightScreenshot.png)

Navigate to the **Config** folder to define global track geometry and material settings.

![Default Config](Documentation/Images/Screenshots/TrackGenerationDefaultConfigScreenshot.png)

Click on the **TrackGenerationDefaultConfig** ScriptableObject and look in the Inspector to view current default track configuration settings.

![Default Config Settings](Documentation/Images/Screenshots/DefaultConfigSettingsScreenshot.png)

Any changes made to these parameters will be referenced by all track components. When *Use Config* is enabled and *Generate Track* is pressed, these settings drive the component’s generation. Use this system to maintain a consistent style across all track components.

---

## Additional Details

For a detailed breakdown of system architecture and implementation, see:  
`Documentation/TrackGenerationArchitecture.md`