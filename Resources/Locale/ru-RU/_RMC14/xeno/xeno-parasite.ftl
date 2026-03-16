# General parasite messages
rmc-xeno-failed-cant-infect = Мы не можем заразить { $target }!
rmc-xeno-failed-cant-reach = Мы не можем дотянуться до { $target }, они должны лежать!
rmc-xeno-failed-target-dead = Мы не можем заражать мёртвых!
rmc-xeno-infect-success = Паразит врезается и срывает { $clothing } { $target }!
rmc-xeno-infect-fail = Паразит врезается в { $clothing } { $target }!
rmc-xeno-failed-parasite-dead = Мёртвое дитя не может заразить!
rmc-xeno-cant-throw = Мы не можем бросить { $target }!
# Infection messages
rmc-xeno-parasite-dead = { CAPITALIZE($parasite) } не движется.
rmc-xeno-parasite-announce-infect = Мы чувствуем, что { $xeno } заразил носителя в локации { $location }!
rmc-xeno-parasite-royal-final-death = После завершения миссии по распространению королевской крови вы чувствуете, что ваша жизненная сила угасает...
rmc-xeno-royal-parasite-infections-remaining =
    { $count ->
        [one] { $count } инфекция осталась
       *[other] { $count } инфекций осталось
    }
rmc-xeno-royal-parasite-no-infections-left = У нас больше нет инфекций!
rmc-xeno-royal-parasite-cooldown = Мы должны отдохнуть ещё { $seconds } секунд перед следующей инфекцией.
rmc-xeno-royal-parasite-last-infection = Это была наша последняя инфекция.
rmc-xeno-royal-parasite-ghost-role-name = Royal parasite.
# Parasite interaction messages
rmc-xeno-parasite-player-pickup = { CAPITALIZE($parasite) } может справиться сама!
rmc-xeno-parasite-nonplayer-pull = Если мы потянем { $parasite }, это может ей повредить!
# Parasite AI state messages
rmc-xeno-parasite-ai-active = Она бодрствует.
rmc-xeno-parasite-ai-idle = Она отдыхает.
rmc-xeno-parasite-ai-dying = [color=red]Она должна вернуться в безопасное место![/color]
rmc-xeno-parasite-ai-eaten = { CAPITALIZE($parasite) } яростно поедается другими паразитами-каннибалами!
# Throw action messages
rmc-xeno-throw-parasite-current = { $cur_paras }/{ $max_paras } паразитов
rmc-xeno-throw-royal-parasite-current = Королевский паразит: { $cur_royals }/{ $max_royals }
rmc-xeno-throw-parasite-too-many-parasites = Мы не можем нести больше паразитов! ({ $current }/{ $max })
rmc-xeno-throw-parasite-too-many-royals = Мы не можем нести больше королевских паразитов! ({ $current }/{ $max })
rmc-xeno-throw-no-parasites = Нет доступных обычных паразитов!
rmc-xeno-throw-no-royal-parasites = Нет доступных королевских паразитов для броска!
rmc-xeno-throw-wrong-parasite-type-royal = Мы не можем бросать королевского паразита так беспечно!
rmc-xeno-throw-wrong-parasite-type-regular = Бросок с такой силой убьёт паразита!
# Ghost role descriptions
rmc-xeno-parasite-ghost-role-name = Паразит
ccm-xeno-royal-parasite-ghost-role-name = Королевский лицехват
# Shared ghost role time messages
rmc-xeno-egg-ghost-need-time = Вы умерли слишком недавно. Вы не можете стать паразитом ещё 3 минуты ({ $seconds } секунд осталось).
rmc-xeno-egg-ghost-need-time-round = Вы не можете стать паразитом до того, как пройдёт достаточно времени в раунде ({ $seconds } секунд осталось).
rmc-xeno-egg-ghost-bypass-time = Вы успешно заразили свою цель. Вы можете снова стать паразитом.
rmc-xeno-egg-ghost-royal-confirm = Вы уверены, что хотите стать королевским паразитом?
rmc-xeno-egg-ghost-confirm = Вы уверены, что хотите стать паразитом?
rmc-xeno-egg-royal-ghost-verb = Стать королевским паразитом
rmc-xeno-parasite-take-title = Стать паразитом?
rmc-xeno-parasite-take-royal-title = Стать королевским паразитом?
rmc-xeno-egg-not-alive = Яйцо/Носитель мертвы или не живы!
# Egg messages
rmc-xeno-egg-wrong-type-royal = Королевских паразитов можно помещать только в яйца королевских паразитов
rmc-xeno-egg-wrong-type-regular = Только королевских паразитов можно помещать в яйца королевских паразитов
# Carrier availability messages
rmc-xeno-parasite-ghost-roles-available =
    { $count ->
        [one] { $count } доступная роль призрака
       *[other] { $count } доступных ролей призрака
    }
rmc-xeno-parasite-ghost-carrier-none = { $xeno } не имеет хранящихся паразитов
rmc-xeno-parasite-ghost-carrier-reserved = { CAPITALIZE($xeno) } зарезервировала оставшихся паразитов для себя.
rmc-xeno-parasite-ghost-carrier-royal-none = { $xeno } не имеет хранящихся королевских паразитов
rmc-xeno-parasite-ghost-carrier-dead = { $xeno } мертва и все её паразиты умерли вместе с ней.
rmc-xeno-parasite-carrier-death = Щебечущая масса крошечных инопланетян пытается убежать от { $xeno }!
# Ghost possession failure messages
rmc-xeno-parasite-ghost-invalid = Вы не являетесь действительным привидением.
rmc-xeno-parasite-ghost-take-failed = Не удалось взять под контроль паразита.
rmc-xeno-parasite-ghost-no-session = Не удалось найти вашу сессию для передачи паразиту.
rmc-xeno-parasite-ghost-dead = Этот паразит мёртв или истощен.
# Reserve parasite UI
rmc-xeno-reserve-parasites-title = Зарезервировать Паразитов
rmc-xeno-reserve-parasites-label = Обычные Паразиты
rmc-xeno-reserve-parasites-apply = Применить
rmc-xeno-reserve-royal-parasites-unavailable = Королевские паразиты в хранилище постоянно защищены и не могут быть зарезервированы. Призраки могут стать королевскими паразитами только через яйца или паразитов на земле.
# Parasite confirmation dialog
rmc-xeno-parasite-confirm-text = Вы уверены?
