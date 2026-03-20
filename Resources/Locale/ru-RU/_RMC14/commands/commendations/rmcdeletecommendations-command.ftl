cmd-rmcdeletecommendations-desc = Удаляет похвалы по раунду, отправителю, получателю или ID.
cmd-rmcdeletecommendations-help =
    Использование:
    rmcdeletecommendations id <ID_похвалы>
      - Удаляет одну похвалу по её ID
    
    rmcdeletecommendations round <ID_раунда> <тип>
      - Удаляет все похвалы указанного типа за конкретный раунд
      - тип: фильтр типа похвалы
    
    rmcdeletecommendations round <ID_раунда> <тип> giver <имя_или_ID_пользователя>
      - Удаляет похвалы в раунде, выданные конкретным игроком
      - тип: фильтр типа похвалы
    
    rmcdeletecommendations round <ID_раунда> <тип> receiver <имя_или_ID_пользователя>
      - Удаляет похвалы в раунде, полученные конкретным игроком
      - тип: фильтр типа похвалы
    
    Примеры:
      rmcdeletecommendations id 128
      rmcdeletecommendations round 42 medal
      rmcdeletecommendations round 42 jelly giver ИмяИгрока
      rmcdeletecommendations round 42 medal receiver ИмяИгрока
cmd-rmcdeletecommendations-invalid-arguments = Неверные аргументы!
cmd-rmcdeletecommendations-invalid-round-id = Неверный ID раунда!
cmd-rmcdeletecommendations-invalid-id = Неверный ID похвалы!
cmd-rmcdeletecommendations-invalid-type = Неверный тип '{ $type }'!
cmd-rmcdeletecommendations-invalid-player-mode = Неверный режим игрока! Должно быть 'giver' или 'receiver'.
cmd-rmcdeletecommendations-player-not-found = Игрок '{ $player }' не найден.
cmd-rmcdeletecommendations-no-results = Похвалы не найдены.
cmd-rmcdeletecommendations-id-header = Удалена похвала { $id }:
cmd-rmcdeletecommendations-round-header = Удалены похвалы для Раунда { $round } (всего { $count }):
cmd-rmcdeletecommendations-format = ID [{ $id }] { $type }: { $name } — { $giverUserName } ({ $giver }) → { $receiverUserName } ({ $receiver }) Раунд { $round }: { $text }
cmd-rmcdeletecommendations-admin-announcement = { $admin } удалил(а) похвалы с ID: { $ids }
cmd-rmcdeletecommendations-admin-announcement-round = { $admin } удалил(а) похвалы для Раунда { $round } с ID: { $ids }
cmd-rmcdeletecommendations-hint-mode = Режим (id или round)
cmd-rmcdeletecommendations-hint-mode-id = Удалить похвалу по ID
cmd-rmcdeletecommendations-hint-mode-round = Удалить похвалы по раунду
cmd-rmcdeletecommendations-hint-round-id = ID раунда
cmd-rmcdeletecommendations-hint-commendation-id = ID похвалы
cmd-rmcdeletecommendations-hint-type = Тип похвалы
cmd-rmcdeletecommendations-hint-player-mode = Режим игрока (giver или receiver)
cmd-rmcdeletecommendations-hint-player-giver = Похвалы, выданные игроком
cmd-rmcdeletecommendations-hint-player-receiver = Похвалы, полученные игроком
cmd-rmcdeletecommendations-hint-player = Имя или UserId игрока
