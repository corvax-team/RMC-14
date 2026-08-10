rmc-intel-suffix = { $baseName } ( { $number } )
rmc-intel-unlocked = { $baseName } ( { $unlocked } )
rmc-intel-label-name = {$baseName} {$label}
rmc-intel-label-name-parenthetical = {$baseName} ({$label})
rmc-intel-serial-name = { $baseName } #{ $serial }
rmc-intel-serial-examine = Серийный номер: { $serial }.
rmc-intel-announcement = ТЕХ. ДОКЛАД: { $points } очков доступно.
rmc-intel-announcement-gain = ТЕХ. ДОКЛАД: { $points } очков доступно (+{ $change }).
rmc-intel-reports = Отчёты
rmc-intel-folders = Папки
rmc-intel-manuals = Руководства
rmc-intel-data = Данные
rmc-intel-retrieve = Вернуть
rmc-intel-misc = Разное
rmc-intel-personal = Личные подсказки

rmc-intel-data-disk-uploaded = {$baseName} (загружен)
rmc-intel-color-red = [color=#eb4034]красный[/color]
rmc-intel-color-black = [color=#000000]чёрный[/color]
rmc-intel-color-blue = [color=#3449eb]синий[/color]
rmc-intel-color-yellow = [color=#ebe534]жёлтый[/color]
rmc-intel-color-white = [color=#ffffff]белый[/color]
rmc-intel-color-grey = [color=#949494]серый[/color]
rmc-intel-color-green = [color=#5dbf36]зелёный[/color]
rmc-intel-color-cracked-blue = [color=#3449eb]треснутый синий[/color]
rmc-intel-color-bloodied-blue = [color=#3449eb]окровавленный красный[/color]
rmc-intel-color-unknown = непомеченный
rmc-intel-clue-found = Вы поняли что-то о {$clue}.
rmc-intel-personal-clues-added = Новые догадки, были добавлены к вашим личным подсказкам.
rmc-intel-clue-label-number = #{$number}
rmc-intel-clue-label-serial = #{$serial}
rmc-intel-clue-label-unmarked = нет видимого ярлыка
rmc-intel-clue-paper-scrap = { $intel } в локации { $area }.
rmc-intel-clue-progress-report = Доклад о ходе работ в локации { $area }.
rmc-intel-clue-folder = { $intel } в локации { $area }.
rmc-intel-clue-technical-manual = { $intel } в локации { $area }.
rmc-intel-clue-experimental-device = Найти { $intel } в локации { $area }.
rmc-intel-clue-data-disk = {$color} диск [bold]{$label}[/bold], ключ дешифровки — [bold]{$key}[/bold] в {$area}.
rmc-intel-clue-data-terminal = Загружены данные с терминала [bold]{$label}[/bold], пароль — [bold]{$password}[/bold] в {$area}.
rmc-intel-clue-safe = Взломайте сейф {$label}, пароль от сейфа [bold]{$code}[/bold] в {$area}.
rmc-intel-data-terminal-password-prompt = Введите пароль
rmc-intel-data-terminal-no-power = Этот терминал не имеет питания!
rmc-intel-data-terminal-no-comms = Терминал показывает отсутствие подключения к сети
rmc-intel-data-terminal-wrong-password = Терминал отклоняет пароль
rmc-intel-data-terminal-started = Вы начали загружать данные
rmc-intel-data-terminal-uploading = Кажется терминал уже загружает данные, лучше позаботиться, чтобы никто не прервал его!
rmc-intel-data-terminal-finished = Терминал мягко пищит, по завершении загрузки.
rmc-intel-data-terminal-complete = Вы видите сообщение на экране, что загрузка данных завершена успешно.
rmc-intel-disk-reader-key-prompt = Введите ключ дешивроки
rmc-intel-disk-reader-no-power = Терминал SIGINT не имеет питания.
rmc-intel-disk-reader-occupied = В терминале SIGINT уже есть диск, подождите завершения до процесса!
rmc-intel-disk-reader-empty = В терминале SIGINT нет вставленного диска.
rmc-intel-disk-reader-wrong-key = Читающий модуль жужжит, извлекая диск. Вам стоит попробовать другой пароль.
rmc-intel-disk-reader-insert-failed = Диск нельзя вставить.
rmc-intel-disk-reader-started = Вы вставляете диск и вводите ключ дешифровки.
rmc-intel-disk-reader-finished = Терминал SIGINT мягко пищит, по завершении загрузки и извлекает диск.
rmc-intel-disk-reader-power-lost = Терминал SIGINT выключается, по мере отключения энергии в зоне действия и извлекает диск.
rmc-intel-disk-reader-disk-complete = Читающий модуль, отображает сообщение, сообщающее, что диск уже был прочтён и отказывается принимать его.
rmc-intel-safe-code-prompt = Введите комбинацию сейфа.
rmc-intel-safe-wrong-code = Сейф не открывается.
rmc-intel-safe-complete = Сейф открывается.
rmc-intel-reading-start = Вы начинаете читать {$thing}.
rmc-intel-reading-cancelled = Вы отвлекаетесь и теряете смысл прочитанного, вам придётся читать сначала.
rmc-intel-reading-inactive = Вы не заметили ничего значимого. Скорее всего вам потребуется найти инструкции на кусочках бумаги.
rmc-intel-reading-finished = Вы закончили читать {$thing}.
rmc-intel-console-typing-start = Вы начинаете забивать свои догадки в компьютер...
rmc-intel-console-typing-no-new = Вы начинаете забивать свои догадки в компьютер... Но вы не можете добавить ничего нового...
rmc-intel-console-typing-cancelled = Вы отвлекаетесь и теряете смысл записанного, вам придётся печатать сначала...
rmc-intel-console-submit-no-new = ...Но вы не можете добавить ничего нового...
rmc-intel-console-submit-done = ...И всё! Вы добавили {$amount} записей!
rmc-intel-survivor-pickup =
    { CAPITALIZE($thing) } вам ничем не поможет.
    Сначала постарайтесь выбраться живыми.
rmc-intel-survivor-xeno-pull =
    Попытка тащить { $thing } с собой только замедлит меня.
        Сначала мне нужно сосредоточиться на поиске помощи.
rmc-intel-survivor-corpse-pull =
    Я не могу спасти { $thing }, это только замедлит меня.
        Сначала мне нужно сосредоточиться на поиске помощи.
rmc-intel-survivor-read =
    Вам незачем читать { $thing }.
    Сначала постарайтесь выбраться живыми.
rmc-ui-intel-title = Цели в технологическом древе морской пехоты
rmc-ui-intel-header = [bold]Цели в технологическом древе морской пехоты[/bold]
rmc-ui-intel-tech-points = [bold]Технические очки[/bold]
rmc-ui-intel-tier = [bold]Уровень[/bold]
rmc-ui-intel-objectives = [bold]Цели[/bold]
rmc-ui-intel-documents = [color=#5B88B0]Документы:[/color]
rmc-ui-intel-upload-data = [color=#5B88B0]Загружено данных:[/color]
rmc-ui-intel-retrieve-items = [color=#5B88B0]Извлечено предметов:[/color]
rmc-ui-intel-miscellaneous = [color=#5B88B0]Разное:[/color]
rmc-ui-intel-analyze-chemicals = [color=#5B88B0]Анализировано химикатов:[/color]
rmc-ui-intel-rescue-survivors = [color=#5B88B0]Спасено выживших:[/color]
rmc-ui-intel-recover-corpses = [color=#5B88B0]Забрано тел:[/color]
rmc-ui-intel-colony-comms = [color=#5B88B0]Связь колонии:[/color]
rmc-ui-intel-colony-power = [color=#5B88B0]Энергия колонии:[/color]
rmc-ui-intel-clues = [bold]Подсказки[/bold]
rmc-ui-intel-points-value = { $value }
rmc-ui-intel-tier-value = { $value }
rmc-ui-intel-total-credits = Всего заработано кредитов: { $value }
rmc-ui-intel-progress = { $current } / { $total }
rmc-ui-intel-infinite-progress = { $current } / ∞
rmc-ui-intel-colony-status =
    { $online ->
        [true] Онлайн.
       *[false] Оффлайн.
    }

## Tech Control Console
rmc-ui-tech-tier-header = [font size=14][bold]Уровень: { $tier }[/bold][/font]
rmc-ui-tech-points-value = Технические очки: { $value }
rmc-ui-tech-points = [font size=14][bold]Очки: { $points }[/bold][/font]
rmc-ui-tech-repurchasable = Выкупить
rmc-ui-tech-incremental-price = Увеличивающаяся цена: +{ $increase } за покупку
rmc-ui-tech-purchase-button = Купить

rmc-ui-tech-information-header = [bold]Информация[/bold]
rmc-ui-tech-name-label = [color=#5B88B0]Название:[/color]
rmc-ui-tech-description-label = [color=#5B88B0]Описание:[/color]
rmc-ui-tech-cost-label = [color=#5B88B0]Стоимость:[/color]
rmc-ui-tech-statistics-label = [color=#5B88B0]Статистика:[/color]

## Tech Tree Options
rmc-intel-tech-unlock-tier-1-name = Разблокировать Уровень 1
rmc-intel-tech-unlock-tier-2-name = Разблокировать Уровень 2
rmc-intel-tech-unlock-tier-3-name = Разблокировать Уровень 3
rmc-intel-tech-unlock-tier-4-name = Разблокировать Уровень 4
rmc-intel-tech-unlock-tier-description = Переход дерева технологий на следующий этап развития.
rmc-intel-tech-requisition-arc-name = Хамви БРК
rmc-intel-tech-requisition-arc-description = Разблокирует БРК конфигурацию бронеавтомобиля Хамви.
rmc-intel-tech-requisition-budget-name = Увеличение бюджета снабжения
rmc-intel-tech-requisition-budget-description = Выделение дополнительных ресурсов отделу снабжения для закупок.
rmc-intel-tech-dropship-budget-name = Увеличение бюджета десантного шаттла
rmc-intel-tech-dropship-budget-description = Выделение дополнительных ресурсов для фабрикатора компонентов десантного корабля.
## CCM Start
ccm-intel-tech-requsition-tank-name = Танк
ccm-intel-tech-requsition-tank-description = Разрешает развертывание тяжелого бронированного танка. Главная ударная сила на передовой.
## CCM End
rmc-intel-tech-ob-he-name = Доп. снаряды ОБ - Фугасные
rmc-intel-tech-ob-he-description = Осколочно-фугасные боеприпасы для орбитальной пушки.
rmc-intel-tech-ob-incendiary-name = Доп. снаряды ОБ - Зажигательные
rmc-intel-tech-ob-incendiary-description = Зажигательные боеприпасы для орбитальной пушки.
rmc-intel-tech-ob-cluster-name = Доп. снаряды ОБ - Кассетные
rmc-intel-tech-ob-cluster-description = Кассетные боеприпасы для орбитальной пушки.
rmc-intel-tech-wake-troops-name = Разбудить дополнительный отряд
rmc-intel-tech-wake-troops-description = Вывод из криосна дополнительных пехотинцев для усиления операции.
rmc-intel-tech-wake-specialist-name = Разбудить дополнительного специалиста
rmc-intel-tech-wake-specialist-description = Вывод из криосна ещё одного обученного специалиста.
rmc-intel-tech-nuclear-device-name = Ядерное устройство
rmc-intel-tech-nuclear-device-description = Приобретение тактического ядерного заряда. Доступно только через 80 минут после начала операции. Единственный способ быть во всём уверенным.

## Tech Tree Announcements
rmc-intel-tech-announcement-special-assets-author = УПРАВЛЕНИЕ АКТИВАМИ «ДАВИДА»
rmc-intel-tech-announcement-defcon-author = ПОВЫШЕНА БОЕВАЯ ГОТОВНОСТЬ «ДАВИДА»
rmc-intel-tech-announcement-arc = Для данной операции санкционировано развёртывание системы БРК.
rmc-intel-tech-announcement-requisition-budget = Для данной операции санкционирован дополнительный бюджет снабжения.
rmc-intel-tech-announcement-dropship-budget = Для данной операции санкционированы дополнительные очки фабрикатора компонентов десантного корабля.
rmc-intel-tech-announcement-tier-2 = УРОВЕНЬ ОЦЕНКИ УГРОЗЫ ПОВЫШЕН ДО 2.

              Разрешено использование активов Уровня 2
## CCM Start
ccm-intel-tech-announcement-tank = Для наземных операций разрешена поддержка тяжелой бронетехникой.
## CCM End
rmc-intel-tech-announcement-ob-he = На склад снабжения доставлены дополнительные фугасные боеголовки для орбитальной бомбардировки.
rmc-intel-tech-announcement-ob-incendiary = На склад снабжения доставлены дополнительные зажигательные боеголовки для орбитальной бомбардировки.
rmc-intel-tech-announcement-ob-cluster = На склад снабжения доставлены дополнительные кассетные боеголовки для орбитальной бомбардировки.
rmc-intel-tech-announcement-tier-3 = УРОВЕНЬ ОЦЕНКИ УГРОЗЫ ПОВЫШЕН ДО 3.

              Разрешено использование активов Уровня 3
rmc-intel-tech-announcement-wake-troops = Дополнительный отряд выводится из криосна.
rmc-intel-tech-announcement-wake-specialist = Дополнительный специалист выводится из криосна.
rmc-intel-tech-announcement-tier-4 = УРОВЕНЬ ОЦЕНКИ УГРОЗЫ ПОВЫШЕН ДО 4.

              Разрешено использование активов Уровня 4
rmc-intel-tech-announcement-nuclear-device = Использование ядерного вооружения санкционировано. Заряд будет доставлен на склад снабжения через ASRS.