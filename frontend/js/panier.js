
// Variables globales
let cart = [
    // Données de test - à remplacer par les vraies données
    {
        id: 1,
        type: 'duo',
        name: 'Offre Duo',
        sport: 'Natation',
        price: 130,
        quantity: 1,
        selected: false
    },
    {
        id: 2,
        type: 'solo',
        name: 'Offre Solo',
        sport: 'Athlétisme',
        price: 75,
        quantity: 2,
        selected: false
    },
    {
        id: 3,
        type: 'famille',
        name: 'Offre Famille',
        sport: 'Basketball',
        price: 220,
        quantity: 1,
        selected: false
    }
];

let isLoggedIn = false; // Simulation - à connecter avec votre système d'authentification

// Menu mobile
function toggleMenu() {
    const navMenu = document.getElementById('navMenu');
    navMenu.classList.toggle('active');
}

// Initialisation de la page
function initCart() {
    displayCartItems();
    updateCartSummary();
    updateCartCount();
}

// Afficher les articles du panier
function displayCartItems() {
    const cartItemsList = document.getElementById('cartItemsList');
    const emptyCart = document.getElementById('emptyCart');
    const cartActions = document.getElementById('cartActions');
    const orderSummary = document.getElementById('orderSummary');

    if (cart.length === 0) {
        emptyCart.style.display = 'block';
        cartItemsList.innerHTML = '';
        cartActions.style.display = 'none';
        orderSummary.style.display = 'none';
        return;
    }

    emptyCart.style.display = 'none';
    cartActions.style.display = 'flex';
    orderSummary.style.display = 'block';

    cartItemsList.innerHTML = '';
    
    cart.forEach(item => {
        const cartItemElement = document.createElement('div');
        cartItemElement.className = `cart-item ${item.selected ? 'selected' : ''}`;
        cartItemElement.innerHTML = `
            <div class="item-header">
                <input type="checkbox" class="item-checkbox" ${item.selected ? 'checked' : ''} 
                        onchange="toggleItemSelection(${item.id})">
                <div class="item-info">
                    <div class="item-title">${item.name}</div>
                    <div class="item-sport">🏆 ${getSportDisplay(item.sport)}</div>
                </div>
            </div>
            <div class="item-details">
                <div class="quantity-controls">
                    <button class="qty-btn" onclick="changeQuantity(${item.id}, -1)" ${item.quantity <= 1 ? 'disabled' : ''}>-</button>
                    <span class="quantity-display">${item.quantity}</span>
                    <button class="qty-btn" onclick="changeQuantity(${item.id}, 1)">+</button>
                </div>
                <div class="item-price">
                    <div class="unit-price">${item.price}€ / unité</div>
                    <div class="total-price">${item.price * item.quantity}€</div>
                </div>
            </div>
        `;
        cartItemsList.appendChild(cartItemElement);
    });
}

// Obtenir l'affichage du sport
function getSportDisplay(sport) {
    const sports = {
        'natation': 'Natation - Bassin Olympique',
        'athletisme': 'Athlétisme - Stade de France',
        'basketball': 'Basketball - Accor Arena',
        'surf': 'Surf - Teahupo\'o, Tahiti',
        'gymnastique': 'Gymnastique - Bercy Arena',
        'tennis': 'Tennis - Roland Garros'
    };
    return sports[sport] || sport;
}

// Changer la quantité d'un article
function changeQuantity(itemId, change) {
    const item = cart.find(item => item.id === itemId);
    if (item) {
        item.quantity = Math.max(1, item.quantity + change);
        displayCartItems();
        updateCartSummary();
        updateCartCount();
        showMessage('Quantité mise à jour !', 'success');
    }
}

// Basculer la sélection d'un article
function toggleItemSelection(itemId) {
    const item = cart.find(item => item.id === itemId);
    if (item) {
        item.selected = !item.selected;
        displayCartItems();
        updateDeleteButton();
    }
}

// Mettre à jour le bouton de suppression
function updateDeleteButton() {
    const deleteBtn = document.getElementById('deleteBtn');
    const hasSelectedItems = cart.some(item => item.selected);
    deleteBtn.disabled = !hasSelectedItems;
}

