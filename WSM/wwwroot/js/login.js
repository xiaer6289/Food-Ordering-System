const themes = [
    {
        background: "#F9F9F9",       
        color: "#1A1A1A",            
        primaryColor: "#AEDFF7"     
    },
    {
        background: "#FFF5E1",       
        color: "#333333",            
        primaryColor: "#FFB085"      
    },
    {
        background: "#FAF9F6",      
        color: "#2E2E2E",            
        primaryColor: "#FFC6AC"      
    },
    {
        background: "#FFF9E5",       
        color: "#2E2E2E",            
        primaryColor: "#FFD59E"      
    },
    {
        background: "#FFF0F5",       
        color: "#2F2F2F",            
        primaryColor: "#DDA0DD"      
    },
    {
        background: "#E8F6EF",       
        color: "#2C3E50",            
        primaryColor: "#B8E994"      
    }
];


const setTheme = (theme) => {
    const root = document.querySelector(":root");
    root.style.setProperty("--background", theme.background);
    root.style.setProperty("--color", theme.color);
    root.style.setProperty("--primary-color", theme.primaryColor);
    root.style.setProperty("--glass-color", theme.glassColor);
};

const displayThemeButtons = () => {
    const btnContainer = document.querySelector(".theme-btn-container");
    themes.forEach((theme) => {
        const div = document.createElement("div");
        div.className = "theme-btn";
        div.style.cssText = `background: ${theme.background}; width: 25px; height: 25px`;
        btnContainer.appendChild(div);
        div.addEventListener("click", () => setTheme(theme));
    });
};

displayThemeButtons();
