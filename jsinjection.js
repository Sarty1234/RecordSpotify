
//       Finding UI elements
const songNameLabel = document.querySelector(`[data-testid="context-item-link"]`);
const songAuthorLabel = document.querySelector(`[data-testid="context-item-info-artist"]`);

const nextSongButton = document.querySelector(`[data-testid="control-button-skip-forward"]`);
const pauseButton = document.querySelector(`[data-testid="control-button-playpause"]`);


function GetSongName() {
    const name = songNameLabel?.textContent?.trim() || "Unknown Title";
    const artist = songAuthorLabel?.textContent?.trim() || "Unknown Artist";
    const response = `${name}  ●  ${artist}`;
    console.log(response);
    return response;
}


function OnSongChange(newName) {
    // alert(GetSongName());
    console.log("!!!!!!!   New Song" + `${newName}`);
}


function delay(time) {
  return new Promise(resolve => setTimeout(resolve, time));
}



8
let lastSong = "";
let updateID = 0;
function Update() {
    console.log("Update #" + `${updateID}`);

    const currentSong = GetSongName();
    if (currentSong !== lastSong) 
    {
        lastSong = currentSong;
        let response = OnSongChange(currentSong);
    }


    updateID++;
};


async function Start() 
{
    alert("start");
    while (true) {
        Update();
        await delay(100);
    }
}


Start();