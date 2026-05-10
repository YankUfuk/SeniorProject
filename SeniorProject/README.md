# SimuCrisis - Pandemic Disaster Management Simulation

## Purpose
SimuCrisis is a Unity-based educational simulation that visualizes simplified pandemic spread and the effects of public health interventions.  
The project is designed as an understandable MVP for academic presentation and demonstration.

## Main Features
- Simplified SEIR disease model
- Region-based simulation
- Lockdown intervention
- Vaccination intervention
- Hospital capacity investment
- Dashboard UI
- Color-coded infection map
- Simple statistics visualization

## How to Run
1. Open the project in Unity.
2. Open `MainSimulationScene` (or the main scene file in `Assets/Scenes`, such as `Main` in this project setup).
3. Press **Play**.
4. Use the UI buttons:
   - **Start**
   - **Pause**
   - **Next Day**
   - intervention buttons (lockdown, vaccination, hospital capacity)

## Technical Overview
- `RegionData` stores the state of each region (SEIR values, hospital capacity, lockdown state).
- `DiseaseModel` updates disease values per simulated day.
- `SimulationManager` controls simulation flow, timing, interventions, and reset behavior.
- `RegionView` visualizes each region’s infection status and selection state.
- `DashboardUI` displays totals, selected region, budget, infection rate, and hospital warnings.
