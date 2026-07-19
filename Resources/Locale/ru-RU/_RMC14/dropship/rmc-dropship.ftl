rmc-dropship-pre-flight-fueling =
    Десантный корабль всё ещё проходит предполётную дозаправку. Взлёт возможен через { $minutes } { $minutes ->
        [one] минуту
        [few] минуты
       *[other] минут
    }.
rmc-dropship-pre-hijack =
    Этот терминал будет заблокирован ещё { $minutes } { $minutes ->
        [one] минуту
        [few] минуты
       *[other] минут
    }.
rmc-dropship-invalid-hijack = Из терминала вырываются вспышки данных, но их суть за пределами вашего понимания.

rmc-dropship-terminal-clueless = Вы растерянно смотрите на {$terminal}.
rmc-dropship-no-destinations-nearby = Рядом с вами нет пунктов назначения для десантных кораблей!
rmc-dropship-already-incoming = Сюда уже летит десантный корабль!
rmc-dropship-already-here = Здесь уже находится десантный корабль!
rmc-dropship-not-primary-shuttle = Шаттл не отвечает на запросы, похоже, это не основной шаттл.
rmc-dropship-call-to-location = Вы вызываете один из десантных кораблей в своё местоположение.
rmc-dropship-none-available = Нет доступных десантных кораблей! Подождите немного.
rmc-dropship-no-remote-control = Этот десантный корабль не имеет включённого дистанционного управления.
rmc-dropship-busy = Этот десантный корабль сейчас занят. Пожалуйста, повторите попытку позже.
rmc-dropship-now-on-its-way = Этот десантный корабль уже в пути.

rmc-dropship-weapons-title = Консоль управления вооружением
rmc-dropship-weapons-main-screen-text =
    К.К.М.П.
    СУО (Система Управления Оружием)
    Версия 0.1
rmc-dropship-weapons-weapon-selected =
    { $weapon }
    БОЕПРИПАСЫ ОТСУТСТВУЮТ
rmc-dropship-weapons-weapon-selected-ammo =
    { $weapon }
    { $ammo }
    Снаряды: { $rounds } / { $maxRounds }
rmc-dropship-weapons-target-strike =
    ЗАХВАТ ЦЕЛИ
    
    Режим удара: { $mode }
    Конфиг. удара: { $weapon }
    Выбранная цель: { $target }
    Смещение: { $xOffset }, { $yOffset }
rmc-dropship-weapons-equip-weapon-ammo =
    { $weapon }
    { $rounds } { $rounds ->
        [one] снаряд
        [few] снаряда
       *[other] снарядов
    }
