# Getting Started with the Multilingual App Toolkit

## Install

See http://aka.ms/matinstall

## Configure
Add the languages that you support so that language XLF files are generated for each one.

## Convert

Converting existing resx/resw resources using https://github.com/TheMATDude/ResourceToXliff
Merge them into a single XLF for each language.

## Working

When you build, translations in `.xlf` files will be used to automatically generate language appropriate `.resw` files so you should only update the XLF file.
