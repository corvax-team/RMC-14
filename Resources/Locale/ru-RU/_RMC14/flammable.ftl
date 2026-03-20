rmc-immune-to-ignition-examine = [color=cyan]{ CAPITALIZE(SUBJECT($ent)) } не может быть { $direct ->
        [true] { "" }
       *[false] { "косвенно " }
    }воспламенен![/color]
rmc-immune-to-fire-tile-damage-examine = [color=cyan]{ CAPITALIZE(SUBJECT($ent)) } не получает урона от огня на плитках![/color]
rmc-fire-armor-debuff-modifier-examine = [color=cyan]Броня { POSS-ADJ($ent) } снижается на { $percentage }% меньше, когда { SUBJECT($ent) } стоит на зеленом огне![/color]