rmc-dropship-weapons-equip = СНАРЯДИТЬ
rmc-dropship-weapons-fire-mission = ОГН. ЗАДАЧА
rmc-dropship-weapons-target = ЦЕЛЬ
rmc-dropship-weapons-maps = КАРТЫ
rmc-dropship-weapons-cams = КАМЕРЫ
rmc-dropship-weapons-cancel = ОТМЕНА
rmc-dropship-weapons-exit = ВЫХОД
rmc-dropship-weapons-lock = БЛОКИРОВКА
rmc-dropship-weapons-clear = ОЧИСТИТЬ
rmc-dropship-weapons-enable = ВКЛ
rmc-dropship-weapons-disable = ВЫКЛ
rmc-dropship-weapons-deploy = РАЗВЕРНУТЬ
rmc-dropship-weapons-retract = СВЕРНУТЬ
rmc-dropship-weapons-auto-deploy = АВТО-РАЗВЕРТКА
rmc-dropship-weapons-offset-calibration = КАЛИБРОВКА СМЕЩЕНИЯ
rmc-dropship-weapons-offset-calibration-does-not-affect-direct-bombardment = Не влияет на прямую бомбардировку!
rmc-dropship-weapons-fire = ОГОНЬ
rmc-dropship-weapons-strike = УДАР
rmc-dropship-weapons-vector = ВЕКТОР
rmc-dropship-weapons-night-vision-on = ПНВ-ВКЛ
rmc-dropship-weapons-night-vision-off = ПНВ-ВЫКЛ
rmc-dropship-weapons-weapon = ОРУДИЕ
rmc-dropship-weapons-previous = ПРЕД
rmc-dropship-weapons-next = СЛЕД
rmc-dropship-weapons-fire-no-weapon = Орудие не выбрано.
rmc-dropship-weapons-fire-not-flying = Огонь возможен только в полёте.
rmc-dropship-weapons-fire-not-skilled = У вас нет квалификации для стрельбы из этого орудия!
rmc-dropship-weapons-fire-no-ammo = Боезапас { $weapon } исчерпан.
rmc-dropship-weapons-fire-cooldown = Перегрев { $weapon }. Ожидайте охлаждения.
rmc-dropship-attached = Установлено: { $attachment }.
rmc-dropship-weapons-point-ammo = Заряжено: { $ammo }.
rmc-dropship-weapons-rounds-left = Заряжено { $current } из { $max } снарядов.
rmc-dropship-utility-activate-not-flying = Вспомогательные системы доступны только в полёте.
rmc-dropship-utility-not-flyby = { $utility } доступно только при пролёте над зоной.
rmc-dropship-utility-not-skilled = У вас нет квалификации для использования этой системы!
rmc-dropship-utility-cooldown = { $utility } на перезарядке. Повторите попытку позже.
rmc-dropship-flyby-no-skill = У вас нет квалификации для выполнения маневра пролёта.
rmc-dropship-fabricator-title = Фабрикатор компонентов
rmc-dropship-fabricator-points = Очки: { $points }
rmc-dropship-fabricator-equipment = [bold]Снаряжение[/bold]
rmc-dropship-fabricator-ammo = [bold]Боеприпасы[/bold]
rmc-dropship-fabricator-fabricate = Создать ({ $cost })
rmc-dropship-fabricator-busy = Фабрикатор занят выполнением операции.
rmc-dropship-firemission-warning = ЯРКИЕ ВСПЫШКИ В НЕБЕ НА { $direction }!
rmc-dropship-firemission-warning-above = ЗАЛП АРТИЛЛЕРИИ ОБРУШИВАЕТСЯ ПРЯМО НА ВАС!
rmc-dropship-paradrop-target-screen-text =
    HPU-1 Система десантирования
    { $hasTarget }
rmc-dropship-paradrop-target-screen-target-none =
    ЦЕЛЬ НЕ ЗАХВАЧЕНА.
    Десантирование недоступно.
rmc-dropship-paradrop-target-screen-target-targeting =
    ЗАХВАТ: { $dropTarget }.
    Десантирование доступно.
rmc-dropship-paradrop-lock-no-target = Цель не выбрана.
rmc-dropship-paradrop-lock-target-not-flying = Модуль десантирования доступен только в полёте.
rmc-dropship-medevac-system-screen-text = RMU-4M Система "Медэвак"
rmc-dropship-fulton-system-screen-text = RMU-19 Система извлечения "Фултон"
rmc-dropship-paradrop-failed = Ваши страховочные ремни заклинило, не давая вам спрыгнуть!
rmc-dropship-locked = Доступ к управлению заблокирован на { $minutes } мин.
rmc-dropship-locked-out = Шаттл не отвечает. Повторите попытку через { $minutes } мин.
rmc-dropship-locked-out-bypass = Вы частично обошли протоколы блокировки! Продолжайте!
rmc-dropship-locked-out-bypass-complete = Блокировка успешно снята!
rmc-dropship-equipment-deployer-text = { $deployName }
rmc-dropship-equipment-deployer-health = Состояние: { $status }
rmc-dropship-equipment-deployer-ammo = Боезапас: { $ammoCount } / { $totalAmmoCount }
rmc-dropship-equipment-deployer-status = Статус развертки: { $deployed }
rmc-dropship-equipment-deployer-auto-deploy = Авто-развертка: { $autoDeploy }
rmc-dropship-equipment-enabled = ВКЛЮЧЕНО
rmc-dropship-equipment-disabled = ВЫКЛЮЧЕНО
rmc-dropship-equipment-deployed = РАЗВЕРНУТО
rmc-dropship-equipment-undeployed = СВЕРНУТО
rmc-dropship-equipment-operational = ИСПРАВНО
rmc-dropship-equipment-damaged = ПОВРЕЖДЕНО
rmc-dropship-equipment-destroyed = УНИЧТОЖЕНО
rmc-dropship-launch-bay-screen-text = LAG-14 Внутренняя пусковая установка турелей
rmc-dropship-launch-bay-screen-text-loaded =
    LAG-14 Внутренняя пусковая установка турелей
    
    Загружено: { $loaded }
    
    Боеприпасы: { $current } / { $max }
rmc-dropship-launch-alarm-xeno-shutdown = Мы кромсаем { $console }, заставляя его замолкнуть!
