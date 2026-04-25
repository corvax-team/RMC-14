# List Commendations Command
cmd-rmclistcommendations-desc = Выводит список похвал по раунду, игроку, ID или последним записям.
cmd-rmclistcommendations-help =
    Использование:
    rmclistcommendations last <количество> [тип]
      - Выводит список последних похвал
      - количество: число последних записей для отображения
      - тип: фильтр типа похвалы (по умолчанию — все)

    rmclistcommendations round <ID_раунда> [тип]
      - Выводит все похвалы за конкретный раунд
      - тип: фильтр типа похвалы (по умолчанию — все)

    rmclistcommendations id <ID_похвалы>
      - Выводит одну конкретную похвалу по её ID

    rmclistcommendations player giver <имя_или_ID> <количество> [тип]
      - Выводит похвалы, выданные игроком
      - количество: число последних записей для отображения
      - тип: фильтр типа похвалы (по умолчанию — все)

    rmclistcommendations player receiver <имя_или_ID> <количество> [тип]
      - Выводит похвалы, полученные игроком
      - количество: число последних записей для отображения
      - тип: фильтр типа похвалы (по умолчанию — все)

    Примеры:
      rmclistcommendations last 10
      rmclistcommendations last 5 jelly
      rmclistcommendations round 42
      rmclistcommendations round 42 medal
      rmclistcommendations id 128
      rmclistcommendations player giver PlayerName 10
      rmclistcommendations player receiver PlayerName 5 jelly
# Errors
cmd-rmclistcommendations-invalid-arguments = Неверные аргументы!
cmd-rmclistcommendations-invalid-round-id = Неверный ID раунда!
cmd-rmclistcommendations-invalid-id = Неверный ID похвалы!
cmd-rmclistcommendations-invalid-type = Неверный тип '{ $type }'!
cmd-rmclistcommendations-invalid-player-mode = Неверный режим игрока! Должно быть 'giver' или 'receiver'.
cmd-rmclistcommendations-invalid-count = Неверное количество! Должно быть положительным числом.
cmd-rmclistcommendations-player-not-found = Игрок '{ $player }' не найден.
cmd-rmclistcommendations-no-results = Похвалы не найдены.
# Headers
cmd-rmclistcommendations-last-header = Отображение { $count } последних похвал (запрошено: { $total }):
cmd-rmclistcommendations-round-header = Похвалы для Раунда { $round } (всего { $count }):
cmd-rmclistcommendations-id-header = Похвала { $id }:
cmd-rmclistcommendations-giver-header = Отображение { $count } последних выданных похвал (запрошено: { $total }):
cmd-rmclistcommendations-receiver-header = Отображение { $count } последних полученных похвал (запрошено: { $total }):
# Format
cmd-rmclistcommendations-format = ID [{ $id }] { $type }: { $name } — { $giverUserName } ({ $giver }) → { $receiverUserName } ({ $receiver }) Раунд { $round }: { $text }
# Completion hints
cmd-rmclistcommendations-hint-mode = Режим (last, round, id или player)
cmd-rmclistcommendations-hint-mode-last = Показать последние похвалы
cmd-rmclistcommendations-hint-mode-round = Показать похвалы по раунду
cmd-rmclistcommendations-hint-mode-id = Показать похвалу по ID
cmd-rmclistcommendations-hint-mode-player = Показать похвалы по игроку
cmd-rmclistcommendations-hint-round-id = ID раунда
cmd-rmclistcommendations-hint-commendation-id = ID похвалы
cmd-rmclistcommendations-hint-player-mode = Режим игрока (giver или receiver)
cmd-rmclistcommendations-hint-player-giver = Похвалы, выданные игроком
cmd-rmclistcommendations-hint-player-receiver = Похвалы, полученные игроком
cmd-rmclistcommendations-hint-player = Имя пользователя или UserId
cmd-rmclistcommendations-hint-count = Количество записей для отображения
cmd-rmclistcommendations-hint-type = Фильтр типа похвалы
