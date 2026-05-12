# V2 хирургия — строки интерфейса и названия операций.

# ---- Окно ------------------------------------------------------------

cmu-medical-surgery-window-title = Хирургическая операция
cmu-medical-surgery-window-hint = Выберите часть тела, выберите операцию, затем нажмите на пациента нужным инструментом.
cmu-medical-surgery-no-eligible = Здесь нет доступных операций.
cmu-medical-surgery-section-parts = Части тела
cmu-medical-surgery-section-surgeries = Операции
cmu-medical-surgery-section-surgeries-on = Операции на: { $part }
cmu-medical-surgery-arm-button = Начать операцию
cmu-medical-surgery-cancel-armed = Отменить операцию
cmu-medical-surgery-step-hint = Шаг { $step }/{ $total } — { $label } ({ $tool })
cmu-medical-surgery-step-hint-prereq = Подготовительный шаг { $step }/{ $total } — { $label } ({ $tool })
cmu-medical-surgery-armed-heading = ПОДГОТОВЛЕНО

# ---- Блок текущей операции ------------------------------------------

cmu-medical-surgery-in-progress-heading = ОПЕРАЦИЯ ИДЁТ
cmu-medical-surgery-in-progress-subtitle = { $surgery } · { $part }
cmu-medical-surgery-in-progress-credit = Начал: { $surgeon } · { $elapsed } назад
cmu-medical-surgery-step-now = Шаг { $step }: { $label }
cmu-medical-surgery-action-hint = Нажмите на { $part }, держа в руке { $tool }.
cmu-medical-surgery-action-hint-no-tool = Нажмите на { $part }, чтобы продолжить.
cmu-medical-surgery-continue-button = Продолжить операцию
cmu-medical-surgery-abandon-button = Бросить операцию
cmu-medical-surgery-choose-next-heading = Выбор следующей операции
cmu-medical-surgery-choose-next-hint = Продолжите другое восстановление на этой части тела или закройте рану.

# ---- Статусы частей тела --------------------------------------------

cmu-medical-surgery-part-heading = { $part }
cmu-medical-surgery-part-condition-healthy = Здорова
cmu-medical-surgery-part-condition-locked = На { $other } уже идёт другая операция — сначала завершите или бросьте её
cmu-medical-surgery-part-condition-no-eligible = Нет доступных операций

cmu-medical-surgery-condition-incision-open = Разрез открыт
cmu-medical-surgery-condition-ribcage-open = Грудная клетка раскрыта
cmu-medical-surgery-condition-fracture = { $severity } перелом
cmu-medical-surgery-condition-internal-bleed = Внутреннее кровотечение
cmu-medical-surgery-condition-in-progress = Операция в процессе
cmu-medical-surgery-condition-missing = Отсечена

# ---- Категории в BUI ------------------------------------------------

cmu-medical-surgery-category-fracture = Переломы
cmu-medical-surgery-category-bleed = Внутренние кровотечения
cmu-medical-surgery-category-remove_organ = Извлечение органов
cmu-medical-surgery-category-transplant = Пересадка органов
cmu-medical-surgery-category-suture = Ушивание органов
cmu-medical-surgery-category-head_organ = Операции на голове
cmu-medical-surgery-category-reattach = Пришивание конечности
cmu-medical-surgery-category-close_up = Закрытие
cmu-medical-surgery-category-general = Прочее

# ---- Осмотр ---------------------------------------------------------

