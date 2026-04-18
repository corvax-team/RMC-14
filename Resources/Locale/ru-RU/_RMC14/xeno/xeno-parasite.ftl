# General parasite messages
rmc-xeno-failed-cant-infect = Мы не можем заразить { $target }!
rmc-xeno-failed-cant-reach = Мы не можем дотянуться до { $target }, цель должна лежать!
rmc-xeno-failed-target-dead = Мы не можем заражать мёртвых!
rmc-xeno-infect-success = Паразит врезается в { $target } и срывает { $clothing }!
rmc-xeno-infect-fail = Паразит ударяется о { $clothing } { $target }!
rmc-xeno-failed-parasite-dead = Мёртвое дитя не может заражать!
rmc-xeno-cant-throw = Мы не можем бросить { $target }!
# Infection messages
rmc-xeno-parasite-dead = { CAPITALIZE($parasite) } не двигается.
rmc-xeno-parasite-announce-infect = Мы чувствуем, что { $xeno } заразил носителя в локации { $location }!
# Parasite interaction messages
rmc-xeno-parasite-player-pickup = { CAPITALIZE($parasite) } справится сама!
rmc-xeno-parasite-nonplayer-pull = Если мы потянем { $parasite }, это может ей навредить!
# Parasite AI state messages
rmc-xeno-parasite-ai-active = Она бодрствует.
rmc-xeno-parasite-ai-idle = Она отдыхает.
rmc-xeno-parasite-ai-dying = [color=red]Она должна вернуться в безопасное место![/color]
rmc-xeno-parasite-ai-eaten = { CAPITALIZE($parasite) } яростно пожирается другими паразитами-каннибалами!
# Throw action messages
rmc-xeno-throw-parasite-current = Паразитов: { $cur_paras }/{ $max_paras }
rmc-xeno-throw-parasite-too-many-parasites = Мы не можем нести больше паразитов! ({ $current }/{ $max })
rmc-xeno-throw-parasite-too-many-royals = Мы не можем нести больше королевских паразитов! ({ $current }/{ $max })
rmc-xeno-throw-no-parasites = Нет доступных обычных паразитов!
rmc-xeno-throw-wrong-parasite-type-royal = Мы не можем бросать королевского паразита так беспечно!
rmc-xeno-throw-wrong-parasite-type-regular = Бросок с такой силой убьёт паразита!
# Ghost role descriptions
rmc-xeno-parasite-ghost-role-name = Паразит
# Shared ghost role time messages
rmc-xeno-egg-ghost-need-time = Вы умерли слишком недавно. Вы сможете стать паразитом через { $seconds } сек. (всего 3 мин. ожидания).
rmc-xeno-egg-ghost-need-time-round = Вы не можете стать паразитом, пока не пройдёт достаточно времени с начала раунда ({ $seconds } сек. осталось).
rmc-xeno-egg-ghost-bypass-time = Вы успешно заразили свою цель. Ожидание сброшено, вы можете снова стать паразитом.
rmc-xeno-parasite-take-title = Стать паразитом?
rmc-xeno-egg-not-alive = Яйцо или носитель мертвы!
# Egg messages
rmc-xeno-egg-wrong-type-royal = В королевские яйца можно помещать только королевских паразитов.
rmc-xeno-egg-wrong-type-regular = В королевские яйца нельзя помещать обычных паразитов.
# Carrier availability messages
rmc-xeno-parasite-ghost-roles-available =
    { $count ->
        [one] Доступна { $count } роль призрака
        [few] Доступно { $count } роли призрака
       *[other] Доступно { $count } ролей призрака
    }
rmc-xeno-parasite-ghost-carrier-none = У { $xeno } нет запасных паразитов.
rmc-xeno-parasite-ghost-carrier-dead = { $xeno } мертва, и все её паразиты погибли вместе с ней.
# Ghost possession failure messages
rmc-xeno-parasite-ghost-invalid = Вы не можете занять эту роль.
rmc-xeno-parasite-ghost-take-failed = Не удалось взять под контроль паразита.
rmc-xeno-parasite-ghost-no-session = Сессия не найдена.
rmc-xeno-parasite-ghost-dead = Этот паразит мёртв или истощен.
# Reserve parasite UI
rmc-xeno-reserve-parasites-title = Зарезервировать паразитов
rmc-xeno-reserve-parasites-label = Обычные паразиты
rmc-xeno-reserve-parasites-apply = Применить
# Parasite confirmation dialog
rmc-xeno-parasite-confirm-text = Вы уверены?
