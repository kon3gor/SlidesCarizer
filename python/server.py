from flask import Flask, request
from catizer import *
from utils import getId
import json


app = Flask(__name__)


@app.route('/catize', methods=['POST'])
def catize():
    if request.get_json():
        jdata = request.get_json()
        try:
            presentationLink = jdata["presentationLink"]
        except:
            return {"response": "wrong json"}
        presentationId = getId(presentationLink)
        if presentationId:
            main(presentationId)
            return {"response": "catized"}
        else:
            return {"response": "wrong link"}

    return {"response": "bad request"}

@app.route("/add", methods=["POST"])
def add():
    return "nice"


@app.route('/test')
def test():
    return "It's okay"
