import requests
import json
import re
import random
from values import *

def getSlideNames(service, presentationId) -> list:
    presentation = service.presentations().get(presentationId=presentationId).execute()
    slides = presentation.get("slides")
    slide_names = []
    for slide in slides:
        slide_names.append(slide.get("objectId"))
    return slide_names


def calculateCoords(img_per_slide) -> list:
    coords = []
    for i in range(img_per_slide):
        x = random.randrange(0, MAX_X, 100000)
        y = random.randrange(0, MAX_Y, 100000)

        coords.append((x, y))
    return coords


def getPhotoNames(token) -> list:
    body = {
        "query": '{repository(owner: "kon3gor", name: "SlidesCatizer") { filename:object(expression: "master:images/") {...on Tree{ entries{ name } } } } }'
    }
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    response = json.loads(requests.post("https://api.github.com/graphql", headers=headers, data=json.dumps(body)).content)
    entities = response["data"]["repository"]["filename"]["entries"]
    photo_names = []
    for entity in entities:
        photo_names.append(entity["name"])
    return photo_names


def getId(link) -> str:
    # link example - https://docs.google.com/presentation/d/19ek5lJoJRLA-oD7lUNifY9GGcs8_chCNOwqdcYMmlaA/edit?usp=sharing
    pattern = r'https://docs.google.com/presentation/d/\S+'
    if re.match(pattern, link):
        presentationId = link[39:].split('/')[0]
        return presentationId

    return None
