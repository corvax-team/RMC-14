gun-selected-mode-examine = Выбран режим огня: [color={ $color }]{ $mode }[/color].
gun-fire-rate-examine = Скорострельность: [color={ $color }]{ $fireRate }[/color] в секунду.
gun-selector-verb = Изменить на { $mode }
gun-selected-mode = Выбран режим: { $mode }
gun-disabled = Вы не можете использовать оружие!
gun-clumsy = Оружие взрывается вам в лицо!
gun-set-fire-mode = Выбран режим { $mode }
gun-magazine-whitelist-fail = Это не помещается в оружие!
gun-magazine-fired-empty = Боеприпасы закончились!
# SelectiveFire
gun-SemiAuto = полуавтомат
gun-Burst = отсечка
gun-FullAuto = автомат
# BallisticAmmoProvider
gun-ballistic-cycle = Перезарядка
gun-ballistic-cycled = Перезаряжено
gun-ballistic-cycled-empty = Разряжено
gun-ballistic-cycle-delayed = Вы начинаете разряжать { $entity }. Стойте смирно...
gun-ballistic-cycle-delayed-cancelled = Вы прекратили разряжать { $entity }.
gun-ballistic-cycle-delayed-empty = { $entity } уже разряжен.
gun-ballistic-transfer-invalid = { CAPITALIZE($ammoEntity) } нельзя поместить в { $targetEntity }!
gun-ballistic-transfer-empty = В { CAPITALIZE($entity) } пусто.
gun-ballistic-transfer-target-full = { CAPITALIZE($entity) } уже полностью заряжен.
gun-ballistic-transfer-cancelled = Ваша перезарядка была прервана!
gun-ballistic-transfer-primed = Вы не можете зарядить взведённый { $ammoEntity }!
# CartridgeAmmo
gun-cartridge-spent = Он [color=red]израсходован[/color].
gun-cartridge-unspent = Он [color=lime]не израсходован[/color].
# BatteryAmmoProvider
gun-battery-examine =
    Заряда хватит на [color={ $color }]{ $count }[/color] { $count ->
        [one] выстрел
        [few] выстрела
       *[other] выстрелов
    }.
# CartridgeAmmoProvider
gun-chamber-bolt-ammo = Затвор не закрыт
gun-chamber-bolt = Затвор [color={ $color }]{ $bolt }[/color].
gun-chamber-bolt-closed = затвор закрыт
gun-chamber-bolt-opened = затвор открыт
gun-chamber-bolt-close = Закрыть затвор
gun-chamber-bolt-open = Открыть затвор
gun-chamber-bolt-closed-state = закрыт
gun-chamber-bolt-open-state = открыт
gun-chamber-rack = Передёрнуть затвор
# MagazineAmmoProvider
gun-magazine-examine =
    Тут [color={ $color }]{ $count }[/color] { $count ->
        [one] штука
        [few] штуки
       *[other] штук
    }.
# RevolverAmmoProvider
gun-revolver-empty = Разрядить револьвер
gun-revolver-full = Револьвер полностью заряжен
gun-revolver-insert = Заряжен
gun-revolver-spin = Вращать барабан
gun-revolver-spun = Барабан вращается
# GunSpreadModifier
examine-gun-spread-modifier-reduction = Разброс был уменьшен на [color=yellow]{ $percentage }%[/color].
examine-gun-spread-modifier-increase = Разброс был увеличен на [color=yellow]{ $percentage }%[/color].
gun-speedloader-empty = Спидлоадер пуст
