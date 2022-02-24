# Setup

First, get the Arduino IDE: https://www.arduino.cc/en/software

Then you probably want to download the DRV2605 library.
In Arduino IDE, go to Tools > Library Manager > Search "DRV2605" and download "Adafruit DRV2605 Library" (should be the first one)

Finally, get esp32 in your board manager.
In Arduino IDE, go to File > Preferences >
Enter https://dl.espressif.com/dl/package_esp32_index.json into the “Additional Board Manager URLs”
Go to Tools > Board > Boards Manager > Search "esp32" and download the first option
After that it should be fine.

# Difference of each file

## DRV2605_Control 
First version of the device (messy board)

## DRV2605_Control_MultipleDRV2605
Second version of the device, input via serial port 

## DRV2605_Control_MultipleDRV2605
Second version of the device, input via wifi

## DRV2605_Control_String
First version of the device (messy board), input with string

## The rest
Just some junk
