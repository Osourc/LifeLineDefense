# 🧬 Life Line Defense

**Life Line Defense** is a 2D mobile tower-defense game built in **Unity**. In this game, players control immune cells to defend the human body from invading pathogens. The game features an in-game shop, DNA-based currency, and user data management via **MongoDB** using **C# drivers**. Real-money purchases are handled through a **custom PayPal integration** (not SDK-based).

---

## 🛠️ Tech Stack

- **Game Engine:** Unity (URP, C#)
- **Database:** MongoDB Atlas (NoSQL, cloud-hosted)
- **Backend:** C# MongoDB Drivers
- **Payments:** PayPal (manual integration via HTTP/webhooks)
- **Platform:** Android

---

## 🎮 Key Features

- **Login & Guest Access**
  - User registration and login
  - Guest access with auto-generated ID
  - Banned user detection and access control

- **Gameplay**
  - Real-time defense mechanics using immune cells
  - Pathogen waves and strategic placement
  - Swipeable almanac system with detailed enemy info

- **In-Game Shop**
  - Uses DNA currency earned through gameplay
  - Purchasable backgrounds, soundtracks, and SFX
  - Items are moved to the top once purchased

- **Inventory & Settings**
  - Persistent inventory using MongoDB
  - One active background, sound, and SFX applied at a time
  - Synced with server via C# MongoDB drivers

- **Manual PayPal Integration**
  - Server-side payment handling (C# backend)
  - Redirect to PayPal checkout via REST API
  - Webhook for payment confirmation and DNA top-up

---

## 🧩 MongoDB Document Structure
## SAMPLE
Each player document in MongoDB follows this structure:

```json
{
  "_id": "string",
  "Username": "string",
  "GameName": "string",
  "ShaPassword": "string",
  "DnaCount": 100,
  "Inventory": {
    "Background": ["Nebula", "CyberGrid"],
    "Sound": ["ChillWave"],
    "SoundEffect": ["Zap"]
  },
  "AppliedBackground": "Nebula",
  "AppliedSound": "ChillWave",
  "AppliedSoundEffect": "Zap",
  "IsBanned": false
}
