# Miner
ent-CCMMinerBase = буровая установка
    .desc = Тяжелая автономная установка, предназначенная для добычи полезных ископаемых из глубин планеты.
ent-CCMMinerPhoron = фороновая буровая установка
    .desc = Тяжелая автономная установка, настроенная на извлечение кристаллов форона.
ent-CCMMinerPlatinum = платиновая буровая установка
    .desc = Тяжелая автономная установка, настроенная на извлечение самородков платины.
ent-CCMMinerDebug = буровая установка
    .desc = Очень быстрый бур для тестов.
    .suffix = ДЕБАГ
# Modules
ent-CCMMinerModuleAutomation = модуль автоматизации бура
    .desc = Модуль управления логистикой: когда руда добыта, она автоматически продается в бюджет снабжения.
ent-CCMMinerModuleSpeed = модуль разгона бура
    .desc = Модуль, разгоняющий двигатель бура для значительно более быстрой добычи полезных ископаемых.
ent-CCMMinerModuleReinforced = модуль укрепления бура
    .desc = Укрепляет структурную целостность установки, позволяя ей выдерживать гораздо больше повреждений перед выходом из строя.
# Crates
ent-CCMOreCrateBase = ящик с рудой
    .desc = Небольшой ящик, наполненный переработанной рудой.
ent-CCMOreCratePhoron = ящик с фороновой рудой
    .desc = { ent-CCMOreCrateBase.desc }
ent-CCMOreCratePlatinum = ящик с платиновой рудой
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
