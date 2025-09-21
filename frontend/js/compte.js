// compte.js - Gestion de l'espace utilisateur

// Configuration API
const API_BASE = '/api/compte';
let currentUser = null;
let tickets = [];
let orders = [];

// Données de test utilisateur
const userData = {
    firstName: 'Jean',
    lastName: 'Dupont',
    email: 'jean.dupont@email.com',
    memberSince: '2024-05-15'
};

// Initialisation au chargement de la page
document.addEventListener('DOMContentLoaded', function() {
    initializeAccount();
    setupEventListeners();
    showSection('tickets'); // Section par défaut
});

// Initialisation du compte
async function initializeAccount() {
    try {
        await loadUserProfile();
        await loadTickets();
        await loadOrders();
        updateWelcomeMessage();
    } catch (error) {
        console.error('Erreur lors de l\'initialisation:', error);
        showError('Erreur lors du chargement des données');
    }
}

// Configuration des écouteurs d'événements
function setupEventListeners() {
    // Navigation des sections
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const sectionId = link.getAttribute('data-section');
            showSection(sectionId);
        });
    });

    // Masquer les messages au clic
    document.querySelectorAll('.message').forEach(msg => {
        msg.addEventListener('click', () => {
            msg.style.display = 'none';
        });
    });
}

// Navigation entre les sections
function showSection(sectionId) {
    // Masquer toutes les sections
    document.querySelectorAll('.section').forEach(section => {
        section.classList.remove('active');
    });

    // Retirer la classe active de tous les liens
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });

    // Afficher la section demandée
    const targetSection = document.getElementById(sectionId);
    if (targetSection) {
        targetSection.classList.add('active');
    }
    
    // Activer le lien correspondant
    const activeLink = document.querySelector(`[data-section="${sectionId}"]`);
    if (activeLink) {
        activeLink.classList.add('active');
    }
}

// Charger le profil utilisateur
async function loadUserProfile() {
    try {
        const response = await fetch(`${API_BASE}/profil`, {
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`
            }
        });
        
        if (response.ok) {
            currentUser = await response.json();
            populateUserForm();
        } else {
            // Utiliser les données de test en cas d'erreur
            currentUser = userData;
            populateUserForm();
        }
    } catch (error) {
        console.error('Erreur chargement profil:', error);
        // Utiliser les données de test
        currentUser = userData;
        populateUserForm();
    }
}

// Remplir le formulaire avec les données utilisateur
function populateUserForm() {
    if (currentUser) {
        document.getElementById('firstName').value = currentUser.firstName || currentUser.prenom || '';
        document.getElementById('lastName').value = currentUser.lastName || currentUser.nom || '';
        document.getElementById('email').value = currentUser.email || '';
    }
}

// Mettre à jour le message de bienvenue
function updateWelcomeMessage() {
    const welcomeMsg = document.getElementById('welcomeMessage');
    if (welcomeMsg && currentUser) {
        const name = currentUser.firstName || currentUser.prenom || 'Utilisateur';
        welcomeMsg.textContent = `Bienvenue ${name} dans votre espace personnel`;
    }
}

// Charger les billets
async function loadTickets() {
    try {
        const response = await fetch(`${API_BASE}/billets`, {
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`
            }
        });
        
        if (response.ok) {
            tickets = await response.json();
            displayTickets();
        } else {
            // Garder les billets de test du HTML
            console.log('Utilisation des données de test pour les billets');
        }
    } catch (error) {
        console.error('Erreur chargement billets:', error);
    }
}

// Afficher les billets
function displayTickets() {
    const container = document.getElementById('ticketsContainer');
    if (!container || tickets.length === 0) return;

    container.innerHTML = tickets.map(ticket => `
        <div class="ticket-card" onclick="toggleTicketDetails(this)">
            <div class="ticket-header">
                <div class="ticket-title">${ticket.titre}</div>
                <div class="ticket-status status-${ticket.statut.toLowerCase()}">${getStatusText(ticket.statut)}</div>
            </div>
            <div class="ticket-info">
                📅 ${formatDate(ticket.dateEvenement)}<br>
                📍 ${ticket.lieu}<br>
                🎫 Billet #${ticket.numeroBillet}
            </div>
            <div class="ticket-details">
                <div class="detail-row">
                    <span><strong>Numéro du billet :</strong></span>
                    <span>${ticket.numeroBillet}</span>
                </div>
                <div class="detail-row">
                    <span><strong>Statut :</strong></span>
                    <span>${ticket.statutDescription}</span>
                </div>
                <div class="detail-row">
                    <span><strong>Place :</strong></span>
                    <span>${ticket.place}</span>
                </div>
                <div class="qr-actions">
                    ${ticket.peutVoirQR ? `<button class="btn btn-primary btn-small" onclick="viewQRCode(event, ${ticket.id})">👁️ Voir le QR code</button>` : ''}
                    ${ticket.peutTelecharger ? `<button class="btn btn-secondary btn-small" onclick="downloadPDF(event, ${ticket.id})">📄 Télécharger PDF</button>` : ''}
                    ${ticket.peutVoirQR ? `<button class="btn btn-outline btn-small" onclick="sendByEmail(event, ${ticket.id})">📧 Envoyer par email</button>` : ''}
                </div>
            </div>
        </div>
    `).join('');
}

