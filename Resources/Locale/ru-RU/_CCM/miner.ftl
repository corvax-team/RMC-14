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
miner-examine-storage = Модуль хранения заполнен на [color=cyan]{ $count } / { $max }[/color].
miner-examine-full = [color=green]Модуль заполнен![/color] Нажмите рукой, чтобы упаковать руду в ящик.
miner-examine-repair-destroyed = { $miner } сильно повреждена, видны внутренние механизмы. Используйте [color=orange]сварку[/color], чтобы починить его!
miner-examine-repair-medium = { $miner } повреждена, наружу торчат оборванные провода. Используйте [color=orange]кусачки[/color], чтобы починить его!
miner-examine-repair-small = { $miner } слегка повреждена: видны вмятины и ослабленные трубы. Используйте [color=orange]гаечный ключ[/color], чтобы починить его!
miner-repair-not-needed = { CAPITALIZE($miner) } не нуждается в ремонте.
miner-repair-different-tool = Этим инструментом нельзя починить { $miner }.
miner-examine-module = Установленный модуль: { $module }.
miner-module-automation = Автоматизация
miner-module-speed = Ускорение
miner-module-reinforced = Укрепление
miner-module-unknown = Неизвестный модуль
miner-module-broken = { CAPITALIZE($miner) } сломан, модуль установить невозможно.
miner-module-already-installed = В { $miner } уже установлен модуль.
miner-module-installed = Вы успешно установили { $module } в { $miner }.
miner-module-removed = Вы успешно извлекли модуль из { $miner }.
miner-module-removal-start = Вы начинаете извлечение модуля из { $miner }...
