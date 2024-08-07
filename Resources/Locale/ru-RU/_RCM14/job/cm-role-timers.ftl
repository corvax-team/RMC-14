role-timer-human-roles = любые человеческие роли
role-timer-medical-roles = любые медицинские роли
role-timer-engineering-roles = любые инженерные роли
role-timer-dropship-roles = любые роли пилотов
role-timer-total-department-insufficient = Вам требуется на [color=yellow]{ TOSTRING($time, "0") }[/color] { $time ->
        [one] минуту
        [few] минуты
       *[other] минут
	   } больше, чтобы [color={ $rolesColor }]{ $roles }[/color] чтобы сыграть за эту роль.
role-timer-total-department-too-high = Вам требуется на [color=yellow]{ TOSTRING($time, "0") }[/color] { $time ->
        [one] минуту
        [few] минуты
       *[other] минут
	   } меньше, чем [color={ $departmentColor }]{ $rolesColor }[/color] чтобы сыграть за эту роль. (Вы пытаетесь играть на роли стажера?)
