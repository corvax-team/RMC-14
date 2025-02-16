# Requisition Computer
requisition-paperwork-receiver-name = Подразделение логистики
requisition-paperwork-reward-message = Подтверждение получено! Переведено { $amount } из корабельного бюджета

# Requisition Invoice
requisition-paper-print-name = Накладная { $name }
requisition-paper-print-manifest = [head=2]
    {$containerName}[/head][bold]{$content}[/bold][head=2]
    МАССА { $weight } ФНТ.
    ПАРТИЯ { $lot }
    С/Н { $serialNumber }[/head]
requisition-paper-print-content = - {$count} {$item}
