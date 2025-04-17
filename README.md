# ⚔️ Multiplayer Shooter (v1.0)

A solo or local LAN multiplayer Unity game featuring fast-paced combat, silly NPCs, and survival gameplay. Built using Unity and Netcode for GameObjects.

GitHub Repo: [https://github.com/xuzhihuiCSY/LocalMultiPlayerShooting](https://github.com/xuzhihuiCSY/LocalMultiPlayerShooting)

---

## 🎮 Game Modes

- **Solo Play** – Jump in and fight alone.
- **LAN Multiplayer** – Play with friends on the same local network.
  - One player **hosts** the game.
  - Others **join via IP address**.
  - 🔥 **Important:** The **host** should temporarily **disable the firewall**, otherwise connections from client players may be blocked.

---

## 🧪 How to Test / Play the Game

There are **three ways** to try out the game:

### 🔧 1. Run in Unity (Recommended for Developers)

If you want to test or modify the game in Unity:

1. Clone or download this repository:  
   [https://github.com/xuzhihuiCSY/LocalMultiPlayerShooting](https://github.com/xuzhihuiCSY/LocalMultiPlayerShooting)
2. Open Unity Hub and click **Add Project**, then select the folder.
3. Make sure you have **Unity 2022.3 LTS** or later installed.
4. Open the scene named `GameScene` in `/Scenes/` folder.
5. Click **Play** to test in the Unity Editor.

---

### 📂 2. Run the Raw Build

If you just want to play without installing:

1. Download the `.zip` from the [Releases page](https://github.com/xuzhihuiCSY/LocalMultiPlayerShooting/releases).
2. Extract the folder.
3. Run `MultiplayerShooterCore.exe`.

✅ Quick and easy — no install needed.

---

### 🖥 3. Use the Windows Installer

If you prefer an installer-style setup:

1. Download `MultiplayerShooter.exe` from the [Releases](https://github.com/xuzhihuiCSY/LocalMultiPlayerShooting/releases).
2. Run the installer to install the game on your system.

> ⚠️ **Important Note:**  
> `MultiplayerShooter.exe` was created using Inno Setup and is **not digitally signed**.  
> Windows Defender may block it with a message like:
> > “Windows protected your PC.”
> 
> To continue:
> - Click **"More info"**
> - Then click **"Run anyway"**

This is expected behavior for indie developers without a digital certificate.

---

## 💻 System Requirements

- **OS:** Windows 10/11
- **Processor:** Intel or AMD CPU
- **Memory:** 4 GB RAM
- **Graphics:** DirectX 11 compatible GPU
- **Network:** Required for LAN multiplayer only

---

## 📦 Latest Release

- **Version:** 1.0
- **Date:** April 2025
- [Download v1.0 (Windows)](https://github.com/xuzhihuiCSY/LocalMultiPlayerShooting/releases)

---

## 🚧 Known Issues

- Windows may block the `.exe` installer due to unsigned publisher.
- LAN multiplayer may be blocked by firewall — **host should disable firewall temporarily**.
- Only supports **LAN**, no online matchmaking yet.

---

## 📢 Feedback

Found a bug? Have suggestions?  
Open an [issue](https://github.com/xuzhihuiCSY/LocalMultiPlayerShooting/issues) or leave feedback!

---
