
//       Finding UI elements
const songNameLabel = document.querySelector(`[data-testid="context-item-link"]`);
const songAuthorLabel = document.querySelector(`[data-testid="context-item-info-artist"]`);

const nextSongButton = document.querySelector(`[data-testid="control-button-skip-forward"]`);
const pauseButton = document.querySelector(`[data-testid="control-button-playpause"]`);


function GetSongName() {
    const name = songNameLabel?.textContent?.trim() || "Unknown Title";
    const artist = songAuthorLabel?.textContent?.trim() || "Unknown Artist";
    const response = '${name}  ●  ${artist}'
    return response;
}


function OnSongChange() {
    alert(GetSongName());
}


let lastSong = "";
const callback = (mutationsList, observer) => {
    alert("stat");
    for (const mutation of mutationsList) {
        if (mutation.type === 'childList' || mutation.type === "characterData")
        {
            const currentSong = GetSongName();

            if (currentSong === lastSong) {
                continue;
            }
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
};
    
 // ///////////////////   do simple while loop 100 ms delay checking for update
const observer = new MutationObserver(callback);

// 4. Start observing the target element with specific configurations
observer.observe(songNameLabel, {
        childList: true,  
        characterData: true,
        subtree: true
});
alert("stat");

// To stop watching the element later:
// observer.disconnect();
