<h1 align="center">Smart Room (Android)</h1>

<p align="center">
    <a href="https://github.com/VegetaTheKing/SmartRoom/actions/workflows/build-test-app.yml"><img src="https://github.com/VegetaTheKing/SmartRoom/actions/workflows/build-test-app.yml/badge.svg"></a>
    <a href="/./LICENSE"><img src="https://img.shields.io/github/license/VegetaTheKing/SmartRoom"></a>
</p>

<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about">About</a></li>
    <li><a href="#features">Features</a></li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#project-structure">Project structure</a></li>
        <li><a href="#set-up">Set up</a></li>
      </ul>
    </li>
    <li><a href="#documentation">Documentation</a></li>
    <li><a href="#license">License</a></li>
  </ol>
</details>

## About

One day I thought about adding some cool lights in my room, but it seemed too easy for me. So I decided to not only add lights, but also to make my room a bit smarter.

Currently, the app allows you to set PWM values for N-MOSFETs or Arduino pins. You can also read values of pins or custom text/value coded in Arduino using its ID.

## Features

_click image to play GIF_

<table>
    <tr>
        <th style="text-align:center"><a href="/./docs/img/switches.gif"><img alt="switches" src="/./docs/img/preview_switches.png"></a></th>
        <th style="text-align:center"><a href="/./docs/img/switch_new.gif"><img alt="new switch" src="/./docs/img/preview_switch_new.png"></a></th>
        <th style="text-align:center"><a href="/./docs/img/sensors.gif"><img alt="sensors" src="/./docs/img/preview_sensors.png"></a></th>
        <th style="text-align:center"><a href="/./docs/img/macros.gif"><img alt="macros" src="/./docs/img/preview_macros.png"></a></th>
        <th style="text-align:center"><a href="/./docs/img/macros_new.gif"><img alt="reuse switches" src="/./docs/img/preview_macros_new.png"></a></th>
    </tr>
    <tr>
        <td align="center">Control devices</td>
        <td align="center">Add new switches</td>
        <td align="center">Read sensors</td>
        <td align="center">Run macros</td>
        <td align="center">Re-use created switches</td>
    </tr>
</table>

## Getting Started

### Project structure

`Docs` - Documentation directory<br>
`SmartRoom` - Xamarin.Android project<br>
`SmartRoom.Emulator` - .NET Core 3.1 project<br>
`SmartRoom.Tests` - .NET Framework 4.7.2 project<br>
`SmartRoom.WiFi` - PlatformIO ESP-01S project<br>
`SmartRoom.Controller` - PlatformIO Arduino Nano project

### Set up

##### Controller projects
1. Clone repository
    `git clone https://github.com/VegetaTheKing/SmartRoom.git`
2. Install PlatformIO IDE from <a href="https://marketplace.visualstudio.com/items?itemName=platformio.platformio-ide">link</a>
3. Open VS Code
4.  In PIO Home click  Open project
5. Navigate to repo location and open `SmartRoom.WiFi` or `SmartRoom.Controller`


##### Application projects
1. Clone repository
    `git clone https://github.com/VegetaTheKing/SmartRoom.git`
2. Open `SmartRoom.sln`

`NOTE: If you run into problems with packages. Remove SmartRoom reference from SmartRoom.Tests project and try again. Don't forget to later add a reference`

## Documentation

<ol>
    <li><a href="/./docs/Schematic.md#schematic">Schematic</a></li>
    <li>
        <a href="/./docs/Communication.md#communication">Communication</a>
        <ul>
            <li><a href="/./docs/Communication.md#get-package">GET package</a></li>
            <li><a href="/./docs/Communication.md#set-package">SET package</a></li>
            <li><a href="/./docs/Communication.md#feedback-package">FEEDBACK package</a></li>
            <li><a href="/./docs/Communication.md#special-packages">Special packages</a></li>
        </ul>
    </li>
</ol>

## [License](/./LICENSE)

```
MIT License

Copyright (c) 2021 Mateusz Ciaglo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```