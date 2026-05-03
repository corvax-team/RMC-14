# Give Commendation Command
cmd-rmcgivecommendation-desc = Награждает игрока медалью или королевским желе.
cmd-rmcgivecommendation-help =
    Использование: rmcgivecommendation <имя_отправителя> <получатель> <имя_персонажа> <тип> <номер_награды> <текст_цитаты> [ID_раунда]
    Аргументы:
    имя_отправителя: кто выдаёт награду в рамках IC (ОБЯЗАТЕЛЬНО используйте кавычки, если есть пробелы)
    получатель: имя пользователя (username) или UserId игрока
    имя_персонажа: имя персонажа в игре (ОБЯЗАТЕЛЬНО используйте кавычки, если есть пробелы)
    тип: medal (медаль) или jelly (желе)
    номер_награды: число (используйте Tab, чтобы увидеть доступные варианты)
    текст_цитаты: причина награждения (ОБЯЗАТЕЛЬНО в кавычках)
    ID_раунда: номер раунда, по умолчанию — текущий (необязательно)

    Примеры:
      rmcgivecommendation "Верховное командование ККМП" PlayerName "John Doe" medal 1 "За исключительную храбрость"
      rmcgivecommendation "Королева-Мать" XenoPlayer "XX-Alpha" jelly 2 "За защиту улья"
      rmcgivecommendation "Верховное командование ККМП" PlayerName "John Doe" medal 1 "За исключительную храбрость" 42
# Errors
cmd-rmcgivecommendation-invalid-arguments = Неверное количество аргументов!
cmd-rmcgivecommendation-invalid-type = Неверный тип! Должно быть 'medal' или 'jelly'.
cmd-rmcgivecommendation-invalid-award-type = Неверный тип { $type }! Должно быть от 1 до { $max }.
cmd-rmcgivecommendation-empty-citation = Текст цитаты не может быть пустым!
cmd-rmcgivecommendation-player-not-found = Игрок '{ $player }' не найден.
# Success
cmd-rmcgivecommendation-success = Награда { $award } вручена игроку { $player }!
cmd-rmcgivecommendation-admin-announcement = { $admin } вручил(а) { $type } "{ $award }" игроку { $receiver } (персонаж: { $character }) за Раунд { $round }
# Completion hints
cmd-rmcgivecommendation-hint-giver = IC имя отправителя (будьте внимательны при вводе)
cmd-rmcgivecommendation-hint-giver-highcommand = Стандартный отправитель для медалей морпехов
cmd-rmcgivecommendation-hint-giver-queen-mother = Стандартный отправитель для желе ксеноморфов
cmd-rmcgivecommendation-hint-receiver = Имя пользователя или UserId получателя
cmd-rmcgivecommendation-hint-receiver-name = IC имя персонажа получателя (будьте внимательны при вводе)
cmd-rmcgivecommendation-hint-type = Тип (medal или jelly)
cmd-rmcgivecommendation-hint-type-medal = Наградить морпеха медалью
cmd-rmcgivecommendation-hint-type-jelly = Наградить ксеноморфа королевским желе
cmd-rmcgivecommendation-hint-medal-type = Тип медали (1-{ $count })
cmd-rmcgivecommendation-hint-jelly-type = Тип желе (1-{ $count })
cmd-rmcgivecommendation-hint-invalid-type = Тип должен быть 'medal' или 'jelly'
cmd-rmcgivecommendation-hint-citation = Текст цитаты (будьте внимательны при вводе IC причины)
cmd-rmcgivecommendation-hint-round = ID раунда (необязательно)
cmd-rmcgivecommendation-hint-round-current = Текущий раунд
