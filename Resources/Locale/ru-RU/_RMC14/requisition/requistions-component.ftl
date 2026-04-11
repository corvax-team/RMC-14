# Requisition Computer
requisition-paperwork-receiver-name = Подразделение логистики
requisition-paperwork-reward-message = Подтверждение получено! Переведено { $amount } из корабельного бюджета.
# Requisition Invoice
requisition-paper-print-name = Накладная { $name }
requisition-paper-print-manifest = [head=2]
    { $containerName }[/head][bold]{ $content }[/bold][head=2]
    МАССА { $weight } ФНТ.
    ПАРТИЯ { $lot }
    С/Н { $serialNumber }[/head]
requisition-paper-print-content = — { $count } { $item }
# Supply Drop Console
ui-supply-drop-consle-name = Консоль сброса припасов
ui-supply-drop-console-name-bolded = [bold]СБРОС ПРИПАСОВ[/bold]
ui-supply-drop-console-longitude = Долгота:
ui-supply-drop-console-latitude = Широта:
ui-supply-drop-pad-status = [bold]Статус платформы снабжения[/bold]
ui-supply-drop-console-update = Обновить
ui-supply-drop-console-ready = Готово к запуску!
ui-supply-drop-console-launch = ЗАПУСТИТЬ
ui-supply-drop-console-launch-confirmation = Подтвердить сброс припасов?
ui-supply-drop-console-cooldown = { $time } сек. до следующего запуска
ui-supply-drop-crate-status =
    { $hasCrate ->
        [true] Статус платформы: ящик загружен.
       *[false] Ящик отсутствует.
    }
