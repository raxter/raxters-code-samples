Greetings! My name is raxter and if you are here, I've probably applied to a job with you! Do please go ahead and hire me.

Not convinced yet... so, I've made this repository to house code and systems examples to show that I actually am a programmer of 12+ years experience. 

A lot of included here are riffed off of or loosly based off of ideas and concepts that have been used in actual titles (albeit, often simplified for demonstration purposes). But all code here is bespoke and my own.

Mutiscene-Area based Scene Streaming
===
  - Allows sets of scenes to be loaded in and out automatically based on player position. 
  - Useful for open world games that require seamless traversal over a large segmentable world
  - Includes editor tools for loading + unloading scenes in edit mode
  - Areas defined via scriptable objects

Camera Perspective-to-Orthographic View-Matrix Shifter
---
  - Fun tool that modyfies a camera's perspective matrix to lerp smoothly from a perspective view to an orthographic one
  - Look, this used to be very impressive to know the maths of this all before Unity and/or AI just gave us the matricies...
    
Vertex Offset Wave Effect
---
  - Offsets a vertex by a height based on 3 material defined sine waves
  - Calculates normal of surfaces via gradient
  - Working on the Low-Poly look... it's coming 
  
https://github.com/user-attachments/assets/2453ec4f-b02d-44a8-96dd-7d6479cf28fa

![image](https://github.com/user-attachments/assets/8dbd0fb8-db32-4d2f-b61a-a3b9f66f24b2)

Fully Definable Receipe System
===
  - Allows one to make concoctions defined as a set of solids and liquids
  - Allows for definitions of how concoctions are 'cooked' (transformed) via different tools/processes
  - Wildcard compatible for defining reciepes across multiple types
  - E.g. Leaves + hot water => tea
    - `Chamomile Leaf + Water ==(heat)==> "Chamomile Tea"`
    - `*FLOWER   Leaf + Water ==(heat)==> "*FLOWER   Tea"`
  - Cutting Vegitables
    - `Whole Broccolli    ==(cut)==> Chopped Broccolli`
    - `Whole *VEG         ==(cut)==> Chopped *VEG`
    - `*     *VEG + Water ==(cut)==> Boiled  *VEG + *VEG Stock + Water`
  - Has a well defined way of naming Concoctions
    - e.g.
    - example rule: `Chamomile Leaf + Water ==(heat)==> Boiled Chamomile Leaf + Boiled Camomile(l) + Water`
    - naming rule: `Boiled *FLOWER(l)   + Water    --> "*FLOWER Tea"`
    - naming rule: `Boiled *FLOWER Leaf + [LIQUID] --> "Bits of boiled *FLOWER Leaf in [LIQUID]"`
    - applied:
      - `Chamomile Leaf + Water ==(heat)==> Boiled Chamomile Leaf + Boiled Chamomile(l)`
      - `Boiled Chamomile Leaf + Boiled Chamomile(l) + Water --> "Bits of boiled Chamomile Leaf in Chamomile Tea"`
      
![image](https://github.com/user-attachments/assets/6f546412-05f1-4e56-ab6e-0fc52fc2f546)


Coming soon!
---
- Narrative and Logic Flow Timeline Controller
  - Allows for logic based flow within a unity Timeline
  - Logic defined by a signal-like
- Procedurally animated whale
  - Based off of To Be A Whale's whale controller, adapted for more fluid controls
  - 2 stage system of blended animations and a intermediate animation controller, that is then driven by a high level system.

External libraries and assets used:
 - [Triinspector](https://github.com/codewriter-packages/Tri-Inspector)
 - [Dotween](https://dotween.demigiant.com/)
 - Whale model and rig from Lohika Games (coming soon)
 - Additional art from [OpenGameArt](https://opengameart.org/) (when I get around to prettifying this all)
