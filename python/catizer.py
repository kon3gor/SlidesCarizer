import os
from googleapiclient.discovery import build
from google_auth_oauthlib.flow import InstalledAppFlow
from google.auth.transport.requests import Request
import pickle
from values import *
from utils import getPhotoNames, getSlideNames, calculateCoords

#--------------------------------------------------------------

def initSlides():
    creds = None
    if os.path.exists('token.pickle'):
        with open('token.pickle', 'rb') as token:
            creds = pickle.load(token)
    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            flow = InstalledAppFlow.from_client_secrets_file(
                'credentials.json', SCOPES)
            creds = flow.run_local_server(port=0)
        with open('token.pickle', 'wb') as token:
            pickle.dump(creds, token)

    service = build('slides', 'v1', credentials=creds)

    return service

#--------------------------------------------------------------

def sendRequest(requests, service, presentationId):
    body = {
        'requests': requests
    }
    service.presentations().batchUpdate(presentationId=presentationId, body=body).execute()

#--------------------------------------------------------------

def generateRequest(photo_names, slide_names, base_img_url) -> list:
    requests = []
    emu4M = {
        'magnitude': 4000000,
        'unit': 'EMU'
    }

    num_of_img = 0
    if len(slide_names) > len(photo_names):
        num_of_img = len(slide_names) // len(photo_names)
    else:
        num_of_img = len(photo_names) // len(slide_names)

    for slide_name in slide_names:
        coords = calculateCoords(num_of_img)
        for i in range(num_of_img):
            requests.append({
                'createImage': {
                'url': base_img_url + photo_names[i],
                'elementProperties': {
                    'pageObjectId': slide_name,
                    'size': {
                        'height': emu4M,
                        'width': emu4M
                    },
                'transform': {
                    'scaleX': 0.5,
                    'scaleY': 0.5,
                    'translateX': coords[i][0],
                    'translateY': coords[i][1],
                    'unit': 'EMU'
                        }
                    }
                }
            })
        photo_names = photo_names[num_of_img:]
    return requests

#--------------------------------------------------------------

def main(presentationId):
    service = initSlides()
    photo_names = getPhotoNames(githubToken)
    slide_names = getSlideNames(service, presentationId)
    requests = generateRequest(photo_names, slide_names, baseImgUrl)
    sendRequest(requests, service, presentationId)
