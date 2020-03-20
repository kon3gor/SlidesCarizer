document.getElementById("catize").onclick = function() {clicked()};

function clicked() {
  var http = new XMLHttpRequest();
  var url = 'https://catizer-api.herokuapp.com//catize';
  http.open('POST', url, true);

  //Send the proper header information along with the request
  http.setRequestHeader('Content-type', 'application/json');

  http.onreadystatechange = function() {//Call a function when the state changes.
    if(http.readyState == 4 && http.status == 200) {
        alert(JSON.parse(http.responseText));
    }
  }
  var json = JSON.stringify({"presentationLink":  document.getElementById("input").value});
  http.send(json);
  console.log("it works");
}
