cmu-medical-scanner-body-map-header        = Карта тела
cmu-medical-scanner-pulse-label            = Пульс:
cmu-medical-scanner-body-parts-header      = Части тела
cmu-medical-scanner-organs-header          = Органы
cmu-medical-scanner-fractures-header       = Переломы
cmu-medical-scanner-bleeds-header          = Внутренние кровотечения
cmu-medical-scanner-pulse-stopped          = [color=red][bold]Пульс отсутствует — сердце остановилось[/bold][/color]
cmu-medical-scanner-pulse-bpm              = { $bpm } уд/мин
cmu-medical-scanner-part-line              = { $part }: { $current }/{ $max } HP
cmu-medical-scanner-part-suffix-splinted   = (шина)
cmu-medical-scanner-part-suffix-cast       = (гипс)
cmu-medical-scanner-part-suffix-wounds =
    { $count ->
        [one] ({ $count } рана)
        [few] ({ $count } раны)
       *[many] ({ $count } ран)
    }
cmu-medical-scanner-organ-line             = { $organ }: { $stage } ({ $current }/{ $max })
cmu-medical-scanner-organ-removed          = { $organ }: [color=red]УДАЛЁН[/color]
cmu-medical-scanner-fracture-line-exact    = { $part }: перелом ({ $severity })
cmu-medical-scanner-fracture-line-vague    = { $part }: обнаружен перелом
cmu-medical-scanner-fracture-suppressed    = (подавлено)
cmu-medical-scanner-bleed-exact            = { $part }: { $rate } кровопотери/сек
cmu-medical-scanner-bleed-vague            = Обнаружено внутреннее кровотечение (место неизвестно)

cmu-medical-stethoscope-pulse              = Сердечный ритм: { $bpm }.
cmu-medical-stethoscope-pulse-qualitative  = Пульс { $description }.
cmu-medical-stethoscope-no-pulse           = Сердцебиение не обнаружено.
cmu-medical-stethoscope-no-heart           = В грудной клетке пациента отсутствует сердце.
cmu-medical-stethoscope-lungs-precise      = Лёгкие: { $stage }.
cmu-medical-stethoscope-lungs-qualitative  = Дыхание { $description }.
cmu-medical-stethoscope-no-lungs           = В грудной клетке пациента отсутствуют лёгкие.

cmu-medical-scanner-section-head           = Голова
cmu-medical-scanner-section-torso          = Торс
cmu-medical-scanner-section-arms           = Руки
cmu-medical-scanner-section-legs           = Ноги
cmu-medical-scanner-section-organs         = Органы
cmu-medical-scanner-hp                     = HP
cmu-medical-scanner-bone                   = Кость
cmu-medical-scanner-fracture               = Перелом: { $severity }
cmu-medical-scanner-fracture-vague         = Перелом: обнаружен
cmu-medical-scanner-bleed-internal         = Внутреннее кровотечение
cmu-medical-scanner-pain-unknown           = Боль: ?
cmu-medical-scanner-pain-none              = Боль: отсутствует
cmu-medical-scanner-pain-mild              = Боль: слабая
cmu-medical-scanner-pain-moderate          = Боль: умеренная
cmu-medical-scanner-pain-severe            = Боль: сильная
cmu-medical-scanner-pain-shock             = Боль: шок

# V2-ε Stat-sheet redesign — dark cards + status banner + body chart
cmu-medical-scanner-card-body              = Тело
cmu-medical-scanner-card-organs            = Органы
cmu-medical-scanner-card-reagents          = Реагенты в крови
cmu-medical-scanner-card-recommended       = Рекомендации

cmu-medical-scanner-stat-health            = ЗДОРОВЬЕ
cmu-medical-scanner-stat-pulse             = ПУЛЬС
cmu-medical-scanner-stat-blood             = КРОВЬ
cmu-medical-scanner-stat-temp              = ТЕМП °C
cmu-medical-scanner-stat-pulse-stopped     = 0
cmu-medical-scanner-stat-deceased-short    = МЁРТВ

