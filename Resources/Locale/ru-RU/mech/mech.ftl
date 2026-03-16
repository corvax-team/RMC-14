# UI
mech-menu-title = панель управления мехом
mech-equipment-label = Снаряжение
mech-equipment-begin-install = Installing the { THE($item) }...
mech-equipment-finish-install = Finished installing the { THE($item) }
mech-equipment-select-popup = { $item } selected
mech-equipment-select-none-popup = Nothing selected
mech-modules-label = Модули
# Verbs
mech-verb-enter = Войти
mech-verb-exit = Извлечь пилота
mech-ui-open-verb = Открыть панель управления
# Installation
mech-install-begin-popup = Установка { THE($item) }...
mech-slot-display = Open Slots: { $amount }
mech-no-enter = You cannot pilot this.
mech-eject-pilot-alert = { $user } is pulling the pilot out of the { $item }!
mech-install-finish-popup = Установка { THE($item) } завершена
mech-cannot-modify-closed-popup = Нельзя модифицировать при закрытой кабине!
mech-duplicate-installed-popup = Идентичный предмет уже установлен.
mech-cannot-insert-broken-popup = Нельзя ничего вставлять, пока мех сломан.
mech-equipment-slot-full-popup = Нет свободных слотов для снаряжения.
mech-module-slot-full-popup = Нет свободных слотов для модулей.
mech-equipment-whitelist-fail-popup = Снаряжение несовместимо с этим мехом.
mech-module-whitelist-fail-popup = Модуль несовместим с этим мехом.
# Selection
mech-select-popup = { $item } выбрано
mech-select-none-popup = Ничего не выбрано
# Radial menu
mech-radial-no-equipment = Нет снаряжения
# Status displays
mech-integrity-display-label = Целостность
mech-integrity-display = { $amount } %
mech-integrity-display-broken = СЛОМАН
mech-energy-display-label = Энергия
mech-energy-display = { $amount } %
mech-energy-missing = ОТСУТСТВУЕТ
mech-equipment-slot-display = Снаряжение: { $used }/{ $max }
mech-module-slot-display = Модули: { $used }/{ $max }
mech-grabber-capacity = { $current }/{ $max }
mech-no-data-status = Нет данных о герметичности
mech-generator-output = Выход: { $rate } Вт
mech-generator-fuel = Топливо: { $amount } ({ $name })
# Atmospheric system
mech-cabin-pressure-label = Воздух в кабине:
mech-cabin-pressure-level = { $level } кПа
mech-cabin-temperature-label = Температура:
mech-cabin-temperature-level = { $tempC } °C
mech-air-toggle = Переключить
mech-cabin-purge = Продуть
mech-airtight-unavailable-label = Кабина негерметична
mech-tank-pressure-label = Воздух в баллоне:
mech-tank-pressure-level =
    { $state ->
        [ok] { $pressure } кПа
       *[na] Н/Д
    }
# Fan system
mech-fan-label = Вентилятор:
mech-fan-status-label = Статус вентилятора:
mech-fan-status-level =
    { $state ->
        [on] Вкл
        [idle] Ожидание
        [off] Выкл
       *[na] Н/Д
    }
mech-fan-missing = Нет модуля вентилятора
mech-filter-enabled = Фильтр
# Access restriction
mech-no-enter-popup = Вы не можете пилотировать это.
# Alert
mech-eject-pilot-alert-popup = { $user } извлекает пилота из { $item }!
# Settings access banner
mech-settings-no-access-label = Доступ запрещён
mech-remove-disabled-tooltip = Нельзя извлечь, пока внутри находится пилот.
