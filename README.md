<p align="center">
  <img src="Images/Logo.png" alt="Logo">
</p>

|          Team 16          	|
|:-------------------------:	|
|       Mo'men Mohamed      	|
| Mohamed Ahmed Mahmoud Gad 	|
|       Mostafa Ashraf      	|
|        Amr Mahmoud        	|
|      Ahmed Abdulgafar     	|

# Al-Andalosya for Medical Equipment - Endoscopic Simulator

The project is an endoscopic simulator designed to **navigate**, **inspect**, and **analyze anomalies (tumors)** within a simulated anatomical environment. 

## Mechanics & Controls

### Navigation
- **Movement:** Use `W`/`S`/`A`/`D` (or arrow keys) to move the endoscope forward, backward, left, and right.

- **Vertical Movement:** Press `E` to ascend and `Q` to descend.

- **Speed Boost:** Hold `Left Shift` to accelerate movement speed.

- **Looking around:** Move the mouse to orient the camera (Pitch and Yaw).


### Illumination & Optical Systems
- **Light Control:** Use the **Mouse ScrollWheel** to adjust the LED light intensity to properly illuminate dark cavities. 

- **Optical Zoom:** Use `Page Up` and `Page Down` to zoom the field of view (FOV) in and out respectively.


### Gameplay & Missions
- **Proximity:** As the endoscope approaches the target tumor, proximity audio voiceovers will automatically trigger to help guide you.

- **Capturing Images:** Press the `Spacebar` to capture a scan. To pass the beginner mission, you need to be within a specific distance (2.5 units) looking directly at the target anomaly.  

- **Analysis Mode:** Once close enough and looking directly at the anomaly, you can press `R` to toggle 
**Analysis Mode**. This switches your cursor onto the screen, where you can left-click and trace over the designated bounding box to evaluate your accuracy (Expert Mission).

> **Note:** If the expert or beginner mode becomes bugged, please press the reset button to restart the mission.

## Captured Images Output
Whenever you take an image using the `Spacebar`, the system generates an actual photo (`EndoscopicScan_[timestamp].png`). 

These captured images are saved directly in the application's data directory for further image processing and analysis. You can find your scanned images output in the following directory folder:
`Al-Andalosya for Medical Equipment_Data/`

![Output Scan](Al-Andalosya%20for%20Medical%20Equipment_Data/EndoscopicScan_20260430_222710.png)
