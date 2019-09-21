# Sky's Oxygen Not Included mods

The source code for my Oxygen Not Included mods.

Many thanks to Peter Han and Cairath for their help and also for graciously letting me reference their code. I wouldn't have been able to mod ONI without their help. In particular, the build system used in this solution was originally written by Cairath.

## Issues

If you find what you think is a bug, please feel free to open a report in [Issues](https://github.com/skairunner/sky-oni-mods/issues). If you do not include steps for reproduction, your issue will likely not be acted on.

## Mods

### Drain

Adds a new building to the Plumbing tab called the Drain, built with 100kg of metal. It passively pumps liquids inside it into pipes at a rate of 100g/s, and is thus useful for buildings that "dribble" liquid like the Natural Gas Generator, or for cleaning up Algae Terrarium automation.

### Radiate Heat In Space

Add mechanic where select buildings passively radiate heat when in space. The radiated amount of heat scales by the fourth power of the temperature of the building. This allows buildings to be potentially operated in space without a "drop of liquid" cooling system, although many buildings will have to be periodically disabled, because they do not radiate enough heat to keep themselves cool while continuously operating.

#### Approximate net heat production at 20C

Automatic Dispenser: -160 DTU/s  
Auto-Sweeper: 1200 DTU/s*  
Battery: 550 DTU/s☨  
Battery, Jumbo: -100 DTU/s  
Battery, Smart: -600 DTU/s 
Ceiling Light: 400 DTU/s  
Conveyor Loader: 1600 DTU/s**  
Duplicant Checkpoint: 450 DTU/s☨☨  
Gantry: 1500 DTU/s***  
Lamp: 300 DTU/s
Power Transformer: 500 DTU/s  
Power Transformer, Large: 500 DTU/s  
Robo-Miner: 700 DTU/s****  
Smart Storage Bin: -120 DTU/s  

\* Radiates 800 DTU/s even if inactive  
\*\* Radiates 400 DTU/s even if inactive  
\*\*\* Radiates 500 DTU/s even if inactive  
\*\*\*\* Radiates 1300 DTU/s even if inactive  
☨ Battery has a net positive heat produced even though it produces less than the other batteries, because its surface area is smaller.  
☨☨ Duplicant Checkpoint has a tiny surface area and is also shiny, and thus radiates very little.

### Radiator

Adds a new building to the Utilities tab called the Radiator, built with 400kg of Refined Metal. It is passively warmed up by liquid piped through it, and when it is in space (no backwall!), it passively cools itself by radiating its heat to space. It radiates more heat the hotter it is. Unlike the thermo-tuners, it does not take power but also does not actively pump heat. The Radiator does not overheat or be entombed, but it is damageable by meteorites. The Radiator also does not need access to the sun. It is unlocked through the research Temperature Modulation.

Aside from radiating heat into space, the Radiator can also be used as a more compact version of a typical radiant pipe loop. Simply plop a few in the fluid of your choice and route liquid through it!