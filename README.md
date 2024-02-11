# <picture> <img src = "logo_128.png" width = 75> </picture> <font size = "7"> HMSection </font> 

HMSection is a grasshopper plugin for the analysis of cross-sections. It uses the CrossSection.net package (https://github.com/IbrahimFahdah/CrossSection.Net), which is based on the Python package sectionproperties (https://github.com/robbievanleeuwen/section-properties).

# Usage

Sections are built from closed polyline curves using the `HMSec - Contour` component. Contours can have holesy use the `HMSec - Hole` component to create them.

The main analysis takes place in the `HMSec - Analysis` component. This component requires the analysis settings component to be connected. Mesh properties can be controlled by roughness and angle settings. By default, only elastic analysis is performed due to the performance cost of warping and plastic analysis.

The output of the analysis can be viewed using the `HMSec - Elastic` and `HMSec - Plastic` components. The mesh can be visualized using the `HMSec - Mesh` component.


<picture>
    <img src = "sample.png">
</picture>

# Restrictions
  
- Contours must be closed polylines.
- The plugin projects 3D polylines onto the XY plane and only considers the X and Y coordinates in calculations.
- Holes cannot touch contours.
- Each hole can only be within one contour and cannot overlap with other contours.

# Disclaimer

- The results have not been properly validated. It remains the user's responsibility to confirm and accept the output.
- Compound sections (made from multiple contours) may be buggy and may not always work.

# Acknowledgments

- https://github.com/IbrahimFahdah/CrossSection.Net
- https://github.com/robbievanleeuwen/section-properties
- https://github.com/ParametricCamp/TutorialFiles
