
// Variables globales
let currentSport = 0;
let currentOffer = 0;
const totalSports = 4;
const totalOffers = 3;


// Carousel Sports
function showSport(index) {
    currentSport = index;
    const container = document.getElementById('sportsContainer');
    container.style.transform = `translateX(-${index * 100}%)`;
    
    // Mise à jour des points de navigation
    document.querySelectorAll('.sports-section .nav-dot').forEach((dot, i) => {
        dot.classList.toggle('active', i === index);
    });
}

// Carousel Offres
function showOffer(index) {
    currentOffer = index;
    const container = document.getElementById('offersContainer');
    container.style.transform = `translateX(-${index * 100}%)`;
    
    // Mise à jour des points de navigation
    document.querySelectorAll('.offers-section .nav-dot').forEach((dot, i) => {
        dot.classList.toggle('active', i === index);
    });
}

// Auto-défilement des sports
function autoScrollSports() {
    currentSport = (currentSport + 1) % totalSports;
    showSport(currentSport);
}

// Auto-défilement des offres
function autoScrollOffers() {
    currentOffer = (currentOffer + 1) % totalOffers;
    showOffer(currentOffer);
}

// Démarrage des auto-défilements
setInterval(autoScrollSports, 4000); // Change toutes les 4 secondes
setInterval(autoScrollOffers, 5000); // Change toutes les 5 secondes
