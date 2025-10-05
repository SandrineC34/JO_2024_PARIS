// compte.js - Gestion de la page Mon Compte avec appels API

// Configuration de l'API
const API_BASE_URL = '/api';

// État de l'application
let currentUser = null;
let userTickets = [];
let userOrders = [];

// ===========================
// INITIALISATION
// ===========================

document.addEventListener('DOMContentLoaded', async () => {
    console.log('Initialisation de la page Mon Compte');
    // Vérifier l'authentification
    await checkAuthentication();
    // Charger les données initiales (si connecté ou démo)
    await loadUserData();
    // Configurer la navigation
    setupNavigation();
    // Afficher la section par défaut
    showSection('tickets');
});

// ===========================
// AUTHENTIFICATION
// ===========================

async function checkAuthentication() {
    try {
        const response = await fetch(`${API_BASE_URL}/Auth/current`, {
            credentials: 'include'
        });
        if (!response.ok) {
            showError('Authentification requise (profil démo activé)');
            // DEV ONLY: Injecter un utilisateur fictif pour l'UI
            currentUser = { prenom: 'Démo', nom: 'Utilisateur' };
            updateWelcomeMessage();
            return;
            // PRODUCTION: Remettre la ligne ci-dessous pour redirection
            // window.location.href = '/connexion.html';
        }
        currentUser = await response.json();
        updateWelcomeMessage();
    } catch (error) {
        console.error('Erreur d\'authentification:', error);
        showError('Erreur de connexion. Veuillez vous reconnecter.');
        // DEV ONLY: Inject faux utilisateur si erreur API backend
        currentUser = { prenom: 'Démo', nom: 'Utilisateur' };
        updateWelcomeMessage();
    }
}

function updateWelcomeMessage() {
    const welcomeElement = document.getElementById('welcomeMessage');
    if (welcomeElement && currentUser) {
        welcomeElement.textContent = `Bienvenue ${currentUser.prenom} ${currentUser.nom}`;
    }
}

// ===========================
// CHARGEMENT DES DONNÉES
// ===========================

async function loadUserData() {
    if (!currentUser) return;
    try {
        await loadTickets();
        await loadOrders();
        await loadUserProfile();
    } catch (error) {
        console.error('Erreur de chargement des données:', error);
        showError('Impossible de charger vos données');
    }
}

// Charger les billets
async function loadTickets() {
    try {
        const response = await fetch(`${API_BASE_URL}/Billets`, {
            credentials: 'include'
        });
        if (!response.ok) {
            throw new Error('Erreur lors du chargement des billets');
        }
        userTickets = await response.json();
        displayTickets();
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de charger vos billets');
        userTickets = []; // Assure que displayTickets affiche l'état vide
        displayTickets();
    }
}

function displayTickets() {
    const container = document.getElementById('ticketsContainer');
    if (!container) return;
    if (!userTickets || userTickets.length === 0) {
        container.innerHTML = `
            <div style="text-align: center; padding: 40px; color: #666;">
                <p style="font-size: 1.2em;">📭 Vous n'avez pas encore de billets</p>
                <a href="/offres.html" class="btn btn-primary" style="margin-top: 20px;">
                    Découvrir les offres
                </a>
            </div>
        `;
        return;
    }
    container.innerHTML = userTickets.map(billet => `
        <div class="ticket-card" onclick="toggleTicketDetails(this)">
            <div class="ticket-header">
                <div class="ticket-title">${billet.titre}</div>
                <div class="ticket-status ${getStatusClass(billet.statut)}">
                    ${billet.statut}
                </div>
            </div>
            <div class="ticket-info">
                📅 ${formatDate(billet.dateEpreuve)}<br>
                📍 ${billet.lieu}<br>
                🎫 Billet #${billet.numero}
            </div>
            <div class="ticket-details">
                <div class="detail-row">
                    <span><strong>Numéro du billet :</strong></span>
                    <span>${billet.numero}</span>
                </div>
                <div class="detail-row">
                    <span><strong>Statut :</strong></span>
                    <span>${getStatusText(billet)}</span>
                </div>
                <div class="detail-row">
                    <span><strong>Place :</strong></span>
                    <span>${billet.place || 'Non attribuée'}</span>
                </div>
                <div class="qr-actions">
                    ${billet.statut === 'Actif' ? `
                        <button class="btn btn-primary btn-small" onclick="viewQRCode(event, ${billet.id})">
                            👁️ Voir le QR code
                        </button>
                        <button class="btn btn-secondary btn-small" onclick="downloadPDF(event, ${billet.id})">
                            📄 Télécharger PDF
                        </button>
                        <button class="btn btn-outline btn-small" onclick="sendByEmail(event, ${billet.id})">
                            📧 Envoyer par email
                        </button>
                    ` : `
                        <button class="btn btn-secondary btn-small" disabled>
                            ✅ Billet utilisé
                        </button>
                        <button class="btn btn-secondary btn-small" onclick="downloadPDF(event, ${billet.id})">
                            📄 Télécharger PDF
                        </button>
                    `}
                </div>
            </div>
        </div>
    `).join('');
}