// Charger les commandes
async function loadOrders() {
    try {
        const response = await fetch(`${API_BASE}/commandes`, {
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`
            }
        });
        
        if (response.ok) {
            orders = await response.json();
            displayOrders();
        }
    } catch (error) {
        console.error('Erreur chargement commandes:', error);
    }
}

// Afficher les commandes
function displayOrders() {
    const tbody = document.querySelector('.orders-table tbody');
    if (!tbody || orders.length === 0) return;

    tbody.innerHTML = orders.map(order => `
        <tr>
            <td><strong>${order.numeroCommande}</strong></td>
            <td>${formatDate(order.dateCommande)}<br>${formatTime(order.dateCommande)}</td>
            <td>${order.description}</td>
            <td><strong>${order.montantTotal}€</strong></td>
            <td><span class="ticket-status status-${order.statut.toLowerCase()}">${order.statut}</span></td>
        </tr>
    `).join('');
}

// Basculer les détails d'un billet
function toggleTicketDetails(ticketCard) {
    const details = ticketCard.querySelector('.ticket-details');
    const isExpanded = ticketCard.classList.contains('expanded');

    // Fermer tous les autres billets
    document.querySelectorAll('.ticket-card').forEach(card => {
        card.classList.remove('expanded');
        const cardDetails = card.querySelector('.ticket-details');
        if (cardDetails) {
            cardDetails.style.display = 'none';
        }
    });

    // Basculer le billet actuel
    if (!isExpanded) {
        ticketCard.classList.add('expanded');
        if (details) {
            details.style.display = 'block';
        }
    }
}

// Voir le QR code d'un billet
async function viewQRCode(event, ticketId = null) {
    event.stopPropagation();
    
    try {
        const id = ticketId || extractTicketIdFromCard(event.target);
        const response = await fetch(`${API_BASE}/billet/${id}/qr`, {
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`
            }
        });
        
        if (response.ok) {
            const data = await response.json();
            showQRCodeModal(data.qrCode, data.numeroBillet);
        } else {
            showError('Impossible de générer le QR code');
        }
    } catch (error) {
        console.error('Erreur QR code:', error);
        showError('Erreur lors de la génération du QR code');
    }
}

// Télécharger le PDF d'un billet
async function downloadPDF(event, ticketId = null) {
    event.stopPropagation();
    
    try {
        const id = ticketId || extractTicketIdFromCard(event.target);
        const response = await fetch(`${API_BASE}/billet/${id}/pdf`, {
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`
            }
        });
        
        if (response.ok) {
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `billet-${id}.pdf`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
            showSuccess('PDF téléchargé avec succès');
        } else {
            showError('Impossible de télécharger le PDF');
        }
    } catch (error) {
        console.error('Erreur téléchargement PDF:', error);
        showError('Erreur lors du téléchargement');
    }
}

// Envoyer un billet par email
async function sendByEmail(event, ticketId = null) {
    event.stopPropagation();
    
    try {
        const id = ticketId || extractTicketIdFromCard(event.target);
        const response = await fetch(`${API_BASE}/billet/${id}/email`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`,
                'Content-Type': 'application/json'
            }
        });
        
        if (response.ok) {
            const result = await response.json();
            showSuccess(result.message);
        } else {
            showError('Impossible d\'envoyer l\'email');
        }
    } catch (error) {
        console.error('Erreur envoi email:', error);
        showError('Erreur lors de l\'envoi par email');
    }
}

// Sauvegarder les paramètres utilisateur
async function saveSettings(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const userData = {
        prenom: formData.get('firstName'),
        nom: formData.get('lastName'),
        email: formData.get('email')
    };
    
    try {
        const response = await fetch(`${API_BASE}/profil`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userData)
        });
        
        if (response.ok) {
            showSuccess('Informations mises à jour avec succès');
            currentUser = { ...currentUser, ...userData };
            updateWelcomeMessage();
        } else {
            const error = await response.json();
            showError(error.message || 'Erreur lors de la mise à jour');
        }
    } catch (error) {
        console.error('Erreur sauvegarde:', error);
        showError('Erreur lors de la sauvegarde');
    }
}

