// js/common.js - Fonctionnalités communes (structure frontend/)

// Menu mobile
function toggleMenu() {
    const navMenu = document.getElementById('navMenu');
    if (navMenu) {
        navMenu.classList.toggle('active');
    }
}

// Smooth scroll pour les liens d'ancrage
document.addEventListener('DOMContentLoaded', function() {
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
    const navLinks = document.querySelectorAll('#navMenu a');
    navLinks.forEach(link => {
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
    // Attendre un peu que le header soit chargé
    setTimeout(() => {
        markActiveNavItem();
    }, 100);
});

// Fonction pour mettre à jour le compteur de panier
function updateCartCount() {
    const cartCount = localStorage.getItem('cartCount') || 0;
    const updateCounter = () => {
        const cartElement = document.getElementById('cart-count');
        if (cartElement) {
            cartElement.textContent = cartCount;
        }
    };
    
    // Essayer maintenant
    updateCounter();
    
    // Et réessayer après le chargement du header
    setTimeout(updateCounter, 200);
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

// Fonction utilitaire pour charger du contenu dynamiquement
// Cette fonction est gardée pour compatibilité mais n'est plus utilisée
// car chaque page charge son header/footer directement
async function loadHTML(elementId, filePath) {
    try {
        const response = await fetch(filePath);
        if (!response.ok) {
            console.warn(`${filePath} non trouvé - vérifiez le chemin`);
            return;
        }
        const html = await response.text();
        const element = document.getElementById(elementId);
        if (element) {
            element.innerHTML = html;
        }
    } catch (error) {
        console.warn('Erreur lors du chargement de', filePath, ':', error);
    }
}