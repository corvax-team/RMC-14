# Requisition Computer
requisition-paperwork-receiver-name = Подразделение логистики
requisition-paperwork-reward-message = Подтверждение получено! Переведено { $amount } из корабельного бюджета
# Requisition Invoice
requisition-paper-print-name = Накладная { $name }
requisition-paper-print-manifest = [head=2]
    { $containerName }[/head][bold]{ $content }[/bold][head=2]
    МАССА { $weight } ФНТ.
    ПАРТИЯ { $lot }
    С/Н { $serialNumber }[/head]
requisition-paper-print-content = - { $count } { $item }
# Supply Drop Console
ui-supply-drop-consle-name = Supply Drop Console
ui-supply-drop-console-name-bolded = [bold]SUPPLY DROP[/bold]
ui-supply-drop-console-longitude = Longitude:
ui-supply-drop-console-latitude = Latitude:
ui-supply-drop-pad-status = [bold]Supply Pad Status[/bold]
ui-supply-drop-console-update = Update
ui-supply-drop-console-ready = Ready to fire!
ui-supply-drop-console-launch = LAUNCH SUPPLY DROP
ui-supply-drop-console-cooldown = { $time } seconds until next launch
ui-supply-drop-crate-status =
    { $hasCrate ->
        [true] Supply Pad Status: crate loaded.
       *[false] No crate loaded.
    }
