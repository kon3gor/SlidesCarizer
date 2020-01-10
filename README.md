# SlidesCatizer

One day, I discovered that I add lots of cats to my presentations. And it's really hard to place them in the right place. 
So I decided to create an api that could do it randomly so that I colud focus on the informative content of the presentation

 ## How to use C# version
_Well, C# version is like a demo of this api. I don't recomend to use it, but if you want I won't stop you._
### Here is an instruction
1) Clone of download _C#_ directory.
2) Visit [google slides api website](https://developers.google.com/slides) and dowload an **credentials.json** file.\
_Quick tip: you can open [quickstart](https://developers.google.com/slides/quickstart/dotnet) and press **enable the Google Slide Api** button_
3) Create new json file like this:
```json
  {
    "presentation_id": "id of your presentation",
    "img_base_url": "something like this - https://raw.githubusercontent.com/kon3gor/SlidesCatizer/master/images/",
    "token": "token to access github api"
  }
```
4) Put both json files in the same directory with .exe file.
5) Open comand promt in this folder.
6) Type `SlidesCatizer *path to the json file you created at the step 3*
7) Congrads, you run it !!! Now wait untill it stop working, close comand promt and enjoy the result.
