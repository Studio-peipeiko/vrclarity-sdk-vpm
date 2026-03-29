# VRClarity SDK Setup Guide

An anonymous metrics collection SDK for VRChat worlds. Simply place a Prefab from the right-click menu to automatically collect the following data:

| Metric | Description |
|---|---|
| Stay Duration | 9-stage milestones at 1 / 5 / 15 / 30 / 60 / 120 / 240 / 360 / 480 minutes |
| Movement Distance | 6-stage milestones at 10m / 50m / 150m / 400m / 1000m / 2500m |
| Visit Count | Cumulative visit count per player (1-200, 20 buckets) |
| Platform | 5 types: PCVR / Desktop / Quest / Android Mobile / iOS |
| Player Count | Concurrent player count in instance (0-80 players, every 5 minutes) |

All collected data is anonymous. No personal information such as displayName or userId is ever sent.

---

## Requirements

- Unity 2022.3 or later
- VRChat SDK Worlds 3.5.0 or later
- UdonSharp 1.1.0 or later

---

## 1. Obtain an API Key

1. Log in to the [VRClarity Dashboard](https://vrclarity.net) with your Discord account
2. Navigate to the **SDK Keys** page from the left menu
3. Click "Create New Key"
4. Select the target World ID
5. Copy the displayed **Key ID** and **Encryption Key** and save them securely

> **Important:** The Encryption Key is only shown once on this screen. It cannot be retrieved after leaving the page, so make sure to copy it immediately.

---

## 2. Install the Package

### Via VCC (VRChat Creator Companion)

1. Open VCC
2. Go to **Settings** > **Packages** > **Add Repository**
3. Enter the following URL:
   ```
   https://studiopeipeiko.github.io/vrclarity-sdk-vpm/vpm.json
   ```
4. Click **Add**
5. Add **VRClarity SDK** from your project's **Manage Project** page

### Manual Installation

1. Copy the `packages/net.vrclarity.sdk` folder into your Unity project's `Packages/` directory
2. Or use Package Manager's **Add package from disk** and select `package.json`

---

## 3. Set Up in Your World

### 3-1. Place the Prefab

Right-click in the Hierarchy and select **VRClarity > Create Tracker**.

### 3-2. Configure the Inspector

Enter the following in the VRClarityTracker component Inspector:

| Field | Value | Format |
|---|---|---|
| **Key ID** | Key ID from the dashboard | `sk_` + 24 hex characters |
| **Encryption Key** | Encryption key from the dashboard | 64 hex characters |
| **World ID** | Target world ID | `wrld_xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |

Validation results will appear next to each field:
- **OK** — Format is correct
- **Red text** — Format issue detected

### 3-3. Bake URLs

Once all fields are correctly filled, the **Bake URLs** button becomes active.

- **Manual bake:** Click the "Bake URLs" button in the Inspector
- **Automatic bake:** URLs are automatically baked when building the world

After baking, expand "URL Pool Status" to verify the generated URL counts:

```
Stay URLs:      9 / 9
Move URLs:      6 / 6
Visit URLs:    20 / 20
Platform URLs:  5 / 5
PC URLs:       81 / 81
Total:        121 / 121 URLs baked
```

### 3-4. Build & Upload

Build and upload your world as usual. URLs are automatically baked at build time, and metrics collection begins as soon as players visit your world.

---

## 4. View Data on the Dashboard

Once players visit your world, data will appear on the VRClarity dashboard:

- **Stay Duration Distribution:** "150 players stayed 1+ min, 80 stayed 5+ min, 20 stayed 60+ min, 5 stayed 240+ min, 2 stayed 360+ min, 1 stayed 480+ min"
- **Movement Distance Distribution:** "200 players moved 10m+, 50 moved 50m+, 10 moved 1000m+"
- **Visit Count Distribution:** "200 first-time, 80 second-time, 50 at 10 visits, 10 at 50 visits..." (bucket-based, tracking up to 200 visits)
- **Platform Distribution:** "PCVR 40%, Desktop 25%, Quest 30%, Android 3%, iOS 2%"
- **Player Count Timeline:** "20 on join, 35 after 5min, 50 peak at 10min, 25 after 30min" (updated every 5 minutes)

---

## Collected Metrics Details

### Stay Duration (9 stages)

Measures elapsed time since the player joined the instance using a milestone approach.

| Milestone | Meaning |
|---|---|
| 1 min | Entered the world |
| 5 min | Stayed briefly |
| 15 min | Spent some time |
| 30 min | Explored in depth |
| 60 min | Long session |
| 120 min | Very long session |
| 240 min | Extremely long session |
| 360 min | Extended stay (6 hours) |
| 480 min | VR sleep / All-day session (8 hours) |

### Movement Distance (6 stages)

Measures cumulative movement distance using a milestone approach.

| Milestone | Meaning |
|---|---|
| 10m | Moved a little |
| 50m | Walked around the world |
| 150m | Actively explored |
| 400m | Covered a wide area |
| 1000m | Very actively moving |
| 2500m | Extremely long distance |

### Visit Count (20 buckets)

Records cumulative visit count per player using PlayerData Persistence. Uses a bucket approach where low counts are tracked individually and higher counts use wider intervals.

Buckets: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 40, 50, 75, 100, 150, 200

Example: A 19th visit is recorded as the "15" bucket (assigned to the nearest bucket at or below the actual count).

### Platform (5 types)

Detects and reports the player's platform when they join an instance.

| ID | Platform | Detection Method |
|---|---|---|
| 1 | PCVR | PC + VR mode |
| 2 | Desktop | PC + non-VR mode |
| 3 | Quest | Android + VR mode |
| 4 | Android Mobile | Android + non-VR mode |
| 5 | iOS | iOS build |

### Player Count (0-80 players)

Periodically reports the concurrent player count in the instance.

**Sending Schedule:**
- On player join (initial)
- Every 5 minutes thereafter

**Player Count Range:**
- Tracks 0-80 players
- Instances with 81+ players are recorded as "80"

**Features:**
- Tracks private instance data unavailable via VRChat API
- Useful for monitoring real-time concurrent players and peak hours
- Visualizes player count trends during events

---

## Troubleshooting

### URLs are empty after build

- Verify that Key ID / Encryption Key / World ID are all correctly entered in the Inspector
- Click "Bake URLs" manually and check for error messages
- Check the Console for errors starting with `[VRClarity]`

### Data not appearing on the dashboard

- Verify the world was uploaded correctly
- Check that the API Key is active (not revoked) on the SDK Keys page
- Ensure the World ID tied to the API Key matches the one entered in the Inspector

### Console shows `Heartbeat send failed`

- Check internet connectivity
- Verify the API Key hasn't been revoked
- Check if the rate limit is being exceeded

---

## Technical Specifications

- **Communication:** HTTPS GET (via VRCStringDownloader)
- **Encryption:** Industry-standard encryption
- **Send Interval:**
  - Event sending (Stay/Move/Visit/Platform): Queue-managed, periodic transmission
  - Player count sending: Every 5 minutes
- **Total URLs:** 121 (Stay 9 + Move 6 + Visit 20 + Platform 5 + PC 81)
- **Privacy:** displayName / userId are never sent. Only event type and numeric values are transmitted
- **Persistence:** PlayerData Persistence for visit counting
- **Synchronization:** Not required. Each player operates independently on their local client
- **Player Count Limit:** 0-80 players (81+ recorded as 80)

---

## ⚖️ Terms of Service

By using the VRClarity SDK, you agree to the following:

### Data Ownership
- **All collected metrics data is owned by VRClarity**
- World creators can view statistics about their worlds on the dashboard, but ownership of the collected data belongs to VRClarity

### Prohibited Activities
The following activities are strictly prohibited:
- **Endpoint modification**: Changing the SDK's transmission destination to anything other than VRClarity
- **Data misappropriation**: Providing or selling collected data to third parties
- **SDK abuse**: Reverse engineering, extracting/publishing encryption keys, etc.

Violations may result in SDK suspension, data deletion, and/or legal action.

For details, see the [Terms of Service (ToS)](../../../ToS_en.md).

---

## License

MIT License
