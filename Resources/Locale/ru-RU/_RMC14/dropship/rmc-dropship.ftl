rmc-dropship-pre-flight-fueling =
    Десантный корабль всё ещё проходит предполётную дозаправку и пока не может взлететь. Пожалуйста, подождите ещё { $minutes } { $minutes ->
        [one] минуту
        [few] минуты
       *[other] минут
    }, и повторите попытку.
rmc-dropship-pre-hijack =
    Этот терминал не будет работать ещё { $minutes } { $minutes ->
        [one] минуту
        [few] минуты
       *[other] минут
    }.
rmc-dropship-invalid-hijack = Lights flash from the terminal but you can't comprehend their meaning.
rmc-dropship-weapons-title = Консоль вооружения
rmc-dropship-weapons-main-screen-text =
    К.К.М.П.
    Система управления оружием корабля
    V 0.1
rmc-dropship-weapons-weapon-selected =
    { $weapon }
    Нет снарядов
rmc-dropship-weapons-weapon-selected-ammo =
    { $weapon }
    { $ammo }
    Снаряды: { $rounds } / { $maxRounds }
rmc-dropship-weapons-target-strike =
    Захват цели
    
    Режим удара: { $mode }
    
    Конфиг. удара: { $weapon }
    
    Выбранная цель: { $target }
    
    Смещение { $xOffset },{ $yOffset }

#  Вектор атаки {$vector}

#  Смещение 0,0

#  Компьютер наведения НЕИСПРАВЕН

rmc-dropship-weapons-equip-weapon-ammo =
    { $weapon }
    { $rounds } { $rounds ->
        [one] снаряд
        [few] снаряда
       *[other] снарядов
    }
rmc-dropship-weapons-equip = СНАРЯЖ
rmc-dropship-weapons-fire-mission = ОГН-ЗАД
rmc-dropship-weapons-target = ЦЕЛЬ
rmc-dropship-weapons-maps = КАРТА
rmc-dropship-weapons-cams = КАМЕРЫ
rmc-dropship-weapons-cancel = ОТМЕНА
rmc-dropship-weapons-exit = ВЫХОД
rmc-dropship-weapons-lock = LOCK
rmc-dropship-weapons-clear = CLEAR
rmc-dropship-weapons-offset-calibration =
    Смещен.
    Камеры
rmc-dropship-weapons-offset-calibration-does-not-affect-direct-bombardment = Не влияет на прямой обстрел!
rmc-dropship-weapons-fire = ОГОНЬ
rmc-dropship-weapons-strike = УДАР
rmc-dropship-weapons-vector = ВЕКТОР
rmc-dropship-weapons-night-vision-on = НВ-ВКЛ
rmc-dropship-weapons-night-vision-off =
    НВ-
    ВЫКЛ
rmc-dropship-weapons-weapon = ОРУДИЕ
rmc-dropship-weapons-previous = ^
rmc-dropship-weapons-next = v
rmc-dropship-weapons-fire-no-weapon = Орудие не выбрано.
rmc-dropship-weapons-fire-not-flying = Десантные корабли могут вести огонь только в полете.
rmc-dropship-weapons-fire-not-skilled = У вас нет подготовки, чтобы стрелять из этого орудия!
rmc-dropship-weapons-fire-no-ammo = У { $weapon } закончился боезапас.
rmc-dropship-weapons-fire-cooldown = { $weapon } только что выстрелило, подождите, пока оно остынет.
rmc-dropship-attached = Сюда установлено { $attachment }.
rmc-dropship-weapons-point-gun = Сюда установлено { $weapon }.
rmc-dropship-weapons-point-ammo = Сюда заряжен { $ammo }.
rmc-dropship-weapons-rounds-left = В нём { $current } из { $max } снарядов.
rmc-dropship-utility-activate-not-flying = Вспомогательные системы могут быть активированы только в полете.
rmc-dropship-utility-not-flyby = { $utility } может быть использовано только во время пролёта.
rmc-dropship-utility-not-skilled = У вас нет подготовки, чтобы использовать эту систему!
rmc-dropship-utility-cooldown = { $utility } только что был использован, нужно немного подождать, прежде чем использовать его снова.
rmc-dropship-flyby-no-skill = У вас нет подготовки, чтобы выполнить пролёт.
rmc-dropship-fabricator-title = Фабрикатор частей
rmc-dropship-fabricator-points = Очки: { $points }
rmc-dropship-fabricator-equipment = [bold]Снаряжение[/bold]
rmc-dropship-fabricator-ammo = [bold]Боеприпасы[/bold]
rmc-dropship-fabricator-fabricate = Создать ({ $cost })
rmc-dropship-firemission-warning = A DROPSHIP FIRES TOWARDS THE { $direction }
rmc-dropship-firemission-warning-above = A DROPSHIP FIRES RIGHT ONTOP OF YOU!
rmc-dropship-paradrop-target-screen-text =
    HPU-1 Paradrop Deployment System
    { $hasTarget }
rmc-dropship-paradrop-target-screen-target-none =
    No locked target found.
    Paradropping not available.
rmc-dropship-paradrop-target-screen-target-targeting =
    Locked to { $dropTarget }.
    Paradropping available.
rmc-dropship-paradrop-lock-no-target = No target selected.
rmc-dropship-paradrop-lock-target-not-flying = You can only enable the paradrop module while in flight.
rmc-dropship-fabricator-busy = Фабрикатор запчастей десантных кораблей занят. Пожалуйста, дождитесь завершения предыдущей операции.