// Supprimer les articles sélectionnés
function deleteSelected() {
    const selectedItems = cart.filter(item => item.selected);
    if (selectedItems.length === 0) {
        showMessage('Aucun article sélectionné', 'error');
        return;
    }

    if (confirm(`Supprimer ${selectedItems.length} article(s) sélectionné(s) ?`)) {
        cart = cart.filter(item => !item.selected);
        displayCartItems();
        updateCartSummary();
        updateCartCount();
        showMessage(`${selectedItems.length} article(s) supprimé(s) !`, 'success');
    }
}

// Vider tout le panier
function clearCart() {
    if (cart.length === 0) {
        showMessage('Le panier est déjà vide', 'error');
        return;
    }

    if (confirm('Êtes-vous sûr de vouloir vider complètement votre panier ?')) {
        cart = [];
        displayCartItems();
        updateCartSummary();
        updateCartCount();
        showMessage('Panier vidé !', 'success');
    }
}

// Mettre à jour le résumé de commande
function updateCartSummary() {
    const subtotalHT = cart.reduce((total, item) => total + (item.price * item.quantity), 0);
    const taxRate = 0.20; // 20% TVA
    const taxAmount = subtotalHT * taxRate;
    const totalTTC = subtotalHT + taxAmount;

    // Calcul HT à partir du TTC (les prix affichés sont TTC)
    const realSubtotalHT = subtotalHT / (1 + taxRate);
    const realTaxAmount = subtotalHT - realSubtotalHT;

    document.getElementById('subtotalHT').textContent = realSubtotalHT.toFixed(2) + '€';
    document.getElementById('taxAmount').textContent = realTaxAmount.toFixed(2) + '€';
    document.getElementById('totalTTC').textContent = subtotalHT.toFixed(2) + '€';
}

// Mettre à jour le compteur du panier
function updateCartCount() {
    const totalItems = cart.reduce((total, item) => total + item.quantity, 0);
    document.getElementById('cart-count').textContent = totalItems;
}

// Procéder à la commande
function proceedToCheckout() {
    if (cart.length === 0) {
        showMessage('Votre panier est vide !', 'error');
        return;
    }

    if (!isLoggedIn) {
        document.getElementById('loginNotice').classList.add('show');
        document.querySelector('.login-notice').scrollIntoView({ behavior: 'smooth' });
        return;
    }

    // Simulation de la redirection vers la page de commande
    if (confirm('Procéder au paiement ?\n\n📋 Récapitulatif :\n' + 
                cart.map(item => `• ${item.name} x${item.quantity} - ${item.price * item.quantity}€`).join('\n') +
                '\n\n💰 Total : ' + cart.reduce((total, item) => total + (item.price * item.quantity), 0) + '€')) {
        alert('🎫 Redirection vers la page de paiement...\n\nEn développement : Cette fonctionnalité sera disponible dans la prochaine version.');
    }
}

// Aller à la page de connexion
function goToLogin() {
    alert('🔐 Redirection vers la page de connexion...\n\nEn développement : Page de connexion à venir.');
    // Simulation de connexion pour les tests
    isLoggedIn = true;
    document.getElementById('loginNotice').classList.remove('show');
    showMessage('Connexion simulée réussie !', 'success');
}

// Retour (fonction pour le bouton retour)
function goBack() {
    window.history.back();
}

// Afficher un message
function showMessage(text, type) {
    const messageElement = document.getElementById(type === 'success' ? 'successMessage' : 'errorMessage');
    messageElement.textContent = text;
    messageElement.classList.add('show');
    
    setTimeout(() => {
        messageElement.classList.remove('show');
    }, 3000);
}

// Fermer le menu mobile quand on clique sur un lien
document.querySelectorAll('#navMenu a').forEach(link => {
    link.addEventListener('click', () => {
        document.getElementById('navMenu').classList.remove('active');
    });
});

// Initialisation au chargement de la page
window.addEventListener('load', () => {
    initCart();
});

// Simulation de sauvegarde des données (pour integration future avec C#.NET)
function saveCartToServer() {
    // Cette fonction sera utilisée pour sauvegarder le panier côté serveur
    console.log('Sauvegarde du panier:', cart);
}

// Charger le panier depuis le serveur (pour intégration future)
function loadCartFromServer() {
    // Cette fonction sera utilisée pour charger le panier depuis le serveur
    console.log('Chargement du panier depuis le serveur');
}
