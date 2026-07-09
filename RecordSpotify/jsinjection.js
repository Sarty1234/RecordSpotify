const playerBox = document.querySelector(`[data-testid="now-playing-widget"]`);

const nextSongButton = document.querySelector(`[data-testid="control-button-skip-forward"]`);
const pauseButton = document.querySelector(`[data-testid="control-button-playpause"]`);


function GetSongName() {
    const songNameLabel = document.querySelector(`[data-testid="context-item-link"]`);
    const songAuthorLabel = document.querySelector(`[data-testid="context-item-info-artist"]`);

    const name = songNameLabel?.textContent?.trim() || "Unknown Title";
    const artist = songAuthorLabel?.textContent?.trim() || "Unknown Artist";
    const response = `${name}  ●  ${artist}`
    return response;
}


function OnSongChange(newName) {
    console.log(`Song changed to ${newName}`);
    const url = 'http://localhost:2034/song-update/';

    fetch(url, {method: 'POST',
        headers: {'Content-Type': 'text/plain'}, 
        body: newName}
    )
    .then(response => response.text())
    .then(status => {
        console.log("Server response: \n" + status);
        if (status === "start") {
            console.log("Server requested start");
            pauseButton.click();
        }
        else if (status === "skip") {
            console.log("Server requested skip");
            nextSongButton.click();
        }
    })
    .catch(error => console.error('Error connecting to c# server: ', error));
}


let lastSong = "";
const callback = (mutationsList, observer) => {
    for (const mutation of mutationsList) {
        if (mutation.type === 'childList' || mutation.type === "characterData")
        {
            const currentSong = GetSongName();

            if (currentSong !== lastSong) {
                lastSong = currentSong;

                let response = OnSongChange(currentSong);
                break;
            }
        }
    }
};
    

const observer = new MutationObserver(callback);

// 4. Start observing the target element with specific configurations
observer.observe(playerBox, {
        childList: true,  
        characterData: true,
        subtree: true
});

// To stop watching the element later:
// observer.disconnect();
