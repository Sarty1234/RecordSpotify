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
    console.log(GetSongName());
}


let lastSong = "";
const callback = (mutationsList, observer) => {
    for (const mutation of mutationsList) {
        if (mutation.type === 'childList' || mutation.type === "characterData")
        {
            const currentSong = GetSongName();

            if (currentSong !== lastSong) {
                lastSong = currentSong;

                let response = OnSongChange();

                // if (response === "skipSong") {
                //     nextSongButton.click();
                // }
                //  else if (response === "start") {
                //     pauseButton.click();
                // }
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
