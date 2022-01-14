from cmath import pi
import os
import requests
import json
import string
from time import sleep
from tqdm import tqdm
from urllib.parse import urlparse

ALL_ITEMS_FILE = "all.json"
TARKOV_ITEMS = "tarkov_items.json"
TARKOV_MAIN_TAGS = "tarkov_main_tags.json"
ICONS_DIR_SM = "./icons_sm"
ICONS_DIR_LG = "./icons_lg"
TRANSLATION_DIR = "./translation"
NAME_TO_UID_DIR = "./name_to_uid"

LANGUAGES = ["en", "ru", "de", "fr", "es", "cn", "cz", "hu", "tr"]

def format_filename(s):
    """Take a string and return a valid filename constructed from the string.
    Uses a whitelist approach: any characters not present in valid_chars are
    removed. Also spaces are replaced with underscores.

    Note: this method may produce invalid filenames such as ``, `.` or `..`
    When I use this method I prepend a date string like '2009_01_15_19_46_32_'
    and append a file extension like '.txt', so I avoid the potential of using
    an invalid filename.

    """
    valid_chars = "-_.() %s%s" % (string.ascii_letters, string.digits)
    filename = ''.join(c for c in s if c in valid_chars)
    filename = filename.replace(' ','_') # I don't like spaces in filenames.
    return filename

def request_sleep(): # 250 requests by min
    sleep(1 / 10)

def request_get_all_items():
    url = "https://tarkov-market.com/api/v1/items/all"
    headers = {
    'x-api-key': 'YMlN2KaJfBVk7V1M',
    'Content-Type': 'application/json'
    }
    response = requests.request("GET", url, headers=headers)
    return json.loads(response.text)

def request_item(uid, lang):
    url = "https://tarkov-market.com/api/v1/item"

    payload = json.dumps({
    "uid": uid,
    "lang": lang
    })
    headers = {
    'x-api-key': 'YMlN2KaJfBVk7V1M',
    'Content-Type': 'application/json'
    }
    response = requests.request("POST", url, headers=headers, data=payload)
    return json.loads(response.text)

def img_url_to_filename(img_url, img_dir):
    a = urlparse(img_url)
    file_name = os.path.basename(a.path)
    file_name = format_filename(file_name)
    return os.path.join(img_dir, file_name)

def download_icon(img_url, img_dir):
    if not os.path.exists(img_dir):
        os.makedirs(img_dir)

    file_name = img_url_to_filename(img_url, img_dir)
    if os.path.exists(file_name):
        return False

    response = requests.get(img_url)

    file = open(file_name, "wb")
    file.write(response.content)
    file.close()
    return True

def get_all_items():
    if os.path.exists(ALL_ITEMS_FILE):
        with open(ALL_ITEMS_FILE, encoding='utf-8') as json_file:
            return json.load(json_file)
    else:
        jdata = get_all_items()
        with open(ALL_ITEMS_FILE, 'w', encoding='utf-8') as f:
            json.dump(jdata, f, ensure_ascii=False, indent=4)
        return jdata

def cashing_translations(all_items, ln):
    if not os.path.exists(TRANSLATION_DIR):
        os.makedirs(TRANSLATION_DIR)

    print(f"Сaching translation. Language {ln}")
    file_name = f"{TRANSLATION_DIR}/{ln}.json"
    
    translation = {}
    if os.path.exists(file_name):
        with open(file_name, encoding='utf-8') as json_file:
            translation = json.load(json_file)

    for item in tqdm(all_items):
        uid = item["uid"]
        orig_name = item["name"]
        orig_shortName = item["shortName"]

        if orig_name in translation and orig_shortName in translation:
            continue

        if ln == "en":
            translation[orig_name] = orig_name
            translation[orig_shortName] = orig_shortName
        else:   
            try:
                ln_item = request_item(uid, ln)[0]
                translation[orig_name] = ln_item["name"]
                translation[orig_shortName] = ln_item["shortName"]
            except Exception:
                print(f"\nError get item {orig_name} - {uid}\n")
            request_sleep()

    with open(file_name, 'w', encoding='utf-8') as f:
        json.dump(translation, f, ensure_ascii=False, indent=4)

