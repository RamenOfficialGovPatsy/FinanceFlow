@echo off
:: Устанавливаем кодировку UTF-8
chcp 65001 > nul
title FinanceFlow Database Setup

echo ===================================================
echo   Автоматическая настройка БД для FinanceFlow
echo ===================================================
echo.
echo ВНИМАНИЕ: Скрипт использует стандартного суперпользователя 'postgres'.
echo Пароль по умолчанию предполагается 'postgres'.
echo.

:: --- НАСТРОЙКИ ПОДКЛЮЧЕНИЯ ---
set PGUSER=postgres
set PGPASSWORD=postgres

:: 0. Проверка доступности PostgreSQL
echo [1/3] Проверка подключения к PostgreSQL...
psql -U %PGUSER% -c "SELECT version();" > nul 2>&1
if errorlevel 1 (
    echo.
    echo [ERROR] Не удается подключиться к PostgreSQL!
    echo Возможные причины:
    echo 1. Сервер PostgreSQL не запущен.
    echo 2. Пароль суперпользователя 'postgres' не совпадает.
    echo 3. PostgreSQL не добавлен в переменные среды (PATH).
    goto :error
)

:: 1. Создание пользователя (безопасно, если уже есть)
echo [2/3] Настройка пользователя financeflow_user...
psql -U %PGUSER% -c "DO \$\$ BEGIN CREATE USER financeflow_user WITH PASSWORD 'Ff_Postgres_Mdk_2025!'; EXCEPTION WHEN duplicate_object THEN RAISE NOTICE 'Пользователь уже существует'; END \$\$;" > nul 2>&1
:: Даем права на создание БД (на всякий случай)
psql -U %PGUSER% -c "ALTER USER financeflow_user CREATEDB;" > nul 2>&1

:: 2. Создание базы данных (безопасно, если уже есть)
echo [3/3] Настройка базы данных financeflow_db...
:: Хитрая проверка: создаем базу, только если её нет
psql -U %PGUSER% -c "SELECT 'CREATE DATABASE financeflow_db OWNER financeflow_user' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'financeflow_db')\gexec" > nul 2>&1

:: 3. Накатывание структуры и данных
echo.
echo Применение структуры таблиц и данных...
if exist seed_data.sql (
    :: -q уменьшает шум в консоли
    psql -U %PGUSER% -d financeflow_db -f seed_data.sql
    echo [OK] База данных успешно развернута!
) else (
    echo [ERROR] Файл seed_data.sql не найден!
    echo Убедитесь, что он лежит в одной папке с этим скриптом.
    goto :error
)

echo.
echo ===================================================
echo   Установка завершена успешно!
echo ===================================================
echo Теперь можно запускать FinanceFlow.exe
echo.
pause
exit /b 0

:error
echo.
echo ===================================================
echo   Установка прервана из-за ошибки
echo ===================================================
pause
exit /b 1