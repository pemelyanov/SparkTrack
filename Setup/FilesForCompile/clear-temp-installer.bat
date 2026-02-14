@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

REM Проверяем, передан ли параметр
if "%~1"=="" (
    echo Ошибка: Не указан файл для удаления.
    echo Использование: %~nx0 "полный_путь_к_файлу"
    pause
    exit /b 1
)

set "target_file=%~1"
set "max_attempts=30"
set "attempt=0"
set "delay_seconds=2"

REM Проверяем существование файла
if not exist "%target_file%" (
    echo Файл "%target_file%" не найден.
    pause
    exit /b 1
)

echo Начинаю удаление файла: "%target_file%"
echo Максимальное количество попыток: %max_attempts%
echo Интервал между попытками: %delay_seconds% сек.
echo.

:delete_loop
set /a attempt+=1
echo Попытка %attempt% из %max_attempts%...

REM Пытаемся удалить файл
del /f /q "%target_file%" 2>nul

if not exist "%target_file%" (
    echo.
    echo Успех! Файл успешно удален после %attempt% попытки(ок).
    exit /b 0
)

if %attempt% geq %max_attempts% (
    echo.
    echo Не удалось удалить файл после %max_attempts% попыток.
    echo Возможно, файл все еще используется другим процессом.
    echo Путь к файлу: "%target_file%"
    pause
    exit /b 1
)

echo Файл еще занят, жду %delay_seconds% секунд...
timeout /t %delay_seconds% /nobreak >nul
goto delete_loop