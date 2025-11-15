// js/common.js - Fonctionnalités communes (structure frontend/)

// --- VARIABLES GLOBALES ---
// Centralise les infos réutilisées dans tout le site
const SITE_INFO = {
    siteName: "SportTickets jo2024",
    siteURL: "https://www.sporttickets-jo2024.fr",
    organizerName: "SportEvents France",
    organizerEmail: "contact@sportevents.fr",
    organizerPhone: "+33 1 23 45 67 89",
    organizerAddress: "12 rue des Champions, 75000 Paris, France",
    organizerLegal: "SARL au capital de 20 000 € - SIRET 51234567800012 - RCS Paris",
    paymentProvider: "Stripe",
    hostName: "Hebergeur",
    hostURL: "https://www.hebergeur.com",
    hostAddress: "2 rue Kellermann, 59100 Roubaix, France",
    hostPhone: "+33 9 72 10 10 07",
    year: new Date().getFullYear()
};


// Menu mobile
function toggleMenu() {
    const navMenu = document.getElementById('navMenu');
    if (navMenu) {
        navMenu.classList.toggle('active');
    }
}

// Fonction utilitaire pour charger du contenu dynamiquement
async function loadHTML(elementId, filePath) {
    console.log(`Tentative de chargement de ${filePath} dans #${elementId}`);
    
    try {
        const response = await fetch(filePath);
        console.log(`Status pour ${filePath}: ${response.status}`);
        
        if (!response.ok) {
            console.error(`❌ ${filePath} non trouvé (${response.status})`);
            console.error('URL complète:', response.url);
            return;
        }
        
        const html = await response.text();
        console.log(`✅ ${filePath} chargé (${html.length} caractères)`);
        
        const element = document.getElementById(elementId);
        if (element) {
            element.innerHTML = html;
            console.log(`✅ Contenu inséré dans #${elementId}`);
        } else {
            console.error(`❌ Élément #${elementId} introuvable dans le DOM`);
        }
    } catch (error) {
        console.error(`❌ Erreur lors du chargement de ${filePath}:`, error);
    }
}


// --- REMPLACE LES VARIABLES DANS LE HTML ---
function replaceTemplateVars(container = document) {
    const html = container.innerHTML;
    container.innerHTML = html.replace(/\{\{(.*?)\}\}/g, (match, key) => {
        const trimmedKey = key.trim();
        return SITE_INFO[trimmedKey] !== undefined ? SITE_INFO[trimmedKey] : match;
    });
}

// Fonction pour mettre à jour le compteur de panier
function updateCartCount() {
    const cartCount = localStorage.getItem('cartCount') || 0;
    const cartElement = document.getElementById('cart-count');
    if (cartElement) {
        cartElement.textContent = cartCount;
    }
}

// Fonction pour marquer l'élément de navigation actif
function markActiveNavItem() {
    const currentPage = window.location.pathname.split('/').pop() || 'index.html';
    const navLinks = document.querySelectorAll('#navMenu a');
    
    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href === currentPage || 
            (currentPage === '' && href === 'index.html') ||
            (currentPage === 'index.html' && href === '/') ||
            (currentPage === '/' && href === 'index.html')) {
            link.classList.add('active');
        }
    });
}

// Initialisation au chargement de la page
document.addEventListener('DOMContentLoaded', async function() {
    console.log('DOMContentLoaded - Début initialisation');
    console.log('URL actuelle:', window.location.href);
    console.log('Pathname:', window.location.pathname);
    
    // Charger header et footer
    await loadHTML('header-container', '/header.html');
    await loadHTML('footer-container', '/footer.html');
    
    // Attendre un peu pour que le header soit chargé
    setTimeout(() => {
        console.log('Initialisation des événements après 200ms');
        
        // Gestion du smooth scroll
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                e.preventDefault();
                const target = document.querySelector(this.getAttribute('href'));
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth'
                    });
                }
            });
        });
        
        // Fermer le menu mobile quand on clique sur un lien
        document.querySelectorAll('#navMenu a').forEach(link => {
            link.addEventListener('click', () => {
                const navMenu = document.getElementById('navMenu');
                if (navMenu) {
                    navMenu.classList.remove('active');
                }
            });
        });
        
        // Mettre à jour le compteur de panier
        updateCartCount();
        
        // Marquer la page active dans la navigation
        markActiveNavItem();
        
        console.log('✅ Initialisation terminée');
    }, 200);
});