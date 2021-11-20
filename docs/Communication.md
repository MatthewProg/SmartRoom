## Documentation

<a href="/./README.md">Main page</a>
<ol>
    <li><a href="/./docs/Schematic.md#schematic">Schematic</a></li>
    <li>
        <a href="#communication">Communication</a>
        <ul>
            <li><a href="#get-package">GET package</a></li>
            <li><a href="#set-package">SET package</a></li>
            <li><a href="#feedback-package">FEEDBACK package</a></li>
            <li><a href="#special-packages">Special packages</a></li>
        </ul>
    </li>
</ol>

## Communication

ESP-01S creates WiFi access point to which the device connects and starts telnet server to communicate.
You can change SSID and password of AP as well as telnet server IP and port in the beginning of `{filename}` in `SmartRoom.WiFi` project.<br>

##### Default values

|Property|Value|
|:-:|:-:|
|SSID|Smart Room|
|Password|donteventry|
|Sever IP|192.168.4.1|
|Port|23|

<hr>

### GET package
GET package allows you to get a value of a pin or text data

|Bit|Description|
|:---:|:---|
|1|Bit is set to 0|
|2-3|Receiver:<br>0 - Arduino pin<br>1 - TLC pin<br>2 - ID (set in Arduino)<br>3 - [NULL]|
|4-8|Pin/ID number (0-31)|

Example:<br>
`0b00100001 - Get value from Arduino pin 1(D1)`<br>
`0b00000110 - Get value from TLC pin 6`

<hr>

### SET package
SET package allows you to set a value of a pin

|Bit|Description|
|:---:|:---|
|1|Bit is set to 1|
|2|Receiver:<br>0 - Arduino pin<br>1 - TLC pin|
|3|Fade:<br>0 - false<br>1 - true|
|4-8|Pin/ID number (0-31)|
|9-16|Value (0-255)

Example:<br>
`0b10100001 0b11001100 - Set value of Arduino pin 1(D1) to 204`<br>
`0b10000110 0b00001111 - Set value of TLC pin 6 to 15`

<hr>

### Feedback package
Feedback package is always sent from the controller to the device when received data

|Bit|Description|
|:---:|:---|
|1|Type:<br>0 - Value<br>1 - Text|
|2-3|Receiver:<br>0 - Arduino pin<br>1 - TLC pin<br>2 - ID (set in Arduino)<br>3 - End of Transmission|
|4-8|Pin/ID number (0-31)|
|9+|Value or ASCII (8bit)|

Text transmission always need to be ended with ASCII `0x03`<br>
When device ended processing/transmitting, it sends EOT `0b01100000`

Example:<br>
`0b00100011 0b00011111 0b01100000 - Value of TLC pin 3 is 31`<br>
`0b1100010 0b01001111 0b01101011 0b00000011 0b01100000 - Text of ID 2 is "Ok"`

<hr>

### Special packages

|Binary|Hex|Dec|Description|Usage|
|:---:|:---:|:---:|:---|:---|
|0b01100000|0x60|96|End of transmission|Ends every transmission|
|0b11100000|0xE0|224|Ping package|Checks if client is available|
|0b00000011|0x03|3|End of text|Ends every text transmission|
