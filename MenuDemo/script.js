document.addEventListener('DOMContentLoaded', () => {
    const btnStart = document.getElementById('btn-start');
    const btnSettings = document.getElementById('btn-settings');
    const btnCredits = document.getElementById('btn-credits');
    
    const uiLayer = document.getElementById('ui-layer');
    const boatContainer = document.getElementById('boat-container');
    const transitionOverlay = document.getElementById('transition-overlay');

    // Button sound effect placeholder
    const playClickSound = () => {
        // In a real game, play an audio file here.
        console.log("Button clicked sound effect.");
    };

    btnStart.addEventListener('click', () => {
        playClickSound();
        
        // 1. Hide the UI immediately
        uiLayer.classList.add('ui-hidden');

        // 2. Make the boat sail away
        // We remove the floating class so the floating animation doesn't interfere
        // with the CSS transform transition for sailing away.
        boatContainer.classList.remove('floating');
        
        // Use a small timeout to ensure the class swap renders properly
        setTimeout(() => {
            boatContainer.classList.add('sailing-away');
        }, 50);

        // 3. Slowly fade to black to start the game
        setTimeout(() => {
            transitionOverlay.classList.add('fade-to-black');
            
            // After fade completes, simulate loading the game scene
            setTimeout(() => {
                console.log("Loading Game Scene: Yamato Awakening...");
                // window.location.href = 'game.html'; // Example redirect
                
                // Show a text for the demo purposes
                transitionOverlay.innerHTML = '<div style="color: white; font-family: \'Cinzel\', serif; font-size: 2rem; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); text-align: center; font-weight: 700; letter-spacing: 4px;">ENTERING YAMATO...<br><span style="font-size: 1rem; color: #e0a96d; font-family: \'Inter\', sans-serif;">Demo placeholder end</span></div>';
            }, 3500);

        }, 1500); // Wait 1.5s before fading
    });

    btnSettings.addEventListener('click', () => {
        playClickSound();
        alert("Settings Menu placeholder. (In-game, this would open a UI panel for Audio/Video/Controls)");
    });

    btnCredits.addEventListener('click', () => {
        playClickSound();
        alert("Credits Menu placeholder. (In-game, this would scroll a list of team members)");
    });
});
