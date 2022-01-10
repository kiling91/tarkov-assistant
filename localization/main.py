from pathlib import Path
import xml.etree.ElementTree as ET
import re
import os
import csv
import json

class ModuleInfo:
    module = ""
    module_path = ""
    namespace = ""

class Translation:
    language = ""
    translation = ""

class LocalizationText:
  content = ""
  translations = []
  modules = []

def find_or_create_localization_text(text_array, content):
    for text in text_array:
        if text.content == content:
            return text

    res = LocalizationText()
    res.content = content
    res.modules = []
    res.translations = []
    text_array.append(res)
    return res

def find_or_create_module_info(modules, module_path, module):
    for item in modules:
        if item.module_path == module_path:
            return item

    res = ModuleInfo()
    res.module = module
    res.module_path = module_path
    res.namespace = ""
    modules.append(res)

def get_content(text):
    regex = r"\"(.*?)\""
    matches = re.finditer(regex, text, re.MULTILINE)
    res = []
    for match in matches:
        for group in match.groups():
            res.append(group)
    return "".join(res)

def find_localizer(all_text):
    regex = r"IStringLocalizer<(\w*?)>(\s*?)(\w*?);"
    matches = re.finditer(regex, all_text, re.MULTILINE)

    res = []
    for match in matches:
        temp = []
        for group in match.groups():
            temp.append(group)

        if len(temp) != 3:
            raise Exception("Ошибка парсинга модуля")

        if len(res) == 3:
            if temp[0] != res[0] or temp[2] != res[2]:
                raise Exception("Ошибка парсинга модуля")
        res = temp

    if len(res) == 0:
        return None, None
    
    return res[0], res[2]

def read_module(localizations, path):
    with open(path, 'r', encoding="utf8") as file:
        all_text = file.read().replace("\n", " ")

    # Ищим переменную локализации
    module, localizer = find_localizer(all_text)

    if module == None:
        return

    path_module, _ = os.path.splitext(path.name)
    if module != path_module:
        raise Exception("Имя модуля не совпадает с именем модуля в локализаторе")

    # Получаем текст заключеный между _localizer[ и ]
    regex = f"{localizer}\[(.*?)\]"
    matches = re.finditer(regex, all_text, re.MULTILINE)

    for match in matches:
        for group in match.groups():
            content = get_content(group)
            item = find_or_create_localization_text(localizations, content)
            find_or_create_module_info(item.modules, str(path), module)

def find_translation(language, localization_text):
    for tr in localization_text.translations:
        if tr.language == language:
            return tr.translation
    return language

def load_translation(prefix, languages, translation_folder_csv, localization_text):
    if not os.path.exists(translation_folder_csv):
        os.makedirs(translation_folder_csv)

    for ln in languages:
        path = Path(os.path.join(translation_folder_csv, f"{prefix}-{ln}.csv"))

        # Ищим перевод в csv
        find = False   
        if path.is_file():
            with open(path, 'r', encoding='utf-8') as csvfile:
                reader = csv.reader(csvfile, delimiter=';')
                for row in reader:
                    if len(row) == 2 and row[0] != "":
                        if row[0] == localization_text.content:
                            ts = Translation()
                            ts.language = ln
                            ts.translation = row[1]
                            localization_text.translations.append(ts)
                            find = True
                            break
        
        # Если не найден, то добавляем пустой перевод
        if not find:
            ts = Translation()
            ts.language = ln
            ts.translation = ""
            localization_text.translations.append(ts)
            
            with open(path, 'a', encoding='utf-8', newline='') as csvfile:
                writer = csv.writer(csvfile, delimiter=';')
                writer.writerow([localization_text.content, ""])  

def xml(resx, ln, text):
    if not resx.is_file():
        root = ET.Element('root')
        tree = ET.ElementTree(root)
        tree.write(resx, encoding="utf8", xml_declaration=False)
        
    tree = ET.parse(resx)
    root = tree.getroot()

    # Ищим
    find_node = False
    for data in root:
        if data.get("name") == text.content:
            find_node = True
            # Пробуем обновить перевод
            for value in data:
                value.text = find_translation(ln, text) # Ищим перевод из таблицы
            break

    if not find_node:
        data = ET.SubElement(root, 'data')
        data.set('name', text.content)
        data.set('xml:space', 'preserve')

        value = ET.SubElement(data, 'value')
        value.text = find_translation(ln, text) # Ищим перевод из таблицы

    tree.write(resx, encoding="utf8", xml_declaration=False)


# api_folder - каталог api в котором должна находиться папка с ресурсами
# languages - массив языков для которых нужно создать ресурсы
# translation_folder_csv - каталог содержащий csv с переводами соответствующих языков

def parser(prefix, project_folder, languages, translation_folder_csv):
    localizations = []

    # Идем по всем cs файлам проекта и собираем текст для локализации
    for path in Path(project_folder).rglob('*.cs'):
        try:
            read_module(localizations, path)
        except Exception as e:
            print(f"Ошибка чтения модуля {path}:")
            print(f"\t{str(e)}")
            return

    # Создаем каталог Resources если его нет
    resources_dir = os.path.join(project_folder, "Resources")
    if not os.path.exists(resources_dir):
        os.makedirs(resources_dir)

    for text in localizations:
        print(f"{text.content}")
        # Загружаем переводы из csv
        load_translation(prefix, languages, translation_folder_csv, text)
        for module in text.modules:
            for ln in languages:
                resx = os.path.relpath(module.module_path, project_folder)
                resx = resx.split(os.sep)
                resx[-1] = f"{module.module}.{ln}.resx"
                resx = ".".join(resx)
                resx = os.path.join(resources_dir, resx)
                resx = Path(resx)
                xml(resx, ln, text)

# Main
with open('options.json') as json_file:
    data = json.load(json_file)

    base_catalog = os.path.normpath(data['base_catalog'])
    languages = data['languages']
    translations = data['translations']
    projects = data['projects']

    for project_folder in projects:
        prefix = project_folder
        project_folder = os.path.join(base_catalog, os.path.normpath(project_folder))
        parser(prefix, project_folder, languages, translations)