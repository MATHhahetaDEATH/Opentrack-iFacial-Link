# Opentrack UDP Input Protocol Documentation

This document describes the data format and protocol for the **"UDP over network"** input source in `opentrack`.

## 1. Protocol Specifications

- **Protocol**: UDP (User Datagram Protocol)
- **Default Port**: `4242` (Configurable in the `opentrack` software)
- **Packet Size**: Exactly **48 Bytes**
- **Endianness**: Native (on Windows/x86 systems, this is **Little-Endian**)

## 2. Data Structure

The packet consists of 6 contiguous `double` values (64-bit IEEE 754 floating point numbers).

| Byte Offset | Data Type | Parameter | Description | Standard Unit |
| :--- | :--- | :--- | :--- | :--- |
| `0` | `double` | **TX** | X Translation (Left/Right) | Centimeters (cm) |
| `8` | `double` | **TY** | Y Translation (Up/Down) | Centimeters (cm) |
| `16` | `double` | **TZ** | Z Translation (Forward/Back) | Centimeters (cm) |
| `24` | `double` | **Yaw** | Rotation around Y-axis | **Degrees** |
| `32` | `double` | **Pitch** | Rotation around X-axis | **Degrees** |
| `40` | `double` | **Roll** | Rotation around Z-axis | **Degrees** |

> [!TIP]
> **ARKit Integration Hint**: ARKit typically outputs translations in **meters**. To match `opentrack` expectations, multiply your translation values by **100** (to get cm). Rotations from ARKit (Euler angles or Quaternions) must be converted to **Degrees**.

## 3. Source Code References

### Payload Reception
In `tracker-udp/ftnoir_tracker_udp.cpp`, the data is read directly into a `double[6]` array:

```cpp
// From: opentrack-master/tracker-udp/ftnoir_tracker_udp.cpp
void udp::run()
{
    // ...
    const qint64 sz = sock.readDatagram(reinterpret_cast<char*>(last_recv_pose2), sizeof(double[6]));
    // ...
}
```

### Parameter Order
The order of axes in the `data` array is defined in the core API:

```cpp
// From: opentrack-master/api/plugin-api.hpp
enum Axis : int
{
    NonAxis = -1,
    TX = 0, TY = 1, TZ = 2,

    Yaw = 3, Pitch = 4, Roll = 5,
    Axis_MIN = TX, Axis_MAX = 5,

    Axis_COUNT = 6,
};
```

## 4. Implementation Example (C#)

If you are writing your translation app in C#, you can use the following snippet to pack the data:

```csharp
public byte[] CreateOpentrackPacket(double tx, double ty, double tz, double yaw, double pitch, double roll)
{
    byte[] packet = new byte[48];
    Buffer.BlockCopy(BitConverter.GetBytes(tx), 0, packet, 0, 8);
    Buffer.BlockCopy(BitConverter.GetBytes(ty), 0, packet, 8, 8);
    Buffer.BlockCopy(BitConverter.GetBytes(tz), 0, packet, 16, 8);
    Buffer.BlockCopy(BitConverter.GetBytes(yaw), 0, packet, 24, 8);
    Buffer.BlockCopy(BitConverter.GetBytes(pitch), 0, packet, 32, 8);
    Buffer.BlockCopy(BitConverter.GetBytes(roll), 0, packet, 40, 8);
    return packet;
}
```
