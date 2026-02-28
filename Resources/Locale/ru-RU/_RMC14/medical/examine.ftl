rmc-medical-examine-unrevivable = Глаза[color=purple][italic]{ CAPITALIZE(POSS-ADJ($victim)) } пусты, нет признаков жизни.[/italic][/color]
rmc-medical-examine-headless = [color=purple][italic]{ CAPITALIZE(SUBJECT($victim)) } { CONJUGATE-BE($victim) } определённо мёртв.[/italic][/color]
rmc-medical-examine-unconscious = [color=lightblue]{ CAPITALIZE(SUBJECT($victim)) } { GENDER($victim) ->
        [epicene] кажется
       *[other] кажется
    } контужен.[/color]
rmc-medical-examine-dead = [color=red]{ CAPITALIZE(SUBJECT($victim)) } { CONJUGATE-BE($victim) } не дышит.[/color]
rmc-medical-examine-dead-simple-mob = [color=red]{ CAPITALIZE(SUBJECT($victim)) } { CONJUGATE-BE($victim) } МЁРТВ. Выбросил всё на ветер.[/color]
rmc-medical-examine-dead-xeno = [color=red]{ CAPITALIZE(SUBJECT($victim)) } { CONJUGATE-BE($victim) } МЁРТВ. Выбросил всё на ветер. Отправляйся на свои ксено-небеса.[/color]
rmc-medical-examine-alive = [color=green]{ CAPITALIZE(SUBJECT($victim)) } { CONJUGATE-BE($victim) } жив и дышит.[/color]
rmc-medical-examine-bleeding = [color=#d10a0a]{ CAPITALIZE(SUBJECT($victim)) } { CONJUGATE-HAVE($victim) } кровоточащие раны на теле { POSS-ADJ($victim) }.[/color]
rmc-medical-examine-verb = Показать медицинские взаимодействия
