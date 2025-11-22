-- 1. КАТЕГОРИИ
INSERT INTO "GoalCategories" (
        "CategoryId",
        "Name",
        "Icon",
        "Color",
        "SortOrder",
        "IsActive",
        "CreatedAt"
    ) OVERRIDING SYSTEM VALUE
VALUES (
        1,
        'Техника',
        '📱',
        '#311B92',
        1,
        true,
        CURRENT_TIMESTAMP
    ),
    (
        2,
        'Транспорт',
        '🚗',
        '#880E4F',
        2,
        true,
        CURRENT_TIMESTAMP
    ),
    (
        3,
        'Путешествия',
        '✈️',
        '#1A237E',
        3,
        true,
        CURRENT_TIMESTAMP
    ),
    (
        4,
        'Недвижимость',
        '🏠',
        '#B45309',
        4,
        true,
        CURRENT_TIMESTAMP
    ),
    (
        5,
        'Образование',
        '🎓',
        '#065F46',
        5,
        true,
        CURRENT_TIMESTAMP
    ),
    (
        6,
        'Здоровье',
        '🏥',
        '#EC4899',
        6,
        true,
        CURRENT_TIMESTAMP
    ),
    (
        7,
        'Развлечения',
        '🎮',
        '#8B5CF6',
        7,
        true,
        CURRENT_TIMESTAMP
    ),
    (
        8,
        'Другое',
        '⭐',
        '#6B7280',
        8,
        true,
        CURRENT_TIMESTAMP
    );
-- Обновляем счетчик ID для категорий
SELECT setval(
        pg_get_serial_sequence('"GoalCategories"', 'CategoryId'),
        8
    );
-- 2. ЦЕЛИ
INSERT INTO "Goals" (
        "GoalId",
        "CategoryId",
        "Title",
        "TargetAmount",
        "CurrentAmount",
        "StartDate",
        "EndDate",
        "Priority",
        "IsCompleted",
        "CreatedAt"
    ) OVERRIDING SYSTEM VALUE
VALUES -- 1. IPHONE 15 PRO (Техника) - В процессе
    (
        1,
        1,
        'IPHONE 15 PRO',
        120000,
        45000,
        CURRENT_DATE - INTERVAL '45 days',
        CURRENT_DATE + INTERVAL '45 days',
        2,
        FALSE,
        CURRENT_TIMESTAMP
    ),
    -- 2. ПОЕЗДКА В ЯПОНИЮ (Путешествия) - В процессе
    (
        2,
        3,
        'ПОЕЗДКА В ЯПОНИЮ',
        200000,
        62000,
        CURRENT_DATE - INTERVAL '120 days',
        CURRENT_DATE + INTERVAL '120 days',
        3,
        FALSE,
        CURRENT_TIMESTAMP
    ),
    -- 3. МАГИСТРАТУРА (Образование) - В процессе (Высокий приоритет)
    (
        3,
        5,
        'МАГИСТРАТУРА',
        300000,
        90000,
        CURRENT_DATE - INTERVAL '180 days',
        CURRENT_DATE + INTERVAL '180 days',
        1,
        FALSE,
        CURRENT_TIMESTAMP
    ),
    -- 4. Ремонт кухни (Недвижимость) - Старт
    (
        4,
        4,
        'Ремонт кухни',
        250000,
        25000,
        CURRENT_DATE - INTERVAL '10 days',
        CURRENT_DATE + INTERVAL '60 days',
        2,
        FALSE,
        CURRENT_TIMESTAMP
    ),
    -- 5. Первый взнос на авто (Транспорт) - Долгосрок
    (
        5,
        2,
        'Первый взнос',
        500000,
        75000,
        CURRENT_DATE - INTERVAL '200 days',
        CURRENT_DATE + INTERVAL '100 days',
        1,
        FALSE,
        CURRENT_TIMESTAMP
    ),
    -- 6. Игровой ПК (Техника) - ВЫПОЛНЕНО
    (
        6,
        1,
        'Игровой ПК',
        150000,
        150000,
        CURRENT_DATE - INTERVAL '90 days',
        CURRENT_DATE - INTERVAL '5 days',
        1,
        TRUE,
        CURRENT_TIMESTAMP
    ),
    -- 7. Зубные импланты (Здоровье) - Почти готово
    (
        7,
        6,
        'Лечение зубов',
        80000,
        10000,
        CURRENT_DATE - INTERVAL '5 days',
        CURRENT_DATE + INTERVAL '30 days',
        1,
        FALSE,
        CURRENT_TIMESTAMP
    ),
    -- 8. Подушка безопасности (Другое) - ВЫПОЛНЕНО
    (
        8,
        8,
        'Фин. подушка',
        100000,
        100000,
        CURRENT_DATE - INTERVAL '365 days',
        CURRENT_DATE - INTERVAL '30 days',
        3,
        TRUE,
        CURRENT_TIMESTAMP
    );
-- Обновляем счетчик ID для целей
SELECT setval(pg_get_serial_sequence('"Goals"', 'GoalId'), 8);
-- 3. ИСТОРИЯ ПОПОЛНЕНИЙ
INSERT INTO "GoalDeposits" (
        "GoalId",
        "Amount",
        "DepositDate",
        "Comment",
        "DepositType"
    )
VALUES -- Для iPhone (ID 1)
    (
        1,
        10000,
        CURRENT_DATE - INTERVAL '40 days',
        'Старт',
        'regular'
    ),
    (
        1,
        15000,
        CURRENT_DATE - INTERVAL '20 days',
        'Аванс',
        'salary'
    ),
    (
        1,
        20000,
        CURRENT_DATE - INTERVAL '5 days',
        'Подарок',
        'other'
    ),
    -- Для Японии (ID 2)
    (
        2,
        30000,
        CURRENT_DATE - INTERVAL '100 days',
        'Отпускные',
        'bonus'
    ),
    (
        2,
        32000,
        CURRENT_DATE - INTERVAL '10 days',
        'Копилка',
        'regular'
    ),
    -- Для Магистратуры (ID 3)
    (
        3,
        45000,
        CURRENT_DATE - INTERVAL '150 days',
        'Семестр 1',
        'regular'
    ),
    (
        3,
        45000,
        CURRENT_DATE - INTERVAL '10 days',
        'Семестр 2',
        'regular'
    ),
    -- Для Авто (ID 5)
    (
        5,
        75000,
        CURRENT_DATE - INTERVAL '190 days',
        'Продажа старой техники',
        'other'
    ),
    -- Для Игрового ПК (ID 6 - Выполнено)
    (
        6,
        50000,
        CURRENT_DATE - INTERVAL '80 days',
        'Начало',
        'regular'
    ),
    (
        6,
        100000,
        CURRENT_DATE - INTERVAL '10 days',
        'Премия годовая',
        'bonus'
    );
-- 4. АНАЛИТИЧЕСКИЕ ОТЧЕТЫ
INSERT INTO "AnalyticsReports" (
        "ReportType",
        "ReportDate",
        "TotalGoals",
        "CompletedGoals",
        "TotalTargetAmount",
        "TotalCurrentAmount",
        "AverageProgress",
        "GeneratedAt"
    )
VALUES (
        'monthly',
        CURRENT_DATE - INTERVAL '1 month',
        6,
        1,
        1200000,
        350000,
        29.5,
        CURRENT_TIMESTAMP - INTERVAL '1 month'
    ),
    (
        'monthly',
        CURRENT_DATE,
        8,
        2,
        1600000,
        557000,
        34.8,
        CURRENT_TIMESTAMP
    );