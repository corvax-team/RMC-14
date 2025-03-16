rmc-bioscan-ares-announcement = [color=white][font size=16][bold]АРЕС v3.2 Статус биосканирования[/bold][/font][/color][color=red][font size=14][bold]
    { $message }[/bold][/font][/color]
rmc-bioscan-ares =
    Биосканирование завершено.
    
    Датчики показывают { $shipUncontained ->
        [0] отсутствие
       *[other] { $shipUncontained }
    } { $shipUncontained ->
        [0] сигнатур неизвестных форм жизни
        [1] сигнатуру неизвестной формы жизни
        [few] сигнатуры неизвестных форм жизни
       *[other] сигнатур неизвестных форм жизни
    } на борту корабля{ $shipLocation ->
        [none] { "" }
       *[other] , в том числе около отсека { $shipLocation },
    } и { $onPlanet ->
        [0] отсутствие
       *[other] примерно { $onPlanet }
    } { $onPlanet ->
        [0] сигнатур
        [1] сигнатуру
        [few] сигнатуры
       *[other] сигнатур
    } в зоне боевых действий{ $planetLocation ->
        [none] .
       *[other] , в том числе около { $planetLocation }
    }
rmc-bioscan-xeno-announcement = [color=#318850][font size=14][bold]Королева-мать достигает вашего сознания из далёких миров.
    { $message }[/bold][/font][/color]
rmc-bioscan-xeno =
    Моим детям и их Королеве: Я чувствую { $onShip ->
        [0] отсутствие носителей
        [1] одного носителя
       *[other] примерно { $onShip } носителей
    } в металлическом улье{ $shipLocation ->
        [none] { "" }
       *[other] , в том числе около места { $shipLocation },
    } и { $onPlanet ->
        [0] отсутствие носителей
       *[other] { $onPlanet }
    }, рассеянных по разным местам{ $planetLocation ->
        [none] .
       *[other] , в том числе около { $planetLocation }
    }