cmu-medical-surgery-examine-patient-in-progress = [color=#dca94c]Идёт операция «{ $surgery }» (хирург: { $surgeon }) — далее: { $next }.[/color]
cmu-medical-surgery-examine-part-in-progress = [color=#dca94c]На этой части тела идёт операция «{ $surgery }» (хирург: { $surgeon }) — далее: { $next }.[/color]
cmu-medical-surgery-examine-part-abandoned = [color=#888888]Открытая рана — операция не выполняется.[/color]

# ---- Закрывающие шаги -----------------------------------------------

cmu-medical-surgery-step-close-incision-label = Закрыть разрез
cmu-medical-surgery-step-mend-ribcage-label = Восстановить грудную клетку
cmu-medical-surgery-step-mend-skull-label = Восстановить череп
cmu-medical-surgery-step-mend-bones-label = Восстановить кости
cmu-medical-surgery-step-close-bones-label = Закрыть кости

# ---- Вооружённый шаг ------------------------------------------------

cmu-medical-surgery-armed-none = (операция не выбрана)
cmu-medical-surgery-armed-step = Подготовлено: { $surgery } — шаг { $step } ({ $tool })
cmu-medical-surgery-armed-cancelled = Операция отменена.
cmu-medical-surgery-armed-expired = Выбор операции истёк.

# ---- Всплывающие сообщения ------------------------------------------

cmu-medical-surgery-wrong-part = Это не та часть тела, для которой была выбрана операция.
cmu-medical-surgery-wrong-tool = Для этого шага нужен другой инструмент.
cmu-medical-surgery-wrong-tool-damage = Вы соскальзываете с инструментом { $tool }!
cmu-medical-surgery-no-tool = Для этого шага нужен хирургический инструмент.
cmu-medical-surgery-wrong-limb = Эта конечность не подходит ни к одному пустому слоту пациента.

# ---- Категории инструментов -----------------------------------------

cmu-medical-surgery-tool-category-scalpel = Скальпель
cmu-medical-surgery-tool-category-hemostat = Гемостат
cmu-medical-surgery-tool-category-retractor = Ранорасширитель
cmu-medical-surgery-tool-category-cautery = Прижигатель
cmu-medical-surgery-tool-category-bone_saw = Костная пила
cmu-medical-surgery-tool-category-bone_setter = Костоправ
cmu-medical-surgery-tool-category-bone_gel = Костный гель
cmu-medical-surgery-tool-category-bone_graft = Костный трансплантат
cmu-medical-surgery-tool-category-organ_clamp = Зажим для органов

# ---- Названия шагов -------------------------------------------------

cmu-medical-surgery-step-realign-simple-label = Сопоставить простой перелом
cmu-medical-surgery-step-realign-compound-label = Сопоставить сложный перелом
cmu-medical-surgery-step-realign-comminuted-label = Сопоставить оскольчатый перелом
cmu-medical-surgery-step-apply-gel-label = Нанести костный гель
cmu-medical-surgery-step-apply-gel-second-label = Нанести костный гель (второй слой)
cmu-medical-surgery-step-insert-graft-label = Установить костный трансплантат
cmu-medical-surgery-step-cauterize-bleed-label = Пережать внутреннее кровотечение
cmu-medical-surgery-step-clamp-liver-label = Пережать сосуды печени
cmu-medical-surgery-step-clamp-lungs-label = Пережать сосуды лёгких
cmu-medical-surgery-step-clamp-kidneys-label = Пережать сосуды почек
cmu-medical-surgery-step-clamp-heart-label = Пережать сосуды сердца
cmu-medical-surgery-step-clamp-stomach-label = Пережать сосуды желудка
cmu-medical-surgery-step-extract-liver-label = Извлечь печень
cmu-medical-surgery-step-extract-lungs-label = Извлечь лёгкие
cmu-medical-surgery-step-extract-kidneys-label = Извлечь почки
cmu-medical-surgery-step-extract-heart-label = Извлечь сердце
cmu-medical-surgery-step-extract-stomach-label = Извлечь желудок
cmu-medical-surgery-step-reinsert-liver-label = Установить новую печень
cmu-medical-surgery-step-reinsert-lungs-label = Установить новые лёгкие
cmu-medical-surgery-step-reinsert-kidneys-label = Установить новые почки
cmu-medical-surgery-step-reinsert-stomach-label = Установить новый желудок
cmu-medical-surgery-step-transplant-heart-label = Пересадить донорское сердце
cmu-medical-surgery-step-suture-liver-label = Ушить печень
cmu-medical-surgery-step-suture-lungs-label = Ушить лёгкие
cmu-medical-surgery-step-suture-kidneys-label = Ушить почки
cmu-medical-surgery-step-suture-heart-label = Ушить сердце
cmu-medical-surgery-step-suture-stomach-label = Ушить желудок
cmu-medical-surgery-step-amputate-limb-label = Ампутировать конечность
cmu-medical-surgery-step-trim-necrotic-stump-label = Иссечь некротическую культю
cmu-medical-surgery-step-prep-reattachment-socket-label = Подготовить место для пришивания
cmu-medical-surgery-step-reattach-limb-label = Пришить отсечённую конечность
cmu-medical-surgery-step-debride-eschar-label = Очистить струп

# ---- Названия операций ----------------------------------------------

cmu-medical-surgery-name-set-fracture = Вправление перелома
cmu-medical-surgery-name-stop-internal-bleeding = Остановить внутреннее кровотечение
cmu-medical-surgery-name-remove-liver = Удаление печени
cmu-medical-surgery-name-remove-lungs = Удаление лёгких
cmu-medical-surgery-name-remove-kidneys = Удаление почек
cmu-medical-surgery-name-remove-heart = Удаление сердца
cmu-medical-surgery-name-remove-stomach = Удаление желудка
cmu-medical-surgery-name-replace-liver = Замена печени
cmu-medical-surgery-name-replace-lungs = Замена лёгких
cmu-medical-surgery-name-replace-kidneys = Замена почек
cmu-medical-surgery-name-transplant-heart = Пересадка сердца
cmu-medical-surgery-name-replace-stomach = Замена желудка
cmu-medical-surgery-name-suture-liver = Ушивание печени
cmu-medical-surgery-name-suture-lungs = Ушивание лёгких
cmu-medical-surgery-name-suture-kidneys = Ушивание почек
cmu-medical-surgery-name-suture-heart = Ушивание сердца
cmu-medical-surgery-name-suture-stomach = Ушивание желудка
cmu-medical-surgery-name-repair-brain = Восстановление мозга
cmu-medical-surgery-name-repair-eyes = Восстановление глаз
cmu-medical-surgery-name-repair-ears = Восстановление ушей
cmu-medical-surgery-name-remove-limb = Ампутация конечности
cmu-medical-surgery-name-reattach-limb = Пришивание конечности
cmu-medical-surgery-name-remove-larva = Извлечение личинки
cmu-medical-surgery-name-debride-eschar = Очистка струпа

# ---- Части тела ------------------------------------------------------

cmu-medical-body-part-head = Голова
cmu-medical-body-part-torso = Торс
cmu-medical-body-part-arm = Рука
cmu-medical-body-part-left-arm = Левая рука
cmu-medical-body-part-right-arm = Правая рука
cmu-medical-body-part-leg = Нога
cmu-medical-body-part-left-leg = Левая нога
cmu-medical-body-part-right-leg = Правая нога
cmu-medical-body-part-hand = Кисть
cmu-medical-body-part-left-hand = Левая кисть
cmu-medical-body-part-right-hand = Правая кисть
cmu-medical-body-part-foot = Стопа
cmu-medical-body-part-left-foot = Левая стопа
cmu-medical-body-part-right-foot = Правая стопа
cmu-medical-body-part-tail = Хвост

# ---- Missing UI sections --------------------------------------------

cmu-medical-surgery-section-patient = Пациент
cmu-medical-surgery-section-workflow = Рабочий процесс
cmu-medical-surgery-workflow-ready = Операция не выбрана
cmu-medical-surgery-workflow-active = { $surgery } выполняется на { $part }.

cmu-medical-surgery-no-part-selected = Выберите часть тела.
cmu-medical-surgery-procedure-detail = { $step } / { $tool }

cmu-medical-surgery-close-up-button = Завершить
cmu-medical-surgery-continue-with-button = Продолжить с { $surgery }
cmu-medical-surgery-actions-heading = Действия

# ---- Missing conditions ----------------------------------------------

cmu-medical-surgery-condition-skull-open = Череп вскрыт
cmu-medical-surgery-condition-bones-open = Кости вскрыты
cmu-medical-surgery-condition-eschar = Струп

cmu-medical-fracture-severity-hairline = волосяной
cmu-medical-fracture-severity-simple = простой
cmu-medical-fracture-severity-compound = открытый
cmu-medical-fracture-severity-comminuted = оскольчатый
cmu-medical-fracture-stabilized-prefix = стабилизированный 
cmu-medical-examine-fracture-description = { $stabilized }{ $severity } перелом

# ---- Armed system extras --------------------------------------------

cmu-medical-surgery-auto-armed = Выбрано { $surgery }.
cmu-medical-surgery-auto-continue = Продолжается { $surgery }.
cmu-medical-surgery-choose-repair-or-close = Выберите восстановление органа или закройте рану.

# ---- Failure / interaction popups -----------------------------------

cmu-medical-surgery-improvised-mishap = Самодельный { $tool } соскальзывает и наносит дополнительную травму.
cmu-medical-surgery-step-failed = Операция срывается и вызывает травму.
cmu-medical-surgery-step-failed-with-tool = { $tool } соскальзывает и вызывает хирургическую травму.

cmu-medical-surgery-missing-skills = Вы не умеете выполнять этот шаг.
cmu-medical-surgery-cannot-start = Эта операция больше недоступна.

cmu-medical-surgery-needs-operating-table = Перенесите пациента на операционный стол.
cmu-medical-surgery-remove-helmet = Снимите шлем.
cmu-medical-surgery-remove-armor = Снимите броню.

cmu-medical-surgery-patient-not-lying = Пациент должен лежать или быть зафиксирован.
cmu-medical-surgery-patient-not-controlled = Пациенту нужна анестезия, сильные обезболивающие или фиксация.

cmu-medical-surgery-self-pain-control = Самооперация требует сильных обезболивающих.
cmu-medical-surgery-self-not-secured = Пристегните себя к креслу, кровати или каталке.
cmu-medical-surgery-self-not-allowed = Вы не можете выполнить эту операцию на себе.

cmu-medical-surgery-step-pain-interrupted = Боль пациента прерывает хирургический шаг.
cmu-medical-surgery-welder-not-lit = Сначала зажгите инструмент.

cmu-medical-amputation-success = Конечность удалена.

# ---- Autodoc ---------------------------------------------------------

cmu-autodoc-window-title = Автодок
cmu-autodoc-no-patient = Нет пациента
cmu-autodoc-status-no-pod = Рядом не подключена капсула автодока.
cmu-autodoc-status-empty = Подключённая капсула пуста.
cmu-autodoc-status-ready = Готов к постановке автоматических процедур в очередь.
cmu-autodoc-status-running = Выполняются процедуры из очереди.
cmu-autodoc-current-idle = Текущая процедура: ожидание
cmu-autodoc-current-step = Текущая процедура: { $step }
cmu-autodoc-current-step-timed = Текущая процедура: { $step } ({ $time } осталось)
cmu-autodoc-current-step-detail = { $surgery } / { $part } / { $step }
cmu-autodoc-start-button = Запуск
cmu-autodoc-stop-button = Остановить
cmu-autodoc-clear-button = Очистить
cmu-autodoc-eject-button = Извлечь пациента
cmu-autodoc-remove-button = Удалить
cmu-autodoc-queue-button = В очередь
cmu-autodoc-queue-heading = Очередь
cmu-autodoc-parts-heading = Части тела
cmu-autodoc-surgeries-heading = Операции
cmu-autodoc-queue-empty = Нет процедур в очереди.
cmu-autodoc-queue-summary = Процедур в очереди: { $count }
cmu-autodoc-available-procedures = Доступно процедур: { $count }
cmu-autodoc-part-procedures = Процедур: { $count }
cmu-autodoc-surgery2-required = Для постановки процедур автодока требуется навык хирургии 2.
cmu-autodoc-no-surgeries = Здесь нет доступных операций.
cmu-autodoc-queue-row = #{ $index } { $surgery } на { $part } - { $step }
cmu-autodoc-surgery-row = { $surgery } - { $step }
cmu-autodoc-automated-step-label = Автоматический цикл восстановления
cmu-autodoc-automated-step-note = Автодок восстанавливает цель по таймеру машины.
cmu-autodoc-repair-wounds-surgery = Лечение ран / ожогов
cmu-autodoc-procedure-time-note = Автоматическая процедура: { $time }.
cmu-autodoc-minutes = { $minutes } мин

# ---- Body scanner ----------------------------------------------------

cmu-body-scanner-window-title = Сканер тела
cmu-body-scanner-no-patient = Нет пациента
cmu-body-scanner-status-no-pod = Рядом не подключена капсула сканера тела.
cmu-body-scanner-status-empty = Подключённая капсула сканера пуста.
cmu-body-scanner-status-ready = Сканирование пациента готово.
cmu-body-scanner-status-no-skill = Для завершения сканирования требуется навык хирургии 1.
cmu-body-scanner-boost-active = Хирургическая калибровка активна: осталось { $time }.
cmu-body-scanner-boost-inactive = Хирургическая калибровка не выполнена.
cmu-body-scanner-scan-heading = Сканирование
cmu-body-scanner-terms-heading = Слои срезов
cmu-body-scanner-targets-heading = Активные показания среза
cmu-body-scanner-start-button = Начать калибровку
cmu-body-scanner-reset-button = Сбросить калибровку
cmu-body-scanner-eject-button = Извлечь пациента
cmu-body-scanner-surgery1-required = Для сканирования тела требуется навык хирургии 1.
cmu-body-scanner-no-scan-lines = Нет данных сканирования.
cmu-body-scanner-diagnostic-summary = Диагностических строк: { $count }
cmu-body-scanner-match-summary = Зафиксировано { $matched }/{ $required }, осталось { $time }
cmu-body-scanner-match-summary-idle = Зафиксировано { $matched }/{ $required }, не начато
cmu-body-scanner-calibrated-summary = Откалибровано, помощь активна ещё { $time }
cmu-body-scanner-calibrated-badge = ОТКАЛИБРОВАНО { $time }
cmu-body-scanner-calibration-ready = 2:00
cmu-body-scanner-lockout-summary = Активный срез заблокирован, осталось { $time }
cmu-body-scanner-lockout-status = Активный срез заблокирован: { $time } осталось.
cmu-body-scanner-lockout-detail = Калибровка провалена. Дождитесь снятия блокировки.
cmu-body-scanner-no-surgical-targets = Цели не обнаружены.
cmu-body-scanner-no-surgical-targets-detail = Усиление не получено.
cmu-body-scanner-calibration-heading = Скан анатомических срезов
cmu-body-scanner-sweep-title = Послойное сканирование
cmu-body-scanner-sweep-detail = Настройте слой для начала.
cmu-body-scanner-layer-selected = Настроенный слой - { $locked }/{ $total } зафиксировано
cmu-body-scanner-layer-ready = { $locked }/{ $total } зафиксировано
cmu-body-scanner-layer-empty = Аномальных показаний нет
cmu-body-scanner-signal-locked = Сигнал зафиксирован
cmu-body-scanner-signal-ready = { $detail } - зафиксируйте на голубом
cmu-body-scanner-start-status = Начните калибровку для запуска сканирования.
cmu-body-scanner-ready-status = Настройте слой и фиксируйте аномалии, пока волна голубая.
cmu-body-scanner-armed-status = Слой настроен: { $layer }. Фиксируйте сигналы, когда волна входит в голубую зону.
cmu-body-scanner-penalty-status = Неверное время или слой: -{ $seconds }с.
cmu-body-scanner-feedback-correct = Сигнал зафиксирован.
cmu-body-scanner-feedback-wrong-timing = Волна прошла мимо зоны захвата: -{ $seconds }с.
cmu-body-scanner-feedback-wrong-layer = Помеха слоя: -{ $seconds }с.
cmu-body-scanner-expired-status = Время истекло. Сбросьте калибровку для повторной попытки.
cmu-body-scanner-complete-status = Все сигналы зафиксированы. Хирургическая помощь откалибрована.
cmu-body-scanner-timer-active = ТАЙМЕР АКТИВНОГО СРЕЗА
cmu-body-scanner-timer-expired = ВРЕМЯ ИСТЕКЛО
cmu-body-scanner-timer-locked = СРЕЗ ЗАБЛОКИРОВАН
cmu-body-scanner-timer-detail = Зафиксируйте сигналы до закрытия окна сканирования.
cmu-body-scanner-no-layer-signals = Нет аномальных показаний на { $layer }.
cmu-body-scanner-interference-title = Неопределённый сигнал
cmu-body-scanner-interference-detail = Помехи на { $layer }
cmu-body-scanner-decoy-ready = { $detail } - шумовой отклик
cmu-body-scanner-decoy-vitals-1 = Скачок сердечного ритма
cmu-body-scanner-decoy-vitals-2 = Колебание кислорода в крови
cmu-body-scanner-decoy-detail-vitals = временный артефакт показателей
cmu-body-scanner-decoy-skeleton-1 = Тень микротрещины кости
cmu-body-scanner-decoy-skeleton-2 = Призрак смещения сустава
cmu-body-scanner-decoy-detail-skeleton = нестабильный контур кости
cmu-body-scanner-decoy-organs-1 = Размытость органа
cmu-body-scanner-decoy-organs-2 = Отражение плотности
cmu-body-scanner-decoy-detail-organs = нестабильная плотность органов
cmu-body-scanner-decoy-tissue-1 = Вспышка поверхностной ткани
cmu-body-scanner-decoy-tissue-2 = Полоса сосудистого шума
cmu-body-scanner-decoy-detail-tissue = шумный отклик мягких тканей
cmu-body-scanner-triage-stable = Стабильные показатели
cmu-body-scanner-triage-serious = Серьёзные повреждения
cmu-body-scanner-triage-critical = Критические повреждения
cmu-body-scanner-triage-clear = Немедленных аномалий не обнаружено.
cmu-body-scanner-health-stable = Стабильно
cmu-body-scanner-health-damaged = Повреждено
cmu-body-scanner-health-critical = Критично
cmu-body-scanner-section-vitals = Показатели
cmu-body-scanner-section-body = Тело
cmu-body-scanner-section-organs = Органы
cmu-body-scanner-term-assigned = { $term } -> { $target }
cmu-body-scanner-target-filled = { $target }: { $term }
cmu-body-scanner-line-state = Состояние: { $state }
cmu-body-scanner-line-damage = Урон: всего { $total } (удар { $brute }, ожог { $burn })
cmu-body-scanner-line-blood = Кровь: { $blood } / { $max }
cmu-body-scanner-heart-stopped = Сердце: активность не обнаружена
cmu-body-scanner-heart-active = Сердце: { $bpm } уд/мин
cmu-body-scanner-line-no-data = Диагностические данные отсутствуют.
cmu-body-scanner-line-part = { $part }: { $details }
cmu-body-scanner-part-health = HP { $current } / { $max }
cmu-body-scanner-part-wounds = Необработанных ран: { $count }
cmu-body-scanner-part-fracture = Перелом: { $severity }
cmu-body-scanner-part-bleed = Внутреннее кровотечение { $rate }/с
cmu-body-scanner-part-eschar = струп
cmu-body-scanner-part-splinted = наложена шина
cmu-body-scanner-part-cast = наложен гипс
cmu-body-scanner-part-tourniquet = наложен жгут
cmu-body-scanner-part-missing-limb = отсутствующая / оторванная конечность
cmu-body-scanner-line-organ = { $organ }: { $stage } ({ $current } / { $max })
cmu-body-scanner-line-missing-organ = Отсутствует { $organ } в { $part }

cmu-limb-printer-window-title = Принтер конечностей
cmu-limb-printer-header = Создание конечностей
cmu-limb-printer-matrix-heading = Матрица синтеза
cmu-limb-printer-blood-heading = Шаблон крови
cmu-limb-printer-no-beaker = Стакан с матрицей не вставлен.
cmu-limb-printer-no-syringe = Шприц с кровью не вставлен.
cmu-limb-printer-fluid-amount = { $current } / { $max } ед.
cmu-limb-printer-matrix-cost = { $cost } ед. матрицы за печать
cmu-limb-printer-blood-cost = { $cost } ед. крови за печать
cmu-limb-printer-remove-beaker = Извлечь стакан
cmu-limb-printer-remove-syringe = Извлечь шприц
cmu-limb-printer-left-heading = Левая
cmu-limb-printer-right-heading = Правая
cmu-limb-printer-print-ready = Готово к печати
cmu-limb-printer-status-ready = Готов к синтезу.
cmu-limb-printer-missing-beaker = Вставьте стакан с биогенной матрицей.
cmu-limb-printer-missing-matrix = Недостаточно биогенной матрицы.
cmu-limb-printer-missing-syringe = Вставьте шприц с кровью пациента.
cmu-limb-printer-missing-blood = Недостаточно образца крови пациента.
cmu-limb-printer-printed = Напечатано: { $limb }.
cmu-limb-printer-left-arm = Левая рука
cmu-limb-printer-left-leg = Левая нога
cmu-limb-printer-right-arm = Правая рука
cmu-limb-printer-right-leg = Правая нога
cmu-limb-printer-slot-beaker = стакан с матрицей
cmu-limb-printer-slot-syringe = шприц с кровью
