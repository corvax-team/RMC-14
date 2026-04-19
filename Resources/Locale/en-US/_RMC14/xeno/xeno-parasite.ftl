# General parasite messages
rmc-xeno-failed-cant-infect = We can't infect {THE($target)}!
rmc-xeno-failed-cant-reach = We can't reach {$target}, they need to be lying down!
rmc-xeno-failed-target-dead = We can't infect the dead!
rmc-xeno-infect-success = The tiny xenonid smashes against {$target}'s {$clothing} and rips it off!
rmc-xeno-infect-fail = The tiny xenonid smashes against {$target}'s {$clothing}!
rmc-xeno-failed-parasite-dead = We can't infect with a dead child!
rmc-xeno-cant-throw = We can't throw {THE($target)}!
# Infection messages
rmc-xeno-parasite-dead = {CAPITALIZE(SUBJECT($parasite))} {CONJUGATE-BE($parasite)} not moving.
rmc-xeno-parasite-announce-infect = We sense that a {$xeno} has infected a host at {$location}!
# Parasite interaction messages
rmc-xeno-parasite-player-pickup = {CAPITALIZE($parasite)} can handle {REFLEXIVE($parasite)}!
rmc-xeno-parasite-nonplayer-pull = Pulling the {$parasite} might hurt {OBJECT($parasite)}!
# Parasite AI state messages
rmc-xeno-parasite-ai-active = {CAPITALIZE(SUBJECT($parasite))} seems to be active.
rmc-xeno-parasite-ai-idle = {CAPITALIZE(SUBJECT($parasite))} {CONJUGATE-BE($parasite)} resting.
rmc-xeno-parasite-ai-dying = [color=red]{CAPITALIZE(SUBJECT($parasite))} needs to return to safety![/color]
rmc-xeno-parasite-ai-eaten = The {CAPITALIZE($parasite)} is furiously cannibalized by the other nearby children!
# Throw action messages
rmc-xeno-throw-parasite-current = {$cur_paras}/{$max_paras} parasites
rmc-xeno-throw-parasite-too-many-parasites = Cannot carry any more parasites! ({$current}/{$max})
rmc-xeno-throw-parasite-too-many-royals = Cannot carry any more royal parasites! ({$current}/{$max})
rmc-xeno-throw-no-parasites = No regular parasites available!
rmc-xeno-throw-wrong-parasite-type-royal = We cannot throw a royal parasite so carelessly!
rmc-xeno-throw-wrong-parasite-type-regular = Throwing with this much force will kill the parasite!
# Ghost role descriptions
rmc-xeno-parasite-ghost-role-name = Parasite
# Shared ghost role time messages
rmc-xeno-egg-ghost-need-time = You ghosted too recently. You cannot become a parasite until 3 minutes have passed ({$seconds} seconds remaining).
rmc-xeno-egg-ghost-need-time-round = You cannot become a parasite until enough time has passed passed in the round ({$seconds} seconds remaining).
rmc-xeno-egg-ghost-bypass-time = You successfully infected your target. You may become a parasite again.
rmc-xeno-parasite-take-title = Take Parasite?
rmc-xeno-egg-not-alive = The egg/carrier is dead or not alive!
# Egg messages
rmc-xeno-egg-wrong-type-royal = Royal parasites can only be placed in royal parasite eggs
rmc-xeno-egg-wrong-type-regular = Only royal parasites can be placed in royal parasite eggs
# Carrier availability messages
rmc-xeno-parasite-ghost-roles-available = {$count ->
    [one] {$count} available ghost role
    *[other] {$count} available ghost roles
}
rmc-xeno-parasite-ghost-carrier-none = {$xeno} has no stored parasites
rmc-xeno-parasite-ghost-carrier-dead = {THE($xeno)} is dead and all {POSS-ADJ($xeno)} parasites died with {OBJECT($xeno)}.
# Ghost possession failure messages
rmc-xeno-parasite-ghost-invalid = You are not a valid ghost.
rmc-xeno-parasite-ghost-take-failed = Failed to take control of the parasite.
rmc-xeno-parasite-ghost-no-session = Could not find your session to transfer to the parasite.
rmc-xeno-parasite-ghost-dead = This parasite is dead or spent.
# Reserve parasite UI
rmc-xeno-reserve-parasites-title = Reserve Parasites
rmc-xeno-reserve-parasites-label = Regular Parasites
rmc-xeno-reserve-parasites-apply = Apply
# Parasite confirmation dialog
rmc-xeno-parasite-confirm-text = Are you sure?
