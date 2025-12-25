ccm-motion-detector-scan-disabled = { CAPITALIZE($md) } должен быть активирован, чтобы просканировать { $target }.
ccm-motion-detector-scan-start-self = Вы начинаете перенастраивать { CAPITALIZE($md) }, чтобы просканировать внутренности { $target } на наличие сигнатур.
ccm-motion-detector-scan-start-others = { $user } возится с { CAPITALIZE($md) }, направляя его на { $target }.
ccm-motion-detector-scan-stop-self = Вы прекращаете попытку сканировать внутренности { $target }.
ccm-motion-detector-scan-stop-others = { $user } перестаёт возиться с { CAPITALIZE($md) }.
ccm-motion-detector-scan-finish-self = Вы заканчиваете перенастройку { CAPITALIZE($md) } и сканирование { $target } на наличие сигнатур.
ccm-motion-detector-scan-finish-others = { $user } заканчивает возиться с { CAPITALIZE($md) }.
ccm-motion-detector-scan-result =
    { CAPITALIZE($md) } показывает
    { $humans ->
        [0] ни одной сигнатуры
       *[other] примерно { $humans } сигнатур
    }
    { $xenos ->
        [0] и ни одной аномальной
       *[other] и около { $xenos } аномальных сигнатур
    } внутри { $target }.
ccm-motion-detector-scan-empty = { CAPITALIZE($md) } не улавливает никаких сигнатур - похоже, транспорт пуст. В теории.
