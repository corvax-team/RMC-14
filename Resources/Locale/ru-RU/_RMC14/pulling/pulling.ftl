cm-pull-whitelist-denied = Мы не пользуемся { $name }, с чего бы нам это трогать?
cm-pull-whitelist-denied-dead =
    { $name } { GENDER($name) ->
        [male] мёртв
        [female] мертва
        [epicene] мертвы
       *[neuter] мертво
    }, с чего бы нам { SUBJECT($name) } трогать?
rmc-pull-paralyze-self = Вы пытаетесь потянуть { $pulled }, но получаете удар хвостом по голове!
rmc-pull-paralyze-others = { $puller } пытается потянуть { $pulled }, но вместо этого получает удар хвостом по голове
rmc-pull-infect-self = Вы пытаетесь потянуть { $pulled }, но на вас прыгают и заражают!
rmc-pull-infect-others = { $puller } пытается потянуть { $pulled }, но на них прыгают и заражают!
rmc-prevent-pull-alive =
    Вы не можете тащить { $target } пока { SUBJECT($target) } { GENDER($target) ->
        [male] ещё жив
        [female] ещё жива
        [epicene] ещё живы
       *[neuter] ещё живо
    }!
rmc-pull-aggressive-self = Вы агрессивно хватаете { $pulled }!
rmc-pull-aggressive-others = { $puller } агрессивно хватает { $pulled }!
rmc-pull-break-start-self = Вы пытаетесь вырваться из хватки { $puller }!
rmc-pull-break-start-others = { $pulled } пытается вырваться из хватки { $puller }!
rmc-pull-break-finish-self = Вы вырываетесь из хватки { $puller }!
rmc-pull-break-finish-others = { $pulled } вырывается из хватки { $puller }!