// Charger les commandes
async function loadOrders() {
    try {
        const response = await fetch(`${API_BASE_URL}/Commandes`, {
            credentials: 'include'
        });
        if (!response.ok) {
            throw new Error('Erreur lors du chargement des commandes');
        }
        userOrders = await response.json();
        displayOrders();
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de charger vos commandes');
        userOrders = [];
        displayOrders();
    }
}

function displayOrders() {
    const tbody = document.querySelector('#orders tbody');
    if (!tbody) return;
    if (!userOrders || userOrders.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" style="text-align: center; padding: 40px; color: #666;">
                    Aucune commande pour le moment
                </td>
            </tr>
        `;
        return;
    }
    tbody.innerHTML = userOrders.map(commande => `
        <tr>
            <td><strong>${commande.numero}</strong></td>
            <td>
                ${formatDate(commande.dateAchat)}<br>
                ${formatTime(commande.dateAchat)}
            </td>
            <td>
                ${commande.items.map(item => `
                    • ${item.offreNom}<br>
                    • ${item.quantite}x billet${item.quantite > 1 ? 's' : ''} à ${item.prix}€
                `).join('<br>')}
            </td>
            <td><strong>${commande.montantTotal}€</strong></td>
            <td>
                <span class="ticket-status ${getStatusClass(commande.statut)}">
                    ${commande.statut}
                </span>
            </td>
        </tr>
    `).join('');
}

// Charger le profil utilisateur
async function loadUserProfile() {
    try {
        const response = await fetch(`${API_BASE_URL}/Compte/profile`, {
            credentials: 'include'
        });
        if (!response.ok) {
            throw new Error('Erreur lors du chargement du profil');
        }
        const profile = await response.json();
        fillProfileForm(profile);
    } catch (error) {
        console.error('Erreur:', error);
        // En dev, tu peux remplir avec le pseudo-user si besoin
        fillProfileForm({ prenom: currentUser ? currentUser.prenom : '', nom: currentUser ? currentUser.nom : '', email: '' });
    }
}

function fillProfileForm(profile) {
    document.getElementById('firstName').value = profile.prenom || '';
    document.getElementById('lastName').value = profile.nom || '';
    document.getElementById('email').value = profile.email || '';
}

// ===========================
// ACTIONS SUR LES BILLETS
// ===========================

function toggleTicketDetails(card) {
    const details = card.querySelector('.ticket-details');
    const isOpen = details.style.display === 'block';
    document.querySelectorAll('.ticket-details').forEach(d => d.style.display = 'none');
    if (!isOpen) {
        details.style.display = 'block';
    }
}

async function viewQRCode(event, billetId) {
    event.stopPropagation();
    try {
        const response = await fetch(`${API_BASE_URL}/Billets/${billetId}`, {
            credentials: 'include'
        });
        if (!response.ok) {
            throw new Error('Erreur lors de la récupération du QR code');
        }
        const billet = await response.json();
        showQRCodeModal(billet);
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible d\'afficher le QR code');
    }
}

function showQRCodeModal(billet) {
    const modal = document.createElement('div');
    modal.className = 'modal-overlay';
    modal.innerHTML = `
        <div class="modal-content">
            <div class="modal-header">
                <h3>QR Code - ${billet.numero}</h3>
                <button class="modal-close" onclick="this.closest('.modal-overlay').remove()">✕</button>
            </div>
            <div class="modal-body" style="text-align: center; padding: 30px;">
                <img src="${billet.codeQR}" alt="QR Code" style="max-width: 300px; border: 2px solid #004e92;">
                <p style="margin-top: 20px; color: #666;">
                    ${billet.titre}<br>📅 ${formatDate(billet.dateEpreuve)}
                </p>
            </div>
        </div>
    `;
    document.body.appendChild(modal);
}

async function downloadPDF(event, billetId) {
    event.stopPropagation();
    try {
        const response = await fetch(`${API_BASE_URL}/Billets/${billetId}/download`, {
            method: 'POST',
            credentials: 'include'
        });
        if (!response.ok) {
            throw new Error('Erreur lors du téléchargement');
        }
        const result = await response.json();
        showSuccess('PDF généré avec succès ! Téléchargement en cours...');
        console.log('Download URL:', result.downloadUrl);
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de télécharger le PDF');
    }
}

async function sendByEmail(event, billetId) {
    event.stopPropagation();
    if (!confirm('Voulez-vous recevoir ce billet par email ?')) return;
    try {
        const response = await fetch(`${API_BASE_URL}/Billets/${billetId}/email`, {
            method: 'POST',
            credentials: 'include'
        });
        if (!response.ok) throw new Error('Erreur lors de l\'envoi');
        const result = await response.json();
        showSuccess(result.message);
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible d\'envoyer l\'email');
    }
}

// ===========================
// GESTION DU PROFIL
// ===========================

async function saveSettings(event) {
    event.preventDefault();
    const formData = {
        prenom: document.getElementById('firstName').value,
        nom: document.getElementById('lastName').value,
        email: document.getElementById('email').value
    };
    try {
        const response = await fetch(`${API_BASE_URL}/Compte/profile`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify
