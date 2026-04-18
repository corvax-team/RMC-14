set-game-preset-command-description = Установить игровой пресет для указанного количества предстоящих раундов. Может отображать имя и описание другого пресета, чтобы обмануть игроков.
set-game-preset-command-help-text = setgamepreset <id> [количество раундов, по умолчанию 1]
set-game-preset-optional-argument-not-integer = Если второй аргумент предоставлен, он должен быть числом.
set-game-preset-preset-error = Не удаётся найти игровой пресет "{ $preset }"
#set-game-preset-preset-set = Установлен пресет "{ $preset }"
set-game-preset-preset-set-finite =
    Установлен пресет "{ $preset }" на { $rounds ->
        [one] следующий раунд
        [few] следующие { $rounds } раунда
       *[other] следующие { $rounds } раундов
    }.
