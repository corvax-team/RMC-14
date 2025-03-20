cm-gun-unskilled = Похоже, вы не знаете как использовать { $gun }
cm-gun-no-ammo-message = У вас не осталось боеприпасов!
cm-gun-use-delay =
    Вам нужно выждать { $seconds } { $seconds ->
        [one] секунду
        [few] секунды
        *[other] секунд
    }  перед следующим выстрелом!
cm-gun-pump-examine = [bold]Нажмите вашу клавишу [color=cyan]уникального действия[/color] (Пробел по умолчанию) чтобы передёрнуть цевьё перед выстрелом.[/bold]
cm-gun-pump-first-with = Сначала вам нужно передёрнуть цевьё при помощи { $key }!
cm-gun-pump-first = Сначала вам нужно передёрнуть цевьё!

rmc-breech-loaded-open-shoot-attempt = Сначала вам нужно закрыть затвор!
rmc-breech-loaded-not-ready-to-shoot = Сначала вам нужно открыть и закрыть затвор!
rmc-breech-loaded-closed-load-attempt = Сначала вам нужно открыть затвор!
rmc-breech-loaded-closed-extract-attempt = Сначала вам нужно открыть затвор!

rmc-wield-use-delay =
    Вам нужно подождать { $seconds }  { $seconds ->
        [one] секунду
        [few] секунды
        *[other] секунд
    } перед тем, как взять { $wieldable } в две руки!
rmc-shoot-use-delay =
    Вам нужно подождать { $seconds }  { $seconds ->
        [one] секунду
        [few] секунды
        *[other] секунд
    } перед тем, как вы сможете выстрелить из { $wieldable }!

rmc-shoot-harness-required = Нужна упряжь
rmc-wear-smart-gun-required = Чтобы надеть их, вы должны быть оснащены смартганом.

rmc-shoot-id-lock-unauthorized = Спусковой крючок заблокирован. Неавторизованный пользователь.
rmc-id-lock-unauthorized = Действие запрещено. Неавторизованный пользователь.
rmc-id-lock-authorization = Вы подняли { $gun }, зарегистрировав себя в качестве владельца.
rmc-id-lock-authorization-combat = ( CAPITALIZE{$gun} ) регистрирует вас в качестве владельца.
rmc-id-lock-toggle-lock = Вы { $action } ID-замок на { $gun }.

rmc-id-lock-color-unauthorized = красный
rmc-id-lock-color-authorized = жёлто-зелёный
rmc-id-lock-toggle-on = заблокировали
rmc-id-lock-toggle-off = разблокировали

rmc-iff-toggle = Вы { $action } систему "свой-чужой" на { $gun }.
rmc-iff-toggle-off = отключаете
rmc-iff-toggle-on = включаете

rmc-revolver-spin = Вы вращаете барабан.

rmc-examine-text-weapon-accuracy = Текущий множитель точности составляет [color={ $colour }]{TOSTRING($accuracy, "F2")}[/color].

rmc-examine-text-scatter-max = Текущий максимальный разброс составляет [color={ $colour }]{TOSTRING($scatter, "F1")}[/color] градусов.
rmc-examine-text-scatter-min = Текущий минимальный разброс составляет [color={ $colour }]{TOSTRING($scatter, "F1")}[/color] градусов.
rmc-examine-text-shots-to-max-scatter =
      Максимальный разброс будет достигнут за [color={ $colour }]{ $shots } { $shots ->
        [one] выстрел
        [few] выстрела
        *[other] выстрелов
    }[/color] .
rmc-examine-text-iff = [color=cyan]Это оружие будет игнорировать и стрелять мимо союзников![/color]
rmc-examine-text-id-lock-no-user = [color=chartreuse]Оно не зарегистрировано. Поднимите его, чтобы зарегистрировать себя в качестве владельца.[/color]
rmc-examine-text-id-lock = [color=chartreuse]Оно зарегистрировано на [/color][color={ $colour }]{ $name }[/color][color=chartreuse].[/color]
rmc-examine-text-id-lock-unlocked = [color=chartreuse]Оно зарегистрировано на [/color][color={ $colour }]{ $name }[/color][color=chartreuse], но ограничения на огонь сняты.[/color]

rmc-gun-rack-examine = [bold]Нажмите вашу клавишу [color=cyan]уникального действия[/color] (Пробел по умолчанию) чтобы передёрнуть затвор перед выстрелом.[/bold]
rmc-gun-rack-first-with = Сначала вам нужно передёрнуть затвор оружия при помощи { $key }!
rmc-gun-rack-first = Сначала вам нужно передёрнуть затвор оружия!

rmc-assisted-reload-fail-angle = Вы должны стоять позади { $target }, чтобы перезарядить {POSS-ADJ( $target) } оружие!
rmc-assisted-reload-fail-full = { CAPITALIZE(POSS-ADJ($target)) } { $weapon } уже заряжен.
rmc-assisted-reload-fail-mismatch = { CAPITALIZE($ammo) } нельзя зарядить в { $weapon }!
rmc-assisted-reload-start-user = Вы начинаете перезаряжать { $weapon } { $target }! Стойте на месте...
rmc-assisted-reload-start-target = { $reloader } заряжать { $ammo } в ваш { $weapon }! Стойте на месте...

rmc-gun-stacks-hit-single = В яблочко!
rmc-gun-stacks-hit-multiple =
    В яблочко! { $hits }{ $hits ->
        [one] попадание
        [few] попадания
        *[other] попаданий
    } подряд!
rmc-gun-stacks-reset = { CAPITALIZE($weapon) } издаёт писк, теряя данные о наведении на цель и возвращаесь к обычному режиму стрельбы.