// Changer le mot de passe
async function changePassword(event) {
    event.preventDefault();
    
    const formData = new FormData(event.target);
    const newPassword = formData.get('newPassword');
    const confirmPassword = formData.get('confirmPassword');
    
    if (newPassword !== confirmPassword) {
        showError('Les mots de passe ne correspondent pas');
        return;
    }
    
    if (!isValidPassword(newPassword)) {
        showError('Le mot de passe ne respecte pas les critères de sécurité');
        return;
    }
    
    const passwordData = {
        motDePasseActuel: formData.get('currentPassword'),
        nouveauMotDePasse: newPassword,
        confirmerMotDePasse: confirmPassword
    };
    
    try {
        const response = await fetch(`${API_BASE}/motdepasse`, {
            method: 'PUT',
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(passwordData)
        });
        
        if (response.ok) {
            showSuccess('Mot de passe changé avec succès');
            event.target.reset();
        } else {
            const error = await response.json();
            showError(error.message || 'Erreur lors du changement de mot de passe');
        }
    } catch (error) {
        console.error('Erreur changement mot de passe:', error);
        showError('Erreur lors du changement de mot de passe');
    }
}

// Télécharger les données utilisateur (RGPD)
async function downloadUserData() {
    try {
        const response = await fetch(`${API_BASE}/donnees`, {
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`
            }
        });
        
        if (response.ok) {
            const data = await response.json();
            const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'mes-donnees-jo2024.json';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
            showSuccess('Données téléchargées avec succès');
        } else {
            showError('Impossible de télécharger les données');
        }
    } catch (error) {
        console.error('Erreur téléchargement données:', error);
        showError('Erreur lors du téléchargement des données');
    }
}

// Confirmer la suppression du compte
function confirmDeleteAccount() {
    if (confirm('⚠️ ATTENTION ⚠️\n\nVoulez-vous vraiment supprimer définitivement votre compte ?\n\nCette action est irréversible et supprimera :\n- Toutes vos informations personnelles\n- Votre historique de commandes\n- Tous vos billets\n\nTapez "SUPPRIMER" pour confirmer :')) {
        const confirmation = prompt('Pour confirmer, tapez "SUPPRIMER" :');
        if (confirmation === 'SUPPRIMER') {
            deleteAccount();
        }
    }
}

// Supprimer le compte
async function deleteAccount() {
    try {
        const response = await fetch(`${API_BASE}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${getAuthToken()}`
            }
        });
        
        if (response.ok) {
            alert('Compte supprimé avec succès. Vous allez être déconnecté.');
            logout();
        } else {
            const error = await response.json();
            showError(error.message || 'Erreur lors de la suppression du compte');
        }
    } catch (error) {
        console.error('Erreur suppression compte:', error);
        showError('Erreur lors de la suppression du compte');
    }
}

// Fonctions utilitaires
function getAuthToken() {
    return localStorage.getItem('authToken') || 'demo-token';
}

function extractTicketIdFromCard(element) {
    const card = element.closest('.ticket-card');
    const numberElement = card.querySelector('.ticket-info');
    const text = numberElement.textContent;
    const match = text.match(/#([^\\s]+)/);
    return match ? match[1] : '1';
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('fr-FR', {
        day: '2-digit',
        month: 'long',
        year: 'numeric'
    });
}

function formatTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleTimeString('fr-FR', {
        hour: '2-digit',
        minute: '2-digit'
    });
}

function getStatusText(status) {
    const statusMap = {
        'Actif': 'Actif',
        'Scanne': 'Scanné',
        'Expire': 'Expiré',
        'Annule': 'Annulé'
    };
    return statusMap[status] || status;
}

function isValidPassword(password) {
    const minLength = password.length >= 8;
    const hasUpper = /[A-Z]/.test(password);
    const hasLower = /[a-z]/.test(password);
    const hasDigit = /\d/.test(password);
    const hasSpecial = /[!@#$%^&*]/.test(password);
    
    return minLength && hasUpper && hasLower && hasDigit && hasSpecial;
}

function showSuccess(message) {
    const successEl = document.getElementById('successMessage');
    if (successEl) {
        successEl.textContent = `✅ ${message}`;
        successEl.style.display = 'block';
        setTimeout(() => {
            successEl.style.display = 'none';
        }, 5000);
    }
}

function showError(message) {
    const errorEl = document.getElementById('errorMessage');
    if (errorEl) {
        errorEl.textContent = `❌ ${message}`;
        errorEl.style.display = 'block';
        setTimeout(() => {
            errorEl.style.display = 'none';
        }, 5000);
    }
}

function showQRCodeModal(qrCode, ticketNumber) {
    // Créer une modal pour afficher le QR code
    const modal = document.createElement('div');
    modal.className = 'qr-modal';
    modal.innerHTML = `
        <div class="qr-modal-content">
            <h3>QR Code - Billet ${ticketNumber}</h3>
            <div class="qr-code-display">${qrCode}</div>
            <button onclick="this.closest('.qr-modal').remove()">Fermer</button>
        </div>
    `;
    document.body.appendChild(modal);
}

function logout() {
    localStorage.removeItem('authToken');
    window.location.href = '/login.html';
}