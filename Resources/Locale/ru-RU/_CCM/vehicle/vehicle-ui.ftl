ccm-ui-vehicle-status-title = Статус транспорта
ccm-ui-vehicle-hull-integrity = Целостность корпуса: { $integrity }%
ccm-ui-vehicle-hull-destroyed = Корпус уничтожен
ccm-ui-vehicle-door-state-label = Двери
ccm-ui-vehicle-door-state =
    { $locked ->
        [true] Заблокированы
       *[false] Разблокированы
    }
ccm-ui-vehicle-armor-resistances =
    { $unfolded ->
        [true] ↑ Сопротивления брони
       *[false] ↓ Сопротивления брони
    }
ccm-ui-vehicle-resistance-entry =
    { $type ->
        [Heat] Биологическая защита:
        [Slash] Защита от порезов:
        [Piercing] Баллистическая защита:
        [Blunt] Защита от ударов:
        [Expl] Взрывоустойчивость:
       *[other] { $type }:
    }
ccm-ui-vehicle-passengers =
    { $unfolded ->
        [true] ↑ Пассажиры
       *[false] ↓ Пассажиры
    }
ccm-ui-vehicle-total-passengers = Пассажиров:
ccm-ui-vehicle-passengers-category = Живые:
ccm-ui-vehicle-dead-category = Раненые:
ccm-ui-vehicle-xeno-category = Ксеноморфы:
ccm-ui-vehicle-role-reserved-slot =
    { $name ->
        [Crewmen] Экипаж:
        [Synthetic-Unit] Синтетики:
       *[other] { $name }:
    }
ccm-ui-vehicle-hardpoints = Узлы вооружения
ccm-ui-vehicle-no-hardpoints = Нет установленных узлов
ccm-ui-vehicle-hardpoint-integrity = Целостность: { $integrity }%
ccm-ui-vehicle-hardpoint-destroyed = Уничтожено
ccm-ui-vehicle-ammo = Боеприпасы: { $current } / { $max }
ccm-ui-vehicle-mags = Магазины: { $current } / { $max }
ccm-ui-vehicle-spare-mags = Запасные магазины:
ccm-ui-select-hardpoint-title = Выбрать точку крепления
ccm-ui-select-hardpoint-contain = Доступные точки крепления:
ccm-vehicle-ui-no-any-hardpoint = Отсутствуют доступные точки крепления.
ccm-vehicle-ui-magazine-loaded = ✓
ccm-vehicle-ui-magazine-empty = ✗
ccm-vehicle-ui-ammo-info = | Боеприпасы: { $current }/{ $max }
ccm-vehicle-ui-hardpoint-button = { $name } [{ $status }]{ $ammo }
ccm-vehicle-ui-spare-info = Запасные магазины: { $current }/{ $max }
ccm-vehicle-ui-available-weapons = Доступное оружие:
ccm-vehicle-ui-loaded-empty-legend = [✓] = Загруженный магазин | [✗] = Пустой
ccm-vehicle-ui-click-to-reload = Нажмите на оружие для перезарядки из запасных магазинов
ccm-vehicle-ui-window-title = Загрузчик боекомплекта
ccm-vehicle-slot-treads = Передвижение
ccm-vehicle-slot-support = Вспомогательное оборудование
ccm-vehicle-slot-secondary = Вторичное вооружение
ccm-vehicle-slot-primary = Основное вооружение
ccm-vehicle-slot-special = Специальный модуль
ccm-ui-attachable-holder-strip-ui-empty-slot = [Пусто]
ccm-vehicle-holder-strip-ui-title = Снятие модулей
