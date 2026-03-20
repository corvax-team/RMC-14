@echo off
chcp 65001 >nul

py -m pip install --upgrade typing_extensions pydash PyYAML chardet fluent

py yamlextractor.py
py keyfinder.py
py clean_duplicates.py
py clean_empty.py

pause