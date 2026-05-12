# SimuCrisis - Pandemic Disaster Management Simulation

## Purpose
SimuCrisis is a presentation-friendly Unity MVP for an academic senior project.  
It visualizes simplified pandemic spread dynamics and the impact of basic public health interventions in a way that is easy to explain during demonstrations.

## Implemented Features (Current MVP)
- Simplified SEIR disease model
- Region-based simulation
- Lockdown intervention
- Vaccination intervention
- Hospital capacity investment
- Budget-based decision layer
- Dashboard UI
- Color-coded region infection map
- Simple statistics/bar visualization
- Local educational JSON datasets
- Optional online dataset loading with local/scenario/fallback safety
- `ScenarioData` and `ScenarioManager` support for configurable scenarios
- Metrics and lightweight simulation snapshots
- Policy history for recent intervention actions
- Baseline comparison for key run metrics
- Educational info panel with short SEIR/intervention explanations

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
- `SimpleBarChartUI` provides a compact visual distribution of SEIR state percentages.
- `ScenarioData` and `RegionInitialData` define editable scenario parameters and initial region states.
- `ScenarioManager` can select from multiple `ScenarioData` assets in the Inspector.
- `DatasetParser`, `PandemicDataset`, and `PandemicDataRecord` support bundled local JSON datasets.
- `OnlineDatasetLoader` can optionally load a JSON dataset from a URL, while local/scenario/fallback data keeps the demo reliable.
- `MetricsCollector` stores lightweight daily snapshots for summary metrics.
- `PolicyEvent` records recent successful interventions for dashboard history.
- `SimulationRunSummary` supports a simple baseline comparison of key metrics.
- `EducationalInfoUI` shows short explanations for students during the demo.

## Educational Dataset Files
The project includes bundled local JSON datasets in `Assets/Data`:
- `medium_outbreak_dataset.json`
- `high_outbreak_dataset.json`

These files are educational sample data for demonstration and testing. They are inspired by pandemic simulation concepts and are not medical prediction data.

Optional online dataset loading is available for experiments, but bundled local datasets are recommended for the final demo because they keep the project reliable without internet access.

## Current Limitations / Future Work
- Full scenario comparison screen
- CSV/PDF export
- Historical dataset import
- Advanced line charts
- Polished result screen