cmu-medical-scanner-status-stable          = СТАБИЛЕН
cmu-medical-scanner-status-serious         = ТЯЖЁЛОЕ
cmu-medical-scanner-status-critical        = КРИТИЧЕСКОЕ
cmu-medical-scanner-status-deceased        = МЁРТВ

cmu-medical-scanner-severity-healthy       = Здоров
cmu-medical-scanner-severity-bruised       = Ушиб
cmu-medical-scanner-severity-damaged       = Поврежден
cmu-medical-scanner-severity-critical      = Критично
cmu-medical-scanner-severity-severed       = Отсечено

cmu-medical-scanner-chip-fracture-vague    = Перелом
cmu-medical-scanner-chip-suppressed-suffix =  (подавл.)
cmu-medical-scanner-chip-bleed             = ВК
cmu-medical-scanner-chip-bleeding          = Кровотечение
cmu-medical-scanner-chip-splint            = Шина
cmu-medical-scanner-chip-cast              = Гипс
cmu-medical-scanner-chip-tourniquet        = Жгут
cmu-medical-scanner-wound-small            = небольшая рана
cmu-medical-scanner-wound-deep             = глубокая рана
cmu-medical-scanner-wound-gaping           = зияющая рана
cmu-medical-scanner-wound-massive          = массивная рана
cmu-medical-scanner-eschar                 = струп
cmu-medical-scanner-chip-wounds =
    { $count ->
        [one] { $count } рана
        [few] { $count } раны
       *[many] { $count } ран
    }
# Skill-gate hints
cmu-medical-scanner-skill-hint-fractures   = Недостаточно подготовки для обнаружения переломов или внутренних кровотечений (требуется Med-1).
cmu-medical-scanner-skill-hint-organs      = Недостаточно подготовки для оценки повреждений органов (требуется Med-2).

# Legacy V2-ε Mix B keys
cmu-medical-scanner-vitals-pain            = Боль
cmu-medical-scanner-stable-summary         = Стабильно: { $list }
cmu-medical-scanner-acute-issues-header    = Острые проблемы
cmu-medical-scanner-acute-severed          = Отсечено: { $part }
cmu-medical-scanner-acute-fracture         = { $severity } перелом: { $part }
cmu-medical-scanner-acute-fracture-vague   = Перелом: { $part }
cmu-medical-scanner-acute-bleed            = Внутреннее кровотечение: { $part }
cmu-medical-scanner-acute-bleed-vague      = Обнаружено внутреннее кровотечение
cmu-medical-scanner-acute-organ            = { $stage }: { $organ }
cmu-medical-scanner-acute-organ-removed    = Удалён: { $organ }
cmu-medical-scanner-organ-removed-short    = Удалён

# Organ display names
cmu-medical-scanner-organ-heart            = Сердце
cmu-medical-scanner-organ-lungs            = Лёгкие
cmu-medical-scanner-organ-liver            = Печень
cmu-medical-scanner-organ-brain            = Мозг
cmu-medical-scanner-organ-kidneys          = Почки
cmu-medical-scanner-organ-stomach          = Желудок
cmu-medical-scanner-organ-eyes             = Глаза
cmu-medical-scanner-organ-ears             = Уши

cmu-medical-stethoscope-pain-mild          = Пациент выглядит испытывающим дискомфорт.
cmu-medical-stethoscope-pain-moderate      = Пациент испытывает заметную боль.
cmu-medical-stethoscope-pain-severe        = Пациент испытывает сильную боль.
cmu-medical-stethoscope-pain-shock         = Пациент находится в шоке.

# ---- Missing stat / cards / loading ---------------------------------

cmu-medical-scanner-card-patient           = Пациент
cmu-medical-scanner-card-damage            = Профиль повреждений
cmu-medical-scanner-loading                = Получение данных сканирования
cmu-medical-scanner-loading-subtext        = обработка состояния сервера

cmu-medical-scanner-stat-shock-risk        = РИСК ШОКА

# ---- Missing pulse qualitative --------------------------------------

cmu-medical-scanner-pulse-qualitative      = Пульс { $description }.

# ---- Missing skill / physiology -------------------------------------

cmu-medical-scanner-synthetic-physiology   = Обнаружена синтетическая физиология
