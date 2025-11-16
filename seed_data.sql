-- Заполнение таблицы Goals (Цели)
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
    )
VALUES (
        1,
        1,
        'Новый iPhone',
        120000.00,
        45000.00,
        CURRENT_DATE - INTERVAL '2 months',
        CURRENT_DATE + INTERVAL '4 months',
        1,
        false,
        CURRENT_TIMESTAMP
    ),
    (
        2,
        3,
        'Отдых в Сочи',
        80000.00,
        25000.00,
        CURRENT_DATE - INTERVAL '1 month',
        CURRENT_DATE + INTERVAL '3 months',
        2,
        false,
        CURRENT_TIMESTAMP
    ),
    (
        3,
        2,
        'Первый взнос на машину',
        300000.00,
        75000.00,
        CURRENT_DATE - INTERVAL '3 months',
        CURRENT_DATE + INTERVAL '9 months',
        1,
        false,
        CURRENT_TIMESTAMP
    ),
    (
        4,
        5,
        'Курсы английского',
        40000.00,
        40000.00,
        CURRENT_DATE - INTERVAL '6 months',
        CURRENT_DATE - INTERVAL '1 month',
        2,
        true,
        CURRENT_TIMESTAMP
    );
-- Заполнение таблицы GoalDeposits (Пополнения)
INSERT INTO "GoalDeposits" (
        "DepositId",
        "GoalId",
        "Amount",
        "DepositDate",
        "Comment",
        "DepositType"
    )
VALUES (
        1,
        1,
        15000.00,
        CURRENT_DATE - INTERVAL '1 month',
        'Аванс',
        'salary'
    ),
    (
        2,
        1,
        30000.00,
        CURRENT_DATE - INTERVAL '15 days',
        'Премия',
        'bonus'
    ),
    (
        3,
        2,
        25000.00,
        CURRENT_DATE - INTERVAL '7 days',
        'Накопления',
        'regular'
    ),
    (
        4,
        3,
        50000.00,
        CURRENT_DATE - INTERVAL '2 months',
        'Накопления',
        'regular'
    ),
    (
        5,
        3,
        25000.00,
        CURRENT_DATE - INTERVAL '1 month',
        'Подработка',
        'freelance'
    ),
    (
        6,
        4,
        20000.00,
        CURRENT_DATE - INTERVAL '5 months',
        'Стипендия',
        'other'
    ),
    (
        7,
        4,
        20000.00,
        CURRENT_DATE - INTERVAL '3 months',
        'Стипендия',
        'other'
    );
-- Заполнение таблицы AnalyticsReports (Аналитические отчеты)
INSERT INTO "AnalyticsReports" (
        "ReportId",
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
        1,
        'monthly',
        DATE_TRUNC('month', CURRENT_DATE),
        4,
        1,
        540000.00,
        185000.00,
        34.26,
        CURRENT_TIMESTAMP
    ),
    (
        2,
        'quarterly',
        DATE_TRUNC('quarter', CURRENT_DATE),
        4,
        1,
        540000.00,
        185000.00,
        34.26,
        CURRENT_TIMESTAMP
    );