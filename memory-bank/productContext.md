# Product Context

Describe the product.

## Overview

Provide a high-level overview of the project.

## Core Features

- Feature 1
- Feature 2

## Technical Stack

- Tech 1
- Tech 2

## Project Description

Unity 6 action roguelite with a Hades-style room loop, Nordic first act, and medieval Japan second act. Current work focuses on combat feel, shared damage handling, and runtime placeholder visuals.



## Architecture

Player movement, combat, health, and camera juice are split into separate MonoBehaviours. Damage now flows through a shared damage contract (DamageInfo/IDamageable), enemies route through Health, and a procedural visual fallback can build a stylized runtime enemy model.



## Technologies

- Unity 6 (6000.0.72f1)
- C#
- Unity physics
- Unity UI / IMGUI prototype
- Runtime primitive-based character visuals



## Libraries and Dependencies

- UnityEngine
- UnityEngine.Events