def cashing_icons(all_items):
    print(f"Сaching icons")
    for item in tqdm(all_items):
        try:
            if download_icon(item["icon"], ICONS_DIR_SM):
                request_sleep()
            if download_icon(item["imgBig"], ICONS_DIR_LG):
                request_sleep()
        except Exception:
            name = item["name"]
            print(f"\nError get icon {name}\n")

def cashing_name_to_uid(all_items, ln):
    translation = {}
    with open(f"{TRANSLATION_DIR}/{ln}.json", encoding='utf-8') as json_file:
        translation = json.load(json_file)

    name_to_uid = {}
    for item in all_items:
        uid = item["uid"]
        orig_name = item["name"]
        orig_shortName = item["shortName"]
        tr_name = translation[orig_name]
        tr_shortName = translation[orig_shortName]

        if not tr_name in name_to_uid:
          name_to_uid[tr_name] = []

        name_to_uid[tr_name].append(uid)

        if not tr_shortName in name_to_uid:
          name_to_uid[tr_shortName] = []
        name_to_uid[tr_shortName].append(uid)

    if not os.path.exists(NAME_TO_UID_DIR):
        os.makedirs(NAME_TO_UID_DIR)
    with open(f"{NAME_TO_UID_DIR}/{ln}.json", 'w', encoding='utf-8') as f:
        json.dump(name_to_uid, f, ensure_ascii=False, indent=4)

all_items = get_all_items()

for ln in LANGUAGES:
    cashing_translations(all_items, ln)

cashing_icons(all_items)

for ln in LANGUAGES:
    cashing_name_to_uid(all_items, ln)

#
translations = {}
for ln in LANGUAGES:
    translation = {}
    with open(f"{TRANSLATION_DIR}/{ln}.json", encoding='utf-8') as json_file:
        translation = json.load(json_file)
    translations[ln] = translation

prepare_items = []
for item in all_items:
    pitem = {}
    pitem['uid'] = item['uid'] 
    pitem['tags'] = item['tags']
    pitem['price'] = item['price'] 
    pitem['basePrice'] = item['basePrice'] 
    pitem['avg24hPrice'] = item['avg24hPrice'] 
    pitem['avg7daysPrice'] = item['avg7daysPrice'] 
    pitem['traderName'] = item['traderName'] 
    pitem['traderPrice'] = item['traderPrice'] 
    pitem['traderPriceCur'] = item['traderPriceCur'] 
    pitem['updated'] = item['updated'] 
    pitem['slots'] = item['slots'] 
    pitem['diff24h'] = item['diff24h'] 
    pitem['diff7days'] = item['diff7days'] 
    pitem['iconSm'] = os.path.normpath(img_url_to_filename(item['icon'], ICONS_DIR_SM))
    pitem['iconLg'] = os.path.normpath(img_url_to_filename(item['imgBig'], ICONS_DIR_LG))
    pitem['link'] = item['link'] 
    translation = {}
    for ln in LANGUAGES:
        translation[ln] = {
            "name": translations[ln][item["name"]],
            "shortName": translations[ln][item["shortName"]],
        }
    pitem["translation"] = translation
    prepare_items.append(pitem)

with open(TARKOV_ITEMS, 'w', encoding='utf-8') as f:
    json.dump(prepare_items, f, ensure_ascii=False, indent=4)

    
# Get main tags
main_tags = []
for item in all_items:
    for tag in item['tags']:
        main_tags.append(tag)
        break
main_tags = list(set(main_tags))
with open(TARKOV_MAIN_TAGS, 'w', encoding='utf-8') as f:
    json.dump(main_tags, f, ensure_ascii=False, indent=4)