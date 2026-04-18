# Miner
ent-CCMMinerBase = mining drill
    .desc = A heavy-duty autonomous drill designed to extract valuable minerals from deep beneath the planet's surface.
ent-CCMMinerPhoron = phoron mining drill
    .desc = A heavy-duty autonomous drill tuned to extract phoron crystals.
ent-CCMMinerPlatinum = platinum mining drill
    .desc = A heavy-duty autonomous drill tuned to extract platinum nuggets.
ent-CCMMinerDebug = debug drill
    .desc = A very fast drill for tests.
    .suffix = DEBUG
# Modules
ent-CCMMinerModuleAutomation = miner automation module
    .desc = A control module that automates logistics: once storage is full, mined ore is sold directly to requisitions budget while the drill remains operational.
ent-CCMMinerModuleSpeed = miner overclocking module
    .desc = Overclocks the drill's motor for significantly faster mineral production.
ent-CCMMinerModuleReinforced = miner reinforcement module
    .desc = Reinforces the drill's structural integrity, allowing it to withstand much more damage before failing.
# Crates
ent-CCMOreCrateBase = ore crate
    .desc = A small crate filled with processed ore. Deliver this to the supply elevator.
ent-CCMOreCratePhoron = phoron ore crate
    .desc = { ent-CCMOreCrateBase.desc }
ent-CCMOreCratePlatinum = platinum ore crate
    .desc = { ent-CCMOreCrateBase.desc }
# Examine and UI
miner-examine-storage = Storage module is filled [color=cyan]{ $count } / { $max }[/color].
miner-examine-full = [color=green]Storage is full![/color] Use your hand to pack ore into a crate.

miner-examine-repair-destroyed = { $miner } is severely damaged, internal mechanisms are exposed. Use [color=orange]welding[/color] to repair it!
miner-examine-repair-medium = { $miner } is damaged, torn wires are sticking out. Use [color=orange]wirecutters[/color] to repair it!
miner-examine-repair-small = { $miner } is lightly damaged: dents and loosened pipes are visible. Use a [color=orange]wrench[/color] to repair it!

miner-repair-not-needed = { CAPITALIZE($miner) } does not need repairs.
miner-repair-different-tool = You cannot repair { $miner } with this tool.

miner-examine-module = Installed module: { $module }.
miner-module-automation = Automation
miner-module-speed = Acceleration
miner-module-reinforced = Reinforcement
miner-module-unknown = Unknown module

miner-module-broken = { CAPITALIZE($miner) } is broken and cannot accept a module.
miner-module-already-installed = { $miner } already has a module installed. Remove it with a crowbar first.
miner-module-installed = You successfully install { $module } into { $miner }.
miner-module-removed = You successfully remove the module from { $miner }.
miner-module-removal-start = You begin removing a module from { $miner }...
