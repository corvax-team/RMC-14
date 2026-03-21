cm-tackle-try-self = Вы пытаетесь толкнуть { $target }
cm-tackle-try-target = { $user } пытается толкнуть вас.
cm-tackle-try-observer = { $user } пытается толкнуть { $target }
cm-tackle-success-self = Вы толкаете { $target } на землю!
cm-tackle-success-target = { $user } толкает вас на землю!
cm-tackle-success-observer = { $user } толкает { $target } на землю!
rmc-disarm-shove-others = { CAPITALIZE(THE($performerName)) } { $shoveText } { THE($targetName) }!
rmc-disarm-shove-target = { CAPITALIZE(THE($performerName)) } { $shoveText } вас!
rmc-disarm-shove-self = Вы { $shoveText } { THE($targetName) }!
rmc-disarm-text-skilled =
    { $gender ->
        [male] повалил
        [female] повалила
       *[other] повалил(а)
    }
rmc-disarm-text-1 =
    { $gender ->
        [male] толкнул
        [female] толкнула
       *[other] толкнул(а)
    }
rmc-disarm-text-2 =
    { $gender ->
        [male] оттолкнул
        [female] оттолкнула
       *[other] оттолкнул(а)
    }
rmc-disarm-break-pulls-others = { CAPITALIZE(THE($performerName)) } вырывает { THE($object) } из хватки { THE($targetName) }!
rmc-disarm-break-pulls-self = Вы вырвали { THE($object) } из захвата { THE($targetName) }!
rmc-disarm-break-pulls-target = { CAPITALIZE(THE($performerName)) } вырывает { THE($object) } из ваших рук!
rmc-disarm-attempt-others = { CAPITALIZE(THE($performerName)) } пытается обезоружить { THE($targetName) }!
rmc-disarm-attempt-self = Вы пытаетесь обезоружить { THE($targetName) }!
rmc-disarm-attempt-target = { CAPITALIZE(THE($performerName)) } пытается вас обезоружить!
rmc-disarm-success-others = { CAPITALIZE(THE($performerName)) } обезоруживает { THE($targetName) }!
rmc-disarm-success-self = Вы обезоруживаете { THE($targetName) }!
rmc-disarm-success-target = { CAPITALIZE(THE($performerName)) } обезоруживает вас!
rmc-disarm-discharge-others = { CAPITALIZE(THE($performerName)) } случайно нажимает на курок { $gun } во время борьбы!
rmc-disarm-discharge-self = Вы случайно нажимаете на курок { $gun }!
rmc-disarm-discharge-target = { CAPITALIZE(THE($performerName)) } случайно нажимает курок { $gun } во время борьбы!
