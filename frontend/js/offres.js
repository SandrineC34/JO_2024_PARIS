// Variables globales
let cart = [];
let selectedSport = '';

// Menu mobile
function toggleMenu() {
    const navMenu = document.getElementById('navMenu');
    navMenu.classList.toggle('active');
}

// Sélection de sport
function selectSport(element, sport) {
    // Retirer la sélection précédente
    document.querySelectorAll('.sport-option').forEach(option => {
        option.classList.remove('selected');
    });
    
    // Ajouter la sélection à l'élément cliqué
    element.classList.add('selected');
    selectedSport = sport;
}

// Ajouter au panier
function addToCart(offerType, price) {
    const quantity = parseInt(document.getElementById(`qty-${offerType}`).value);
    const totalPrice = price * quantity;
    
    // Vérifier si un sport est sélectionné
    if (!selectedSport) {
        alert('⚠️ Veuillez d\'abord sélectionner un sport !');
        document.querySelector('.sports-selection').scrollIntoView({ behavior: 'smooth' });
        return;
    }

    // Ajouter l'item au panier
    const cartItem = {
        type: offerType,
        price: price,
        quantity: quantity,
        sport: selectedSport,
        total: totalPrice
    };

    cart.push(cartItem);
    updateCartDisplay();
    showSuccessMessage();
}

// Mettre à jour l'affichage du panier
function updateCartDisplay() {
    const cartCount = document.getElementById('cart-count');
    const cartSummary = document.getElementById('cartSummary');
    const cartItems = document.getElementById('cartItems');
    const cartTotal = document.getElementById('cartTotal');

    // Calculer le nombre total d'articles et le prix total
    let totalItems = 0;
    let totalPrice = 0;

    cart.forEach(item => {
        totalItems += item.quantity;
        totalPrice += item.total;
    });

    // Mettre à jour le compteur dans la navigation
    cartCount.textContent = totalItems;

    if (cart.length > 0) {
        // Afficher le résumé du panier
        cartSummary.style.display = 'block';
        
        // Vider et remplir la liste des articles
        cartItems.innerHTML = '';
        cart.forEach((item, index) => {
            const cartItem = document.createElement('div');
            cartItem.className = 'cart-item';
            cartItem.innerHTML = `
                <span>${getOfferName(item.type)} x${item.quantity}</span>
                <span>${item.total}€</span>
            `;
            cartItems.appendChild(cartItem);
        });

        cartTotal.textContent = totalPrice + '€';
    } else {
        cartSummary.style.display = 'none';
    }
}

// Obtenir le nom de l'offre
function getOfferName(type) {
    const names = {
        'solo': 'Solo',
        'duo': 'Duo',
        'famille': 'Famille'
    };
    return names[type] || type;
}

// Afficher le message de succès
function showSuccessMessage() {
    const message = document.getElementById('successMessage');
    message.style.display = 'block';
    
    // Faire défiler vers le message
    message.scrollIntoView({ behavior: 'smooth' });
    
    // Masquer après 3 secondes
    setTimeout(() => {
        message.style.display = 'none';
    }, 3000);
}

// Aller au panier
function goToCart() {
    alert('🛒 Redirection vers la page panier...\n\nEn développement : Cette fonctionnalité sera disponible dans la prochaine version.');
}

// Afficher les détails d'une offre
function showOfferDetails(offerType) {
    const details = {
        'solo': 'Offre Solo - Parfaite pour une expérience individuelle.\n\n• Billet nominatif\n• Accès à toutes les installations\n• Programme souvenir inclus\n• Assurance annulation disponible en option',
        'duo': 'Offre Duo - Idéale pour partager l\'émotion olympique.\n\n• 2 billets nominatifs\n• Places côte à côte garanties\n• Support prioritaire\n• Kit souvenir duo\n• Économie de 20€',
        'famille': 'Offre Famille - L\'expérience olympique en famille.\n\n• 4 billets nominatifs\n• Places groupées garanties\n• Support client premium\n• Kit famille avec souvenirs\n• Économie maximale de 80€'
    };
    
    alert('📋 ' + details[offerType]);
}

// Fermer le menu mobile quand on clique sur un lien
document.querySelectorAll('#navMenu a').forEach(link => {
    link.addEventListener('click', () => {
        document.getElementById('navMenu').classList.remove('active');
    });
});

// Animation d'entrée des cartes
window.addEventListener('load', () => {
    const cards = document.querySelectorAll('.offer-card');
    cards.forEach((card, index) => {
        setTimeout(() => {
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 200);
    });
});

// Style initial pour l'animation
document.querySelectorAll('.offer-card').forEach(card => {
    card.style.opacity = '0';
    card.style.transform = 'translateY(30px)';
    card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
});
